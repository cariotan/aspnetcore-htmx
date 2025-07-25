static partial class StaticMethods
{
	public static void Tell<T>(this IRequiredActor<T> actorRef, object message)
	{
		actorRef.ActorRef.Tell(message);
	}

	public static async Task<T> Ask<T>(this IRequiredActor<Brain> actorRef, object message, TimeSpan? timeout)
	{
		return await actorRef.ActorRef.Ask<T>(message, timeout);
	}

	public static async Task Ask(this IRequiredActor<Brain> actorRef, object message, TimeSpan? timeout)
	{
		await actorRef.ActorRef.Ask(message, timeout);
	}
}