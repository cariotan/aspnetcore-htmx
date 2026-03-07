using Akka.Actor;
using Akka.Event;
using Akka.Hosting;
using Microsoft.AspNetCore.SignalR;
using ErrorDatabase;
using Microsoft.EntityFrameworkCore;

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

		Receive<Error_NewError>(msg =>
		{
			using var scope = serviceScopeFactory.CreateScope();
			ErrorContext errorContext;
			{
				errorContext = scope.ServiceProvider.GetRequiredService<ErrorContext>();

				if(errorContext.Database.GetPendingMigrations().Any())
				{
					errorContext.Database.Migrate();
				}
			}

			try
			{
				ErrorDatabase.Error error = new()
				{
					Message = msg.Message,
					Exception = msg.Exception?.ToString(),
					DateCreated = DateTime.Now
				};

				if(msg.Id.HasValue)
				{
					error.Id = msg.Id.Value;
				}

				errorContext.UnhandledErrors.Add(error);
				errorContext.SaveChanges();

				brain.Tell(new SendDiscordException(error.Message));
			}
			catch(Exception e)
			{
				logger.Error(e, "Error occurred while saving unhandled error");
			}
		});
	}
}

public record Error_NewError(string Message, Exception? Exception = null, Guid? Id = null) : IErrorCommand;

public interface IErrorCommand;