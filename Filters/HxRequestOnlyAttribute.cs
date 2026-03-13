using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class HxRequestOnlyAttribute : ActionFilterAttribute
{
	public override void OnActionExecuting(ActionExecutingContext context)
	{
		if(!context.HttpContext.Request.Headers.ContainsKey("HX-Request"))
		{
			context.Result = new RedirectToActionResult("Index", "Home", new { errorMessage = "Missing HTMX header." });
		}
	}
}