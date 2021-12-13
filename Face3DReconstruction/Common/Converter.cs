using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace Face3DReconstruction.Common
{
    public class BoolOrToVisibilityConverter : IMultiValueConverter
    {
        public Visibility HiddenVisibility { get; set; }

        public bool IsInverted { get; set; }

        public BoolOrToVisibilityConverter()
        {
            HiddenVisibility = Visibility.Collapsed;
            IsInverted = false;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = values.OfType<IConvertible>().Any(System.Convert.ToBoolean);
            if (IsInverted) flag = !flag;
            return flag ? Visibility.Visible : HiddenVisibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
