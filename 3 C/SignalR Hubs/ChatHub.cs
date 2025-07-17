using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
	public override Task OnConnectedAsync()
	{
		System.Console.WriteLine($"Connection connected: {Context.ConnectionId} ");
		return base.OnConnectedAsync();
	}
}