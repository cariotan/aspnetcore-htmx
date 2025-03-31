using Polly;

public static class ResilienceKeys
{
    public static readonly ResiliencePropertyKey<string> Discord = new("Discord");
}