using Microsoft.AspNetCore.Identity;

static partial class StaticMethods
{
	public static async Task<bool> ValidatePassword(string password, IPasswordValidator<ApplicationUser> passwordValidator, UserManager<ApplicationUser> userManager)
	{
			return (await passwordValidator.ValidateAsync(userManager, new ApplicationUser(), password)).Succeeded;
	}
}