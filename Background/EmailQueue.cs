using System.Threading.Channels;

public class EmailQueue
{
    private readonly Channel<EmailRequest> channel;

    public EmailQueue()
    {
        // Create an unbounded channel (you can also use a bounded channel for limiting memory usage)
        channel = Channel.CreateUnbounded<EmailRequest>();
    }

    // Enqueue an email request to be processed later
    public async Task EnqueueAsync(EmailRequest emailRequest)
    {
        await channel.Writer.WriteAsync(emailRequest);
    }

    // Dequeue an email request (this will block until there's an item or cancellation)
    public async Task<EmailRequest> DequeueAsync(CancellationToken cancellationToken)
    {
        return await channel.Reader.ReadAsync(cancellationToken);
    }
}
