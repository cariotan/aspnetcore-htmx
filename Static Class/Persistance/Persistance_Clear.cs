using PersistantDataDatabase;

static partial class StaticMethods
{
	public static void Persistance_Clear(
		string sessionId,
		PersistantDataContext persistantDataContext
	)
	{
		
		persistantDataContext.SaveChanges();
	}
}