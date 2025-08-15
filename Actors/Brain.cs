using Akka.Event;
using Akka.Routing;

public class Brain : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	public Brain(HttpClient httpClient)
	{
		Receive<IDiscordMsg>(msg =>
		{
			Context.GetOrCreateChild(
				Props.Create(() => new DiscordActor(httpClient)),
				nameof(DiscordActor)
			).Forward(msg);
		});

		Receive<IEmailMsg>(msg =>
		{
			Context.GetOrCreateChild(
				Props.Create(() => new EmailActor(httpClient)),
				nameof(EmailActor)
			).Forward(msg);
		});

		Receive<IUserSessionMsg>(msg =>
		{
			Context.GetOrCreateChild(
				Props.Create(() => new UserSessionActor(httpClient)),
				nameof(UserSessionActor) + msg.SessionId
			).Forward(msg);
		});

		Events();
	}

	private void Events()
	{
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