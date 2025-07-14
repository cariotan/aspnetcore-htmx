static partial class StaticMethods
{
	public static void Tell<T>(this IRequiredActor<T> actorRef, object message)
	{
		actorRef.ActorRef.Tell(message);
	}

	public static async Task<T> Ask<T>(this IRequiredActor<Brain> actorRef, object message, TimeSpan? timeout = null)
	{
		return await actorRef.ActorRef.Ask<T>(message, timeout ?? 10.Seconds());
	}
}