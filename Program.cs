using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

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

builder.Services.SetupSwagger();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.SetupIdentity();
builder.Services.SetupAkka();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.SetupSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
		c.RoutePrefix = "api/swagger";
		c.ConfigObject.PersistAuthorization = true;
	});
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