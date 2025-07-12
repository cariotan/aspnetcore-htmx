using Serilog;

static partial class StaticMethods
{
	public static void SetupSerilog(this WebApplicationBuilder builder)
	{
#if DEBUG
		builder.Host.UseSerilog();
		Log.Logger = new LoggerConfiguration()
		   .MinimumLevel.Information()
		   .WriteTo.File(
			   path: "C:/Ripple-.txt",
			   rollingInterval: RollingInterval.Day,
			   fileSizeLimitBytes: 104857600,
			   rollOnFileSizeLimit: true,
			   outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
		   )
		   .CreateLogger();
#endif
	}
}