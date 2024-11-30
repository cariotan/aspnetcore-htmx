using Microsoft.AspNetCore.Mvc;

public class AccountController : HtmxController
{
	[HttpGet]
	// /Account/Login
	public IActionResult Login()
	{
		return View();
	}
}