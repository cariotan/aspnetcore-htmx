
using Microsoft.AspNetCore.Identity;

static partial class StaticMethods
{
	public static async Task<Result<ApplicationUser>> CreateOrLinkUser(
		string email,
		string providerId,
		string provider,
		string providerFriendlyName,
		UserManager<ApplicationUser> userManager,
		string? area = null
	)
	{
		var dbUser = await userManager.FindByLoginAsync(provider, providerId);

		if (dbUser is not null)
		{
			return dbUser;
		}

		dbUser = await userManager.FindByEmailAsync(email);

		if (dbUser is not null)
		{
			await userManager.AddToRoleAsync(dbUser, "User");

			var result = await userManager.AddLoginAsync(dbUser, new(provider, providerId, providerFriendlyName));
			if (!result.Succeeded)
			{
				return new Utilities.Error(result.Errors.First().Description);
			}

			return dbUser;
		}

		{
			var result = await Auth_CreateUser(email, userManager, area);
			if (result.IsNotSuccessful)
			{
				return new Utilities.Error(result.ErrorMessage.ToString());
			}
			dbUser = result.Value;
		}

		{
			var result = await userManager.AddLoginAsync(dbUser, new(provider, providerId, providerFriendlyName));
			if (!result.Succeeded)
			{
				await userManager.DeleteAsync(dbUser);
				return new Utilities.Error(result.Errors.First().Description);
			}
		}

		return dbUser;
	}
}