using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace V2;

[Authorize]
[Area("V2")]
public class HomeController : Controller
{
	[HttpGet]
 public IActionResult Index()
	{
		return View();
	}
}