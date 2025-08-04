using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api;

[Route("Api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class WeatherController : ControllerBase
{
	[HttpGet]
	[ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[Route("")]
	public IActionResult Get([Required] int integer, string? test)
	{
		if (integer % 2 == 0)
		{
			return Ok(new WeatherResponse(23, "Sunny"));
		}
		else
		{
			return Problem("Integer must be an even number.", statusCode: 200);
		}
	}

	[HttpPost]
	[ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[Route("Post")]
	public IActionResult Post([FromForm] PostRequest postRequest)
	{
		return Problem("This is my dictionary", statusCode: 200);
	}
}

public record PostRequest(string Temperature, string Condition);
public record WeatherResponse(int Temperature, string Condition);
public record WeatherPostResponse(string Test);

/*
======================
 ASP.NET Core Binding Summary
======================

[ApiController] behavior:
- Automatically returns ProblemDetails for 4xx/5xx responses
- Implicitly binds complex types from the request body (assumes JSON)
- Query binding otherwise unless IFormFile

Normal controllers (without [ApiController]):
- No implicit [FromBody] for complex types (body is expensive and only readable once)
- Model binding falls back to: Form → Route → Query → Header
- Complex types can be inferred from form data without needing [FromForm]

Swagger / OpenAPI generation:
- Without binding attributes or [ApiController], OpenAPI assumes all parameters come from query/route
- Even if ASP.NET Core correctly binds form data, Swagger won’t document it as form unless [FromForm] is explicitly added
*/