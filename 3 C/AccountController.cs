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