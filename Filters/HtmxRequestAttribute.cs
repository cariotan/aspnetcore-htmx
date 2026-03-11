using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class HtmxRequestAttribute : ActionFilterAttribute
{
	public override void OnActionExecuting(ActionExecutingContext context)
	{
		if(!context.HttpContext.Request.Headers.ContainsKey("HX-Request"))
		{
			context.Result = new RedirectToActionResult("Index", "Home", new { errorMessage = "Invalid request." });
		}
	}
}