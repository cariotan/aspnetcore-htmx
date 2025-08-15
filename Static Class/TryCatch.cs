static partial class StaticMethods
{
	public static T TryCatch<T>(Func<T> func, Func<T> failFunc)
	{
		try
		{
			return func();
		}
		catch
		{
			return failFunc();
		}
	}
}