using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

public class SqlitePragmaInterceptor : DbConnectionInterceptor
{
	public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
	{
		var command = connection.CreateCommand();
		command.CommandText = @"
			PRAGMA journal_mode = WAL;
			PRAGMA journal_size_limit = 200000000;
			PRAGMA synchronous = NORMAL;
			PRAGMA foreign_keys = ON;
			PRAGMA temp_store = MEMORY;
			PRAGMA cache_size = -32000;
		";
		command.ExecuteNonQuery();
	}
}