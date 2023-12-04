using System;
using System.Text.Json;

namespace Danny.Core.Common;

	public static class Extension
	{
    private static DateTime unixEpochDatetime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static string HttpGetString(this HttpClient httpClient, string codeUrl)
    {
        int tries = 5;
        bool successed = false;

        while (!successed)
        {
            try
            {
                return httpClient.GetAsync(codeUrl).Result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (tries > 0)
                {
                    tries--;
                    continue;
                }
                throw new Exception($"HTTP Request Exception!!! Error {e.Message}, {codeUrl}'");
            }
        }
        return string.Empty;
    }

    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return Convert.ToInt64((dateTime.ToUniversalTime() - unixEpochDatetime).TotalMilliseconds);
    }

    public static DateTime UnixTimestampToDateTime(this long unixTimestamp)
    {
        return unixEpochDatetime.AddMilliseconds(unixTimestamp).ToLocalTime(); 
    }

    public static DateTime FirstDayOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    public static double TryGetDoubleEx(this JsonElement je)
    {
        if (je.ValueKind == JsonValueKind.Number)
        {
            return !je.TryGetDouble(out double ret) ? 0.0 : ret;
        }
        return 0.0;
    }
}

