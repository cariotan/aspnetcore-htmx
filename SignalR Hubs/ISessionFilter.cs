using Microsoft.AspNetCore.SignalR;

public class SessionIdHubFilter : IHubFilter
{
	public async ValueTask<object?> InvokeMethodAsync(
		HubInvocationContext invocationContext,
		Func<HubInvocationContext, ValueTask<object?>> next)
	{
		SetSessionId(invocationContext.Hub, invocationContext.Context);
		return await next(invocationContext);
	}

	public async Task OnConnectedAsync(
		HubLifetimeContext context,
		Func<HubLifetimeContext, Task> next)
	{
		SetSessionId(context.Hub, context.Context);
		await next(context);
	}

	private void SetSessionId(Hub hub, HubCallerContext context)
	{
		if (hub is IHasSessionId hubWithSession)
		{
			var httpContext = context.GetHttpContext();
			if (httpContext != null && httpContext.Request.Cookies.TryGetValue("SessionId", out var sessionId))
			{
				hubWithSession.SessionId = sessionId;
			}
		}
	}
}