using JasperFx;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.SetupWolverineAndMarten();
builder.Services.AddSignalR();
builder.Services.SetupPolly();
builder.Services.AddControllersWithViews();
builder.Services.SetupIdentity();
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

await app.RunJasperFxCommands(args);