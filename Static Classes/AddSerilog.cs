using System.Reflection;
using Serilog;

static partial class StaticMethods
{
	public static void AddSerilog(this WebApplicationBuilder builder)
	{
#if !DEBUG
		Console.WriteLine("\nUsing Serilog for logging. Refer to the logging folder for logs.");

		string folder = $"""C:/{GetAssemblyName()} Logs""";

		Directory.CreateDirectory(folder);

		Log.Logger = new LoggerConfiguration()
		   .MinimumLevel.Information()
		   .WriteTo.File(
			   path: Path.Combine(folder, ".txt"),
			   rollingInterval: RollingInterval.Day,
			   fileSizeLimitBytes: 104857600,
			   rollOnFileSizeLimit: true,
			   outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
		   )
		   .CreateLogger();

		builder.Host.UseSerilog();
#endif
	}
}