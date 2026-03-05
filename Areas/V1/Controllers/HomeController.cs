using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Akka.Hosting;

[Area("V1")]
[Authorize(Roles = "User")]
public class HomeController(
	ILogger<HomeController> logger
) : HtmxController, IHasUser, IHasSessionId
{
	public ApplicationUser CurrentUser { get; set; } = null!;
	public string SessionId { get; set; } = string.Empty;

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		logger.Endpoint(Get, "/Home");

		HxPushUrl(Url.Action("Index", "Home")!);
		return View();
	}
}