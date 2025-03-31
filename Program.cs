using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Simmy;

EmailRequest emailRequest = new();
Debug.Assert(emailRequest.From != null, "Please provide a default value to EmailRequest");

var builder = WebApplication.CreateBuilder(args);

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
	options.Password.RequiredLength = 8;
});

builder.Services.AddSignalR();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddAuthorizationBuilder()
	.SetFallbackPolicy(new AuthorizationPolicyBuilder()
	.RequireAuthenticatedUser()
	.Build()
);

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

app.Run();