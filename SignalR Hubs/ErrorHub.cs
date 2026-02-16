using Microsoft.AspNetCore.SignalR;

public class ErrorHub : Hub, IHasSessionId
{
	public string SessionId { get; set; } = null!;

	public override Task OnConnectedAsync()
	{
		System.Console.WriteLine($"Error Hub connected: {Context.ConnectionId} ");

		var httpContext = Context.GetHttpContext();
		string? sessionId = httpContext?.Request.Cookies["SessionId"];

		Console.WriteLine($"Session ID: {sessionId}");


		Console.WriteLine($"Session ID from hub: {SessionId}");

		return base.OnConnectedAsync();
	}
}