using System.Reflection;
using System.Text;
using System.Text.Json;
using Akka.Event;

public class EmailActor : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public EmailActor(HttpClient httpClient)
	{
		ReceiveAsync<SendEmail>(async msg =>
		{
			var emailRequest = msg;

			if (!string.IsNullOrWhiteSpace(emailRequest?.From?.Email) &&
				!string.IsNullOrWhiteSpace(emailRequest?.To?.Email) &&
				!string.IsNullOrWhiteSpace(emailRequest?.Subject) &&
				!string.IsNullOrWhiteSpace(emailRequest?.Body))
			{
				int maxRetries = 3;
				int delayMilliseconds = 1000;

				for (int attempt = 1; attempt <= maxRetries; attempt++)
				{
					try
					{
						await SendEmailAsync(
							emailRequest.From.Email, emailRequest.From.Name,
							emailRequest.To.Email, emailRequest.To.Name,
							emailRequest.Subject, emailRequest.Body, httpClient);

						return;
					}
					catch (Exception ex)
					{
						if (attempt == maxRetries)
						{
							logger.Error($"SendEmail failed after {maxRetries} attempts: {ex}");
						}
						else
						{
							await Task.Delay(delayMilliseconds);
						}
					}
				}
			}
			else
			{
				logger.Info("Email not sent due to missing required fields (From, To, Subject, or Body)");
			}
		});
	}

	public static async Task SendEmailAsync(string fromEmail, string? fromName, string toEmail, string? toName, string subject, string body, HttpClient httpClient)
	{
		// Check if either email is null or empty, return without doing anything
		if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(toEmail))
		{
			return;
		}

		var apiKey = GetEnvironmentVariable("SENDGRID_API_KEY");
		if (string.IsNullOrEmpty(apiKey))
		{
			throw new InvalidOperationException("SendGrid API key is not set in the environment variables.");
		}

		// Create the 'to' object
		object toObject;
		if (string.IsNullOrEmpty(toName))
		{
			toObject = new { email = toEmail };
		}
		else
		{
			toObject = new { email = toEmail, name = toName };
		}

		// Create the 'from' object
		object fromObject;
		if (string.IsNullOrEmpty(fromName))
		{
			fromObject = new { email = fromEmail };
		}
		else
		{
			fromObject = new { email = fromEmail, name = fromName };
		}

		var requestBody = new
		{
			personalizations = new[]
			{
				new
				{
					to = new[] { toObject },
					subject = subject
				}
			},
			from = fromObject,
			content = new[]
			{
				new { type = "text/plain", value = body }
			}
		};

		var json = JsonSerializer.Serialize(requestBody);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		HttpRequestMessage request = new(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send")
		{
			Headers =
			{
				{ "Authorization", $"Bearer {apiKey}" }
			},
			Content = content
		};

		var response = await httpClient.SendAsync(request);

		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync();

			throw new HttpRequestException($"Failed to send email: {response.StatusCode}, {errorContent}");
		}
	}
}

public class SendEmail : IEmailCommand
{
	public EmailAddress? From { get; init; } = new("noreply@ripplenetworks.com.au", "Ripple Networks");
	public EmailAddress? To { get; init; }
	public string? Subject { get; init; }
	public string? Body { get; init; }
	public string SessionId { get; init; }

	public SendEmail(string sessionId)
	{
		SessionId = sessionId;
	}
}

public class SendEmailException : SendEmail
{
	public SendEmailException(Exception e, string sessionId)
		: base(sessionId)
	{
		To = new("ctan@trucell.com.au", "Cario Tan");

		Subject = $"{GetAssemblyName()} Exception: {e.Message}";

		Body = e.ToString();
	}
}

public class SendEmailNotification : SendEmail
{
	public SendEmailNotification(string title, string notification, string sessionId)
		: base(sessionId)
	{
		To = new("ctan@trucell.com.au", "Cario Tan");
		Subject = title;
		Body = notification;
	}
}

public interface IEmailCommand : IUserSessionCommand;

public record EmailAddress(string Email, string Name);