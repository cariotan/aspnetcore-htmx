using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class HomeController : HtmxController
{
	public HomeController()
	{
	}

	[HttpGet]
	// /Home
	public IActionResult Index()
	{
		return View();
	}
}
