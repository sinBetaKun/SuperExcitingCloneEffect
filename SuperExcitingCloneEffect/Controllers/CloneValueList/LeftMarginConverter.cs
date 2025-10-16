using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SuperExcitingCloneEffect.Controllers.CloneValueList
{
    public class LeftMarginConverter : IValueConverter
    {
        const double Coef = 20;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 値（例: DataContextのLeftOffset）をdoubleに変換
            double left = System.Convert.ToDouble(value);
            return new Thickness(left * Coef, 0, 0, 0); // 左だけ変更
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 双方向バインディング不要なら不要
            return Binding.DoNothing;
        }
    }
}
