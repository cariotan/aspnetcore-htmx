using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

_ = GetDatabasePath();

DotNetEnv.Env.Load();

Directory.CreateDirectory(GetDatabasePath());

IdentityContext identityContext = new();
identityContext.Database.Migrate();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(x =>
{
	x.Conventions.Add(new SetUserConvention());
	x.Conventions.Add(new SetSessionIdConvention());
	x.Filters.Add<CustomExceptionFilter>();
});

// builder.Services.AddSwagger();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.AddIdentity();
builder.Services.AddAkka();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.AddSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	// app.UseSwagger(c =>
	// {
	// 	c.RouteTemplate = "openapi/{documentName}.json";
	// });

	// app.UseSwaggerUI(c =>
	// {
	// 	c.SwaggerEndpoint("/openapi/v1.json", "API V1");
	// 	c.RoutePrefix = "swagger"; // Swagger UI at /swagger
	// 	c.ConfigObject.PersistAuthorization = true;
	// });
	app.MapOpenApi();
	app.MapScalarApiReference(options => options
		.AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme)
		 .AddHttpAuthentication("Bearer", _ => { })
		.WithPersistentAuthentication());
}

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