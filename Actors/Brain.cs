using Akka.Event;
using Akka.Routing;

public class Brain : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public Brain(HttpClient httpClient)
	{
		var emailActor = Context.ActorOf(Props.Create(() => new EmailActor(httpClient)).WithRouter(new SmallestMailboxPool(10)));

		Receive<IUserSessionActorCommand>(msg =>
		{
			var name = nameof(UserSessionActor) + msg.SessionId;

			var actor = Context.Child(name);
			if (actor.IsNobody())
			{
				actor = Context.ActorOf(
					Props.Create(() => new UserSessionActor(httpClient)),
					name
				);
			}

			actor.Forward(msg);
		});

		Receive<IEmailMessage>(msg =>
		{
			emailActor.Forward(msg);
		});
	}
}