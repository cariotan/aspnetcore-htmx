using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

public class GithubController(HttpClient httpClient, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : Controller
{
	static string loginUrl = "https://github.com/login/oauth/authorize";
	static string idTokenUrl = "https://github.com/login/oauth/access_token";
	static string redirectUrl = GetEnvironmentVariable("github-redirect-url");
	static string clientId = GetEnvironmentVariable("github-client-id");
	static string clientSecret = GetEnvironmentVariable("github-client-secret");

	[HttpGet]
	public IActionResult Login()
	{
		var state = Guid.NewGuid().ToString();

		HttpContext.Session.SetString("state", state);

		string scope = "read:user user:email";

		string url = $"""{loginUrl}?client_id={clientId}&redirect_uri={redirectUrl}&scope={scope}&state={state}""";

		return Redirect(url);
	}

	[HttpGet]
	public async Task<IActionResult> Callback(string state, string code)
	{
		var sessionState = HttpContext.Session.GetString("state");

		if (state == sessionState)
		{
			HttpContext.Session.Clear();

			ExternalAuthUserInfo externalUserInfo;
			{
				var response = await httpClient.PostAsync(idTokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>()
				{
					["code"] = code,
					["redirect_uri"] = redirectUrl,
					["client_id"] = clientId,
					["client_secret"] = clientSecret,
				}));

				var data = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

				var dict = System.Web.HttpUtility.ParseQueryString(data);
				var accessToken = dict["access_token"];

				string id = "";
				string email = "";

				// Get user id.
				{
					HttpRequestMessage request = new(HttpMethod.Get, "https://api.github.com/user");
					request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
					request.Headers.Add("User-Agent", "aspnetcore-htmx");

					response = await httpClient.SendAsync(request);
					data = await response.Content.ReadAsStringAsync();

					using JsonDocument userDoc = JsonDocument.Parse(data);

					id = userDoc.RootElement.GetProperty("id").GetInt64().ToString();
				}

				// Get user primary email.
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

			var user = await userManager.FindByLoginAsync("github", externalUserInfo.Id);

			if (user is { })
			{
				await signInManager.SignInAsync(user, true);

				return LocalRedirect("/");
			}
			else
			{
				user = new(externalUserInfo.Email);

				var result = await userManager.CreateAsync(user);

				if (result.Succeeded)
				{
					result = await userManager.AddLoginAsync(user, new("github", externalUserInfo.Id, "Github"));

					if (result.Succeeded)
					{
						await signInManager.SignInAsync(user, true);
						return LocalRedirect("/");
					}
					else
					{
						throw new Exception(result.Errors.First().Description);
					}
				}
				else
				{
					throw new Exception(result.Errors.First().Description);
				}
			}
		}
		else
		{
			return Unauthorized();
		}
	}
}