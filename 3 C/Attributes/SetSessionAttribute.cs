using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class SetSessionAttribute : ActionFilterAttribute
{
	public override void OnActionExecuting(ActionExecutingContext context)
	{
		if (context.Controller is Controller controller && context.Controller is IHasSessionId controllerWithSessionId)
		{

			if (controller.Request.Cookies.TryGetValue("SessionId", out string? sessionId))
			{
				// Cookie exists
				controllerWithSessionId.SessionId = sessionId;
			}
			else
			{
				sessionId = Guid.NewGuid().ToString();

				var options = new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
				};

				controller.Response.Cookies.Append("SessionId", sessionId, options);

				controllerWithSessionId.SessionId = sessionId;
			}
		}

		base.OnActionExecuting(context);
	}
}
