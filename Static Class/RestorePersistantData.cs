using Akka.Hosting;
using Newtonsoft.Json;

static partial class StaticMethods
{
	public static async Task<Result<T?>> RestorePersistantData<T>(
		string sessionId,
		IRequiredActor<Brain> brain
	)
	{
		string dataJson;
		{
			var result = await brain.Ask<Result<string>>(new UserSession_GetPersistantData("NewPatient", sessionId));
			if(result.IsNotSuccessful)
			{
				return new Utilities.Error(result.ErrorMessage.ToString());
			}

			dataJson = result.Value;
		}

		return JsonConvert.DeserializeObject<T>(dataJson) ?? default(T);
	}
}