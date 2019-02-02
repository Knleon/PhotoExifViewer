using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PhotoViewer.Converters
{
    public class MediaTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Errorの場合
            if (value == null || parameter == null || !(value is Enum))
            {
                return Visibility.Collapsed;
            }

            var currentState = value.ToString();
            var stateString = parameter.ToString();

            bool found = false;

            foreach (var state in stateString.Split(','))
            {
                found = (currentState == state.Trim());

                // 見つかったら抜ける
                if (found) break;
            }

            return found ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
