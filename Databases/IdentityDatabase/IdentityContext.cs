using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

/*
dotnet ef migrations add InitialMigration -c IdentityContext
dotnet ef database update -c IdentityContext
*/

public class IdentityContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
	private string connectionString = $"Data Source={DatabasePath}\\DefaultIdentityContext.db";

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