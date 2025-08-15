using Akka.Event;

public interface IUserSessionCommand
{
	string SessionId { get; }
}

public class UserSessionActor : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	protected override void PreStart()
	{
		Context.SetReceiveTimeout(2.Minutes());
	}

	public UserSessionActor(HttpClient httpClient)
	{
		Receive<ReceiveTimeout>(msg =>
		{
			Context.Stop(Self);
		});
	}
}