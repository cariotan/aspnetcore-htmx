using System;

public static class TimeSpanExtensions
{
    // Integer overloads
    public static TimeSpan Second(this int value) => TimeSpan.FromSeconds(value);
    public static TimeSpan Minute(this int value) => TimeSpan.FromMinutes(value);
    public static TimeSpan Hour(this int value) => TimeSpan.FromHours(value);
    public static TimeSpan Day(this int value) => TimeSpan.FromDays(value);
    public static TimeSpan Millisecond(this int value) => TimeSpan.FromMilliseconds(value);

    public static TimeSpan Seconds(this int value) => TimeSpan.FromSeconds(value);
    public static TimeSpan Minutes(this int value) => TimeSpan.FromMinutes(value);
    public static TimeSpan Hours(this int value) => TimeSpan.FromHours(value);
    public static TimeSpan Days(this int value) => TimeSpan.FromDays(value);
    public static TimeSpan Milliseconds(this int value) => TimeSpan.FromMilliseconds(value);

    // Double overloads
    public static TimeSpan Second(this double value) => TimeSpan.FromSeconds(value);
    public static TimeSpan Minute(this double value) => TimeSpan.FromMinutes(value);
    public static TimeSpan Hour(this double value) => TimeSpan.FromHours(value);
    public static TimeSpan Day(this double value) => TimeSpan.FromDays(value);
    public static TimeSpan Millisecond(this double value) => TimeSpan.FromMilliseconds(value);

    public static TimeSpan Seconds(this double value) => TimeSpan.FromSeconds(value);
    public static TimeSpan Minutes(this double value) => TimeSpan.FromMinutes(value);
    public static TimeSpan Hours(this double value) => TimeSpan.FromHours(value);
    public static TimeSpan Days(this double value) => TimeSpan.FromDays(value);
    public static TimeSpan Milliseconds(this double value) => TimeSpan.FromMilliseconds(value);
}
