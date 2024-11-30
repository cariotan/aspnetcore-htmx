using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class IdentityContext : IdentityDbContext<AppUser, AppRole, string>
{
	private string connectionString = "Data Source=C:\\Database\\DefaultIdentityContext.db";

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