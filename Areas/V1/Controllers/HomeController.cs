using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Akka.Hosting;

[Authorize(Roles = "User")]
[Area("V1")]
public class HomeController(
	ILogger<HomeController> logger,
	IRequiredActor<Brain> brain
) : HtmxController, IHasUser, IHasSessionId
{
	public ApplicationUser CurrentUser { get; set; } = null!;
	public string SessionId { get; set; } = string.Empty;

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		logger.Endpoint(Get, "/Home");

		return View();
	}
}