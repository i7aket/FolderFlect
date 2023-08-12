using System;

namespace FolderFlect.Utilities
{
    public static class TimeHelper
    {
        public static string GetInterval(int seconds)
        {
            int days = seconds / 86400;
            int remainder = seconds % 86400;
            int hours = remainder / 3600;
            remainder %= 3600;
            int minutes = remainder / 60;
            seconds = remainder % 60;

            string result = "";

            if (days > 0)
            {
                result += $"{days}d ";
            }
            if (hours > 0 || days > 0)
            {
                result += $"{hours}h ";
            }
            if (minutes > 0 || hours > 0 || days > 0)
            {
                result += $"{minutes}m ";
            }
            result += $"{seconds}s";

            return result.Trim();
        }
    }
}
