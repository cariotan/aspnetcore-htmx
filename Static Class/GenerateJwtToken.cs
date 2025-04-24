using System.IdentityModel.Tokens.Jwt;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

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
			expires: DateTime.Now.AddMinutes(60),
			signingCredentials: new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningSecret)), SecurityAlgorithms.HmacSha256)
		));
	}
}