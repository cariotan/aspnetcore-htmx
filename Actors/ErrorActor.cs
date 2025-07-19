using Akka.Event;

public class ErrorActor : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public ErrorActor(IActorRef brain)
	{
		Receive<Akka.Event.Error>(msg =>
		{
			if (msg.Cause is not null)
			{
				brain.Tell(new SendDiscordException(msg.Cause));	
			}
		});
	}
}