using Akka.Configuration;

static partial class StaticMethods
{
	public static void SetupAkka(this IServiceCollection services)
	{
		services.AddAkka(nameof(ActorSystem), x =>
		{
			var config = ConfigurationFactory.ParseString("""
				akka {
					loglevel = DEBUG
					loggers = ["Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog"]
				}
				""");

			x.AddHocon(config, HoconAddMode.Prepend);
			x.WithActors((system, register, resolver) =>
			{
				var brain = system.ActorOf(resolver.Props<Brain>(), nameof(Brain));
				register.Register<Brain>(brain);
			});
		});
	}
}