using System.Text.Json;
using Polly.Registry;

public class SendDiscordHandler(
	ILogger<SendDiscordHandler> logger,
	ResiliencePipelineProvider<string> resiliencePipelineProvider,
	HttpClient httpClient)
{
	public async Task HandleAsync(SendDiscord msg, CancellationToken stoppingToken)
	{
		logger.LogInformation("Discord dequeued successfully.");

		var url = GetEnvironmentVariable(msg.Key);

		ResilienceContext context = ResilienceContextPool.Shared.Get(stoppingToken);

		context.Properties.Set(ResilienceKeys.Discord, msg.Message);

		var pipeline = resiliencePipelineProvider.GetPipeline<Task>("Discord");

		const int discordCharacterlimit = 2000;

		var chunks = SplitMessageIntoChunks(msg.Message, discordCharacterlimit);

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

// The key is set in appsettings.cs. It corresponds to a discord webhook url.
public record SendDiscord(string Key, string Message)
{
	public static SendDiscord Exception(Exception e)
	{
		return new SendDiscord("Exception", e.ToString());
	}
}