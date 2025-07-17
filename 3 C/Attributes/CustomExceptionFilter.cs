using Microsoft.AspNetCore.Mvc.Filters;

public class CustomExceptionFilter(ILogger<CustomExceptionFilter> logger, IMessageBus mb) : IAsyncExceptionFilter
{
	public async Task OnExceptionAsync(ExceptionContext context)
	{
		logger.LogError(context.Exception, "Unhandled Halo Dashboard exception occurred");

		await mb.PublishAsync(SendEmail.Exception(context.Exception));

		context.HttpContext.Response.Headers.Append("hx-reswap", "none");

		context.ExceptionHandled = true;
	}
}