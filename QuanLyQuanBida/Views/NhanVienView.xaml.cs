using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuanLyQuanBida.Views
{
    /// <summary>
    /// Interaction logic for NhanVienView.xaml
    /// </summary>
    public partial class NhanVienView : UserControl
    {
        public NhanVienView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        // Lọc chỉ cho phép nhập số cho TextBox lương cơ bản
        private void TextBox_PreviewTextInput_NumericOnly(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập: số 0-9, dấu chấm (.), và dấu âm (-)
            if (!char.IsDigit(e.Text, 0) && e.Text != "." && e.Text != "-")
            {
                e.Handled = true;
                return;
            }

            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                string newText = tb.Text.Insert(tb.CaretIndex, e.Text);

                // Không cho phép nhiều dấu chấm
                if (e.Text == "." && newText.Count(c => c == '.') > 1)
                {
                    e.Handled = true;
                    return;
                }

                // Không cho phép dấu âm ở giữa
                if (e.Text == "-" && tb.CaretIndex != 0)
                {
                    e.Handled = true;
                    return;
                }

                // Không cho phép dấu chấm ở đầu hoặc cuối
                if (e.Text == "." && (tb.CaretIndex == 0 || tb.CaretIndex == tb.Text.Length))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
        
        private void MonthYearPicker_CalendarOpened(object sender, RoutedEventArgs e)
        {
            // Tìm Calendar bên trong DatePicker
            var picker = sender as DatePicker;
            if (picker == null) return;

            var popup = picker.Template.FindName("PART_Popup", picker) as System.Windows.Controls.Primitives.Popup;
            if (popup != null && popup.Child is Calendar calendar)
            {
                // 1. Nhảy thẳng vào chế độ chọn Tháng (Year View)
                calendar.DisplayMode = CalendarMode.Year;

                calendar.DisplayModeChanged += (s, args) =>
                {
                    // 2. Nếu người dùng click vào một Tháng (khi đó mode chuyển sang Month View)
                    if (calendar.DisplayMode == CalendarMode.Month)
                    {
                        // Gán ngày vào Picker (mặc định lấy ngày 1 của tháng đó)
                        picker.SelectedDate = calendar.SelectedDate;
                        // Đóng lịch luôn, không cho chọn ngày nữa
                        picker.IsDropDownOpen = false;
                    }
                };
            }
        }

    }
}
