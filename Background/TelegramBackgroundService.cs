using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

public class TelegramBackgroundService(TelegramQueue telegramQueue, ILogger<TelegramBackgroundService> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Telegram background service started.");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var telegramMessage = await telegramQueue.DequeueAsync(stoppingToken);

				logger.LogInformation("Dequeued successfully.");

				// await Chat.SendAsync(telegramMessage.Message);
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				logger.LogCritical("{error}", ex.ToString());
			}
		}
	}
}
