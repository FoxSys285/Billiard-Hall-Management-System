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
