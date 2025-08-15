static partial class StaticMethods
{
	public static IActorRef GetOrCreateChild(this IActorContext context, Props props, string name)
	{
		return context.Child(name)
			.GetOrElse(() => context.ActorOf(props, name));
	}
}