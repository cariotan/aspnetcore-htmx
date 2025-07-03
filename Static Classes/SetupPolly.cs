static partial class StaticMethods
{
	public static IServiceCollection SetupPolly(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddResiliencePipeline("Email", builder =>
		{
			builder.AddRetry(new RetryStrategyOptions
			{
				MaxRetryAttempts = 3,
				Delay = TimeSpan.FromSeconds(2),
				BackoffType = DelayBackoffType.Constant
			});
		});

		serviceCollection.AddResiliencePipeline<string, Task>("Discord", (builder, context) =>
		{
			builder
				.AddChaosFault(0, () => new Exception("Chaos fault"))
				.AddFallback(new FallbackStrategyOptions<Task>
				{
					FallbackAction = async args =>
					{
						if (args.Context.Properties.TryGetValue(ResilienceKeys.Discord, out var discordMessage))
						{
							var mb = context.ServiceProvider.GetRequiredService<IMessageBus>();
							await mb.PublishAsync(SendEmail.Notification("Discord failed. Falling back to email", discordMessage));
						}

						return Outcome.FromResult(Task.CompletedTask);
					}
				})
				.AddRetry(new RetryStrategyOptions<Task>
				{
					ShouldHandle = new PredicateBuilder<Task>().Handle<HttpRequestException>(),
					MaxRetryAttempts = 3,
					Delay = TimeSpan.FromSeconds(2),
					BackoffType = DelayBackoffType.Constant
				});
		});

		return serviceCollection;
	}
}