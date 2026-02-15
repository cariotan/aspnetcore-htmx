using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

public class CustomExceptionFilter(
	ILogger<CustomExceptionFilter> logger,
	IRequiredActor<Brain> brain
) : IExceptionFilter
{
	public void OnException(ExceptionContext context)
	{
		logger.LogError(context.Exception, "Unhandled exception occurred");

		brain.Tell(new NewUnhandledError(context.Exception.Message, context.Exception));

		// 1. Detect if it is an HTMX request
		bool isHtmx = context.HttpContext.Request.Headers.ContainsKey("HX-Request");

		const string ERROR_MESSAGE = "Something went wrong on our end. The issue has been logged and the administrator has been notified. We apologize for the inconvenience.";

		if (isHtmx)
		{
			// HTMX Logic
			context.HttpContext.Response.Headers.Append("hx-reswap", "none");

			Dictionary<string, object> payload = new()
			{
				["show_error_modal"] = new { error = ERROR_MESSAGE }
			};

			string triggerValue = JsonConvert.SerializeObject(payload);
			context.HttpContext.Response.Headers.Append("hx-trigger", triggerValue);

			context.Result = new ObjectResult(payload["show_error_modal"])
			{
				StatusCode = StatusCodes.Status500InternalServerError
			};
		}
		else
		{
			// Standard Browser Logic (Return a View)
			var result = new ViewResult
			{
				ViewName = "Error", // or your specific view name
			};

			context.Result = result;
		}

		context.ExceptionHandled = true;
	}
}