using Microsoft.AspNetCore.Identity;

static partial class StaticMethods
{
	public static void SetupIdentity(this IServiceCollection serviceCollection)
	{
		serviceCollection
			.AddDbContext<IdentityContext>()
			.AddIdentity<ApplicationUser, ApplicationRole>()
			.AddEntityFrameworkStores<IdentityContext>()
			.AddDefaultTokenProviders();

		serviceCollection
			.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = 0.Seconds())
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
	}
}