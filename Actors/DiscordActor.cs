using Akka.Actor;
using Akka.Event;

public class DiscordActor : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public DiscordActor(HttpClient httpClient)
	{
		string key = "";
#if DEBUG
		key = "Test";
#else
		key = msg.Key;
#endif

		var url = GetEnvironmentVariable(key);

		ReceiveAsync<Discord_Send>(async msg =>
		{
			if(string.IsNullOrWhiteSpace(url))
			{
				logger.Error($"Discord {key} is not set. Discord Message: " + msg.Message);
				return;
			}

			try
			{
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

					var response = await httpClient.PostAsJsonAsync(url, content);

					var data = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

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
				}
			}
			catch (Exception e)
			{
				logger.Error(e.Message);
				Context.Parent.Tell(new SendEmailException(e));

				Context.Parent.Tell(new SendEmail()
				{
					To = new EmailAddress("ctan@trucell.com.au", "Cario Tan"),
					Subject = "Discord Message",
					Body = msg.Message,
				});
			}
		});
	}

	private static List<string> SplitMessageIntoChunks(string message, int maxChunkSize)
	{
		List<string> result = [];

		for (int i = 0; i < message.Length; i += maxChunkSize)
		{
			int length = Math.Min(maxChunkSize, message.Length - i);
			result.Add(message.Substring(i, length));
		}

		return result;
	}
}

public record Discord_Send(string Key, string Message) : IDiscordMsg;

public record SendDiscordException : Discord_Send
{
	public SendDiscordException(Exception e)
		: base("Exception", e.ToString())
	{

	}

	public SendDiscordException(string message)
		: base("Exception", message)
	{

	}
}

public interface IDiscordMsg;