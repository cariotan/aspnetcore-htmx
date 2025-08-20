static partial class StaticMethods
{
	public static async Task<string> GenerateRefreshToken(string userId, IdentityContext identityContext)
	{
		var refresh_token = GenerateRandomString();

		var hash = GenerateHash(refresh_token);

		RefreshToken refreshToken = new()
		{
			DateCreated = DateTime.UtcNow,
			DateExpires = DateTime.UtcNow.AddYears(1),
			Hash256ShaToken = hash,
			Purpose = "web",

			UserId = userId
		};

		identityContext.Add(refreshToken);
		await identityContext.SaveChangesAsync();

		return refresh_token;
	}
}