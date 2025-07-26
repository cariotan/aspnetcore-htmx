using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class AccountController(ILogger<AccountController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : HtmxController, IHasUser
{
	public ApplicationUser CurrentUser { get; set; } = null!;

	[AllowAnonymous]
	[HttpGet]
	public IActionResult Login()
	{
		logger.Endpoint(Get, "/Account/Login");

		return View();
	}

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

		Js(registerModel);

		if (registerModel.Password != registerModel.ConfirmPassword)
		{
			ModelState.AddModelError(nameof(registerModel.Password), "Passwords do not match");
			return View(registerModel);
		}

		var result = await userManager.CreateAsync(new(email), registerModel.Password);

		if (result.Succeeded)
		{
			return RedirectToAction("Login");
		}
		else
		{
			ModelState.AddModelError("Error", result.Errors.First().Description);
			return View(registerModel);
		}
	}

	public async Task<IActionResult> Login(LoginModel loginModel)
	{
		logger.Endpoint(Get, "/Account/Login");

		if (!ModelState.IsValid)
		{
			return View(loginModel);
		}

		var result = await signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, true);

		if (result.Succeeded)
		{

			return RedirectToAction("Index", "Home");
		}
		else
		{
			ModelState.AddModelError("Error", "Invalid email or password");
			return View(loginModel);
		}
	}
}