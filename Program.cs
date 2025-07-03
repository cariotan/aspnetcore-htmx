using JasperFx;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Diagnostics;
using System.Text;
using Wolverine.Marten;

DotNetEnv.Env.Load();

EmailRequest emailRequest = new();
Debug.Assert(emailRequest.From != null, "Please provide a default value to EmailRequest");

var builder = WebApplication.CreateBuilder(args);

_ = Directory.CreateDirectory("""C:/inetpub/Ripple Logs""");

// builder.Host.UseSerilog();
// Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .WriteTo.File(
// 	   path: "C:/inetpub/Ripple Logs/Ripple-.txt",
// 	   rollingInterval: RollingInterval.Day,
// 	   fileSizeLimitBytes: 104857600,
// 	   rollOnFileSizeLimit: true,
// 	   outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}" // Custom format
//    )
//    .CreateLogger();

builder.Host.UseWolverine();
// builder.Services.AddMarten(x =>
// {
// 	x.Connection("Host=localhost;Port=5432;Database=;Username=;Password=");
// 	x.UseSystemTextJsonForSerialization();
// 	if (builder.Environment.IsDevelopment())
// 	{
// 		x.AutoCreateSchemaObjects = AutoCreate.All;
// 		x.DisableNpgsqlLogging = true;
// 	}
// }).IntegrateWithWolverine();

// Replace default logging with Serilog
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database setup
builder.Services
	.AddDbContext<IdentityContext>()
	.AddIdentity<ApplicationUser, ApplicationRole>()
	.AddEntityFrameworkStores<IdentityContext>()
	.AddDefaultTokenProviders();

// Authentication configuration setup
builder.Services
	.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromSeconds(0))
	.Configure<IdentityOptions>(options =>
	{
		options.Password.RequireNonAlphanumeric = false;
		options.Password.RequiredLength = 3;
		options.Password.RequireDigit = false;
		options.Password.RequireUppercase = false;
	})
	// // .Configure<IdentityOptions>(options =>
	// // {
	// // 	options.Password.RequireNonAlphanumeric = false;
	// // 	options.Password.RequiredLength = 8;
	// // })
	.ConfigureApplicationCookie(options =>
	{
		options.Cookie.SameSite = SameSiteMode.Strict;
	});
	// .AddAuthentication(options =>
	// {
	// 	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	// 	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	// })
	// .AddJwtBearer(options =>
	// {
	// 	options.TokenValidationParameters = new TokenValidationParameters
	// 	{
	// 		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningSecret)),
	// 		ValidateIssuerSigningKey = true,
	// 		ValidIssuer = "cario",
	// 		ValidAudience = "cario",
	// 	};

// 	options.Events = new JwtBearerEvents
// 	{
// 		OnMessageReceived = context =>
// 		{
// 			Console.WriteLine("Message received");
// 			var accessToken = context.Request.Query["access_token"];

// 			// If the request is for our hub...
// 			var path = context.HttpContext.Request.Path;
// 			if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ChatHub"))
// 			{
// 				// Read the token out of the query string
// 				context.Token = accessToken;
// 			}
// 			return Task.CompletedTask;
// 		}
// 	};
// });

builder.Services.AddSignalR();

// Implement Polly ResiliencePipeline
builder.Services.AddResiliencePipeline("Email", builder =>
{
	builder.AddRetry(new RetryStrategyOptions
	{
		MaxRetryAttempts = 3,
		Delay = TimeSpan.FromSeconds(2),
		BackoffType = DelayBackoffType.Constant
	});
});

builder.Services.AddResiliencePipeline<string, Task>("Discord", (builder, context) =>
{
	builder
		.AddChaosFault(1, () => new Exception("Chaos fault"))
		.AddFallback(new FallbackStrategyOptions<Task>
		{
			FallbackAction = async args =>
			{
				if (args.Context.Properties.TryGetValue(ResilienceKeys.Discord, out var discordMessage))
				{
					var emailQueue = context.ServiceProvider.GetRequiredService<EmailQueue>();
					EmailNotification emailRequest = new("Discord failed. Falling back to email", discordMessage);
					await emailQueue.EnqueueAsync(emailRequest);
				}

				return Outcome.FromResult(Task.CompletedTask);
			}
		})
		.AddRetry(new RetryStrategyOptions<Task>
		{
			ShouldHandle = new PredicateBuilder<Task>().Handle<HttpRequestException>(),
			MaxRetryAttempts = 3,
			Delay = TimeSpan.FromSeconds(2),
			BackoffType = DelayBackoffType.Constant
		});
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapHub<ChatHub>("/ChatHub");

await app.RunJasperFxCommands(args);