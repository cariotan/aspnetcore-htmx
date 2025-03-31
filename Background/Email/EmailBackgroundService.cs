using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Polly.Registry;

public class EmailBackgroundService(EmailQueue emailQueue, ILogger<EmailBackgroundService> logger, ResiliencePipelineProvider<string> resiliencePipelineProvider) : BackgroundService
{
	private static HttpClient httpClient;

	static EmailBackgroundService()
	{
		var handler = new SocketsHttpHandler
		{
			PooledConnectionLifetime = TimeSpan.FromDays(1)
		};

		httpClient = new HttpClient(handler);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Email background service started.");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var emailRequest = await emailQueue.DequeueAsync(stoppingToken);

				logger.LogInformation("Dequeued successfully.");

				var pipeline = resiliencePipelineProvider.GetPipeline("Email");

				await pipeline.ExecuteAsync(async token =>
				{
					if (emailRequest?.From?.Email != null &&
						emailRequest?.To?.Email != null &&
						emailRequest?.Subject != null &&
						emailRequest?.Body != null)
					{
						await SendEmailAsync(emailRequest.From.Email, emailRequest.From.Name,
							emailRequest.To.Email, emailRequest.To.Name,
							emailRequest.Subject, emailRequest.Body);
						logger.LogInformation("Email sent successfully");
					}
					else
					{
						logger.LogInformation("Email not sent due to missing required fields (From, To, Subject, or Body)");
					}
				}, stoppingToken);
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

	public static async Task SendEmailAsync(string fromEmail, string? fromName, string toEmail, string? toName, string subject, string body)
	{
		// Check if either email is null or empty, return without doing anything
		if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(toEmail))
		{
			return;
		}

		var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
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

		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
		var response = await httpClient.PostAsync("https://api.sendgrid.com/v3/mail/send", content);

		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync();
			throw new HttpRequestException($"Failed to send email: {response.StatusCode}, {errorContent}");
		}
	}
}