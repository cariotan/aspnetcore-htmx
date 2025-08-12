using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;

public class SetUserConvention : IApplicationModelConvention
{
	public void Apply(ApplicationModel application)
	{
		foreach (var controller in application.Controllers)
		{
			if (typeof(IHasUser).IsAssignableFrom(controller.ControllerType))
			{
				controller.Filters.Add(new SetUserAttribute());
			}
		}
	}
}

public class SetUserAttribute : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		if (context.Controller is IHasUser controller)
		{
			var userManager = context.HttpContext.RequestServices.GetService<UserManager<ApplicationUser>>();
			var user = await userManager!.GetUserAsync(context.HttpContext.User);

			if (user is { })
			{
				controller.CurrentUser = user;
				await next();
			}
			else
			{
				context.Result = new UnauthorizedResult();
				return;
			}
		}
		else
		{
			await next();
		}
	}
}
