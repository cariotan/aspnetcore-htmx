using Polly.Registry;
using System.Text;
using System.Text.Json;

public class SendEmailHandler(ILogger<SendEmailHandler> logger, ResiliencePipelineProvider<string> r, HttpClient httpClient)
{
	public async Task Handle(SendEmail msg)
	{
		var emailRequest = msg;

		var pipeline = r.GetPipeline("Email");

		await pipeline.ExecuteAsync(async token =>
		{
			if (emailRequest?.From?.Email != null &&
				emailRequest?.To?.Email != null &&
				emailRequest?.Subject != null &&
				emailRequest?.Body != null)
			{
				await SendEmailAsync(emailRequest.From.Email, emailRequest.From.Name,
					emailRequest.To.Email, emailRequest.To.Name,
					emailRequest.Subject, emailRequest.Body, httpClient);
			}
			else
			{
				logger.LogInformation("Email not sent due to missing required fields (From, To, Subject, or Body)");
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

public class SendEmail
{
	public EmailAddress? From { get; init; } = new("noreply@ripplenetworks.com.au", "Ripple Networks");
	public EmailAddress? To { get; init; }
	public string? Subject { get; init; }
	public string? Body { get; init; }

	public static SendEmail Exception(Exception e)
	{
		return new()
		{
			To = new("ctan@trucell.com.au", "Cario Tan"),
			Subject = "An exception has occured for Karisma Kiosk",
			Body = e.ToString(),
		};
	}

	public static SendEmail Notification(string title, string notification)
	{
		return new()
		{
			To = new("ctan@trucell.com.au", "Cario Tan"),
			Subject = title,
			Body = notification,
		};
	}
}

public record EmailAddress(string Email, string Name);