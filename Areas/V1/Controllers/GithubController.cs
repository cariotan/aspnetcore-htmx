using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Area("V1")]
public class GithubController(
	HttpClient httpClient,
	UserManager<ApplicationUser> userManager,
	SignInManager<ApplicationUser> signInManager,
	ILogger<GithubController> logger,
	IDataProtectionProvider dataProtectorProvider,
	EndpointDataSource endpointDataSource
) : Controller
{
	static string loginUrl = "https://github.com/login/oauth/authorize";
	static string idTokenUrl = "https://github.com/login/oauth/access_token";
	string callbackUrl => Environment_GetBaseUrl(Request) + "/GitHub/Callback";
	static string? clientId = GetEnvironmentVariable("github-client-id");
	static string? clientSecret = GetEnvironmentVariable("github-client-secret");

	[HttpGet]
	public IActionResult Login(string redirectUrl)
	{
		logger.Endpoint(Get, "/Github/Login");

		var state = JsonConvert.SerializeObject(new
		{
			redirectUrl,
			csrf = Guid.NewGuid()
		});

		var protector = dataProtectorProvider.CreateProtector("GithubAuth.State");

		string protectedState = protector.Protect(state);
		CookieOptions options = new()
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Lax,
			Expires = DateTimeOffset.UtcNow.AddMinutes(15)
		};
		Response.Cookies.Append("github_auth_state", protectedState, options);

		string scope = "read:user user:email";

		string url = $"""{loginUrl}?client_id={clientId}&redirect_uri={callbackUrl}&scope={scope}&state={protectedState}""";

		return Redirect(url);
	}

	[HttpGet]
	public async Task<IActionResult> Callback(string state, string code)
	{
		logger.Endpoint(Get, "/Github/Callback");

		if(!Request.Cookies.TryGetValue("github_auth_state", out var cookieState))
		{
			return Unauthorized();
		}
		
		if(state != cookieState)
		{
			return Unauthorized();
		}
		
		var protector = dataProtectorProvider.CreateProtector("GithubAuth.State");
		string decryptedState = protector.Unprotect(state);

		JObject stateJson = JObject.Parse(decryptedState);
		string? redirectUrl = (string?)stateJson["redirectUrl"];

		ExternalAuthUserInfo externalUserInfo;
		{
			var response = await httpClient.PostAsync(idTokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>()
			{
				["code"] = code,
				["redirect_uri"] = callbackUrl,
				["client_id"] = clientId ?? throw new Exception("ClientId is null."),
				["client_secret"] = clientSecret ?? throw new Exception("ClientId is null."),
			}));

			var data = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

			var dict = System.Web.HttpUtility.ParseQueryString(data);
			var accessToken = dict["access_token"];

			string? id = "";
			{
				HttpRequestMessage request = new(HttpMethod.Get, "https://api.github.com/user");
				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
				request.Headers.Add("User-Agent", "aspnetcore-htmx");

				response = await httpClient.SendAsync(request);
				data = await response.Content.ReadAsStringAsync();

				JObject userDoc = JObject.Parse(data);

				id = (string?)userDoc["id"];
			}

			string? email = "";
			{
				HttpRequestMessage request = new(HttpMethod.Get, "https://api.github.com/user/emails");
				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
				request.Headers.Add("User-Agent", "aspnetcore-htmx");

				response = await httpClient.SendAsync(request);
				data = await response.Content.ReadAsStringAsync();

				JArray emailDoc = JArray.Parse(data);

				foreach (JObject element in emailDoc)
				{
					bool? primary = (bool?)element["primary"];
					bool? verified = (bool?)element["verified"];

					if (primary == true && verified == true)
					{
						email = (string?)element["email"];
						break;
					}
				}
			}

			if(id is null)
			{
				throw new Exception("Id returned from callback response is null.");
			}

			if(email is null)
			{
				throw new Exception("Email returned from callback response is null.");
			}

			externalUserInfo = new(id.ToString(), email);
		}

		if (!string.IsNullOrWhiteSpace(externalUserInfo.Email))
		{
			var area = GetDefaultArea(endpointDataSource);
			var result = await CreateOrLinkUser(externalUserInfo.Email, externalUserInfo.Id, "google", "Google", userManager, area: area);

			if (result.IsSuccess(out var user))
			{
				if (string.IsNullOrWhiteSpace(redirectUrl))
				{
					await signInManager.SignInAsync(user, true);
					return RedirectToAction("Index", "Home", new { Area = area });
				}
				else
				{
					var access_token = Auth_GenerateJwtToken(user.Id, user.Email!);
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
}