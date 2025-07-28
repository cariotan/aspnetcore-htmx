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

			string cluster = "";

			// cluster = """
			// 	actor {
			// 		provider = cluster
			// 		deployment {
			// 			/Brain/LogActor {
			// 				remote = "akka.tcp://ActorSystem@localhost:8082"
			// 			}
			// 		}
			// 	}
			// 	remote {
			// 		dot-netty.tcp {
			// 			port = 8081
			// 			hostname = localhost
			// 		}
			// 	}
			// 	cluster {
			// 		seed-nodes = ["akka.tcp://ActorSystem@localhost:8081"]
			// 	}
			// """;

			x.AddHocon(ConfigurationFactory.ParseString($$"""
				{{log}}
				akka {
					{{cluster}}
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