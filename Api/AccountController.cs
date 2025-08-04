using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api;

[Route("Api/[controller]")]
[ApiController]
public class AccountController(ILogger<AccountController> logger, UserManager<ApplicationUser> userManager) : ControllerBase
{
	[HttpPost]
	[ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Login(string email, string password)
	{
#if !DEBUG
#error Use OAuth in production.
#endif

		logger.Endpoint(Get, "/Account");

		var user = await userManager.FindByEmailAsync(email);

		var verified = await userManager.CheckPasswordAsync(user ?? new(), password);

		if (verified && user is { })
		{
			var token = GenerateJwtToken(user.Id, email);
			return Ok(new LoginResponse(token));
		}
		else
		{
			return BadRequest("Failed to authenticate");
		}
	}
}

public record LoginResponse(string AccessToken);