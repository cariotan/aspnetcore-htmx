using Akka.Event;

public class DiscordActor : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public DiscordActor(HttpClient httpClient)
	{
		ReceiveAsync<SendDiscord>(async msg =>
		{
			try
			{
#if DEBUG
				var url = GetEnvironmentVariable("Test");
#else
				var url = GetEnvironmentVariable(msg.Key);
#endif

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
				}
			}
			catch (Exception e)
			{
				Context.Parent.Tell(new SendEmailException(e));

#warning Set email subject
				SendEmail sendEmail = new()
				{
					To = new EmailAddress("ctan@trucell.com.au", "Cario Tan"),
					Subject = "Discord Message",
					Body = msg.Message,
				};

				Context.Parent.Tell(sendEmail);
			}
		});
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

public record SendDiscord(string Key, string Message) : IDiscordCommand;

public record SendDiscordException : SendDiscord
{
	public SendDiscordException(Exception e)
		: base("Exception", e.ToString())
	{

	}
}

public interface IDiscordCommand : IUserSessionCommand;