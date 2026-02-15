using Akka.Actor;
using Akka.Event;

// public class ErrorActor : ReceiveActor
// {
// 	readonly ILoggingAdapter logger = Context.GetLogger();

// 	public ErrorActor()
// 	{
// 		Receive<Akka.Event.Error>(msg =>
// 		{
// 			if (msg.Cause is not null)
// 			{
// 				Context.Parent.Tell(new SendDiscordException(new ActorException(msg.Cause)));
// 			}
// 			else
// 			{
// 				Context.Parent.Tell(new SendDiscordException(new ActorException(msg.ToString())));
// 			}
// 		});
// 	}
// }

// #warning Change ActorException to the name of the app.
// public class ActorException : Exception
// {
// 	public ActorException(Exception innerException)
// 		: base("An error occurred in Karisma Kiosk Actors", innerException)
// 	{
// 	}

// 	public ActorException(string message)
// 		: base(message)
// 	{
// 	}
// }