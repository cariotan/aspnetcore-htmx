using Akka.Actor;
using Akka.Hosting;
using System.Runtime.CompilerServices;

static partial class StaticMethods
{
	public static void Tell<T>(this IRequiredActor<T> actorRef, object message)
	{
		actorRef.ActorRef.Tell(message);
	}

	public static async Task<T> Ask<T>(
		this IRequiredActor<Brain> actorRef,
		object message,
		TimeSpan? timeout = null,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNum = 0)
	{
#if DEBUG
		timeout = TimeSpan.FromSeconds(30);
#endif

		try
		{
			return await actorRef.ActorRef.Ask<T>(message, timeout);
		}
		catch (AskTimeoutException)
		{
			string fileName = Path.GetFileName(filePath);
			throw new Exception($"[Actor Timeout] After {timeout?.TotalSeconds ?? 20}s at {fileName}:{lineNum}");
		}
		catch (ArgumentException ex) when (ex.Message.Contains("Ask expected message of type"))
		{
			string actualType = ExtractTypeName(ex.Message);
			string fileName = Path.GetFileName(filePath);

			throw new ActorTypeMismatchException(typeof(T).Name, actualType, fileName, lineNum);
		}

		static string ExtractTypeName(string message)
		{
			try
			{
				// Pulls "String" out of "[System.String]"
				int start = message.IndexOf("[System.") + 8;
				if (start < 8) return "Unknown";

				int end = message.IndexOf("]", start);
				return message.Substring(start, end - start);
			}
			catch
			{
				return "Unknown";
			}
		}
	}

	public static async Task Ask(this IRequiredActor<Brain> actorRef, object message, TimeSpan? timeout = null)
	{
#if DEBUG
		timeout = TimeSpan.FromSeconds(30);
#endif
		await actorRef.ActorRef.Ask(message, timeout);
	}
}