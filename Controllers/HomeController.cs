using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class HomeController(ILogger<HomeController> logger) : HtmxController
{
	[HttpGet]
	public IActionResult Index()
	{
		logger.LogInformation("Get /Home");

		return View();
	}
}
