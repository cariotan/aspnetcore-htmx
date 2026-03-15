using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

public class AreaClaimRedirectAttribute : ActionFilterAttribute
{
 public string Blacklist { get; set; } = string.Empty;

 public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
 {
  var user = context.HttpContext.User;
  var controller = context.RouteData.Values["controller"]?.ToString();

  if(!string.IsNullOrEmpty(Blacklist) && !string.IsNullOrEmpty(controller))
  {
   var ignoredControllers = Blacklist.Split(',').Select(c => c.Trim());
   if(ignoredControllers.Contains(controller, StringComparer.OrdinalIgnoreCase))
   {
    await next();
    return;
   }
  }

  if(user.Identity?.IsAuthenticated == true)
  {
   var userAreaClaim = user.FindFirst("area")?.Value;
   var currentRouteArea = context.RouteData.Values["area"]?.ToString();

   if(
    !string.IsNullOrEmpty(userAreaClaim) && 
    !string.Equals(userAreaClaim, currentRouteArea, StringComparison.OrdinalIgnoreCase))
   {
    var action = context.RouteData.Values["action"]?.ToString();

    context.Result = new RedirectToActionResult(action, controller, new { area = userAreaClaim });
    return;
   }
  }

  await next();
 }
}