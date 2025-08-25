using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

public class GithubController(HttpClient httpClient, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IdentityContext identityContext) : Controller
{
	static string loginUrl = "https://github.com/login/oauth/authorize";
	static string idTokenUrl = "https://github.com/login/oauth/access_token";
	string callbackUrl => GetBaseUrl(Request) + "/GitHub/Callback";
	static string clientId = GetEnvironmentVariable("github-client-id");
	static string clientSecret = GetEnvironmentVariable("github-client-secret");

	[HttpGet]
	public IActionResult Login(string redirectUrl)
	{
		var state = JsonSerializer.Serialize(new
		{
			redirectUrl,
			csrf = Guid.NewGuid()
		});

		HttpContext.Session.SetString("state", state);

		string scope = "read:user user:email";

		string url = $"""{loginUrl}?client_id={clientId}&redirect_uri={callbackUrl}&scope={scope}&state={state}""";

		return Redirect(url);
	}

	[HttpGet]
	public async Task<IActionResult> Callback(string state, string code)
	{
		var sessionState = HttpContext.Session.GetString("state");

		if (state == sessionState)
		{
			HttpContext.Session.Clear();

			using JsonDocument stateJson = JsonDocument.Parse(state);
			string? redirectUrl = stateJson.RootElement.GetProperty("redirectUrl").GetString();

			ExternalAuthUserInfo externalUserInfo;
			{
				var response = await httpClient.PostAsync(idTokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>()
				{
					["code"] = code,
					["redirect_uri"] = callbackUrl,
					["client_id"] = clientId,
					["client_secret"] = clientSecret,
				}));

				var data = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

				var dict = System.Web.HttpUtility.ParseQueryString(data);
				var accessToken = dict["access_token"];

				string id = "";
				{
					HttpRequestMessage request = new(HttpMethod.Get, "https://api.github.com/user");
					request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
					request.Headers.Add("User-Agent", "aspnetcore-htmx");

					response = await httpClient.SendAsync(request);
					data = await response.Content.ReadAsStringAsync();

					using JsonDocument userDoc = JsonDocument.Parse(data);

					id = userDoc.RootElement.GetProperty("id").GetInt64().ToString();
				}

				string email = "";
				{
					HttpRequestMessage request = new(HttpMethod.Get, "https://api.github.com/user/emails");
					request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
					request.Headers.Add("User-Agent", "aspnetcore-htmx");

					response = await httpClient.SendAsync(request);
					data = await response.Content.ReadAsStringAsync();

					using JsonDocument emailDoc = JsonDocument.Parse(data);

					foreach (var element in emailDoc.RootElement.EnumerateArray())
					{
						bool primary = element.GetProperty("primary").GetBoolean();
						bool verified = element.GetProperty("verified").GetBoolean();

						if (primary && verified)
						{
							email = element.GetProperty("email").GetString() ?? "";
							break;
						}
					}
				}

				externalUserInfo = new(id.ToString(), email);
			}

			if (!string.IsNullOrWhiteSpace(externalUserInfo.Email))
			{
				var result = await CreateOrLinkUser(externalUserInfo.Email, externalUserInfo.Id, "github", "Github", userManager);

				if (result.IsSuccess(out var user))
				{
					if (string.IsNullOrWhiteSpace(redirectUrl))
					{
						await signInManager.SignInAsync(user, true);
						return LocalRedirect("/");
					}
					else
					{
						var access_token = GenerateJwtToken(user.Id, user.Email!);
						return Redirect(redirectUrl + $"?access_token={access_token}&");
					}
				}
				else
				{
					throw new Exception(result.ErrorMessage.ToString());
				}
			}
			else
			{
				return BadRequest("Cannot continue with account creation as the Github account does not have an email associated with it.");
			}
		}
		else
		{
			return Unauthorized();
		}
	}
}