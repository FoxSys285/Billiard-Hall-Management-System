using System;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyQuanBida.Converters
{
    /// <summary>
    /// Converter dùng để tính tiền đồ ăn/uống từ tổng tiền (sau giảm giá)
    /// Công thức: FoodAmount = TotalBeforeDiscount - TableFee
    /// Parameter: TableFee
    /// </summary>
    public class CalcFoodAmountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return 0;

            if (!double.TryParse(values[0]?.ToString(), out double totalBeforeDiscount))
                totalBeforeDiscount = 0;

            if (!double.TryParse(values[1]?.ToString(), out double tableFee))
                tableFee = 0;

            double foodAmount = Math.Max(0, totalBeforeDiscount - tableFee);
            return string.Format("{0:N0}", foodAmount);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
