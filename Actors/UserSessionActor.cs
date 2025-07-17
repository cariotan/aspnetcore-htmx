using Akka.Event;

public interface IUserSessionActorCommand
{
	string SessionId { get; set; }
}

public class UserSessionActor : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	protected override void PreStart()
	{
		Context.SetReceiveTimeout(2.Minutes());
	}

	public UserSessionActor()
	{
		Receive<ReceiveTimeout>(msg =>
		{
			Context.Stop(Self);
		});
	}
}