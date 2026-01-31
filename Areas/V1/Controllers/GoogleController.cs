using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Area("V1")]
public class GoogleController(
	HttpClient httpClient,
	UserManager<ApplicationUser> userManager,
	SignInManager<ApplicationUser> signInManager,
	ILogger<GoogleController> logger
) : Controller
{
	static string loginUrl = "https://accounts.google.com/o/oauth2/v2/auth";
	static string idTokenUrl = "https://oauth2.googleapis.com/token";
	string callbackUrl => GetBaseUrl(Request) + "/Google/Callback";
	static string? clientId = GetEnvironmentVariable("google-client-id");
	static string? clientSecret = GetEnvironmentVariable("google-client-secret");

	[HttpGet]
	public IActionResult Login(string redirectUrl)
	{
		var state = JsonConvert.SerializeObject(new
		{
			redirectUrl,
			csrf = Guid.NewGuid()
		});

		HttpContext.Session.SetString("state", state);

		string scope = "openid email profile";

		string url = $"""{loginUrl}?response_type=code&client_id={clientId}&redirect_uri={callbackUrl}&scope={scope}&state={state}""";

		return Redirect(url);
	}

	[HttpGet]
	public async Task<IActionResult> Callback(string state, string code, string scope, string authuser, string prompt)
	{
		var sessionState = HttpContext.Session.GetString("state");

		if (state == sessionState)
		{
			HttpContext.Session.Clear();

			JObject stateJson = JObject.Parse(state);
			string? redirectUrl = (string?)stateJson["redirectUrl"];

			ExternalAuthUserInfo externalUserInfo;
			{
				var response = await httpClient.PostAsync(idTokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>()
				{
					["grant_type"] = "authorization_code",
					["code"] = code,
					["redirect_uri"] = callbackUrl,
					["client_id"] = clientId ?? throw new Exception("ClientId is null."),
					["client_secret"] = clientSecret ?? throw new Exception("ClientSecret is null."),
				}));

				var data = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

				JObject doc = JObject.Parse(data);

				string? id_token = (string?)doc["id_token"];
				// string access_token = doc.RootElement.GetProperty("access_token").GetString();

				var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(id_token);

				var sub = jwtToken.Claims.First(x => x.Type == "sub").Value;
				var email = jwtToken.Claims.First(x => x.Type == "email").Value;

				externalUserInfo = new(sub, email);
			}

			var result = await CreateOrLinkUser(externalUserInfo.Email, externalUserInfo.Id, "google", "Google", userManager);

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
			return Unauthorized();
		}
	}
}

public record ExternalAuthUserInfo(string Id, string Email);