using Akka.Configuration;

static partial class StaticMethods
{
	public static void SetupAkka(this IServiceCollection services)
	{
		services.AddAkka(nameof(ActorSystem), x =>
		{
#if !DEBUG
			x.AddHocon(ConfigurationFactory.ParseString("""
				akka {
					loglevel = DEBUG
					loggers = ["Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog"]
					actor {
						ask-timeout = 5s
					}
				}
				"""), HoconAddMode.Prepend);
#endif

#if DEBUG
			x.AddHocon(ConfigurationFactory.ParseString("""
				akka {
					actor {
						ask-timeout = 5s
					}
				}
				"""), HoconAddMode.Prepend);
#endif

			x.WithActors((system, register, resolver) =>
			{
				var brain = system.ActorOf(resolver.Props<Brain>(), nameof(Brain));
				register.Register<Brain>(brain);
			});
		});
	}
}