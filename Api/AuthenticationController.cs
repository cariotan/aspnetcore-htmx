using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Api;

[Route("Api/[controller]")]
[ApiController]
public class AuthenticationController(UserManager<ApplicationUser> userManager, IdentityContext identityContext) : ControllerBase
{
	static string DummyHash;

	static AuthenticationController()
	{
		DummyHash = new PasswordHasher<ApplicationUser>().HashPassword(new ApplicationUser(), "DummyPassword!123");
	}

	[HttpPost]
	[ProducesResponseType(typeof(AuthenticationResponse), 200)]
	public async Task<ActionResult<AuthenticationResponse>> Login([FromForm][Required] string email, [FromForm][Required] string password)
	{
		var user = await userManager.FindByEmailAsync(email);

		if (user is { })
		{
			var verified = await userManager.CheckPasswordAsync(user, password);

			if (verified)
			{
				var refresh_token = await Auth_GenerateRefreshToken(user.Id, identityContext);

				var access_token = Auth_GenerateJwtToken(user.Id, email);

				return new AuthenticationResponse(access_token, refresh_token);
			}
		}
		else
		{
			_ = userManager.PasswordHasher.VerifyHashedPassword(new(), DummyHash, password);
		}

		return Unauthorized();
	}

	[HttpPost("RequestAccessToken")]
	public async Task<ActionResult<AuthenticationResponse>> RequestAccessToken([FromForm] string refresh_token)
	{
		var hash = Auth_GenerateHash(refresh_token);

		var refreshToken = await identityContext.RefreshTokens
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Hash256ShaToken == hash && x.Purpose == "web" && x.RevokedReason == null && x.DateExpires > DateTime.UtcNow);

		if (refreshToken is { })
		{
			refreshToken.RevokedReason = "RequestAccessToken";
			await identityContext.SaveChangesAsync();

			var access_token = Auth_GenerateJwtToken(refreshToken.UserId, refreshToken.User!.Email!);
			refresh_token = await Auth_GenerateRefreshToken(refreshToken.UserId, identityContext);

			return new AuthenticationResponse(access_token, refresh_token);
		}

		return Unauthorized();
	}

	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[HttpPost("RequestRefreshToken")]
	public async Task<ActionResult<AuthenticationResponse>> RequestRefreshToken()
	{
		var user = await userManager.GetUserAsync(User);
		if (user is { })
		{
			var access_token = Auth_GenerateJwtToken(user.Id, user.Email!);
			var refresh_token = await Auth_GenerateRefreshToken(user.Id, identityContext);

			return new AuthenticationResponse(access_token, refresh_token);
		}
		else
		{
			return Unauthorized();
		}
	}

	public record AuthenticationResponse([property: JsonProperty("access_token")] string AccessToken, [property: JsonProperty("refresh_token")] string RefreshToken);
}
