using Polly.Registry;
using System.Text.Json;

public class DiscordBackgroundService(DiscordQueue DiscordQueue, ILogger<DiscordBackgroundService> logger, ResiliencePipelineProvider<string> resiliencePipelineProvider) : BackgroundService
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
				var DiscordMessage = await DiscordQueue.DequeueAsync(stoppingToken);

				logger.LogInformation("Discord dequeued successfully.");

				var url = await GetDiscordChannelWebhook(DiscordMessage.Key);

				ResilienceContext context = ResilienceContextPool.Shared.Get(stoppingToken);

				context.Properties.Set(ResilienceKeys.Discord, DiscordMessage.Message);

				var pipeline = resiliencePipelineProvider.GetPipeline<Task>("Discord");

				const int discordCharacterlimit = 2000;

				var chunks = SplitMessageIntoChunks(DiscordMessage.Message, discordCharacterlimit);

				for (int i = 0; i < chunks.Count; i++)
				{
					string chunk = chunks[i];
					if (i == chunks.Count - 1)
					{
						chunk += "\n---------------------------";
					}

					var content = new { content = chunk };

					await pipeline.ExecuteAsync<Task>(async context =>
					{
						var response = await httpClient.PostAsJsonAsync(url, content);

						var data = await response.Content.ReadAsStringAsync();

						response.EnsureSuccessStatusCode();

						if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining))
						{
							var remainingRequests = int.Parse(remaining.First());

							if (remainingRequests == 0)
							{
								if (response.Headers.TryGetValues("X-RateLimit-Reset-After", out var resetAfter))
								{
									var seconds = double.Parse(resetAfter.First());

									await Task.Delay((int)(seconds * 1000));
								}
							}
						}

						return Task.CompletedTask;
					},
					context);
				}

				ResilienceContextPool.Shared.Return(context);
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

	private async Task<string> GetDiscordChannelWebhook(string key)
	{
		var appSettings = await File.ReadAllTextAsync("appsettings.json");

		using JsonDocument document = JsonDocument.Parse(appSettings);

		JsonElement root = document.RootElement;

		if (root.TryGetProperty("Discord", out JsonElement discordSection))
		{
			if (discordSection.TryGetProperty(key, out JsonElement value))
			{
				return value.GetString() ?? "";
			}
			else
			{
				logger.LogError("Key '{key}' not found in the Discord section.", key);
				throw new KeyNotFoundException($"Key '{key}' not found in the Discord section.");
			}
		}
		else
		{
			logger.LogError("Discord section not found in appsettings.json");
			throw new InvalidOperationException("Discord section not found in appsettings.json.");
		}
	}

	private List<string> SplitMessageIntoChunks(string message, int maxChunkSize)
	{
		var result = new List<string>();
		for (int i = 0; i < message.Length; i += maxChunkSize)
		{
			int length = Math.Min(maxChunkSize, message.Length - i);
			result.Add(message.Substring(i, length));
		}
		return result;
	}
}