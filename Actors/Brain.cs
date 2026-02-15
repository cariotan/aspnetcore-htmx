using Akka.Event;
using Akka.Routing;
using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.SignalR;

public class Brain : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();
	IServiceScopeFactory serviceScopeFactory;

	public Brain(
		HttpClient httpClient,
		IServiceScopeFactory serviceScopeFactory,
		IRequiredActor<Brain> brain,
		IHubContext<ErrorHub> errorHub
	)
	{
		this.serviceScopeFactory = serviceScopeFactory;

		Receive<IErrorCommand>(msg =>
		{
			Context.GetOrCreateChild(
				Props.Create(() => new ErrorActor(serviceScopeFactory, brain, errorHub)),
				nameof(ErrorActor)
			).Forward(msg);
		});

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

		// Events();
	}

	private void Events()
	{
		// Context.System.EventStream.Subscribe(
		// 	Context.ActorOf(
		// 		Props.Create(() => new ErrorActor(serviceScopeFactory)),
		// 		nameof(ErrorActor)
		// 	),
		// 	typeof(Akka.Event.Error)
		// );

		// Context.System.EventStream.Subscribe(
		// 	Context.ActorOf(
		// 		Props.Create(() => new DeadLetterActor()),
		// 		nameof(DeadLetterActor)
		// 	),
		// 	typeof(UnhandledMessage)
		// );
	}
}