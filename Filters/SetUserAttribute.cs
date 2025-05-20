using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

public class SetUserAttribute : ActionFilterAttribute
{
	public override async void OnActionExecuting(ActionExecutingContext context)
	{
		if (context.Controller is IHasUser controller)
		{
			var userManager = context.HttpContext.RequestServices.GetService<UserManager<ApplicationUser>>();

			var user = await userManager!.GetUserAsync(context.HttpContext.User);

			if (user is { })
			{
				controller.CurrentUser = user;
			}
			else
			{
				controller.CurrentUser = new();
			}
		}
	}
}