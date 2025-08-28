using Akka.Actor;
using Akka.Hosting;

static partial class StaticMethods
{
	public static void Tell<T>(this IRequiredActor<T> actorRef, object message)
	{
		actorRef.ActorRef.Tell(message);
	}

	public static async Task<T> Ask<T>(this IRequiredActor<Brain> actorRef, object message, TimeSpan? timeout = null)
	{
#if DEBUG
		timeout = TimeSpan.FromSeconds(30);
#endif
		return await actorRef.ActorRef.Ask<T>(message, timeout);
	}

	public static async Task Ask(this IRequiredActor<Brain> actorRef, object message, TimeSpan? timeout = null)
	{
#if DEBUG
		timeout = TimeSpan.FromSeconds(30);
#endif
		await actorRef.ActorRef.Ask(message, timeout);
	}
}