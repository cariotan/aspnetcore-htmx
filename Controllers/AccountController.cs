using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class AccountController(ILogger<AccountController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, HttpClient httpClient) : HtmxController
{
	static string GoogleRedirectUrl = GetEnvironmentVariable("google-redirect-url");

	[AllowAnonymous]
	[HttpGet]
	public IActionResult Register()
	{
		logger.Endpoint(Get, "/Account/Register");

		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Register(RegisterModel registerModel)
	{
		logger.Endpoint(Post, "/Account/Register");

		if (!ModelState.IsValid)
		{
			return View(registerModel);
		}

		var email = registerModel.Email.Trim();

		if (registerModel.Password != registerModel.ConfirmPassword)
		{
			ModelState.AddModelError(nameof(registerModel.Password), "Passwords do not match");

			return View(registerModel);
		}

		ApplicationUser newUser = new(email);

		var result = await userManager.CreateAsync(newUser, registerModel.Password);

		if (result.Succeeded)
		{
			await signInManager.SignInAsync(newUser, true);

			if (registerModel.RequireTwoFactor)
			{
				return LocalRedirect("/Account/Enable2fa");
			}
			else
			{
				return LocalRedirect("/");
			}
		}
		else
		{
			ModelState.AddModelError("Error", result.Errors.First().Description);

			return View(registerModel);
		}
	}

	[HttpGet]
	[Authorize]
	public async Task<IActionResult> Enable2fa()
	{
		var user = await userManager.GetUserAsync(User);
		if (user is { })
		{
			var enabled = await userManager.GetTwoFactorEnabledAsync(user);
			if (!enabled)
			{
				await userManager.ResetAuthenticatorKeyAsync(user);

				var authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);

				var otpUri = $"otpauth://totp:{user.Email}?secret={authenticatorKey}";
				var qrBase64 = GetBase64QrCode(otpUri);

				await signInManager.SignInAsync(user, true);

				return View(new Enable2faModel(authenticatorKey!, qrBase64));
			}
			else
			{
				return LocalRedirect("/");
			}
		}
		else
		{
			return Unauthorized();
		}
	}

	[HttpPost]
	[Authorize]
	public async Task<IActionResult> Enable2fa(string code)
	{
		if (!ModelState.IsValid)
		{
			return View();
		}

		var user = await userManager.GetUserAsync(User) ?? throw new Exception($"This won't be null");

		var verified = await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, code);

		if (verified)
		{
			await userManager.SetTwoFactorEnabledAsync(user, true);

			await signInManager.SignInAsync(user, true);

			HxRedirect("/");

			return Ok();
		}
		else
		{
			ModelState.AddModelError(nameof(code), "Code is not correct.");
			return View();
		}
	}

	[AllowAnonymous]
	[HttpGet]
	public IActionResult Login(string? returnUrl)
	{
		logger.Endpoint(Get, "/Account/Login");

		ViewData["ReturnUrl"] = returnUrl;

		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Login(LoginModel loginModel, string? returnUrl)
	{
		logger.Endpoint(Get, "/Account/Login");

		ViewData["ReturnUrl"] = returnUrl;

		if (!ModelState.IsValid)
		{
			return View(loginModel);
		}

		var result = await signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, true);

		if (result.Succeeded)
		{
			if (!string.IsNullOrWhiteSpace(returnUrl))
			{
				return LocalRedirect(returnUrl);
			}
			else
			{
				return LocalRedirect("/");
			}
		}
		else if (result.RequiresTwoFactor)
		{
			return RedirectToAction("TwoFactor", new { returnUrl });
		}
		else
		{
			ModelState.AddModelError("Error", "Invalid email or password");
			return View(loginModel);
		}
	}

	[HttpGet]
	public async Task<IActionResult> TwoFactor(string? returnUrl)
	{
		logger.Endpoint(Get, "/Account/TwoFactor");

		ViewData["ReturnUrl"] = returnUrl;

		var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user is { })
		{
			return View();
		}
		else
		{
			System.Console.WriteLine("No user for two factor");
			return LocalRedirect("/");
		}
	}

	[HttpPost]
	public async Task<IActionResult> TwoFactor(TwoFactorModel twoFactorModel, string? returnUrl)
	{
		logger.Endpoint(Post, "/Account/TwoFactor");

		ViewData["ReturnUrl"] = returnUrl;

		if (!ModelState.IsValid)
		{
			return View(twoFactorModel);
		}

		var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user is { })
		{
			var result = await signInManager.TwoFactorAuthenticatorSignInAsync(twoFactorModel.Code, true, twoFactorModel.RememberClient);
			if (result.Succeeded)
			{
				if (!string.IsNullOrWhiteSpace(returnUrl))
				{
					return LocalRedirect(returnUrl);
				}
				else
				{
					return LocalRedirect("/");
				}
			}
			else
			{
				ModelState.AddModelError("Error", "Code is incorrect.");
				return View(twoFactorModel);
			}
		}
		else
		{
			System.Console.WriteLine("No user for two factor");
			return LocalRedirect("/Login");
		}
	}

	[HttpPost]
	public async Task<IActionResult> Logout()
	{
		await signInManager.SignOutAsync();

		return RedirectToAction("Login");
	}

	[HttpGet]
	public IActionResult AccessDenied()
	{
		return View();
	}

	[HttpGet]
	public IActionResult GoogleLogin()
	{
		var state = Guid.NewGuid();

		HttpContext.Session.SetString("google-state", state.ToString());

		var url = $"""https://accounts.google.com/o/oauth2/v2/auth?response_type=code&client_id={GetEnvironmentVariable("google-client-id")}&redirect_uri={GoogleRedirectUrl}&scope=openid email profile&state={state}""";

		return Redirect(url);
	}

	[HttpGet]
	public async Task<IActionResult> Google(string state, string code, string scope, string authuser, string prompt)
	{
		var url = "https://oauth2.googleapis.com/token";

		var sessionState = HttpContext.Session.GetString("google-state");

		if (state == sessionState)
		{
			var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string>()
			{
				["grant_type"] = "authorization_code",
				["code"] = code,
				["redirect_uri"] = GoogleRedirectUrl,
				["client_id"] = GetEnvironmentVariable("google-client-id"),
				["client_secret"] = GetEnvironmentVariable("google-client-secret"),
			}));

			var googleResponse = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<GoogleResponse>();

			var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(googleResponse!.IdToken);

			var sub = jwtToken.Claims.First(x => x.Type == "sub").Value;
			var email = jwtToken.Claims.First(x => x.Type == "email").Value;

			var user = await userManager.FindByLoginAsync("google", sub);

			if (user is { })
			{
				await signInManager.SignInAsync(user, true);

				return LocalRedirect("/");
			}
			else
			{
				user = new(email);

				var result = await userManager.CreateAsync(user);

				if (result.Succeeded)
				{
					result = await userManager.AddLoginAsync(user, new("google", sub, "Google"));

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

	private class GoogleUserResponse
	{
		[JsonPropertyName("sub")]
		public required string Sub { get; set; }

		[JsonPropertyName("email")]
		public required string Email { get; set; }
	}

	private class GoogleResponse
	{
		[JsonPropertyName("id_token")]
		public required string IdToken { get; set; }

		[JsonPropertyName("access_token")]
		public required string AccessToken { get; set; }
	}
}