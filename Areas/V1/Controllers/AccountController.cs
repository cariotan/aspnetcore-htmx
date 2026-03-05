using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Area("V1")]
public class AccountController(
	ILogger<AccountController> logger,
	UserManager<ApplicationUser> userManager,
	SignInManager<ApplicationUser> signInManager
) : HtmxController
{
	[AllowAnonymous]
	[HttpGet]
	public IActionResult Register()
	{
		logger.Endpoint(Get, "/Account/Register");
		HxPushUrl(Url.Action("Register")!);

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

		ApplicationUser dbUser;
		{
			var result = await Auth_CreateUser(
				registerModel.Email,
				userManager,
				registerModel.Password
			);
			if(result.IsNotSuccessful)
			{
				ModelState.AddModelError("Error", result.ErrorMessage.ToString());
				return View(registerModel);
			}
			dbUser = result.Value;
		}

		await signInManager.SignInAsync(dbUser, true);

		if (registerModel.RequireTwoFactor)
		{
			return RedirectToAction("Enable2fa", "Account");
		}
		else
		{
			return RedirectToAction("Index", "Home");
		}
	}

	[HttpGet]
	[Authorize]
	public async Task<IActionResult> Enable2fa()
	{
		logger.Endpoint(Get, "/Account/Enable2fa");
		HxPushUrl(Url.Action("Enable2fa")!);

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

				HxPushUrl(Url.Action("Enable2fa")!);
				return View(new Enable2faModel(authenticatorKey!, qrBase64));
			}
			else
			{
				return RedirectToAction("Index", "Home");
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
		logger.Endpoint(Post, "/Account/Enable2fa");

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

			HxPushUrl(Url.Action("Index", "Home")!);
			return RedirectToAction("Index", "Home");
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
		HxPushUrl(Url.Action("Login", new { returnUrl })!);

		ViewData["ReturnUrl"] = returnUrl;

		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Login(
		LoginModel loginModel,
		string? returnUrl
	)
	{
		logger.Endpoint(Get, "/Account/Login");

		ViewData["ReturnUrl"] = returnUrl;

		if (!ModelState.IsValid)
		{
			return View(loginModel);
		}

		var result = await signInManager.PasswordSignInAsync(
			loginModel.Email,
			loginModel.Password,
			true,
			true
		);

		if (result.Succeeded)
		{
			if (!string.IsNullOrWhiteSpace(returnUrl))
			{
				HxPushUrl(returnUrl);
				return LocalRedirect(returnUrl);
			}
			else
			{
				return RedirectToAction("Index", "Home");
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
		HxPushUrl(Url.Action("TwoFactor", new { returnUrl })!);

		ViewData["ReturnUrl"] = returnUrl;

		var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user is { })
		{
			return View(new TwoFactorModel("", true));
		}
		else
		{
			Js("No user for two factor");
			return RedirectToAction("Index", "Home");
		}
	}

	[HttpPost]
	public async Task<IActionResult> TwoFactor(
		TwoFactorModel twoFactorModel,
		string? returnUrl
	)
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
			var result = await signInManager.TwoFactorAuthenticatorSignInAsync(
				twoFactorModel.Code,
				true,
				twoFactorModel.RememberClient
			);
			if (result.Succeeded)
			{
				if (!string.IsNullOrWhiteSpace(returnUrl))
				{
					return LocalRedirect(returnUrl);
				}
				else
				{
					return RedirectToAction("Index", "Home");
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
			Js("No user for two factor");
			return RedirectToAction("Login");
		}
	}

	[HttpPost]
	public async Task<IActionResult> Logout()
	{
		logger.Endpoint(Post, "/Account/Logout");

		await signInManager.SignOutAsync();

		return RedirectToAction("Login");
	}

	[HttpGet]
	public IActionResult AccessDenied()
	{
		logger.Endpoint(Get, "/Account/AccessDenied");
		HxPushUrl(Url.Action("AccessDenied")!);

		return View();
	}
}