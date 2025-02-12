using Microsoft.AspNetCore.Mvc;

public class AccountController(ILogger<AccountController> logger) : HtmxController
{
	[HttpGet]
	public IActionResult Login()
	{
		logger.LogInformation("Get /Account/Login");

		return View();
	}
}