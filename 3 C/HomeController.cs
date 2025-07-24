using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class HomeController(ILogger<HomeController> logger) : HtmxController
{
	[HttpGet]
	public IActionResult Index()
	{
		logger.Endpoint(Get, "/Home");

		return View();
	}
}