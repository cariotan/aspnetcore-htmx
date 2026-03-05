using Akka.Actor;
using Akka.Event;
using PersistantDataDatabase;
using Newtonsoft.Json;

public interface IUserSessionMsg
{
	string SessionId { get; }
}

public record UserSession_SavePersistantData(object DataToPersist, string Type, string SessionId) : IUserSessionMsg;
public record UserSession_GetPersistantData(string Type, string SessionId) : IUserSessionMsg;


public class UserSessionActor : ReceiveActor
{
	readonly ILoggingAdapter logger = Context.GetLogger();

	IServiceScopeFactory serviceScopeFactory;
	
	string? sessionId = null;

	protected override void PreStart()
	{
		Context.SetReceiveTimeout(2.Minutes());
	}

	protected override void PostStop()
	{
		using var scope = serviceScopeFactory.CreateScope();
		using PersistantDataContext persistantDataContext = scope.ServiceProvider.GetRequiredService<PersistantDataContext>();
		var dbPersistantData = persistantDataContext.PersistantData.FirstOrDefault(x => x.SessionId == sessionId);
		if(dbPersistantData is not null)
		{
			persistantDataContext.PersistantData.Remove(dbPersistantData);
			persistantDataContext.SaveChanges();
		}
	}

	public UserSessionActor(IServiceScopeFactory serviceScopeFactory)
	{
		this.serviceScopeFactory = serviceScopeFactory;

		Receive<UserSession_SavePersistantData>(Handle);
		Receive<UserSession_GetPersistantData>(Handle);

		Receive<ReceiveTimeout>(msg =>
		{
			logger.Info("User session timed out. Stopping actor.");
			Context.Stop(Self);
		});
	}

	private void Handle(UserSession_SavePersistantData msg)
	{
		sessionId = msg.SessionId;

		using var scope = serviceScopeFactory.CreateScope();
		using PersistantDataContext persistantDataContext = scope.ServiceProvider.GetRequiredService<PersistantDataContext>();

		var jsonData = JsonConvert.SerializeObject(msg.DataToPersist);

		PersistantData? dbPersistantData = persistantDataContext.PersistantData.FirstOrDefault(x => x.SessionId == msg.SessionId);
		if(dbPersistantData is null)
		{
			dbPersistantData = new()
			{
				SessionId = msg.SessionId,
				DateCreated = DateTime.Now,
				Type = msg.Type,
				DataJson = jsonData
			};
			persistantDataContext.PersistantData.Add(dbPersistantData);
		}
		else
		{
			dbPersistantData.DataJson = jsonData;
			dbPersistantData.DateCreated = DateTime.Now;
		}

		persistantDataContext.SaveChanges();
	}

	private void Handle(UserSession_GetPersistantData msg)
	{
		using var scope = serviceScopeFactory.CreateScope();
		using PersistantDataContext persistantDataContext = scope.ServiceProvider.GetRequiredService<PersistantDataContext>();

		var dbPersistantData = persistantDataContext.PersistantData.FirstOrDefault(x =>
			x.SessionId == msg.SessionId &&
			x.Type == msg.Type
		);

		if(dbPersistantData is null)
		{
			Sender.Tell(Result.Failed<string>("No persistant data found."));
			return;
		}

		Sender.Tell(Result.Success(dbPersistantData.DataJson));
	}
}