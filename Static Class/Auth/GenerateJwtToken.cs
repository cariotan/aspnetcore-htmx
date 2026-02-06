using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

static partial class StaticMethods
{
	public static string GenerateJwtToken(string userId, string email)
	{
		Claim[] claims =
		[
			new(ClaimTypes.Name, email),
			new(ClaimTypes.NameIdentifier, userId),
		];

		return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
			claims: claims,
			expires: DateTime.Now.AddHours(1),
			signingCredentials: new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningSecret ?? throw new Exception("SigningSecret is null."))), SecurityAlgorithms.HmacSha256),
			issuer: "cario",
			audience: "cario"
		));
	}
}