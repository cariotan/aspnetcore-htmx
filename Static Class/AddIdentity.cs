using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

static partial class StaticMethods
{
	public static void AddIdentity(this IServiceCollection services)
	{
		services
			.AddDbContext<IdentityContext>()
			.AddIdentity<ApplicationUser, ApplicationRole>()
			.AddEntityFrameworkStores<IdentityContext>()
			.AddDefaultTokenProviders();

		services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = 0.Seconds());

		services.Configure<IdentityOptions>(options =>
		{
			options.Password.RequireNonAlphanumeric = false;
			options.Password.RequiredLength = 3;
			options.Password.RequireDigit = false;
			options.Password.RequireUppercase = false;
		});
		// services.Configure<IdentityOptions>(options =>
		// {
		// 	options.Password.RequireNonAlphanumeric = false;
		// 	options.Password.RequiredLength = 8;
		// });

		services.ConfigureApplicationCookie(options =>
		{
			options.Cookie.SameSite = SameSiteMode.Strict;
		});

		services.AddAuthentication(options =>
		{
			// options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			// options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningSecret)),
				ValidateIssuerSigningKey = true,
				ValidIssuer = "cario",
				ValidAudience = "cario",
			};

			// This is used for endpoints where the authentication headers cannot be supplied for whatever reason like signal r. In which case it will extract it from access_token query parameter.
			options.Events = new JwtBearerEvents
			{
				OnMessageReceived = context =>
				{
					Console.WriteLine("Message received");
					var accessToken = context.Request.Query["access_token"];

					// If the request is for our hub...
					var path = context.HttpContext.Request.Path;
					if (!string.IsNullOrEmpty(accessToken))
					{
						// Read the token out of the query string
						context.Token = accessToken;
					}
					return Task.CompletedTask;
				}
			};
		});
	}
}