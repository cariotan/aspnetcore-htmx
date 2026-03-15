using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Area("V1")]
public class GoogleController(
	HttpClient httpClient,
	UserManager<ApplicationUser> userManager,
	SignInManager<ApplicationUser> signInManager,
	ILogger<GoogleController> logger,
	IDataProtectionProvider dataProtectorProvider,
	EndpointDataSource endpointDataSource
) : Controller
{
	static string loginUrl = "https://accounts.google.com/o/oauth2/v2/auth";
	static string idTokenUrl = "https://oauth2.googleapis.com/token";
	string callbackUrl => Environment_GetBaseUrl(Request) + "/Google/Callback";
	static string? clientId = GetEnvironmentVariable("google-client-id");
	static string? clientSecret = GetEnvironmentVariable("google-client-secret");

	[HttpGet]
	public IActionResult Login(string redirectUrl)
	{
		logger.Endpoint(Get, "/Google/Login");

		var state = JsonConvert.SerializeObject(new
		{
			redirectUrl,
			csrf = Guid.NewGuid()
		});

		var protector = dataProtectorProvider.CreateProtector("GoogleAuth.State");

		string protectedState = protector.Protect(state);
		CookieOptions options = new()
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Lax,
			Expires = DateTimeOffset.UtcNow.AddMinutes(15)
		};
		Response.Cookies.Append("google_auth_state", protectedState, options);

		string scope = "openid email profile";

		string url = $"""{loginUrl}?response_type=code&client_id={clientId}&redirect_uri={callbackUrl}&scope={scope}&state={protectedState}""";

		return Redirect(url);
	}

	[HttpGet]
	public async Task<IActionResult> Callback(string state, string code, string scope, string authuser, string prompt)
	{
		logger.Endpoint(Get, "/Google/Callback");

		if(!Request.Cookies.TryGetValue("google_auth_state", out var cookieState))
		{
			return Unauthorized();
		}
		
		if(state != cookieState)
		{
			return Unauthorized();
		}
		
		var protector = dataProtectorProvider.CreateProtector("GoogleAuth.State");
		string decryptedState = protector.Unprotect(state);

		JObject stateJson = JObject.Parse(decryptedState);
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

		var area = GetDefaultArea(endpointDataSource);
		var result = await CreateOrLinkUser(externalUserInfo.Email, externalUserInfo.Id, "google", "Google", userManager, area: area);

		if(result.IsSuccess(out var user))
		{
			if(string.IsNullOrWhiteSpace(redirectUrl))
			{
				await signInManager.SignInAsync(user, true);
				return RedirectToAction("Index", "Home", new { area });
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
}

public record ExternalAuthUserInfo(string Id, string Email);