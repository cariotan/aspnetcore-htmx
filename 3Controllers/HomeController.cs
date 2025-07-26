using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class HomeController(ILogger<HomeController> logger) : HtmxController, IHasUser
{
	public ApplicationUser CurrentUser { get; set; } = null!;

	[HttpGet]
	public IActionResult Index()
	{
		logger.Endpoint(Get, "/Home");

		throw new Exception($"This is my ");

		return View();
	}
}