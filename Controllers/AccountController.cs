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
}