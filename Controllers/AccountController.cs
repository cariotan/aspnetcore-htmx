using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class AccountController(ILogger<AccountController> l, UserManager<ApplicationUser> u, SignInManager<ApplicationUser> s) : HtmxController
{
	[AllowAnonymous]
	[HttpGet]
	public IActionResult Login()
	{
		l.Endpoint(Get, "/Account/Login");

		return View();
	}
}