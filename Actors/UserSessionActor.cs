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
		Context.SetReceiveTimeout(TimeSpan.FromMinutes(2));
	}

	public UserSessionActor()
	{
		Receive<ReceiveTimeout>(msg =>
		{
			Context.Stop(Self);
		});
	}
}