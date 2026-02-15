using Akka.Actor;
using Akka.Event;
using Akka.Hosting;

public class ErrorActor : ReceiveActor
{
	ILoggingAdapter logger = Context.GetLogger();
	IServiceScopeFactory serviceScopeFactory;

	public ErrorActor(
		IServiceScopeFactory serviceScopeFactory,
		IRequiredActor<Brain> brain
	)
	{
		this.serviceScopeFactory = serviceScopeFactory;

		Receive<NewUnhandledError>(msg =>
		{
			using var scope = serviceScopeFactory.CreateScope();
			ErrorContext errorContext;
			{
				errorContext = scope.ServiceProvider.GetRequiredService<ErrorContext>();
			}

			try
			{
				UnhandledError unhandledError = new()
				{
					Message = msg.Message,
					Exception = msg.Exception?.ToString(),
					DateCreated = DateTime.Now
				};

				errorContext.UnhandledErrors.Add(unhandledError);
				errorContext.SaveChanges();

				brain.Tell(new SendDiscordException(unhandledError.Message));
			}
			catch(Exception e)
			{
				logger.Error(e, "Error occurred while saving unhandled error");
			}
		});
	}
}

public record NewUnhandledError(string Message, Exception? Exception = null) : IErrorCommand;

public interface IErrorCommand;