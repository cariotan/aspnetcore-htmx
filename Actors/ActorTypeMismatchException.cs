public class ActorTypeMismatchException : Exception
{
	public ActorTypeMismatchException(string expected, string actual, string file, int line)
		: base($"[Type Mismatch] Expected <{expected}> but got <{actual}> at {file}:{line}")
	{
	}
}