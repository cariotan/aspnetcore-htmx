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
		Receive<IDiscordMessage>(msg =>
		{
			var name = nameof(DiscordActor) + msg.SessionId;
			var discordActor = Context.Child(name);
			if (discordActor.IsNobody())
			{
				discordActor = Context.ActorOf(
					Props.Create(() => new DiscordActor(httpClient)),
					name);
			}
			discordActor.Forward(msg);
		});

		Receive<IEmailMessage>(msg =>
		{
			Context.Parent.Forward(msg);
		});

		Receive<ReceiveTimeout>(msg =>
		{
			Context.Stop(Self);
		});
	}
}