using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[Area("V1")]
public class HomeController(ILogger<HomeController> logger) : HtmxController, IHasUser
{
	public ApplicationUser CurrentUser { get; set; } = null!;

	[HttpGet]
	public IActionResult Index()
	{
		logger.Endpoint(Get, "/Home");

		return View();
	}
}