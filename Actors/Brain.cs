using Akka.Event;
using Akka.Routing;

public class Brain : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public Brain(HttpClient httpClient)
	{
		var errorActor = Context.ActorOf(
			Props.Create(() => new ErrorActor(Self)),
			nameof(ErrorActor)
		);

		Context.System.EventStream.Subscribe(errorActor, typeof(Akka.Event.Error));

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