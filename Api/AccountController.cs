using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api;

[Route("Api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthenticationController(UserManager<ApplicationUser> userManager) : ControllerBase
{
	[HttpPost]
	[ProducesResponseType(typeof(string), 200)]
	[ProducesResponseType(typeof(ProblemDetails), 400)]
	[ProducesResponseType(typeof(ProblemDetails), 401)]
	public async Task<ActionResult<string>> Login([Required] string email, [Required] string password)
	{
#if !DEBUG
#error Use OAuth in production.
#endif

		var user = await userManager.FindByEmailAsync(email);

		var verified = await userManager.CheckPasswordAsync(user ?? new(), password);

		if (verified && user is { })
		{
			var token = GenerateJwtToken(user.Id, email);
			return token;
		}
		else
		{
			return Problem("Failed to authenticate.", statusCode: 401);
		}
	}
}