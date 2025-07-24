using Akka.Event;
using Akka.Routing;

public class Brain : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	protected override void PreStart()
	{
		base.PreStart();

		var errorActor = Context.ActorOf(
			Props.Create(() => new ErrorActor(Self)).WithRouter(new SmallestMailboxPool(10)),
			nameof(ErrorActor)
		);

		Context.System.EventStream.Subscribe(errorActor, typeof(Akka.Event.Error));
	}

	public Brain(HttpClient httpClient)
	{
		Receive<IUserSessionCommand>(msg =>
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
	}
}