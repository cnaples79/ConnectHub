using System.Globalization;

namespace ConnectHub.App.Converters;

public class BooleanToObjectConverter : IValueConverter
{
    public object TrueObject { get; set; }
    public object FalseObject { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueObject : FalseObject;
        }
        return FalseObject;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.Equals(TrueObject) ?? false;
    }
}
