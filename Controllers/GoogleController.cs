using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

public class GoogleController(HttpClient httpClient, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : Controller
{
	static string loginUrl = "https://accounts.google.com/o/oauth2/v2/auth";
	static string idTokenUrl = "https://oauth2.googleapis.com/token";
	string callbackUrl => GetBaseUrl(Request) + "/Google/Callback";
	static string clientId = GetEnvironmentVariable("google-client-id");
	static string clientSecret = GetEnvironmentVariable("google-client-secret");

	[HttpGet]
	public IActionResult Login(string redirectUrl)
	{
		var state = JsonSerializer.Serialize(new
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

			using JsonDocument stateJson = JsonDocument.Parse(state);
			string? redirectUrl = stateJson.RootElement.GetProperty("redirectUrl").GetString();

			ExternalAuthUserInfo externalUserInfo;
			{
				var response = await httpClient.PostAsync(idTokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>()
				{
					["grant_type"] = "authorization_code",
					["code"] = code,
					["redirect_uri"] = callbackUrl,
					["client_id"] = clientId,
					["client_secret"] = clientSecret,
				}));

				var data = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

				using JsonDocument doc = JsonDocument.Parse(data);

				string id_token = doc.RootElement.GetProperty("id_token").GetString()!;
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