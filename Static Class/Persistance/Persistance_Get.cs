using PersistantDataDatabase;
using Newtonsoft.Json;

static partial class StaticMethods
{
	public static async Task<Result<T?>> Persistance_Get<T>(
		string sessionId,
		string type,
		PersistantDataContext persistantDataContext
	)
	{
		var dbPersistantData = persistantDataContext.PersistantData.FirstOrDefault(x =>
			x.Key == sessionId &&
			x.Type == type
		);

		if(dbPersistantData is null)
		{
			return default(T);
		}

		return JsonConvert.DeserializeObject<T>(dbPersistantData.DataJson);
	}
}