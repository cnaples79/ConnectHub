using System.Globalization;

namespace ConnectHub.App.Converters
{
    public class BoolToLikeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLiked)
            {
                return isLiked ? "like_filled_icon.png" : "like_outline_icon.png";
            }
            return "like_outline_icon.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
