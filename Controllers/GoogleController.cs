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
	static string redirectUrl = GetEnvironmentVariable("google-redirect-url");
	static string clientId = GetEnvironmentVariable("google-client-id");
	static string clientSecret = GetEnvironmentVariable("google-client-secret");

	[HttpGet]
	public IActionResult Login()
	{
		var state = Guid.NewGuid().ToString();

		HttpContext.Session.SetString("state", state);

		string scope = "openid email profile";

		string url = $"""{loginUrl}?response_type=code&client_id={clientId}&redirect_uri={redirectUrl}&scope={scope}&state={state}""";

		return Redirect(url);
	}

	[HttpGet]
	public async Task<IActionResult> Callback(string state, string code, string scope, string authuser, string prompt)
	{
		var sessionState = HttpContext.Session.GetString("state");

		if (state == sessionState)
		{
			var externalUserInfo = await GetExternalAuthUserInfo(code, httpClient);

			var user = await userManager.FindByLoginAsync("google", externalUserInfo.Id);

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
					result = await userManager.AddLoginAsync(user, new("google", externalUserInfo.Id, "Google"));

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

		static async Task<ExternalAuthUserInfo> GetExternalAuthUserInfo(string code, HttpClient httpClient)
		{
			var response = await httpClient.PostAsync(idTokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>()
			{
				["grant_type"] = "authorization_code",
				["code"] = code,
				["redirect_uri"] = redirectUrl,
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

			return new ExternalAuthUserInfo(sub, email);
		}
	}
}

public record ExternalAuthUserInfo(string Id, string Email);