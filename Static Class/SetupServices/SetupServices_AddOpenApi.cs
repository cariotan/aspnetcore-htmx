using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

static partial class StaticMethods
{
	public static void SetupServices_AddOpenApi(this IServiceCollection services)
	{
		services.AddOpenApi(options =>
		{
			// 1) Add Bearer security scheme once (document transformer)
			options.AddDocumentTransformer((document, context, ct) =>
			{
				document.Components ??= new OpenApiComponents();
				document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
				{
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Name = "Authorization",
					Description = "JWT Bearer authentication"
				};
				return Task.CompletedTask;
			});

			// 2) Add security requirement per-authorized endpoint (operation transformer)
			options.AddOperationTransformer((operation, context, ct) =>
			{
				var actionDescriptor = context.Description.ActionDescriptor;

				// AllowAnonymous short-circuit
				var hasAllowAnonymous = actionDescriptor.EndpointMetadata?.OfType<AllowAnonymousAttribute>().Any() == true;
				if (hasAllowAnonymous) return Task.CompletedTask;

				bool hasAuthorize = false;

				// MVC controllers
				if (actionDescriptor is ControllerActionDescriptor cad)
				{
					hasAuthorize =
						cad.ControllerTypeInfo.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Any()
						|| cad.MethodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Any();
				}
				else
				{
					// Minimal APIs or other endpoints
					hasAuthorize = actionDescriptor.EndpointMetadata?.OfType<AuthorizeAttribute>().Any() == true;
				}

				if (!hasAuthorize)
				{
					return Task.CompletedTask;
				}

				operation.Security ??= new List<OpenApiSecurityRequirement>();
				operation.Security.Add(new OpenApiSecurityRequirement
				{
					[new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
					}] = Array.Empty<string>()
				});

				operation.Responses ??= [];

				if (!operation.Responses.ContainsKey("401"))
				{
					operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
				}

				if (!operation.Responses.ContainsKey("403"))
				{
					operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
				}

				return Task.CompletedTask;
			});
		});
	}
}