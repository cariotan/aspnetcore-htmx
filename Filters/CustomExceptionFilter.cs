using System.Text.Json;
using Akka.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class CustomExceptionFilter(ILogger<CustomExceptionFilter> logger, IRequiredActor<Brain> brain) : IExceptionFilter
{
	public void OnException(ExceptionContext context)
	{
		logger.LogError(context.Exception, "Unhandled exception occurred");

		brain.Tell(new SendDiscordException(new ActorException(context.Exception)));

		context.HttpContext.Response.Headers.Append("hx-reswap", "none");

		context.ExceptionHandled = true;

		Dictionary<string, object> payload = new()
		{
			["show_error_modal"] = new
			{
				error = "Something went wrong on our end. The issue has been logged and the administrator has been notified. We apologize for the inconvenience."
			}
		};

		string triggerValue = JsonSerializer.Serialize(payload);

		context.HttpContext.Response.Headers.Append("hx-trigger", triggerValue);

		context.Result = new ObjectResult(payload.First().Value)
		{
			StatusCode = StatusCodes.Status500InternalServerError
		};
	}
}