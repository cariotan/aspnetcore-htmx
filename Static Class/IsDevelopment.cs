internal static class IsDevelopment
{
#if DEBUG
	public static bool Is => true;
#else
	public static bool Is => false;
#endif
}