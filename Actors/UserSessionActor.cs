using Akka.Event;

public interface IUserSessionCommand
{
	string SessionId { get; }
}

public class UserSessionActor : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	protected override void PreStart()
	{
		Context.SetReceiveTimeout(2.Minutes());
	}

	public UserSessionActor(HttpClient httpClient)
	{
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

		Receive<ReceiveTimeout>(msg =>
		{
			Context.Stop(Self);
		});
	}
}