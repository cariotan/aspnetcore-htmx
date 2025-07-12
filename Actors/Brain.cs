using Akka.Event;

public class Brain : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public Brain()
	{
		Receive<Start>(msg =>
		{
			
		});
	}

	public record Start();
}