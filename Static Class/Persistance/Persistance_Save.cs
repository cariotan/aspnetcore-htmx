using Newtonsoft.Json;
using PersistantDataDatabase;

static partial class StaticMethods
{
	public static void Persistance_Save(
		string sessionId,
		string type,
		object data,
		PersistantDataContext persistantDataContext
	)
	{
		var json = JsonConvert.SerializeObject(data);

		var dbPersistantData = persistantDataContext.PersistantData.FirstOrDefault(x =>
			x.SessionId == sessionId &&
			x.Type == type
		);

		if(dbPersistantData is null)
		{
			dbPersistantData = new PersistantData
			{
				DateCreated = DateTime.Now,
				SessionId = sessionId,
				Type = type,
				DataJson = json,
			};

			persistantDataContext.PersistantData.Add(dbPersistantData);
		}
		else
		{
			dbPersistantData.DataJson = json;
			dbPersistantData.DateCreated = DateTime.Now;
		}

		persistantDataContext.SaveChanges();
	}
}