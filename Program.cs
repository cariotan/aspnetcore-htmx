using JasperFx;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Diagnostics;

DotNetEnv.Env.Load();

EmailRequest emailRequest = new();
Debug.Assert(emailRequest.From != null, "Please provide a default value to EmailRequest");

var builder = WebApplication.CreateBuilder(args);

if (false)
{
	// Configure Serilog
	_ = Directory.CreateDirectory("""C:/inetpub/Ripple Logs""");

	builder.Host.UseSerilog();
	Log.Logger = new LoggerConfiguration()
	   .MinimumLevel.Information() // Set minimum log level
	   .WriteTo.File(
		   path: "C:/inetpub/Ripple Logs/Ripple-.txt",
		   rollingInterval: RollingInterval.Day, // Log to file with daily rolling
		   fileSizeLimitBytes: 104857600,        // Set maximum file size to 50 MB
		   rollOnFileSizeLimit: true,           // Roll to a new file when size limit is reached
		   outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}" // Custom format
	   )
	   .CreateLogger();
}

builder.Host.UseWolverine();
builder.Services.AddMarten(options =>
{
	options.Connection("Host=localhost;Port=5432;Database=;Username=;Password=");
	options.UseSystemTextJsonForSerialization();
	if (builder.Environment.IsDevelopment())
	{
		options.AutoCreateSchemaObjects = AutoCreate.All;
	}
});

// Replace default logging with Serilog
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
	.AddEntityFrameworkStores<IdentityContext>()
	.AddDefaultTokenProviders();

builder.Services.AddDbContext<IdentityContext>();

builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromSeconds(0));

builder.Services.ConfigureApplicationCookie(options => options.Cookie.SameSite = SameSiteMode.Strict);

builder.Services.Configure<IdentityOptions>(options =>
{
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequiredLength = 3;
	options.Password.RequireDigit = false;
	options.Password.RequireUppercase = false;
});

// builder.Services.Configure<IdentityOptions>(options =>
// {
// 	options.Password.RequireNonAlphanumeric = false;
// 	options.Password.RequiredLength = 8;
// });

builder.Services.AddSignalR();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.Cookie.SameSite = SameSiteMode.Strict;
});

// builder.Services.AddAuthentication(options =>
// {
// 	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
// 	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// 	.AddJwtBearer(options =>
// 	{
// 		options.TokenValidationParameters = new TokenValidationParameters
// 		{
// 			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningSecret)),
// 			ValidateIssuerSigningKey = true,
// 			ValidIssuer = "cario",
// 			ValidAudience = "cario",
// 		};

// 		options.Events = new JwtBearerEvents
// 		{
// 			OnMessageReceived = context =>
// 			{
// 				Console.WriteLine("Message received");
// 				var accessToken = context.Request.Query["access_token"];

// 				// If the request is for our hub...
// 				var path = context.HttpContext.Request.Path;
// 				if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ChatHub"))
// 				{
// 					// Read the token out of the query string
// 					context.Token = accessToken;
// 				}
// 				return Task.CompletedTask;
// 			}
// 		};
// 	}
// );

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

// TelegramConfig.ChatId = "";

var app = builder.Build();

// Configure the HTTP request pipeline.
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