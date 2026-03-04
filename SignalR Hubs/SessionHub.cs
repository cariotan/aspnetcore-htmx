using Microsoft.AspNetCore.SignalR;

public class SessionHub : Hub
{
	public override Task OnConnectedAsync()
	{
		Js($"SessionHub connected: {Context.ConnectionId}");

		return base.OnConnectedAsync();
	}
}