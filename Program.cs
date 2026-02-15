using System.Collections;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

_ = Environment_GetDatabasePath();

DotNetEnv.Env.Load();
DotNetEnv.Env.Load(Path.Combine(Environment_GetWorkingDirectory(), ".env"));

var keys = Environment_EnvironmentVariables.Keys;

foreach (var key in keys)
{
	if (Environment_EnvironmentVariables[key] is null)
	{
		throw new Exception($"Please set {key} in environment variables");
	}
}

Directory.CreateDirectory(Environment_GetDatabasePath());

IdentityContext identityContext = new();
identityContext.Database.Migrate();

ErrorContext errorContext = new();
errorContext.Database.Migrate();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ErrorContext>();

builder.Services.AddControllersWithViews(x =>
{
	x.Conventions.Add(new SetUserConvention());
	x.Conventions.Add(new SetSessionIdConvention());
	x.Filters.Add<CustomExceptionFilter>();
});

builder.Services.SetupServices_AddSwagger();
// builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.SetupServices_AddIdentity();
builder.Services.SetupServices_AddAkka();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.SetupServices_AddSerilog();
builder.Services.AddCors();


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger(c =>
	{
		c.RouteTemplate = "openapi/{documentName}.json";
	});

	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/openapi/v1.json", "API V1");
		c.RoutePrefix = "swagger"; // Swagger UI at /swagger
		c.ConfigObject.PersistAuthorization = true;
	});
	// app.MapOpenApi();
	// app.MapScalarApiReference(options => options
	// 	.AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme)
	// 	 .AddHttpAuthentication("Bearer", _ => { })
	// 	.WithPersistentAuthentication());
}

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors(x => x
	.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[0])
	.AllowAnyMethod()
	.AllowAnyHeader()
	.AllowCredentials());

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
	name: "v1-primary",
	pattern: "{controller=Home}/{action=Index}/{id?}",
	defaults: new { area = "V1" }
);

app.MapControllerRoute(
	name: "v1-primary-area",
	pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapHub<ChatHub>($"/{nameof(ChatHub)}");
app.MapHub<ErrorHub>($"/{nameof(ErrorHub)}");

app.MapFallback(context =>
{
	context.Response.Redirect("/NotFound");
	return Task.CompletedTask;
});

app.Run();