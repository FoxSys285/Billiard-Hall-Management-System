using QuanLyQuanBida.ViewModels;
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
using System.Windows.Shapes;

namespace QuanLyQuanBida.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (DataContext is MainViewModel vm)
            {
                vm.PropertyChanged += Vm_PropertyChanged;
                AdjustWindowSize(vm.CurrentAccount != null);
            }
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentAccount))
            {
                if (sender is MainViewModel vm)
                {
                    AdjustWindowSize(vm.CurrentAccount != null);
                }
            }
        }

        private void AdjustWindowSize(bool loggedIn)
        {
            if (loggedIn)
            {
                MinWidth = 1400;
                MinHeight = 750;
                Width = 1400;
                Height = 750;
            }
            else
            {
                MinWidth = 400;
                MinHeight = 500;
                Width = 400;
                Height = 500;
            }
            CenterWindowOnScreen();
        }

        private void CenterWindowOnScreen()
        {
            Left = (SystemParameters.WorkArea.Width - Width) / 2 + SystemParameters.WorkArea.Left;
            Top = (SystemParameters.WorkArea.Height - Height) / 2 + SystemParameters.WorkArea.Top;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (this.DataContext is MainViewModel vm)
                {
                    vm.RecordLogoutTime();
                }
            }
            catch { }
        }




        private void ContentControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb != null)
            {
                lb.SelectedIndex = -1;
            }
        }
    }
}
