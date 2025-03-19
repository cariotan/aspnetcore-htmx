public class EmailBackgroundService(EmailQueue emailQueue, ILogger<EmailBackgroundService> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Email background service started.");

		while(!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var emailRequest = await emailQueue.DequeueAsync(stoppingToken);

				logger.LogInformation("Dequeued successfully.");

				// await SendGrid.Standard.SendEmail.SendEmailAsync(emailRequest.From, emailRequest.To, emailRequest.Subject, emailRequest.Body);
			}
			catch(OperationCanceledException)
			{
				break;
			}
			catch(Exception ex)
			{
				logger.LogCritical("{error}", ex.ToString());
			}
		}
	}
}