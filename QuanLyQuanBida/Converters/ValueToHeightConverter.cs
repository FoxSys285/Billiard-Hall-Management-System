using System;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyQuanBida.Converters
{
    // MultiValueConverter: values[0] = value (double), values[1] = max (double)
    // ConverterParameter = max visual height (double)
    public class ValueToHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double value = 0;
                double max = 0;
                double maxHeight = 120; // default

                if (parameter != null)
                {
                    double.TryParse(parameter.ToString(), out maxHeight);
                }

                if (values.Length > 0 && values[0] != null)
                    double.TryParse(values[0].ToString(), out value);
                if (values.Length > 1 && values[1] != null)
                    double.TryParse(values[1].ToString(), out max);

                if (max <= 0) return 4.0; // tiny bar when no data

                double ratio = Math.Max(0, Math.Min(1, value / max));
                double height = ratio * maxHeight;
                // ensure visible
                return Math.Max(4.0, height);
            }
            catch
            {
                return 4.0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
