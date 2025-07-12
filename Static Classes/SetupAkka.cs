static partial class StaticMethods
{
	public static void SetupAkka(this IServiceCollection services)
	{
		services.AddAkka(nameof(ActorSystem), x =>
		{
			x.WithActors((system, register, resolver) =>
			{
				var brain = system.ActorOf(resolver.Props<Brain>(), nameof(Brain));
				register.Register<Brain>(brain);
			});
		});
	}
}