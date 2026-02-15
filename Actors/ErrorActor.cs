using Akka.Actor;
using Akka.Event;
using Akka.Hosting;
using Microsoft.AspNetCore.SignalR;

public class ErrorActor : ReceiveActor
{
	ILoggingAdapter logger = Context.GetLogger();
	IServiceScopeFactory serviceScopeFactory;

	public ErrorActor(
		IServiceScopeFactory serviceScopeFactory,
		IRequiredActor<Brain> brain,
		IHubContext<ErrorHub> errorHub
	)
	{
		this.serviceScopeFactory = serviceScopeFactory;

		Receive<Error_NewUnhandledError>(msg =>
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

		Receive<Error_ShowErrorModal>(msg =>
		{
			errorHub.Clients.Client(msg.connectionId).SendAsync("show_error_modal", msg.message);
		});
	}
}

public record Error_NewUnhandledError(string Message, Exception? Exception = null) : IErrorCommand;
public record Error_ShowErrorModal(string message, string connectionId) : IErrorCommand;

public interface IErrorCommand;