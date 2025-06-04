using Microsoft.AspNetCore.Identity;

public class ApplicationRole : IdentityRole
{
	public ApplicationRole()
	{

	}

	public ApplicationRole(string role)
		: base(role)
	{

	}
}