using Akka.Actor;
using Akka.Event;

public class DeadLetterActor : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public DeadLetterActor()
	{
		Receive<AllDeadLetters>(msg =>
		{
			Context.Parent.Tell(new SendDiscordException(new ActorException(new Exception($"Dead letter received: {msg.Message}"))));
		});
	}
}