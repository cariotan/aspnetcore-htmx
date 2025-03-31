using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Polly;
using Polly.Registry;

public class DiscordBackgroundService(DiscordQueue DiscordQueue, ILogger<DiscordBackgroundService> logger, EmailQueue emailQueue, ResiliencePipelineProvider<string> resiliencePipelineProvider) : BackgroundService
{
	private static HttpClient httpClient;

	static DiscordBackgroundService()
	{
		var handler = new SocketsHttpHandler
		{
			PooledConnectionLifetime = TimeSpan.FromDays(1)
		};

		httpClient = new HttpClient(handler);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Discord background service started.");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var discordMessage = await DiscordQueue.DequeueAsync(stoppingToken);

				logger.LogInformation("Dequeued successfully.");

				var webhook = GetDiscordWebhook(discordMessage.Key);

				ResilienceContext context = ResilienceContextPool.Shared.Get(stoppingToken);

				// Attach custom data to the context
				context.Properties.Set(ResilienceKeys.Discord, discordMessage.Message);

				var pipeline = resiliencePipelineProvider.GetPipeline("Discord");

				await pipeline.ExecuteAsync(async context =>
				{
					var response = await httpClient.PostAsJsonAsync(webhook, discordMessage.Message, context.CancellationToken);

					if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining))
					{
						var remainingRequests = int.Parse(remaining.First());

						if (remainingRequests == 0)
						{
							if (response.Headers.TryGetValues("X-RateLimit-Reset-After", out var resetAfter))
							{
								var seconds = double.Parse(resetAfter.First());
								await Task.Delay((int)(seconds * 1000), context.CancellationToken);
							}
						}
					}
				},
				context);
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

	private string GetDiscordWebhook(string key)
	{
		var config = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.Build();

		var webhook = config[$"Discord:{key}"];

		if (!string.IsNullOrWhiteSpace(webhook))
		{
			return webhook;
		}
		else
		{
			throw new Exception($"Cannot find {key} from appsettings.json");
		}
	}
}
