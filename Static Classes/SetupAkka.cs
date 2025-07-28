using Akka.Configuration;

static partial class StaticMethods
{
	public static void SetupAkka(this IServiceCollection services)
	{
		services.AddAkka(nameof(ActorSystem), x =>
		{
			string log = "";

			#if !DEBUG
			log = """
				loglevel = DEBUG
				loggers = ["Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog"]
				""";
			#endif

			x.AddHocon(ConfigurationFactory.ParseString($$"""
				{{log}}
				akka {
					actor {
						ask-timeout = 5s
					}
				}
				"""), HoconAddMode.Prepend);

			x.WithActors((system, register, resolver) =>
			{
				var brain = system.ActorOf(resolver.Props<Brain>(), nameof(Brain));
				register.Register<Brain>(brain);
			});
		});
	}
}