using Microsoft.AspNetCore.Mvc.Filters;

public class CustomExceptionFilter(ILogger<CustomExceptionFilter> logger, IRequiredActor<Brain> brain) : IExceptionFilter
{
	public void OnException(ExceptionContext context)
	{
		logger.LogError(context.Exception, "Unhandled exception occurred");

		brain.Tell(new SendEmailException(context.Exception));

		context.HttpContext.Response.Headers.Append("hx-reswap", "none");

		context.ExceptionHandled = true;
	}
}