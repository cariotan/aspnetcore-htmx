internal static partial class StaticMethods
{
#if DEBUG
	public static bool IsDevelopment => true;
#else
	public static bool IsDevelopment => false;
#endif
}