using Akka.Event;
using Akka.Routing;

public class Brain : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public Brain(HttpClient httpClient)
	{
		var errorActor = Context.ActorOf(Props.Create(() => new ErrorActor(Self)).WithRouter(new SmallestMailboxPool(10)));
		Context.System.EventStream.Subscribe(errorActor, typeof(Akka.Event.Error));

		var emailActor = Context.ActorOf(Props.Create(() => new EmailActor(httpClient)).WithRouter(new SmallestMailboxPool(10)));
		var discordActor = Context.ActorOf(Props.Create(() => new DiscordActor(httpClient)).WithRouter(new SmallestMailboxPool(10)));

		Receive<IUserSessionCommand>(msg =>
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

		Receive<IEmailCommand>(msg =>
		{
			emailActor.Forward(msg);
		});

		Receive<IDiscordCommand>(msg =>
		{
			discordActor.Forward(msg);
		});
	}
}