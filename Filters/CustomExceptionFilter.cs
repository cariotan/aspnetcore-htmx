using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class CustomExceptionFilter(ILogger<CustomExceptionFilter> logger, DiscordQueue discordQueue) : IAsyncExceptionFilter
{
	public async Task OnExceptionAsync(ExceptionContext context)
	{
		logger.LogError(context.Exception, "Unhandled Halo Dashboard exception occurred");

		DiscordUnhandledException telegramErrorNotification = new(context.Exception);
		await discordQueue.EnqueueAsync(telegramErrorNotification);

		context.HttpContext.Response.Headers.Append("hx-reswap", "none");

		context.ExceptionHandled = true;
	}
}