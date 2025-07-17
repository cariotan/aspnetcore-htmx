using Akka.Event;

public class Brain : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public Brain()
	{
		Receive<IUserSessionActorCommand>(msg =>
		{
			var name = nameof(UserSessionActor) + msg.SessionId;

			var actor = Context.Child(name);
			if (actor.IsNobody())
			{
				actor = Context.ActorOf(
					Props.Create(() => new UserSessionActor()),
					name
				);
			}

			actor.Forward(msg);
		});
	}
}