public class TestWorker(IServiceScopeFactory scopeFactory) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = scopeFactory.CreateScope();
		var mb = scope.ServiceProvider.GetRequiredService<IMessageBus>();

		await mb.PublishAsync(SendDiscord.Exception(new Exception("Test discord")));
		await mb.PublishAsync(SendEmail.Exception(new Exception("Test email")));
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}