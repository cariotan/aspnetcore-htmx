using Akka.Event;
using Akka.Routing;

public class Brain : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public Brain(HttpClient httpClient)
	{
		Context.System.EventStream.Subscribe(
			Context.ActorOf(
				Props.Create(() => new ErrorActor(Self)),
				nameof(ErrorActor)
			),
			typeof(Akka.Event.Error)
		);

		Context.System.EventStream.Subscribe(
			Context.ActorOf(
				Props.Create(() => new DeadLetterActor()),
				nameof(DeadLetterActor)
			),
			typeof(AllDeadLetters)
		);

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