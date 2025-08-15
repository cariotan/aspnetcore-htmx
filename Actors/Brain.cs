using Akka.Event;
using Akka.Routing;

public class Brain : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public Brain(HttpClient httpClient)
	{
		Receive<IUserSessionCommand>(msg =>
		{
			Context.GetOrCreateChild(
				Props.Create(() => new UserSessionActor(httpClient)),
				nameof(UserSessionActor) + msg.SessionId
			).Forward(msg);
		});

		Receive<IDiscordCommand>(msg =>
		{
			Context.GetOrCreateChild(
				Props.Create(() => new DiscordActor(httpClient)),
				nameof(DiscordActor) + msg.SessionId
			).Forward(msg);
		});

		Receive<IEmailCommand>(msg =>
		{
			Context.GetOrCreateChild(
				Props.Create(() => new EmailActor(httpClient)),
				nameof(EmailActor) + msg.SessionId
			).Forward(msg);
		});

		Context.System.EventStream.Subscribe(
			Context.ActorOf(
				Props.Create(() => new ErrorActor()),
				nameof(ErrorActor)
			),
			typeof(Akka.Event.Error)
		);

		Context.System.EventStream.Subscribe(
			Context.ActorOf(
				Props.Create(() => new DeadLetterActor()),
				nameof(DeadLetterActor)
			),
			typeof(UnhandledMessage)
		);
	}
}