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
			Context.GetOrCreateChild(
				Props.Create(() => new UserSessionActor(httpClient)),
				nameof(UserSessionActor) + msg.SessionId
			).Forward(msg);
		});
	}
}