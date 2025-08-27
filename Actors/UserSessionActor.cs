using Akka.Event;

public interface IUserSessionMsg
{
	string SessionId { get; }
}

public class UserSessionActor : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	protected override void PreStart()
	{
		Context.SetReceiveTimeout(TimeSpan.FromSeconds(2));
	}

	public UserSessionActor(HttpClient httpClient)
	{
		Receive<ReceiveTimeout>(msg =>
		{
			Context.Stop(Self);
		});
	}
}