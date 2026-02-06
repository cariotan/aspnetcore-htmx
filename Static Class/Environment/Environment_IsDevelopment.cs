internal static partial class StaticMethods
{
#if DEBUG
	public static bool Environment_IsDevelopment => true;
#else
	public static bool Environment_IsDevelopment => false;
#endif
}