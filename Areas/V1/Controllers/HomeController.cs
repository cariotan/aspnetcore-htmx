using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Akka.Hosting;

[Authorize(Roles = "User")]
[Area("V1")]
public class HomeController(
	ILogger<HomeController> logger,
	IRequiredActor<Brain> brain
) : HtmxController, IHasUser
{
	public ApplicationUser CurrentUser { get; set; } = null!;

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		logger.Endpoint(Get, "/Home");

		return View();
	}
}