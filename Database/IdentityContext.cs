using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

/*
dotnet ef migrations add InitialMigration -c IdentityContext
dotnet ef database update -c IdentityContext
*/

public class IdentityContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
	private string connectionString = $"Data Source={GetDatabasePath()}\\IdentityContext.db";

	public IdentityContext()
	{

	}

	public IdentityContext(string connectionString)
	{
		this.connectionString = connectionString;
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSqlite(connectionString);
	}
}

public class ApplicationUser : IdentityUser
{
	public ApplicationUser()
	{

	}

	public ApplicationUser(string userName)
		: base(userName)
	{
		Email = userName;
	}
}

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