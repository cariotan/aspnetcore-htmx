using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class AccountController(ILogger<AccountController> logger) : HtmxController
{
	[AllowAnonymous]
	[HttpGet]
	public IActionResult Login()
	{
		logger.Endpoint(Get, "/Account/Login");

		return View();
	}
}