using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class AccountController(ILogger<AccountController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : HtmxController, IHasUser
{
	public ApplicationUser CurrentUser { get; set; } = null!;

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
			if (registerModel.RequireTwoFactor)
			{
				await userManager.SetTwoFactorEnabledAsync(newUser, true);
				await userManager.ResetAuthenticatorKeyAsync(newUser);
			}

			return LocalRedirect("/Login");
		}
		else
		{
			ModelState.AddModelError("Error", result.Errors.First().Description);

			return View(registerModel);
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
}