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
using QuanLyQuanBida.ViewModels;

namespace QuanLyQuanBida.Views
{
    /// <summary>
    /// Interaction logic for TrangChuView.xaml
    /// </summary>
    public partial class TrangChuView : UserControl
    {
        public TrangChuView()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                try
                {
                    vm.LoadFullData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"TrangChu load error: {ex.Message}\n{ex.StackTrace}", "Debug", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"TrangChu.Loaded: DataContext is {(DataContext == null ? "null" : DataContext.GetType().FullName)}", "Debug", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
