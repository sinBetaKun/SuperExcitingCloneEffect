using SuperExcitingCloneEffect.Classes;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SuperExcitingCloneEffect.Controllers.CloneValueList
{
    internal class FolderIconVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not CloneGroupValue gv)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
