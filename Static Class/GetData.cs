using Newtonsoft.Json.Linq;

static partial class StaticMethods
{
	public static async Task<Result<string>> GetData(HttpRequestMessage httpRequestMessage, HttpClient httpClient, IRequiredActor<Brain> brain)
	{
		var access_token = await brain.Ask<string>(new GetAccessToken());
		httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);

		HttpResponseMessage response;
		{
			try
			{
				response = await httpClient.SendAsync(httpRequestMessage);
			}
			catch(HttpRequestException)
			{
				var error = ErrorCodes["GEN001"];
				SendDiscordException sendDiscordException = new(new Exception(error), "Developer");
				brain.Tell(sendDiscordException);
				return new Error(error);
			}
		}

		if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
		{
			return new Error(ErrorCodes["GEN002"]);
		}

		var data = await response.Content.ReadAsStringAsync();

		if(!response.IsSuccessStatusCode)
		{
			var json = JObject.Parse(data);
			if(json.ContainsKey("detail"))
			{
				var detailText = json["detail"]!.ToString();

				// Try to parse detail as JSON
				try
				{
					var detailJson = JObject.Parse(detailText);

					if(detailJson.ContainsKey("errors"))
					{
						var errors = (JObject)detailJson["errors"]!;
						var firstProp = errors.Properties().FirstOrDefault();

						if(firstProp != null)
						{
							var firstMessage = firstProp.Value.FirstOrDefault()?.ToString();
							if(!string.IsNullOrEmpty(firstMessage))
							{
								return new Error(firstMessage);
							}
						}
					}
				}
				catch
				{
					// detail is not JSON — fall through
				}

				// If parsing failed or no nested errors, treat detail as the message
				return new Error(detailText);
			}

			return new Error(data);
		}

		return data;
	}
}