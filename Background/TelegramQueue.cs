using System.Threading.Channels;

public class TelegramQueue
{
	private readonly Channel<TelegramMessage> channel;

	public TelegramQueue()
	{
		// Create an unbounded channel (you can also use a bounded channel for limiting memory usage)
		channel = Channel.CreateUnbounded<TelegramMessage>();
	}

	// Enqueue an email request to be processed later
	public async Task EnqueueAsync(TelegramMessage message)
	{
		await channel.Writer.WriteAsync(message);
	}

	// Dequeue an email request (this will block until there's an item or cancellation)
	public async Task<TelegramMessage> DequeueAsync(CancellationToken cancellationToken)
	{
		return await channel.Reader.ReadAsync(cancellationToken);
	}
}

public class TelegramMessage
{
	public string Message { get; set; } = "";
	public DateTime SentDate { get; set; } = DateTime.Now;
}