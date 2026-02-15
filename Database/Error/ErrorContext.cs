using Microsoft.EntityFrameworkCore;

/*
dotnet ef migrations add AddedHandledToUnhandledError -c ErrorContext -o Migrations/ErrorDatabase
dotnet ef database update -c ErrorContext
*/

public class ErrorContext : DbContext
{
		private string connectionString = $"Data Source={Environment_GetDatabasePath()}\\ErrorContext.db";

		public DbSet<UnhandledError> UnhandledErrors { get; set; } = null!;

		public ErrorContext()
		{

		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(connectionString);
		}
}