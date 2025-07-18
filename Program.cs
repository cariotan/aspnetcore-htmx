using Microsoft.AspNetCore.Mvc.Razor;

_ = GetDatabasePath();

DotNetEnv.Env.Load();

Directory.CreateDirectory(GetDatabasePath());

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(x =>
{
	x.Conventions.Add(new GlobalControllerAttributeConvention());
	x.Filters.Add<CustomExceptionFilter>();
});

builder.Services.Configure<RazorViewEngineOptions>(x =>
{
	x.ViewLocationFormats.Clear();
	x.ViewLocationFormats.Add("/2 V/{1}/{0}.cshtml");
	x.ViewLocationFormats.Add("/2 V/Shared/{0}.cshtml");
});

builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.SetupPolly();
builder.Services.SetupIdentity();
builder.Services.SetupAkka();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// builder.SetupSerilog();

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

app.Run();