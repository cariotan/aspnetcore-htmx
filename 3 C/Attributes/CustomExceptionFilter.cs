using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class CustomExceptionFilter(ILogger<CustomExceptionFilter> logger, IRequiredActor<Brain> brain) : IExceptionFilter
{
	public void OnException(ExceptionContext context)
	{
		logger.LogError(context.Exception, "Unhandled exception occurred");

		brain.Tell(new SendDiscordException(context.Exception));

		context.HttpContext.Response.Headers.Append("hx-reswap", "none");

		context.ExceptionHandled = true;

		Dictionary<string, object> payload = new()
		{
			["show_error_modal"] = "Something went wrong on our end. The issue has been logged and the administrator has been notified. We apologize for the inconvenience."
		};

		string triggerValue = JsonSerializer.Serialize(payload);

		context.HttpContext.Response.Headers.Append("hx-trigger", triggerValue);

		context.Result = new ObjectResult(payload)
		{
			StatusCode = StatusCodes.Status500InternalServerError
		};
	}
}