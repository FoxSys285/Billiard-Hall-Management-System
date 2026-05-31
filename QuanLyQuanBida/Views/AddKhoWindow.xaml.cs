using QuanLyQuanBida.ViewModels;
using System.Windows;

namespace QuanLyQuanBida.Views
{
    public partial class AddKhoWindow : Window
    {
        public AddKhoWindow()
        {
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                // Kiểm tra xem lệnh có hợp lệ không (các trường bắt buộc đã nhập đủ chưa)
                var can = (vm.ConfirmNewKhoCommand as RelayCommand)?.CanExecute(null) ?? true;
                if (!can)
                {
                    MessageBox.Show("Vui lòng kiểm tra và nhập đầy đủ thông tin phiếu nhập và chi tiết sản phẩm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                vm.ConfirmNewKho();
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}