using Microsoft.EntityFrameworkCore;

/*
dotnet ef migrations add InitialMigration -c PersistantDataContext -o Migrations/PersistantDataDatabase
dotnet ef database update -c PersistantDataContext
*/

namespace PersistantDataDatabase;

public class PersistantDataContext : DbContext
{
		private string connectionString = $"Data Source={Environment_GetDatabasePath()}\\PersistantDataContext.db";
		private static SqlitePragmaInterceptor sqlitePragmaInterceptor = new();

		public DbSet<PersistantData> PersistantData { get; set; } = null!;

		public PersistantDataContext()
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder
			.Entity<PersistantData>()
			.HasIndex(x => new { x.SessionId, x.Type });
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder
			.AddInterceptors(sqlitePragmaInterceptor)
			.UseSqlite(connectionString);
		}
}