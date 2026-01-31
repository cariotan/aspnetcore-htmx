
using Microsoft.AspNetCore.Identity;

static partial class StaticMethods
{
	public static async Task<Result<ApplicationUser>> CreateOrLinkUser(string email, string providerId, string provider, string providerFriendlyName, UserManager<ApplicationUser> userManager)
	{
		var user = await userManager.FindByLoginAsync(provider, providerId);

		if (user is { })
		{
			return user;
		}
		else
		{
			user = await userManager.FindByEmailAsync(email);

			if (user is { })
			{
				await userManager.AddToRoleAsync(user, "User");

				var result = await userManager.AddLoginAsync(user, new(provider, providerId, providerFriendlyName));

				if (result.Succeeded)
				{
					return user;
				}
				else
				{
					return new Error(result.Errors.First().Description);
				}
			}
			else
			{
				user = new(email);

				var result = await userManager.CreateAsync(user);

				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(user, "User");

					result = await userManager.AddLoginAsync(user, new(provider, providerId, providerFriendlyName));

					if (result.Succeeded)
					{
						return user;
					}
					else
					{
						return new Error(result.Errors.First().Description);
					}
				}
				else
				{
					return new Error(result.Errors.First().Description);
				}
			}
		}
	}
}