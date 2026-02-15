using Microsoft.AspNetCore.SignalR;

public class ErrorHub : Hub
{
	public override Task OnConnectedAsync()
	{
		System.Console.WriteLine($"Error Hub connected: {Context.ConnectionId} ");

		return base.OnConnectedAsync();
	}
}