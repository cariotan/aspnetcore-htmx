using System.Threading.Channels;

public class DiscordQueue
{
	private readonly Channel<DiscordMessage> channel;

	public DiscordQueue()
	{
		// Create an unbounded channel (you can also use a bounded channel for limiting memory usage)
		channel = Channel.CreateUnbounded<DiscordMessage>();
	}

	// Enqueue an email request to be processed later
	public async Task EnqueueAsync(DiscordMessage message)
	{
		await channel.Writer.WriteAsync(message);
	}

	// Dequeue an email request (this will block until there's an item or cancellation)
	public async Task<DiscordMessage> DequeueAsync(CancellationToken cancellationToken)
	{
		return await channel.Reader.ReadAsync(cancellationToken);
	}
}