using System.Globalization;

namespace ConnectHub.App.Converters
{
    public class DateTimeToRelativeTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                var timeDifference = DateTime.UtcNow - dateTime;

                if (timeDifference.TotalMinutes < 1)
                    return "just now";
                if (timeDifference.TotalMinutes < 60)
                    return $"{(int)timeDifference.TotalMinutes}m ago";
                if (timeDifference.TotalHours < 24)
                    return $"{(int)timeDifference.TotalHours}h ago";
                if (timeDifference.TotalDays < 7)
                    return $"{(int)timeDifference.TotalDays}d ago";
                if (timeDifference.TotalDays < 30)
                    return $"{(int)(timeDifference.TotalDays / 7)}w ago";
                if (timeDifference.TotalDays < 365)
                    return $"{(int)(timeDifference.TotalDays / 30)}mo ago";

                return $"{(int)(timeDifference.TotalDays / 365)}y ago";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
