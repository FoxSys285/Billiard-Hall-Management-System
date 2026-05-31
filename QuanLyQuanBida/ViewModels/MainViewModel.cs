using QuanLyQuanBida.Models;
using QuanLyQuanBida.Views;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity;
using static QuanLyQuanBida.ViewModels.MainViewModel;

namespace QuanLyQuanBida.ViewModels
{
    //Ban-Tran Nguyen - BEGIN
    public class MonAnChiTiet
    {
        public string MaMon { get; set; }
        public string TenMon { get; set; }
        public int SoLuong { get; set; }
        public double ThanhTien { get; set; }
    }
    //Ban-Tran Nguyen - END

    public class MainViewModel : BaseViewModel
    {
        // ===================== DATABASE =====================
        private const double PayRatePerHour = 100000;
        private QUANLYBIDAEntities1 db; // initialize only at runtime
        // ===================== USERCONTROL ====================
        private readonly BanView _banVM;
        private readonly DangNhapView _dnVM;
        private readonly KhachHangView _khVM;
        private readonly KhoView _khoVM;
        private readonly LichSuHoaDonView _lsVM;
        private readonly NhanVienView _nvVM;
        private readonly ThongKeView _tkVM;
        private readonly TrangChuView _tcVM;
        private readonly MenuView _menuVM;
        private readonly AdminMenuView _adminMenuVM;
        // ===================== ĐĂNG NHẬP =====================
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        private object _currentAppView;
        public object CurrentAppView
        {
            get => _currentAppView;
            set { _currentAppView = value; OnPropertyChanged(nameof(CurrentAppView)); }
        }

        public ICommand DangNhapCommand { get; set; }
        public ICommand BackLoginCommand { get; set; }
        public ICommand SwitchViewCommand { get; set; }
        // Chuyển đổi giao diện
        public void Switch(Object obj)
        {
            string viewType = obj as string;

            switch (viewType)
            {
                case "TrangChu":
                    CurrentAppView = _tcVM;
                    break;
                case "Menu":
                    CurrentAppView = IsManagerAccount() ? (object)_adminMenuVM : _menuVM;
                    break;
                case "Ban":
                    CurrentAppView = _banVM;
                    break;
                case "LichSu":
                    CurrentAppView = _lsVM;
                    break;
                case "KhachHang":
                    CurrentAppView = _khVM;
                    break;
                case "Kho":
                    CurrentAppView = _khoVM;
                    break;
                case "NhanVien":
                    CurrentAppView = _nvVM;
                    break;
                case "ThongKe":
                    CurrentAppView = _tkVM;
                    break;
                case "DangNhap":
                    CurrentView = _dnVM;
                    break;
            }
        }
        // Đăng xuất
        public void BackLogin(Object obj)
        {
            // before clearing account, record logout time
            try { RecordLogoutTime(); } catch { }
            CurrentView = _dnVM;
            CurrentAppView = _tcVM;
            CurrentAccount = null;
            // Clear any cached login inputs so password box is empty on next show
            InputMK = "";
            InputTK = "";
        }
        // Đăng nhập
        public void DangNhap(Object obj)
        {
            string tk = InputTK;
            string mk = InputMK;
            if (string.IsNullOrEmpty(tk) || string.IsNullOrEmpty(mk))
            {
                MessageBox.Show("Chưa nhập đầy đủ thông tin", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (KiemTraDangNhap(tk, mk))
            {
                // Tạo bảng chấm công khi đăng nhập (ngoại trừ Quản lý/Admin)
                CreateAttendanceIfNeeded();
                InputMK = "";
                InputTK = "";
                LoadThongKe();
                CurrentView = _dnVM;
                CurrentAppView = _tcVM;
                return;
            }
            MessageBox.Show("Sai mật khẩu hoặc tài khoản", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        // ===================== KIỂM TRA DỮ LIỆU ĐĂNG NHẬP =====================
        private NhanVien _currentAccount;
        public NhanVien CurrentAccount
        {
            get { return _currentAccount; }
            set
            {
                _currentAccount = value;
                OnPropertyChanged(nameof(CurrentAccount));
                OnPropertyChanged(nameof(PermisionManager));
                OnPropertyChanged(nameof(IsLogin));
                OnPropertyChanged(nameof(IsLoggedOut));
            }
        }

        public Visibility IsLoggedOut
        {
            get
            {
                return CurrentAccount == null ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private string _inputTK;
        private string _inputMK;
        public string InputTK
        {
            get { return _inputTK; }
            set
            {
                _inputTK = value;
                OnPropertyChanged(nameof(InputTK));
            }
        }
        public string InputMK
        {
            get { return _inputMK; }
            set
            {
                _inputMK = value;
                OnPropertyChanged(nameof(InputMK));
            }
        }

        private int _soHoaDonHomNay;
        public int SoHoaDonHomNay
        {
            get => _soHoaDonHomNay;
            set { _soHoaDonHomNay = value; OnPropertyChanged(nameof(SoHoaDonHomNay)); }
        }

        private double _tongDoanhThuHomNay;
        public double TongDoanhThuHomNay
        {
            get => _tongDoanhThuHomNay;
            set { _tongDoanhThuHomNay = value; OnPropertyChanged(nameof(TongDoanhThuHomNay)); }
        }

        public bool KiemTraDangNhap(string account, string password)
        {
            foreach (var nv in DsNhanVien)
            {
                if (nv.TaiKhoan == account && nv.MatKhau == password)
                {
                    CurrentAccount = nv;
                    return true;
                }
            }
            return false;
        }
        // ===================== PHÂN QUYỀN =====================
        public Visibility IsLogin
        {
            get
            {
                if (CurrentAccount == null)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }
        public Visibility PermisionManager
        {
            get
            {
                if (CurrentAccount == null) return Visibility.Collapsed;
                if (IsManagerAccount())
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        private bool IsManagerAccount()
        {
            var role = CurrentAccount?.ChucVu?.Trim();
            if (string.IsNullOrEmpty(role)) return false;
            return string.Equals(role, "Quản lý", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase);
        }

        // Tạo bản ghi chấm công khi nhân viên đăng nhập (nếu chưa có)
        private void CreateAttendanceIfNeeded()
        {
            try
            {
                if (CurrentAccount == null)
                {
                    Debug.WriteLine("CreateAttendanceIfNeeded: CurrentAccount is null");
                    return;
                }
                if (db == null)
                {
                    Debug.WriteLine("CreateAttendanceIfNeeded: db is null");
                    return;
                }

                var role = CurrentAccount.ChucVu?.Trim() ?? string.Empty;
                if (string.Equals(role, "Quản lý", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"CreateAttendanceIfNeeded: role '{role}' not eligible for attendance creation");
                    return; // không tạo cho quản lý/admin
                }

                var today = DateTime.Today;
                Debug.WriteLine($"CreateAttendanceIfNeeded: checking existence for MaNV={CurrentAccount.MaNV} date={today:yyyy-MM-dd}");
                bool exists = db.BangChamCongs.Any(b => b.MaNV == CurrentAccount.MaNV && DbFunctions.TruncateTime(b.Ngay) == today);
                Debug.WriteLine($"CreateAttendanceIfNeeded: exists={exists}");
                if (!exists)
                {
                    var code = GenerateNextMaBCC();
                    Debug.WriteLine($"CreateAttendanceIfNeeded: generating MaBCC={code}");
                        var newBc = new BangChamCong
                        {
                            MaBCC = code,
                            MaNV = CurrentAccount.MaNV,
                            Ngay = today,
                            GioVao = NowTimeSeconds(),
                            GioRa = null,
                            IsPay = false
                        };
                    db.BangChamCongs.Add(newBc);
                    db.SaveChanges();
                    LoadData_BangChamCong();
                    Debug.WriteLine("CreateAttendanceIfNeeded: attendance created");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo chấm công: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine("CreateAttendanceIfNeeded exception: " + ex);
            }
        }
        private static TimeSpan NowTimeSeconds()
        {
            var now = DateTime.Now;
            // Dùng HH (24 giờ) thay vì hh (12 giờ)
            return new TimeSpan(now.Hour, now.Minute, now.Second);
        }

        // Ghi giờ ra khi đăng xuất hoặc đóng ứng dụng
        public void RecordLogoutTime()
        {
            try
            {
                if (CurrentAccount == null || db == null) return;
                var today = DateTime.Today;
                var now = DateTime.Now;
                var dbItem = db.BangChamCongs.FirstOrDefault(b => b.MaNV == CurrentAccount.MaNV
                    && b.Ngay.HasValue && b.Ngay.Value.Year == today.Year
                    && b.Ngay.Value.Month == today.Month
                    && b.Ngay.Value.Day == today.Day);
                if (dbItem != null)
                {
                    Debug.WriteLine($"RecordLogoutTime: found MaBCC={dbItem.MaBCC} GioVao={dbItem.GioVao} GioRa={dbItem.GioRa}");
                    // Nếu chưa có GioRa hoặc GioRa == TimeSpan.Zero, đặt giờ ra
                    if (dbItem.GioRa == null || dbItem.GioRa == TimeSpan.Zero)
                    {
                        var ts = now.TimeOfDay;
                        var newGioRa = NowTimeSeconds();
                        dbItem.GioRa = newGioRa;
                        db.SaveChanges();
                        LoadData_BangChamCong();
                        Debug.WriteLine($"RecordLogoutTime: updated GioRa to {newGioRa}");
                    }
                    else
                    {
                        Debug.WriteLine($"RecordLogoutTime: GioRa already set to {dbItem.GioRa}");
                    }
                }
                else
                {
                    Debug.WriteLine($"RecordLogoutTime: no attendance record found for MaNV={CurrentAccount.MaNV} date={today:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RecordLogoutTime exception: " + ex);
            }
        }
        
        // Sinh mã MaBCC theo mẫu CCXXX (3 chữ số tăng dần)
        private string GenerateNextMaBCC()
        {
            if (db == null) return "CC001";
            try
            {
                var codes = db.BangChamCongs
                    .Where(b => b.MaBCC != null && b.MaBCC.StartsWith("CC"))
                    .Select(b => b.MaBCC)
                    .ToList();

                int max = 0;
                foreach (var c in codes)
                {
                    if (c.Length <= 2) continue;
                    var numPart = c.Substring(2);
                    if (int.TryParse(numPart, out int n) && n > max) max = n;
                }

                return $"CC{(max + 1):D3}";
            }
            catch
            {
                return "CC001";
            }
        }
        // ===================== DANH SÁCH =====================
        public ObservableCollection<string> DsChucVu { get; set; }
        public ObservableCollection<Ban> DsBan { get; set; }
        public ObservableCollection<Ban> DsBanMo { get; set; }
        public ObservableCollection<BangChamCong> DsBangChamCong { get; set; }
        // View model for attendance rows shown in UI
        public class ChamCongItem : INotifyPropertyChanged
        {
            private string _maBCC;
            private string _maNV;
            private string _tenNV;
            private string _chucVu;
            private DateTime _ngay;
            private double _soGio;
            private bool _isPay;
            private double _soTien;

            public string MaBCC { get => _maBCC; set { _maBCC = value; OnPropertyChanged(nameof(MaBCC)); } }
            public string MaNV { get => _maNV; set { _maNV = value; OnPropertyChanged(nameof(MaNV)); } }
            public string TenNV { get => _tenNV; set { _tenNV = value; OnPropertyChanged(nameof(TenNV)); } }
            public string ChucVu { get => _chucVu; set { _chucVu = value; OnPropertyChanged(nameof(ChucVu)); } }
            public DateTime Ngay { get => _ngay; set { _ngay = value; OnPropertyChanged(nameof(Ngay)); } }
            public double SoGio { get => _soGio; set { _soGio = value; OnPropertyChanged(nameof(SoGio)); } }
            public bool IsPay { get => _isPay; set { _isPay = value; OnPropertyChanged(nameof(IsPay)); OnPropertyChanged(nameof(TrangThaiChamCong)); } }
            public double SoTien { get => _soTien; set { _soTien = value; OnPropertyChanged(nameof(SoTien)); } }
            public string TrangThaiChamCong => IsPay ? "Đã chấm công" : "Chưa chấm công";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ObservableCollection<ChamCongItem> DsBangChamCongView { get; set; }
        private ChamCongItem _selectedChamCongItem;
        public ChamCongItem SelectedChamCongItem { get => _selectedChamCongItem; set { _selectedChamCongItem = value; OnPropertyChanged(nameof(SelectedChamCongItem)); (DeleteChamCongCommand as RelayCommand)?.RaiseCanExecuteChanged(); (PayChamCongCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }
        private BangChamCong _selectedBangChamCong;
        public BangChamCong SelectedBangChamCong
        {
            get => _selectedBangChamCong;
            set
            {
                _selectedBangChamCong = value;
                OnPropertyChanged(nameof(SelectedBangChamCong));

                if (value != null)
                {
                    var nv = DsNhanVien?
                        .FirstOrDefault(x => x.MaNV == value.MaNV);

                    MessageBox.Show(
                        $"Nhân viên: {nv?.TenNV ?? value.MaNV}\n" +
                        $"Ngày công: {value.Ngay:dd/MM/yyyy}",
                        "Chi tiết chấm công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }
        public ObservableCollection<HoaDon> DsHoaDon { get; set; }
        public ObservableCollection<ChiTietHoaDon> DsChiTietHoaDon { get; set; }
        public ObservableCollection<PhieuNhap> DsPhieuNhap { get; set; }
        public ObservableCollection<HoaDon> FilteredHoaDon { get; set; }
        public ObservableCollection<KhachHang> DsKhachHang { get; set; }
        public ObservableCollection<NhanVien> DsNhanVien { get; set; }
        public ObservableCollection<SanPham> DsSanPham { get; set; }
        // ===================== KHO =====================
        // View model cho sản phẩm trong kho
        public class ProductItem
        {
            public string Ma { get; set; }
            public string TenSanPham { get; set; }
            public int TonDu { get; set; }
            public string DonVi { get; set; }
            public SanPham Source { get; set; }
        }
        public ObservableCollection<ProductItem> FilteredSanPham { get; set; }
        public ObservableCollection<Kho> DsKho { get; set; }
        public ObservableCollection<string> DsNhomSanPham { get; set; }
        
        // MENU / ORDER UX
        public ObservableCollection<SanPham> MenuFilteredSanPham { get; set; }
        private SanPham _selectedMenuProduct;
        public SanPham SelectedMenuProduct
        {
            get => _selectedMenuProduct;
            set
            {
                _selectedMenuProduct = value;
                OnPropertyChanged(nameof(SelectedMenuProduct));
                (DeleteProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ViewDetailProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
                UpdateSelectedMenuProductTonKho();
            }
        }

        private int _selectedMenuProductTonKho;
        public int SelectedMenuProductTonKho
        {
            get => _selectedMenuProductTonKho;
            set { _selectedMenuProductTonKho = value; OnPropertyChanged(nameof(SelectedMenuProductTonKho)); }
        }

        private string _menuSearchText;
        public string MenuSearchText
        {
            get => _menuSearchText;
            set { _menuSearchText = value; OnPropertyChanged(nameof(MenuSearchText)); ApplyMenuFilter(); }
        }
        private string _menuSortOrder;
        public string MenuSortOrder
        {
            get => _menuSortOrder;
            set { _menuSortOrder = value; OnPropertyChanged(nameof(MenuSortOrder)); ApplyMenuFilter(); }
        }

        // ===================== NHÂ N VIÊN SEARCH =====================
        private string _searchNhanVien;
        public string SearchNhanVien
        {
            get => _searchNhanVien;
            set { _searchNhanVien = value; OnPropertyChanged(nameof(SearchNhanVien)); ApplyNhanVienFilter(); }
        }

        public ObservableCollection<NhanVien> FilteredNhanVien { get; set; }

        private void UpdateSelectedMenuProductTonKho()
        {
            if (SelectedMenuProduct == null)
            {
                SelectedMenuProductTonKho = 0;
                return;
            }
            var kho = DsKho?.FirstOrDefault(k => k.MaSP == SelectedMenuProduct.Ma);
            SelectedMenuProductTonKho = kho?.TonKho ?? 0;
        }

        private Ban _selectedBanForOrder;
        public Ban SelectedBanForOrder
        {
            get => _selectedBanForOrder;
            set { _selectedBanForOrder = value; OnPropertyChanged(nameof(SelectedBanForOrder)); (AddToOrderCommand as RelayCommand)?.RaiseCanExecuteChanged(); (PlaceOrderCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        public class OrderLine : System.ComponentModel.INotifyPropertyChanged
        {
            public SanPham Product { get; set; }
            private int _quantity;
            public int Quantity
            {
                get => _quantity;
                set
                {
                    if (_quantity != value)
                    {
                        _quantity = value;
                        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Quantity)));
                        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(LineTotal)));
                    }
                }
            }
            public double LineTotal => (Product?.DonGia ?? 0) * Quantity;
            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        }

        public ObservableCollection<OrderLine> SelectedOrderItems { get; set; } = new ObservableCollection<OrderLine>();
        public ICommand AddToOrderCommand { get; set; }
        public ICommand PlaceOrderCommand { get; set; }
        public ICommand ClearOrderCommand { get; set; }
        public ICommand IncreaseQtyCommand { get; set; }
        public ICommand DecreaseQtyCommand { get; set; }
        public ICommand RemoveOrderLineCommand { get; set; }
        public double CurrentOrderTotal => SelectedOrderItems.Sum(x => x.LineTotal);

        private string _searchProductText;
        public string SearchProductText
        {
            get => _searchProductText;
            set
            {
                _searchProductText = value;
                OnPropertyChanged(nameof(SearchProductText));
                ApplyProductFilter();
            }
        }

        private ProductItem _selectedSanPhamKho;
        public ProductItem SelectedSanPhamKho
        {
            get => _selectedSanPhamKho;
            set
            {
                _selectedSanPhamKho = value;
                OnPropertyChanged(nameof(SelectedSanPhamKho));
                (ViewDetailProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _selectedNhom;
        public string SelectedNhom
        {
            get => _selectedNhom;
            set
            {
                _selectedNhom = value;
                OnPropertyChanged(nameof(SelectedNhom));
                ApplyProductFilter();
            }
        }

        // Commands KHO
        public ICommand CheckStockCommand { get; set; }
        public ICommand ExportProductsCommand { get; set; }
        public ICommand DeleteProductCommand { get; set; }
        public ICommand ViewDetailProductCommand { get; set; }
        public ICommand ViewKhoDetailCommand { get; set; }

        // Kho (warehouse) - add new entry (Phiếu nhập mới)
        public ICommand NewKhoCommand { get; set; }
        public ICommand ConfirmNewKhoCommand { get; set; }
        public ICommand CancelNewKhoCommand { get; set; }

        private bool _isAddingKho;
        public bool IsAddingKho { get => _isAddingKho; set { _isAddingKho = value; OnPropertyChanged(nameof(IsAddingKho)); OnPropertyChanged(nameof(VisibileAddKho)); } }
        public Visibility VisibileAddKho => IsAddingKho ? Visibility.Visible : Visibility.Collapsed;

        private string _newKhoMaSP;
        private string _newKhoTenSanPham;
        private int _newKhoSoLuong;
        private string _newKhoDonViTinh;
        private DateTime? _newKhoNgayNhap;
        private string _newKhoMaSPKho;
        private string _newKhoGiaNhapText;
        private SanPham _newKhoSelectedSanPham;

        public string NewKhoMaSP { get => _newKhoMaSP; set { _newKhoMaSP = value; OnPropertyChanged(nameof(NewKhoMaSP)); } }
        public string NewKhoTenSanPham { get => _newKhoTenSanPham; set { _newKhoTenSanPham = value; OnPropertyChanged(nameof(NewKhoTenSanPham)); } }
        public int NewKhoSoLuong { get => _newKhoSoLuong; set { _newKhoSoLuong = value; OnPropertyChanged(nameof(NewKhoSoLuong)); (ConfirmNewKhoCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }
        public string NewKhoDonViTinh { get => _newKhoDonViTinh; set { _newKhoDonViTinh = value; OnPropertyChanged(nameof(NewKhoDonViTinh)); } }
        public DateTime? NewKhoNgayNhap { get => _newKhoNgayNhap; set { _newKhoNgayNhap = value; OnPropertyChanged(nameof(NewKhoNgayNhap)); (ConfirmNewKhoCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }
        public string NewKhoMaSPKho { get => _newKhoMaSPKho; set { _newKhoMaSPKho = value; OnPropertyChanged(nameof(NewKhoMaSPKho)); } }
        public string NewKhoGiaNhapText { get => _newKhoGiaNhapText; set { _newKhoGiaNhapText = value; OnPropertyChanged(nameof(NewKhoGiaNhapText)); } }
        public SanPham NewKhoSelectedSanPham 
        { 
            get => _newKhoSelectedSanPham; 
            set 
            { 
                _newKhoSelectedSanPham = value; 
                OnPropertyChanged(nameof(NewKhoSelectedSanPham)); 
                (ConfirmNewKhoCommand as RelayCommand)?.RaiseCanExecuteChanged(); 
            } 
        }

        // Add product (Admin) overlay state and fields
        private bool _isAddingProduct;
        public bool IsAddingProduct
        {
            get => _isAddingProduct;
            set { _isAddingProduct = value; OnPropertyChanged(nameof(IsAddingProduct)); OnPropertyChanged(nameof(VisibileAddProduct)); }
        }
        public Visibility VisibileAddProduct => IsAddingProduct ? Visibility.Visible : Visibility.Collapsed;

        private string _newProductMa;
        private string _newProductTen;
        private double? _newProductDonGia;
        private string _newProductHinhAnh;

        public string NewProductMa { get => _newProductMa; set { _newProductMa = value; OnPropertyChanged(nameof(NewProductMa)); } }
        public string NewProductTen { get => _newProductTen; set { _newProductTen = value; OnPropertyChanged(nameof(NewProductTen)); (ConfirmAddProductCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }
        public double? NewProductDonGia { get => _newProductDonGia; set { _newProductDonGia = value; OnPropertyChanged(nameof(NewProductDonGia)); (ConfirmAddProductCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }
        public string NewProductHinhAnh { get => _newProductHinhAnh; set { _newProductHinhAnh = value; OnPropertyChanged(nameof(NewProductHinhAnh)); } }

        public ICommand AddImageCommand { get; set; }
        public ICommand ConfirmAddProductCommand { get; set; }
        public ICommand CancelAddProductCommand { get; set; }
        public ICommand NewProductCommand { get; set; }

        private Kho _selectedKhoForNewProduct;
        public Kho SelectedKhoForNewProduct
        {
            get => _selectedKhoForNewProduct;
            set
            {
                _selectedKhoForNewProduct = value;
                if (value != null && string.IsNullOrWhiteSpace(NewProductTen))
                {
                    NewProductTen = value.TenSanPham;
                }
                OnPropertyChanged(nameof(SelectedKhoForNewProduct));
                (ConfirmAddProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Edit product (admin)
        private bool _isEditingProduct;
        public bool IsEditingProduct
        {
            get => _isEditingProduct;
            set { _isEditingProduct = value; OnPropertyChanged(nameof(IsEditingProduct)); OnPropertyChanged(nameof(VisibileEditProduct)); }
        }
        public Visibility VisibileEditProduct => IsEditingProduct ? Visibility.Visible : Visibility.Collapsed;

        private string _editProductMa;
        private string _editProductTen;
        private double? _editProductDonGia;
        private string _editProductHinhAnh;

        public string EditProductMa { get => _editProductMa; set { _editProductMa = value; OnPropertyChanged(nameof(EditProductMa)); } }
        public string EditProductTen { get => _editProductTen; set { _editProductTen = value; OnPropertyChanged(nameof(EditProductTen)); (SaveEditProductCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }
        public double? EditProductDonGia { get => _editProductDonGia; set { _editProductDonGia = value; OnPropertyChanged(nameof(EditProductDonGia)); (SaveEditProductCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }
        public string EditProductHinhAnh { get => _editProductHinhAnh; set { _editProductHinhAnh = value; OnPropertyChanged(nameof(EditProductHinhAnh)); } }

        public ICommand SaveEditProductCommand { get; set; }
        public ICommand CancelEditProductCommand { get; set; }
        public ICommand ChangeEditImageCommand { get; set; }

        // ===================== BÀN =====================
        //Ban-Tran Nguyen - BEGIN

        // --- Danh sách bàn hiển thị sau lọc ---
        private ObservableCollection<Ban> _dsBanHienThi;
        public ObservableCollection<Ban> DsBanHienThi
        {
            get => _dsBanHienThi;
            set { _dsBanHienThi = value; OnPropertyChanged(nameof(DsBanHienThi)); }
        }

        // --- Bộ lọc bàn ---
        public ObservableCollection<string> DsLocBan { get; set; }
            = new ObservableCollection<string> { "Tất cả", "Bàn trống", "Có khách" };

        private string _filterBan = "Tất cả";
        public string FilterBan
        {
            get => _filterBan;
            set { _filterBan = value; OnPropertyChanged(nameof(FilterBan)); ApplyFilterBan(); }
        }

        // --- Bàn đang chọn ---
        private Ban _selectedBan;
        public Ban SelectedBan
        {
            get => _selectedBan;
            set
            {
                _selectedBan = value;

                OnPropertyChanged(nameof(SelectedBan));
                OnPropertyChanged(nameof(TrangThaiHienTai));

                LoadChiTietBan();

                OnPropertyChanged(nameof(DsMonAnCuaBan));
                OnPropertyChanged(nameof(TongTienHienTai));
                OnPropertyChanged(nameof(GioVaoBan));
            }
        }

        // --- Thông tin chi tiết bàn đang chọn ---
        public string TrangThaiHienTai
        {
            get
            {
                if (SelectedBan == null) return "---";
                return SelectedBan.TrangThaiBan == true ? "Có khách" : "Trống";
            }
        }

        private string _gioVaoBan = "--:--";
        public string GioVaoBan
        {
            get => _gioVaoBan;
            set { _gioVaoBan = value; OnPropertyChanged(nameof(GioVaoBan)); }
        }

        private double _tongTienHienTai;
        public double TongTienHienTai
        {
            get => _tongTienHienTai;
            set { _tongTienHienTai = value; OnPropertyChanged(nameof(TongTienHienTai)); }
        }

        // ===== Tiền bàn và chiết khấu (cho hiển thị chi tiết) =====
        private double _tienBan;
        public double TienBan
        {
            get => _tienBan;
            set { _tienBan = value; OnPropertyChanged(nameof(TienBan)); }
        }

        private double _tongTienTruocGiamGia;
        public double TongTienTruocGiamGia
        {
            get => _tongTienTruocGiamGia;
            set { _tongTienTruocGiamGia = value; OnPropertyChanged(nameof(TongTienTruocGiamGia)); }
        }

        private double _soTienGiamGia;
        public double SoTienGiamGia
        {
            get => _soTienGiamGia;
            set { _soTienGiamGia = value; OnPropertyChanged(nameof(SoTienGiamGia)); }
        }

        private string _loaiHoiVien;
        public string LoaiHoiVien
        {
            get => _loaiHoiVien;
            set { _loaiHoiVien = value; OnPropertyChanged(nameof(LoaiHoiVien)); }
        }

        private ObservableCollection<MonAnChiTiet> _dsMonAnCuaBan;
        public ObservableCollection<MonAnChiTiet> DsMonAnCuaBan
        {
            get => _dsMonAnCuaBan;
            set { _dsMonAnCuaBan = value; OnPropertyChanged(nameof(DsMonAnCuaBan)); }
        }

        // --- Overlay Đặt Bàn ---
        private bool _isShowDatBanOverlay;
        public bool IsShowDatBanOverlay
        {
            get => _isShowDatBanOverlay;
            set { _isShowDatBanOverlay = value; OnPropertyChanged(nameof(IsShowDatBanOverlay)); }
        }

        private int _activeOverlayTab;
        public int ActiveOverlayTab
        {
            get => _activeOverlayTab;
            set { _activeOverlayTab = value; OnPropertyChanged(nameof(ActiveOverlayTab)); }
        }

        // --- Thông tin khách hàng trong overlay ---
        private string _datBan_MaKH;
        public string DatBan_MaKH
        {
            get => _datBan_MaKH;
            set { _datBan_MaKH = value; OnPropertyChanged(nameof(DatBan_MaKH)); }
        }

        private string _datBan_TenKH;
        public string DatBan_TenKH
        {
            get => _datBan_TenKH;
            set { _datBan_TenKH = value; OnPropertyChanged(nameof(DatBan_TenKH)); }
        }

        private string _datBan_SDT;
        public string DatBan_SDT
        {
            get => _datBan_SDT;
            set 
            { 
                _datBan_SDT = value; 
                OnPropertyChanged(nameof(DatBan_SDT)); 
                // Chỉ gọi tìm kiếm khi người dùng gõ, không phải khi chọn từ DataGrid
                if (!_isSelectingFromDataGrid)
                {
                    TimKhachHangBySdt();
                    LocDsKhachHangBySdt();
                }
            }
        }

        private int _datBan_DiemTichLuy;
        public int DatBan_DiemTichLuy
        {
            get => _datBan_DiemTichLuy;
            set { _datBan_DiemTichLuy = value; OnPropertyChanged(nameof(DatBan_DiemTichLuy)); }
        }

        private string _thongBaoTimKH = "";
        public string ThongBaoTimKH
        {
            get => _thongBaoTimKH;
            set { _thongBaoTimKH = value; OnPropertyChanged(nameof(ThongBaoTimKH)); }
        }

        // --- Danh sách khách hàng được lọc theo SĐT ---
        private ObservableCollection<KhachHang> _filteredDsKhachHang;
        public ObservableCollection<KhachHang> FilteredDsKhachHang
        {
            get => _filteredDsKhachHang;
            set { _filteredDsKhachHang = value; OnPropertyChanged(nameof(FilteredDsKhachHang)); }
        }

        // --- Loại khách hàng (Hội viên / Khách thường) ---
        private bool _isHoiVien = false;
        public bool IsHoiVien
        {
            get => _isHoiVien;
            set
            {
                if (_isHoiVien == value) return;
                _isHoiVien = value;
                OnPropertyChanged(nameof(IsHoiVien));
                OnPropertyChanged(nameof(IsKhachThuong));
                // KHÔNG reset TenKH / SDT khi switch để tránh mất dữ liệu đang nhập
            }
        }
        public bool IsKhachThuong
        {
            get => !_isHoiVien;
            set { IsHoiVien = !value; }
        }

        // --- Khách hàng được chọn từ DataGrid trong overlay đặt bàn ---
        private bool _isSelectingFromDataGrid = false;
        private KhachHang _selectedKhachHangDatBan;
        public KhachHang SelectedKhachHangDatBan
        {
            get => _selectedKhachHangDatBan;
            set 
            { 
                _selectedKhachHangDatBan = value; 
                OnPropertyChanged(nameof(SelectedKhachHangDatBan));
                if (value != null)
                {
                    // Auto-populate fields khi chọn khách hàng từ danh sách
                    SelectKhachHangDatBan(value);
                }
            }
        }

        // --- Chọn sản phẩm để thêm vào đơn ---
        private SanPham _selectedSanPham;
        public SanPham SelectedSanPham
        {
            get => _selectedSanPham;
            set { _selectedSanPham = value; OnPropertyChanged(nameof(SelectedSanPham)); }
        }

        private int _soLuongDat = 1;
        public int SoLuongDat
        {
            get => _soLuongDat;
            set { _soLuongDat = value < 1 ? 1 : value; OnPropertyChanged(nameof(SoLuongDat)); }
        }

        // --- Danh sách món trong đơn đặt ---
        private ObservableCollection<MonAnChiTiet> _dsDatMon;
        public ObservableCollection<MonAnChiTiet> DsDatMon
        {
            get => _dsDatMon;
            set { _dsDatMon = value; OnPropertyChanged(nameof(DsDatMon)); }
        }

        private MonAnChiTiet _selectedDatMon;
        public MonAnChiTiet SelectedDatMon
        {
            get => _selectedDatMon;
            set { _selectedDatMon = value; OnPropertyChanged(nameof(SelectedDatMon)); }
        }

        private double _tongTienDatMon;
        public double TongTienDatMon
        {
            get => _tongTienDatMon;
            set { _tongTienDatMon = value; OnPropertyChanged(nameof(TongTienDatMon)); }
        }

        // --- Commands BÀN ---
        public ICommand DatBanCommand { get; set; }
        public ICommand DongOverlayCommand { get; set; }
        public ICommand ThemMonCommand { get; set; }
        public ICommand XoaMonCommand { get; set; }
        public ICommand XacNhanDatMonCommand { get; set; }
        public ICommand XuatHoaDonCommand { get; set; }
        public ICommand ThanhToanCommand { get; set; }
        public ICommand TangSoLuongCommand { get; set; }
        public ICommand GiamSoLuongCommand { get; set; }
        public ICommand XoaMonTrenBanCommand { get; set; }
        public ICommand TangSoLuongMonTrenBanCommand { get; set; }
        public ICommand GiamSoLuongMonTrenBanCommand { get; set; }
        // Dùng Command thay RadioButton — tránh lỗi bàn phím tự nhảy
        public ICommand SetHoiVienCommand { get; set; }
        public ICommand SetKhachThuongCommand { get; set; }
        public ICommand SetTabKhachHangCommand { get; set; }
        public ICommand SetTabDatMonCommand { get; set; }

        // ===================== LOGIC BÀN =====================

        private void ApplyFilterBan()
        {
            if (DsBan == null) return;
            IEnumerable<Ban> filtered;
            switch (FilterBan)
            {
                case "Bàn trống": filtered = DsBan.Where(b => b.TrangThaiBan != true); break;
                case "Có khách": filtered = DsBan.Where(b => b.TrangThaiBan == true); break;
                default: filtered = DsBan; break;
            }
            DsBanHienThi = new ObservableCollection<Ban>(filtered);
        }

        private void RefreshBanRealtime(string maBan)
        {
            if (DsBan == null || db == null)
                return;

            var updated = db.Bans.Find(maBan);

            if (updated != null)
            {
                for (int i = 0; i < DsBan.Count; i++)
                {
                    if (DsBan[i].MaBan == maBan)
                    {
                        DsBan[i] = updated;
                        break;
                    }
                }
            }

            ApplyFilterBan();

            var reselect = DsBanHienThi.FirstOrDefault(b => b.MaBan == maBan);

            if (reselect != null)
            {
                // QUAN TRỌNG:
                // Phải set qua property để setter chạy
                SelectedBan = reselect;

                // Reload dữ liệu bàn
                LoadChiTietBan();

                OnPropertyChanged(nameof(DsMonAnCuaBan));
                OnPropertyChanged(nameof(TongTienHienTai));
                OnPropertyChanged(nameof(GioVaoBan));
                OnPropertyChanged(nameof(TrangThaiHienTai));
            }
        }

        private void LoadChiTietBan()
        {
            if (SelectedBan == null || db == null)
            {
                DsMonAnCuaBan = new ObservableCollection<MonAnChiTiet>();
                TongTienHienTai = 0; 
                TienBan = 0;
                TongTienTruocGiamGia = 0;
                SoTienGiamGia = 0;
                LoaiHoiVien = "Regular";
                GioVaoBan = "--:--"; 
                return;
            }
            var hd = DsHoaDon?.FirstOrDefault(h =>
                h.MaBan == SelectedBan.MaBan && h.TrangThaiThanhToan == false);
            if (hd == null)
            {
                DsMonAnCuaBan = new ObservableCollection<MonAnChiTiet>();
                TongTienHienTai = 0;
                TienBan = 0;
                TongTienTruocGiamGia = 0;
                SoTienGiamGia = 0;
                LoaiHoiVien = "Regular";
                GioVaoBan = "--:--"; 
                return;
            }
            GioVaoBan = hd.GioVaoHoaDon.HasValue
                ? hd.GioVaoHoaDon.Value.ToString(@"hh\:mm") : "--:--";
            
            // Tính tiền đồ ăn/uống
            var ds = (DsChiTietHoaDon?
                .Where(ct => ct.MaHoaDon == hd.MaHoaDon)
                .Select(ct => {
                    var sp = DsSanPham?.FirstOrDefault(s => s.Ma == ct.MaThucDon);
                    return new MonAnChiTiet
                    {
                        MaMon = ct.MaThucDon,
                        TenMon = sp?.Ten ?? ct.MaThucDon,
                        SoLuong = ct.SoLuongChiTiet ?? 0,
                        ThanhTien = ct.GiaChiTiet ?? 0
                    };
                }).ToList()) ?? new List<MonAnChiTiet>();
            DsMonAnCuaBan = new ObservableCollection<MonAnChiTiet>(ds);

            double tongTienMon = DsMonAnCuaBan.Sum(m => m.ThanhTien);
            
            // Tính tiền bàn (dùng giờ hiện tại làm giờ ra)
            var gioRaHienTai = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            TienBan = hd.GioVaoHoaDon.HasValue ? 
                BilliardCalculator.CalculateTableFee(hd.GioVaoHoaDon, gioRaHienTai) : 0;

            // Tính tổng tiền trước giảm giá
            TongTienTruocGiamGia = tongTienMon + TienBan;

            // Tính giảm giá từ điểm tích lũy
            int loyaltyPoints = (hd.KhachHang?.DiemTichLuy ?? 0);
            SoTienGiamGia = BilliardCalculator.CalculateDiscount(TongTienTruocGiamGia, loyaltyPoints);
            LoaiHoiVien = BilliardCalculator.GetMembershipTier(loyaltyPoints);

            // Tính tổng tiền sau giảm giá
            TongTienHienTai = TongTienTruocGiamGia - SoTienGiamGia;

            OnPropertyChanged(nameof(DsMonAnCuaBan));
            OnPropertyChanged(nameof(TongTienHienTai));
            OnPropertyChanged(nameof(GioVaoBan));
        }

        private void TimKhachHangBySdt()
        {
            if (string.IsNullOrWhiteSpace(DatBan_SDT)) { ThongBaoTimKH = ""; return; }
            var kh = DsKhachHang?.FirstOrDefault(k => k.SoDienThoai == DatBan_SDT);
            if (kh != null)
            {
                // Tìm thấy khách hàng - update thông tin
                DatBan_MaKH = kh.MaKhachHang; 
                DatBan_TenKH = kh.TenKhachHang;
                DatBan_DiemTichLuy = kh.DiemTichLuy ?? 0;
                // Khách hàng có SĐT trong hệ thống luôn là hội viên
                IsHoiVien = true;
                ThongBaoTimKH = $"✅ Khách cũ: {kh.TenKhachHang}  |  Điểm TL: {kh.DiemTichLuy}  |  Hội viên";
            }
            else
            {
                DatBan_MaKH = GenMaKH(); 
                DatBan_TenKH = ""; 
                DatBan_DiemTichLuy = 0;
                // Khách mới sẽ là hội viên (nếu có SĐT sẽ được lưu vào hệ thống)
                IsHoiVien = true;
                ThongBaoTimKH = "🆕 Khách mới — sẽ được tạo khi xác nhận";
            }
        }

        /// <summary>
        /// Xử lý khi chọn khách hàng từ DataGrid - populate toàn bộ thông tin
        /// </summary>
        private void SelectKhachHangDatBan(KhachHang khachHang)
        {
            if (khachHang == null) return;

            try
            {
                _isSelectingFromDataGrid = true; // Ngăn chặn vòng lặp

                DatBan_MaKH = khachHang.MaKhachHang;
                DatBan_TenKH = khachHang.TenKhachHang;
                DatBan_SDT = khachHang.SoDienThoai ?? "";
                DatBan_DiemTichLuy = khachHang.DiemTichLuy ?? 0;
                
                // KHÔNG tự động thay đổi IsHoiVien - giữ nguyên lựa chọn người dùng
                // Khách hàng có SĐT trong DB luôn là "Hội viên" (có thể không có điểm tích lũy)
                // Chỉ khách hàng được tạo tại thời điểm đặt bàn (nhập thông tin mới) mới là "Khách thường"
                IsHoiVien = true;  // Khách hàng được chọn từ danh sách luôn là hội viên
                
                ThongBaoTimKH = $"✅ Chọn khách hàng: {khachHang.TenKhachHang}  |  Điểm TL: {khachHang.DiemTichLuy ?? 0}  |  Hội viên";
            }
            finally
            {
                _isSelectingFromDataGrid = false;
            }
        }

        /// <summary>
        /// Lọc danh sách khách hàng theo SĐT được nhập
        /// </summary>
        private void LocDsKhachHangBySdt()
        {
            try
            {
                if (DsKhachHang == null || DsKhachHang.Count == 0)
                {
                    FilteredDsKhachHang = new ObservableCollection<KhachHang>();
                    return;
                }

                if (string.IsNullOrWhiteSpace(DatBan_SDT))
                {
                    // Nếu SĐT trống, hiển thị toàn bộ danh sách
                    FilteredDsKhachHang = new ObservableCollection<KhachHang>(DsKhachHang);
                }
                else
                {
                    // Lọc danh sách: khách hàng có SĐT chứa chuỗi được nhập (không phân biệt hoa/thường)
                    var filtered = DsKhachHang
                        .Where(k => !string.IsNullOrEmpty(k.SoDienThoai) && 
                                    k.SoDienThoai.Contains(DatBan_SDT.Trim()))
                        .OrderBy(k => k.SoDienThoai) // Sắp xếp để khách hàng khớp đầu tiên sẽ lên đầu
                        .ToList();
                    
                    FilteredDsKhachHang = new ObservableCollection<KhachHang>(filtered);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi lọc khách hàng: {ex.Message}");
                if (FilteredDsKhachHang == null)
                    FilteredDsKhachHang = new ObservableCollection<KhachHang>();
            }
        }

        private string GenMaKH()
        {
            int max = 0;
            if (db != null)
            {
                try
                {
                    max = db.KhachHangs?.ToList()
                        .Select(k => { int n; return int.TryParse(k.MaKhachHang?.Replace("KH", ""), out n) ? n : 0; })
                        .DefaultIfEmpty(0).Max() ?? 0;
                }
                catch
                {
                    max = DsKhachHang?
                        .Select(k => { int n; return int.TryParse(k.MaKhachHang?.Replace("KH", ""), out n) ? n : 0; })
                        .DefaultIfEmpty(0).Max() ?? 0;
                }
            }
            else
            {
                max = DsKhachHang?
                    .Select(k => { int n; return int.TryParse(k.MaKhachHang?.Replace("KH", ""), out n) ? n : 0; })
                    .DefaultIfEmpty(0).Max() ?? 0;
            }

            // Đảm bảo trả về định dạng KH và 3 chữ số lấp đầy bằng số 0 (Ví dụ: KH001, KH002)
            return $"KH{(max + 1):D3}";
        }

        private string GenMaPN()
        {
            int max = 0;
            try
            {
                if (db != null)
                {
                    max = db.PhieuNhaps?.ToList()
                        .Select(p => { int n; return int.TryParse(p.MaPN?.Replace("PN", ""), out n) ? n : 0; })
                        .DefaultIfEmpty(0).Max() ?? 0;
                }
                else
                {
                    max = DsPhieuNhap?.Select(p => { int n; return int.TryParse(p.MaPN?.Replace("PN", ""), out n) ? n : 0; })
                        .DefaultIfEmpty(0).Max() ?? 0;
                }
            }
            catch
            {
                max = DsPhieuNhap?.Select(p => { int n; return int.TryParse(p.MaPN?.Replace("PN", ""), out n) ? n : 0; })
                    .DefaultIfEmpty(0).Max() ?? 0;
            }
            return $"PN{(max + 1):D3}";
        }

        private string GenMaHD()
        {
            // Tính max từ database thay vì từ DsHoaDon để tránh trùng mã
            int max = 0;
            if (db != null)
            {
                try
                {
                    max = db.HoaDons?.ToList()
                        .Select(h => { int n; return int.TryParse(h.MaHoaDon?.Replace("HD", ""), out n) ? n : 0; })
                        .DefaultIfEmpty(0).Max() ?? 0;
                }
                catch
                {
                    // Nếu có lỗi, fallback về DsHoaDon
                    max = DsHoaDon?
                        .Select(h => { int n; return int.TryParse(h.MaHoaDon?.Replace("HD", ""), out n) ? n : 0; })
                        .DefaultIfEmpty(0).Max() ?? 0;
                }
            }
            else
            {
                max = DsHoaDon?
                    .Select(h => { int n; return int.TryParse(h.MaHoaDon?.Replace("HD", ""), out n) ? n : 0; })
                    .DefaultIfEmpty(0).Max() ?? 0;
            }

            // Sửa :D2 thành :D3 để sinh ra định dạng HD001, HD002,...
            return $"HD{(max + 1):D3}";
        }

        private void TinhTongDatMon() => TongTienDatMon = DsDatMon?.Sum(m => m.ThanhTien) ?? 0;

        private void ExecDatBan(object obj)
        {
            if (SelectedBan == null)
            { MessageBox.Show("Vui lòng chọn bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (SelectedBan.TrangThaiBan == true)
            { MessageBox.Show($"{SelectedBan.TenBan} đang có khách!", "Bàn bận", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            DsDatMon = new ObservableCollection<MonAnChiTiet>(); TongTienDatMon = 0;
            SoLuongDat = 1; SelectedSanPham = null; SelectedDatMon = null; ActiveOverlayTab = 0;
            DatBan_MaKH = GenMaKH(); DatBan_TenKH = ""; DatBan_SDT = ""; DatBan_DiemTichLuy = 0; ThongBaoTimKH = ""; 
            IsHoiVien = false;
            SelectedKhachHangDatBan = null; // Reset selected customer
            // Đảm bảo FilteredDsKhachHang được khởi tạo
            if (FilteredDsKhachHang == null)
                FilteredDsKhachHang = new ObservableCollection<KhachHang>(DsKhachHang ?? new ObservableCollection<KhachHang>());
            else
                FilteredDsKhachHang = new ObservableCollection<KhachHang>(DsKhachHang ?? new ObservableCollection<KhachHang>());
            IsShowDatBanOverlay = true;
        }

        private void ExecDongOverlay(object obj) => IsShowDatBanOverlay = false;

        private void ExecThemMon(object obj)
        {
            if (SelectedSanPham == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double donGia = SelectedSanPham.DonGia ?? 0;

            // ==========================================
            // CHỈ THÊM VÀO DANH SÁCH ĐẶT MÓN TẠM (DsDatMon)
            // ==========================================
            var existing = DsDatMon.FirstOrDefault(m => m.MaMon == SelectedSanPham.Ma);

            if (existing != null)
            {
                existing.SoLuong += SoLuongDat;
                existing.ThanhTien = existing.SoLuong * donGia;
            }
            else
            {
                var monMoi = new MonAnChiTiet
                {
                    MaMon = SelectedSanPham.Ma,
                    TenMon = SelectedSanPham.Ten,
                    SoLuong = SoLuongDat,
                    ThanhTien = SoLuongDat * donGia
                };
                DsDatMon.Add(monMoi);
            }

            // XÓA BỎ HOÀN TOÀN ĐOẠN CODE "DsMonAnCuaBan.FirstOrDefault..." Ở ĐÂY LÀ ĐƯỢC!

            // Cập nhật hiển thị tổng tiền trên form đặt món tạm thời
            TinhTongDatMon();
            SoLuongDat = 1;
        }

        private void ExecXoaMon(object obj)
        {
            if (SelectedDatMon == null)
            { MessageBox.Show("Vui lòng chọn món cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            DsDatMon.Remove(SelectedDatMon); SelectedDatMon = null; TinhTongDatMon();
        }

        private void ExecXoaMonTrenBan(object obj)
        {
            if (SelectedDatMon == null)
            {
                MessageBox.Show("Vui lòng chọn món cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (db == null) { MessageBox.Show("Lỗi: Không kết nối database!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                
                // Lấy hóa đơn của bàn hiện tại
                var hd = DsHoaDon?.FirstOrDefault(h => h.MaBan == SelectedBan.MaBan && h.TrangThaiThanhToan == false);
                if (hd == null) { MessageBox.Show("Không tìm thấy hóa đơn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                
                // Lấy chi tiết hóa đơn tương ứng
                var chiTiet = DsChiTietHoaDon?.FirstOrDefault(ct => ct.MaHoaDon == hd.MaHoaDon && ct.MaThucDon == SelectedDatMon.MaMon);
                if (chiTiet == null) { MessageBox.Show("Không tìm thấy chi tiết món!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                
                // Xóa từ database
                db.ChiTietHoaDons.Remove(chiTiet);
                db.SaveChanges();
                
                // Xóa từ collection UI
                DsMonAnCuaBan.Remove(SelectedDatMon);
                DsChiTietHoaDon.Remove(chiTiet);
                
                // Cập nhật tổng tiền
                LoadChiTietBan();
                SelectedDatMon = null;
                
                MessageBox.Show("Xóa món thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa món: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecTangSoLuongMonTrenBan(object obj)
        {
            if (SelectedDatMon == null)
            {
                MessageBox.Show("Vui lòng chọn món để tăng số lượng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (db == null) { MessageBox.Show("Lỗi: Không kết nối database!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                
                // Lấy hóa đơn của bàn hiện tại
                var hd = DsHoaDon?.FirstOrDefault(h => h.MaBan == SelectedBan.MaBan && h.TrangThaiThanhToan == false);
                if (hd == null) { MessageBox.Show("Không tìm thấy hóa đơn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                
                // Lấy chi tiết hóa đơn tương ứng
                var chiTiet = DsChiTietHoaDon?.FirstOrDefault(ct => ct.MaHoaDon == hd.MaHoaDon && ct.MaThucDon == SelectedDatMon.MaMon);
                if (chiTiet == null) { MessageBox.Show("Không tìm thấy chi tiết món!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                
                // Tính đơn giá
                var mon = SelectedDatMon;
                double donGia = mon.ThanhTien / mon.SoLuong;
                
                // Cập nhật trên database
                chiTiet.SoLuongChiTiet += 1;
                chiTiet.GiaChiTiet = chiTiet.SoLuongChiTiet * donGia;
                db.SaveChanges();
                
                // Cập nhật trên UI
                mon.SoLuong += 1;
                mon.ThanhTien = mon.SoLuong * donGia;
                
                // Tải lại để cập nhật tất cả tiền (bàn + cộng điểm...)
                LoadChiTietBan();
                
                MessageBox.Show("Tăng số lượng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tăng số lượng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecGiamSoLuongMonTrenBan(object obj)
        {
            if (SelectedDatMon == null)
            {
                MessageBox.Show("Vui lòng chọn món để giảm số lượng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var mon = SelectedDatMon;
                if (mon.SoLuong <= 1)
                {
                    MessageBox.Show("Không thể giảm số lượng dưới 1. Hãy xóa món nếu muốn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (db == null) { MessageBox.Show("Lỗi: Không kết nối database!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                
                // Lấy hóa đơn của bàn hiện tại
                var hd = DsHoaDon?.FirstOrDefault(h => h.MaBan == SelectedBan.MaBan && h.TrangThaiThanhToan == false);
                if (hd == null) { MessageBox.Show("Không tìm thấy hóa đơn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                
                // Lấy chi tiết hóa đơn tương ứng
                var chiTiet = DsChiTietHoaDon?.FirstOrDefault(ct => ct.MaHoaDon == hd.MaHoaDon && ct.MaThucDon == SelectedDatMon.MaMon);
                if (chiTiet == null) { MessageBox.Show("Không tìm thấy chi tiết món!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                
                // Tính đơn giá
                double donGia = mon.ThanhTien / mon.SoLuong;
                
                // Cập nhật trên database
                chiTiet.SoLuongChiTiet -= 1;
                chiTiet.GiaChiTiet = chiTiet.SoLuongChiTiet * donGia;
                db.SaveChanges();
                
                // Cập nhật trên UI
                mon.SoLuong -= 1;
                mon.ThanhTien = mon.SoLuong * donGia;
                
                // Tải lại để cập nhật tất cả tiền (bàn + cộng điểm...)
                LoadChiTietBan();
                
                MessageBox.Show("Giảm số lượng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi giảm số lượng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool _isProcessing = false; // Prevent double-click

        private void ExecXacNhanDatMon(object obj)
        {
            if (_isProcessing) return; // Prevent double execution
            _isProcessing = true;

            // Khách thường: không bắt buộc tên và món
            // Hội viên: bắt buộc Tên + SĐT, món vẫn không bắt buộc
            if (IsHoiVien)
            {
                if (string.IsNullOrWhiteSpace(DatBan_TenKH))
                { 
                    _isProcessing = false;
                    MessageBox.Show("Vui lòng nhập họ và tên khách hàng!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); 
                    ActiveOverlayTab = 0; 
                    return; 
                }
                if (string.IsNullOrWhiteSpace(DatBan_SDT))
                { 
                    _isProcessing = false;
                    MessageBox.Show("Hội viên cần nhập số điện thoại!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); 
                    ActiveOverlayTab = 0; 
                    return; 
                }
            }
            // Không còn bắt buộc phải có món — khách có thể chỉ chơi, không gọi món
            if (db == null)
            {
                _isProcessing = false;
                return;
            }
            try
            {
                // Hội viên: tìm/tạo KhachHang có SĐT, tích điểm sau khi thanh toán
                // Khách thường: tạo KhachHang tạm (TenKH="Khách thường", SĐT trống) để không để NULL
                KhachHang kh = null;
                if (IsHoiVien)
                {
                    kh = DsKhachHang?.FirstOrDefault(k => k.SoDienThoai == DatBan_SDT);
                    if (kh == null)
                    {
                        kh = new KhachHang
                        {
                            MaKhachHang = DatBan_MaKH,
                            TenKhachHang = DatBan_TenKH,
                            SoDienThoai = DatBan_SDT,
                            DiemTichLuy = 0
                        };
                        db.KhachHangs.Add(kh);
                        db.SaveChanges(); // ← Lưu KhachHang ngay để tránh lỗi khi tạo HoaDon
                        DsKhachHang?.Add(kh);
                    }
                }


                string maHD = GenMaHD();
                var hd = new HoaDon
                {
                    MaHoaDon = maHD,
                    MaBan = SelectedBan.MaBan,
                    MaNV = CurrentAccount?.MaNV,
                    MaKhachHang = kh?.MaKhachHang, // Nếu là khách thường, kh = null -> thuộc tính này tự động nhận giá trị null
                    NgayHoaDon = DateTime.Now,
                    GioVaoHoaDon = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second),
                    TrangThaiThanhToan = false,
                    TongTienHoaDon = TongTienDatMon
                };
                db.HoaDons.Add(hd);
                db.SaveChanges();
                DsHoaDon?.Add(hd);

                foreach (var mon in DsDatMon)
                {
                    var ct = new ChiTietHoaDon 
                    { 
                        MaHoaDon = maHD, 
                        MaThucDon = mon.MaMon, 
                        SoLuongChiTiet = mon.SoLuong, 
                        GiaChiTiet = mon.ThanhTien 
                    };
                    db.ChiTietHoaDons.Add(ct);
                }
                var banDb = db.Bans.Find(SelectedBan.MaBan);
                if (banDb != null) { banDb.TrangThaiBan = true; SelectedBan.TrangThaiBan = true; }
                db.SaveChanges();

                // ===== XÓA DỮ LIỆU ĐẶT MÓN ĐỂ TRÁNH BUG LẶP LẠI =====
                // Nếu không xóa DsDatMon, khi click button lần 2 sẽ trừ kho gấp đôi
                DsDatMon = new ObservableCollection<MonAnChiTiet>();
                TongTienDatMon = 0;

                LoadData_Kho();
                RefreshKhoGroups();
                ApplyProductFilter();

                UpdateDsBanMo();
                UpdateNgayHoaDon(maHD, hd.NgayHoaDon.Value);
                string maBan = SelectedBan.MaBan;
                IsShowDatBanOverlay = false;
                RefreshBanRealtime(maBan);
                
                // ===== RELOAD CHI TIẾT HÓA ĐƠN TỪ DATABASE =====
                LoadData_ChiTietHoaDon();
                LoadChiTietBan();
                LoadThongKe();
                _isProcessing = false;
                string loaiHienThi = IsHoiVien ? "Hội viên" : "Khách thường";
                string tenHienThi = string.IsNullOrWhiteSpace(DatBan_TenKH) ? "Khách thường" : DatBan_TenKH;
                MessageBox.Show($"✅ Đặt {SelectedBan?.TenBan ?? maBan} thành công!\nKhách: {tenHienThi} | {loaiHienThi} | Giờ vào: {DateTime.Now:HH:mm}",
                    "Đặt bàn thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                _isProcessing = false;
                string errors = string.Join("\n", dbEx.EntityValidationErrors.SelectMany(e => e.ValidationErrors.Select(v => v.PropertyName + ": " + v.ErrorMessage)));
                MessageBox.Show($"Lỗi dữ liệu:\n{errors}\n\n{dbEx.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Data.Entity.Core.UpdateException ex)
            {
                _isProcessing = false;
                MessageBox.Show($"Lỗi cập nhật database:\n{ex.InnerException?.Message ?? ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _isProcessing = false;
                MessageBox.Show($"Lỗi: {ex.InnerException?.Message ?? ex.Message}\n\n{ex.StackTrace}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); 
            }
        }

        private void ExecXuatHoaDon(object obj)
        {
            if (SelectedBan == null) { MessageBox.Show("Vui lòng chọn bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var hd = DsHoaDon?.FirstOrDefault(h => h.MaBan == SelectedBan.MaBan && h.TrangThaiThanhToan == false);
            if (hd == null) { MessageBox.Show("Bàn này chưa có hóa đơn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var kh = DsKhachHang?.FirstOrDefault(k => k.MaKhachHang == hd.MaKhachHang);
            var ctList = DsChiTietHoaDon?.Where(ct => ct.MaHoaDon == hd.MaHoaDon).ToList();

            double tongTienMon = ctList?.Sum(ct => ct.GiaChiTiet ?? 0) ?? 0;
            var gioHienTai = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            double tongTienBan = hd.GioVaoHoaDon.HasValue ? 
                BilliardCalculator.CalculateTableFee(hd.GioVaoHoaDon, gioHienTai) : 0;
            
            double tongTruocGiamGia = tongTienMon + tongTienBan;
            int loyaltyPoints = kh?.DiemTichLuy ?? 0;
            double soTienGiamGia = BilliardCalculator.CalculateDiscount(tongTruocGiamGia, loyaltyPoints);
            double tong = tongTruocGiamGia - soTienGiamGia;

            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════╗");
            sb.AppendLine("║        HÓA ĐƠN QUÁN BIDA         ║");
            sb.AppendLine("╚══════════════════════════════════╝");
            sb.AppendLine($"  Mã HĐ   : {hd.MaHoaDon}");
            sb.AppendLine($"  Bàn     : {SelectedBan.TenBan}");
            bool laHoiVien = kh != null && !string.IsNullOrEmpty(kh.SoDienThoai);
            sb.AppendLine($"  Khách   : {(laHoiVien ? kh.TenKhachHang + "  |  SĐT: " + kh.SoDienThoai : "Khách thường")}");
            if (laHoiVien) 
            {
                sb.AppendLine($"  Điểm TL : {loyaltyPoints} điểm ({BilliardCalculator.GetMembershipTier(loyaltyPoints)})");
            }
            sb.AppendLine($"  Giờ vào : {hd.GioVaoHoaDon?.ToString(@"hh\:mm") ?? "--:--"}");
            sb.AppendLine($"  Giờ hiện: {gioHienTai.Hours:D2}:{gioHienTai.Minutes:D2}  (tạm tính)");
            sb.AppendLine("──────────────────────────────────");
            sb.AppendLine($"  {"Tên món",-20} {"SL",3} {"Thành tiền",12}");
            sb.AppendLine("──────────────────────────────────");
            ctList?.ForEach(ct => {
                var sp = DsSanPham?.FirstOrDefault(s => s.Ma == ct.MaThucDon);
                sb.AppendLine($"  {sp?.Ten ?? ct.MaThucDon,-20} {ct.SoLuongChiTiet,3} {ct.GiaChiTiet,12:N0} đ");
            });
            sb.AppendLine("──────────────────────────────────");
            sb.AppendLine($"  Tiền đồ ăn/uống: {tongTienMon,10:N0} đ");
            sb.AppendLine($"  Tiền bàn chơi  : {tongTienBan,10:N0} đ");
            sb.AppendLine($"  ────────────────────────────────");
            sb.AppendLine($"  Tổng trước giảm: {tongTruocGiamGia,10:N0} đ");
            if (soTienGiamGia > 0)
            {
                sb.AppendLine($"  Giảm giá ({BilliardCalculator.GetMembershipTier(loyaltyPoints)}): {soTienGiamGia,10:N0} đ");
            }
            sb.AppendLine("══════════════════════════════════");
            sb.AppendLine($"  TỔNG TIỀN    : {tong,10:N0} VNĐ");
            sb.AppendLine("  (Tạm tính — chưa thanh toán)");
            MessageBox.Show(sb.ToString(), $"Hóa đơn — {SelectedBan.TenBan}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecThanhToan(object obj)
        {
            if (SelectedBan == null) { MessageBox.Show("Vui lòng chọn bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (SelectedBan.TrangThaiBan != true) { MessageBox.Show("Bàn đang trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var hd = DsHoaDon?.FirstOrDefault(h => h.MaBan == SelectedBan.MaBan && h.TrangThaiThanhToan == false);
            if (hd == null) { MessageBox.Show("Không tìm thấy hóa đơn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            var gioRa = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            double tongTienMon = DsChiTietHoaDon?.Where(ct => ct.MaHoaDon == hd.MaHoaDon).Sum(ct => ct.GiaChiTiet ?? 0) ?? 0;
            double tongTienBan = hd.GioVaoHoaDon.HasValue ? 
                BilliardCalculator.CalculateTableFee(hd.GioVaoHoaDon, gioRa) : 0;
            
            double tongTruocGiamGia = tongTienMon + tongTienBan;
            int loyaltyPoints = (hd.KhachHang?.DiemTichLuy ?? 0);
            double soTienGiamGia = BilliardCalculator.CalculateDiscount(tongTruocGiamGia, loyaltyPoints);
            double tong = tongTruocGiamGia - soTienGiamGia;

            string xacNhan = $"Xác nhận thanh toán {SelectedBan.TenBan}?\n\n" +
                             $"  Giờ vào        : {hd.GioVaoHoaDon?.ToString(@"hh\:mm") ?? "--:--"}\n" +
                             $"  Giờ ra         : {gioRa.Hours:D2}:{gioRa.Minutes:D2}\n" +
                             $"  Tiền đồ ăn/uống: {tongTienMon:N0} VNĐ\n" +
                             $"  Tiền bàn chơi  : {tongTienBan:N0} VNĐ\n" +
                             $"──────────────────────────────\n" +
                             $"  Tổng trước giảm: {tongTruocGiamGia:N0} VNĐ\n";
            
            if (soTienGiamGia > 0 && hd.KhachHang != null)
            {
                xacNhan += $"  Giảm giá ({BilliardCalculator.GetMembershipTier(loyaltyPoints)}): -{soTienGiamGia:N0} VNĐ\n" +
                           $"──────────────────────────────\n";
            }
            
            xacNhan += $"  TỔNG THANH TÓA  : {tong:N0} VNĐ";
            
            if (MessageBox.Show(xacNhan, "Xác nhận thanh toán", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            if (db == null) return;
            try
            {
                // Cập nhật trạng thái hóa đơn
                hd.TrangThaiThanhToan = true;
                hd.GioRaHoaDon = gioRa;
                hd.TongTienHoaDon = tong;

                // Cộng điểm hội viên dựa trên tổng tiền trước giảm giá
                if (!string.IsNullOrEmpty(hd.MaKhachHang))
                {
                    var kh = DsKhachHang?.FirstOrDefault(k => k.MaKhachHang == hd.MaKhachHang);
                    if (kh != null && !string.IsNullOrEmpty(kh.SoDienThoai)) // chỉ cộng điểm Hội viên
                    {
                        int earnedPoints = BilliardCalculator.CalculateLoyaltyPoints(tongTruocGiamGia);
                        kh.DiemTichLuy = (kh.DiemTichLuy ?? 0) + earnedPoints;
                        
                        // Thông báo loại hội viên sau khi cộng điểm
                        string membershipInfo = $"\n\n✓ Cộng {earnedPoints} điểm\n" +
                                              $"  Tổng: {kh.DiemTichLuy} điểm ({BilliardCalculator.GetMembershipTier(kh.DiemTichLuy.Value)})";
                        MessageBox.Show($"Thanh toán thành công!\n{membershipInfo}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Thanh toán thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Thanh toán thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // ===== TRỪ KHO KHI THANH TOÁN =====
                // Lấy tất cả chi tiết hóa đơn (món ăn/uống đã gọi)
                var chiTietList = DsChiTietHoaDon?.Where(ct => ct.MaHoaDon == hd.MaHoaDon).ToList();
                if (chiTietList != null && chiTietList.Count > 0)
                {
                    foreach (var ct in chiTietList)
                    {
                        // Tìm sản phẩm và trừ kho
                        var kho = db.Khoes.FirstOrDefault(k => k.MaSP == ct.MaThucDon);
                        if (kho != null)
                        {
                            // Trừ số lượng từ tồn kho
                            kho.TonKho = (kho.TonKho ?? 0) - (ct.SoLuongChiTiet ?? 0);
                        }
                    }
                }

                // Cập nhật trạng thái bàn
                var banDb = db.Bans.Find(SelectedBan.MaBan);
                if (banDb != null) { banDb.TrangThaiBan = false; SelectedBan.TrangThaiBan = false; }

                // Lưu thay đổi (hóa đơn + kho + bàn + điểm)
                db.SaveChanges();

                // Cập nhật giao diện liên quan đến kho/menu
                LoadData_Kho();
                RefreshKhoGroups();
                ApplyProductFilter();

                UpdateDsBanMo();
                string maBan = SelectedBan.MaBan;
                DsMonAnCuaBan = new ObservableCollection<MonAnChiTiet>(); TongTienHienTai = 0; GioVaoBan = "--:--";
                OnPropertyChanged(nameof(DsMonAnCuaBan));
                OnPropertyChanged(nameof(TongTienHienTai));
                OnPropertyChanged(nameof(GioVaoBan));
                RefreshBanRealtime(maBan);
                LoadThongKe();
                MessageBox.Show(
                    $"✅ Thanh toán thành công!\n\n" +
                    $"  Tiền đồ ăn/uống: {tongTienMon:N0} VNĐ\n" +
                    $"  Tiền bàn chơi   : {tongTienBan:N0} VNĐ\n" +
                    $"  TỔNG            : {tong:N0} VNĐ",
                    "Thanh toán thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            { MessageBox.Show("Lỗi: " + (ex.InnerException?.Message ?? ex.Message), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        //Ban-Tran Nguyen - END
        // ===================== HÓA ĐƠN =====================
        // ===================== PHIẾU NHẬP =====================
        // ===================== KHÁCH HÀNG =====================
        private string _addTenKH;
        private string _addSDT;
        private string _addMaKH;
        private bool _isShowAdd;
        private int? _addDiemTichLuy;
        private List<KhachHang> _allKhachHang;

        public bool IsShowAdd
        {
            get => _isShowAdd;
            set { _isShowAdd = value; OnPropertyChanged(nameof(IsShowAdd)); }
        }

        public string AddTenKH { get => _addTenKH; set { _addTenKH = value; OnPropertyChanged(nameof(AddTenKH)); } }
        public string AddSDT { get => _addSDT; set { _addSDT = value; OnPropertyChanged(nameof(AddSDT)); } }
        public string AddMaKH { get => _addMaKH; set { _addMaKH = value; OnPropertyChanged(nameof(AddMaKH)); } }
        public int? AddDiemTichLuy { get => _addDiemTichLuy; set { _addDiemTichLuy = value; OnPropertyChanged(nameof(AddDiemTichLuy)); } }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                TimKiemKhachHang();
            }
        }

        private KhachHang _selectedKhachHang;
        public KhachHang SelectedKhachHang
        {
            get => _selectedKhachHang;
            set { _selectedKhachHang = value; OnPropertyChanged(nameof(SelectedKhachHang)); }
        }

        // COMMAND
        public ICommand MoThemKhachHangCommand { get; set; }
        public ICommand DongThemKhachHangCommand { get; set; }
        public ICommand XacNhanThemKhachHangCommand { get; set; }
        public ICommand XoaKhachHangCommand { get; set; }
        public ICommand MoSuaKhachHangCommand { get; set; }

        private bool _isEditingCustomer = false;
        public bool IsEditingCustomer
        {
            get => _isEditingCustomer;
            set { _isEditingCustomer = value; OnPropertyChanged(nameof(IsEditingCustomer)); }
        }

        private void TimKiemKhachHang()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DsKhachHang = new ObservableCollection<KhachHang>(_allKhachHang);
            }
            else
            {
                var filtered = _allKhachHang.Where(x =>
                    (x.SoDienThoai != null && x.SoDienThoai.Contains(SearchText)) ||
                    (x.TenKhachHang != null && x.TenKhachHang.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                DsKhachHang = new ObservableCollection<KhachHang>(filtered);
            }

            OnPropertyChanged(nameof(DsKhachHang));
        }

        // ===================== CRUD KHÁCH HÀNG =====================

        public void MoThemKhachHang(object obj)
        {
            IsEditingCustomer = false;

            try
            {
                var codes = db.KhachHangs.Select(k => k.MaKhachHang).ToList();
                int max = 0;

                foreach (var c in codes)
                {
                    if (string.IsNullOrWhiteSpace(c) || c.Length <= 2) continue;

                    var numPart = c.Substring(2);
                    if (int.TryParse(numPart, out int n))
                    {
                        if (n > max) max = n;
                    }
                }

                int next = max + 1;
                AddMaKH = "KH" + next.ToString("D3");
            }
            catch
            {
                AddMaKH = "KH01";
            }

            AddTenKH = string.Empty;
            AddSDT = string.Empty;
            AddDiemTichLuy = 0;
            IsShowAdd = true;
        }

        public void DongThemKhachHang(object obj)
        {
            IsShowAdd = false;
        }

        public void XacNhanThemKhachHang(object obj)
        {
            if (string.IsNullOrWhiteSpace(AddTenKH) || string.IsNullOrWhiteSpace(AddSDT))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin khách hàng", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsEditingCustomer)
            {
                var existing = _allKhachHang.FirstOrDefault(x => x.MaKhachHang == AddMaKH) ?? SelectedKhachHang;

                if (existing != null)
                {
                    existing.TenKhachHang = AddTenKH;
                    existing.SoDienThoai = AddSDT;
                    existing.DiemTichLuy = AddDiemTichLuy;

                    try
                    {
                        var dbEntity = db.KhachHangs.Find(existing.MaKhachHang);

                        if (dbEntity != null)
                        {
                            dbEntity.TenKhachHang = existing.TenKhachHang;
                            dbEntity.SoDienThoai = existing.SoDienThoai;
                            dbEntity.DiemTichLuy = existing.DiemTichLuy;
                            db.SaveChanges();
                        }
                    }
                    catch { }

                    var idxAll = _allKhachHang.FindIndex(x => x.MaKhachHang == existing.MaKhachHang);
                    if (idxAll >= 0) _allKhachHang[idxAll] = existing;

                    var oldItem = DsKhachHang.FirstOrDefault(x => x.MaKhachHang == existing.MaKhachHang);
                    if (oldItem != null)
                    {
                        DsKhachHang.Remove(oldItem);
                        DsKhachHang.Add(existing);
                    }
                }
                OnPropertyChanged(nameof(DsKhachHang));
                IsShowAdd = false;
                IsEditingCustomer = false;
                return;
            }

            var kh = new KhachHang
            {
                MaKhachHang = AddMaKH,
                TenKhachHang = AddTenKH,
                SoDienThoai = AddSDT,
                DiemTichLuy = AddDiemTichLuy
            };

            try
            {
                db.KhachHangs.Add(kh);
                db.SaveChanges();
            }
            catch
            {
            }

            _allKhachHang.Add(kh);
            DsKhachHang.Add(kh);
            IsShowAdd = false;
            OnPropertyChanged(nameof(DsKhachHang));
        }

        public void MoSuaKhachHang(object obj)
        {
            if (SelectedKhachHang == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng để sửa", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsEditingCustomer = true;

            AddMaKH = SelectedKhachHang.MaKhachHang;
            AddTenKH = SelectedKhachHang.TenKhachHang;
            AddSDT = SelectedKhachHang.SoDienThoai;
            AddDiemTichLuy = SelectedKhachHang.DiemTichLuy;
            OnPropertyChanged(nameof(DsKhachHang));
            IsShowAdd = true;
            db.SaveChanges();
            
        }

        public void XoaKhachHang(object obj)
        {
            if (SelectedKhachHang == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng để xóa", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var res = MessageBox.Show($"Xác nhận xóa khách hàng {SelectedKhachHang.TenKhachHang}?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res != MessageBoxResult.Yes) return;

            bool hasInvoices = db.HoaDons.Any(h => h.MaKhachHang == SelectedKhachHang.MaKhachHang);

            if (hasInvoices)
            {
                MessageBox.Show("Không thể xóa khách hàng này vì đã có lịch sử hóa đơn.",
                    "Không thể xóa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dbEntity = db.KhachHangs.Find(SelectedKhachHang.MaKhachHang);

                if (dbEntity != null)
                {
                    db.KhachHangs.Remove(dbEntity);
                    db.SaveChanges();
                }

                _allKhachHang.RemoveAll(x => x.MaKhachHang == SelectedKhachHang.MaKhachHang);
                DsKhachHang.Remove(SelectedKhachHang);

                OnPropertyChanged(nameof(DsKhachHang));

                MessageBox.Show("Xóa khách hàng thành công.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa khách hàng: " + ex.Message,
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ===================== NHÂN VIÊN =====================
        private string _inputTenNV;
        private string _inputChucVu;
        private string _inputLoaiNV;
        private string _inputDiaChi;
        private string _inputSDT;
        private DateTime? _inputNgaySinh;
        private DateTime? _inputNgayVL;
        private string _inputNewTK;
        private string _inputNewMK;
        private string _inputMaNV;
        private double? _inputLuongCB;
        public string InputTenNV
        {
            get { return _inputTenNV; }
            set
            {
                _inputTenNV = value;
                OnPropertyChanged(nameof(InputTenNV));
            }
        }
        public string InputChucVu
        {
            get { return _inputChucVu; }
            set
            {
                _inputChucVu = value;
                OnPropertyChanged(nameof(InputChucVu));
            }
        }
        public string InputLoaiNV
        {
            get { return _inputLoaiNV; }
            set
            {
                _inputLoaiNV = value;
                OnPropertyChanged(nameof(InputLoaiNV));
            }
        }
        public string InputDiaChi
        {
            get { return _inputDiaChi; }
            set
            {
                _inputDiaChi = value;
                OnPropertyChanged(nameof(InputDiaChi));
            }
        }
        public string InputSDT
        {
            get { return _inputSDT; }
            set
            {
                _inputSDT = value;
                OnPropertyChanged(nameof(InputSDT));
            }
        }
        public DateTime? InputNgaySinh
        {
            get { return _inputNgaySinh; }
            set
            {
                _inputNgaySinh = value;
                OnPropertyChanged(nameof(InputNgaySinh));
            }
        }
        public DateTime? InputNgayVL
        {
            get { return _inputNgayVL; }
            set
            {
                _inputNgayVL = value;
                OnPropertyChanged(nameof(InputNgayVL));
            }
        }
        public string InputNewMK
        {
            get { return _inputNewMK; }
            set
            {
                _inputNewMK = value;
                OnPropertyChanged(nameof(InputNewMK));
            }
        }
        public string InputNewTK
        {
            get { return _inputNewTK; }
            set
            {
                _inputNewTK = value;
                OnPropertyChanged(nameof(InputNewTK));
            }
        }
        public string InputMaNV
        {
            get => _inputMaNV;
            set { _inputMaNV = value; OnPropertyChanged(nameof(InputMaNV)); }
        }
        public double? InputLuongCB
        {
            get => _inputLuongCB;
            set { _inputLuongCB = value; OnPropertyChanged(nameof(InputLuongCB)); }
        }
        public bool IsNVEnabled => !IsAddNV;
        private bool _isAddNV;
        public bool IsAddNV
        {
            get => _isAddNV;
            set
            {
                _isAddNV = value;
                OnPropertyChanged(nameof(IsAddNV));
                OnPropertyChanged(nameof(VisibileAddNV));
                OnPropertyChanged(nameof(IsNVEnabled));
                OnPropertyChanged(nameof(OverlayMaskVisibility));
            }
        }
        public ICommand ExitChamCongCommand { get; set; }
        // Hiển thị bảng chấm công
        private Visibility _visibileChamCong = Visibility.Collapsed;
        public Visibility VisibileChamCong
        {
            get => _visibileChamCong;
            set { _visibileChamCong = value; OnPropertyChanged(nameof(VisibileChamCong)); OnPropertyChanged(nameof(OverlayMaskVisibility)); }
        }
        public ICommand VisibileTableChamCongCommand { get; set; }
        private bool KiemTraHopLeNhanVien(NhanVien nv, bool laThemMoi = false)
        {
            if (nv == null) return false;

            // 1. Kiểm tra không được để trống các trường bắt buộc
            if (string.IsNullOrWhiteSpace(nv.TenNV))
            {
                MessageBox.Show("Họ và tên nhân viên không được để trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(nv.ChucVu))
            {
                MessageBox.Show("Vui lòng chọn chức vụ cho nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(nv.SDT))
            {
                MessageBox.Show("Số điện thoại không được để trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 2. Kiểm tra định dạng số điện thoại (Phải là số và có từ 9 đến 11 ký tự)
            string sdtTrim = nv.SDT.Trim();
            if (!sdtTrim.All(char.IsDigit) || sdtTrim.Length < 9 || sdtTrim.Length > 11)
            {
                MessageBox.Show("Số điện thoại không hợp lệ! Vui lòng nhập từ 9 đến 11 chữ số.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(nv.TaiKhoan) || string.IsNullOrWhiteSpace(nv.MatKhau))
            {
                MessageBox.Show("Tài khoản và mật khẩu của nhân viên không được bỏ trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 3. Kiểm tra độ dài mật khẩu (Đã an toàn do có bước chặn rỗng phía trên)
            if (nv.MatKhau.Trim().Length < 4)
            {
                MessageBox.Show("Mật khẩu nhân viên phải có ít nhất 4 ký tự!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 4. Kiểm tra trùng lặp Tài khoản hệ thống (Sử dụng string.Equals bảo mật và an toàn hơn)
            if (DsNhanVien != null)
            {
                string tkMoi = nv.TaiKhoan.Trim();

                bool biTrungTK = laThemMoi
                    ? DsNhanVien.Any(x => x.TaiKhoan != null && string.Equals(x.TaiKhoan.Trim(), tkMoi, StringComparison.OrdinalIgnoreCase))
                    : DsNhanVien.Any(x => x.TaiKhoan != null && string.Equals(x.TaiKhoan.Trim(), tkMoi, StringComparison.OrdinalIgnoreCase) && x.MaNV != nv.MaNV);

                if (biTrungTK)
                {
                    MessageBox.Show("Tài khoản này đã tồn tại trong hệ thống! Vui lòng chọn tài khoản khác.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            // 5. Kiểm tra lương cơ bản
            if (nv.LuongCB == null || nv.LuongCB <= 0)
            {
                MessageBox.Show("Lương cơ bản phải lớn hơn 0!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        public Visibility VisibileAddNV
        {
            get
            {
                if (IsAddNV == false)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }
        public Visibility OverlayMaskVisibility
        {
            get => (VisibileAddNV == Visibility.Visible || VisibileChamCong == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
        }
        // Chức năng hiện bảng thêm nhân viên
        public ICommand VisibileTableAddNVCommand { get; set; }
        public void VisibileTableAddNV(Object obj)
        {
            // Tính toán mã nhân viên mới để hiển thị
            int max = DsNhanVien
                .Select(x =>
                {
                    int n;
                    return int.TryParse(x.MaNV.Replace("NV", ""), out n) ? n : 0;
                })
                .DefaultIfEmpty(0)
                .Max();
            
            InputMaNV = "NV" + (max + 1).ToString("D3");
            
            IsAddNV = true;
            OnPropertyChanged(nameof(IsAddNV));
        }
        public ICommand ExitThemNVCommand { get; set; }
        public void ExitThemNV(Object obj)
        {
            IsAddNV = false;
        }
        // Chọn nhân viên
        private NhanVien _isSelectedNV;
        public NhanVien IsSelectedNV
        {
            get => _isSelectedNV;
            set
            {
                _isSelectedNV = value;
                OnPropertyChanged(nameof(IsSelectedNV));

                if (_isSelectedNV != null)
                {
                    // Tạo đối tượng mới và copy từng thuộc tính để tránh tham chiếu trực tiếp
                    EditingNV = new NhanVien
                    {
                        MaNV = _isSelectedNV.MaNV,
                        TenNV = _isSelectedNV.TenNV,
                        ChucVu = _isSelectedNV.ChucVu,
                        DiaChi = _isSelectedNV.DiaChi,
                        SDT = _isSelectedNV.SDT,
                        NgaySinh = _isSelectedNV.NgaySinh,
                        NgayVL = _isSelectedNV.NgayVL,
                        TaiKhoan = _isSelectedNV.TaiKhoan,
                        MatKhau = _isSelectedNV.MatKhau,
                        LuongCB = _isSelectedNV.LuongCB
                    };
                }
                else
                {
                    EditingNV = null;
                }

                (DeleteNVCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EditNVCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        // Got focus
        public ICommand ClearSelectionNVCommand { get; set; }
        public void ClearSelectionNV(Object obj)
        {
            IsSelectedNV = null;
            OnPropertyChanged(nameof(IsSelectedNV));
        }
        // Chức năng xóa nhân viên
        public ICommand DeleteNVCommand { get; set; }
        private bool CanDeleteNV(object obj)
        {
            return IsSelectedNV != null;
        }
        public void DeleteNV(Object obj)
        {
            if (IsSelectedNV == null)
            {
                MessageBox.Show(
                    "Vui lòng chọn nhân viên",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa {IsSelectedNV.TenNV} ?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Kiểm tra dữ liệu liên quan
                bool hasHoaDon = db.HoaDons.Any(h => h.MaNV == IsSelectedNV.MaNV);
                bool hasChamCong = db.BangChamCongs.Any(c => c.MaNV == IsSelectedNV.MaNV);

                if (hasHoaDon || hasChamCong)
                {
                    MessageBox.Show(
                        "Không thể xóa nhân viên đã có dữ liệu liên quan!",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                var nv = db.NhanViens.Find(IsSelectedNV.MaNV);

                if (nv != null)
                {
                    db.NhanViens.Remove(nv);
                    db.SaveChanges();
                }

                // Refresh list from DB to reflect deletion
                LoadData_NhanVien();
                OnPropertyChanged(nameof(DsNhanVien));
                MessageBox.Show(
                    "Xóa nhân viên thành công",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                IsSelectedNV = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        // Chức năng sửa nhân viên
        private NhanVien _editingNV;
        public NhanVien EditingNV
        {
            get => _editingNV;
            set
            {
                _editingNV = value;
                OnPropertyChanged(nameof(EditingNV));
            }
        }
        public ICommand EditNVCommand { get; set; }
        private bool CanEditNV(object obj)
        {
            return EditingNV != null;
        }
        public void EditNV(Object obj)
        {
            if (EditingNV == null || IsSelectedNV == null)
            {
                MessageBox.Show(
                    "Vui lòng chọn nhân viên cần sửa",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra dữ liệu hợp lệ
            if (!KiemTraHopLeNhanVien(EditingNV, false))
                return;

            try
            {
                // Tìm nhân viên trong database
                var nv = db.NhanViens.Find(IsSelectedNV.MaNV);

                if (nv == null)
                {
                    MessageBox.Show(
                        "Không tìm thấy nhân viên",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Update dữ liệu
                nv.TenNV = EditingNV.TenNV;
                nv.ChucVu = EditingNV.ChucVu;
                nv.DiaChi = EditingNV.DiaChi;
                nv.SDT = EditingNV.SDT;
                nv.NgaySinh = EditingNV.NgaySinh;
                nv.NgayVL = EditingNV.NgayVL;
                nv.TaiKhoan = EditingNV.TaiKhoan;
                nv.MatKhau = EditingNV.MatKhau;
                nv.LuongCB = EditingNV.LuongCB;

                db.SaveChanges();

                // Reload list from database to ensure UI reflects latest data
                LoadData_NhanVien();
                OnPropertyChanged(nameof(DsNhanVien));

                // restore selection to the updated employee
                IsSelectedNV = DsNhanVien?.FirstOrDefault(x => x.MaNV == nv.MaNV);

                MessageBox.Show(
                    $"Đã cập nhật thông tin nhân viên: {nv.TenNV}",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        public ICommand AddNVCommand { get; set; }
        public ICommand SaveChamCongCommand { get; set; }
        public ICommand AddChamCongCommand { get; set; }
        public ICommand DeleteChamCongCommand { get; set; }
        public ICommand PayChamCongCommand { get; set; }

        private bool CanDeleteChamCong(object o) => SelectedChamCongItem != null;
        private bool CanPayChamCong(object o) => SelectedChamCongItem != null && !SelectedChamCongItem.IsPay;

        private void AddChamCong(object o)
        {
            if (DsBangChamCongView == null) DsBangChamCongView = new ObservableCollection<ChamCongItem>();
            var item = new ChamCongItem { MaNV = "", TenNV = "", ChucVu = "", Ngay = DateTime.Today, SoGio = 8 };
            DsBangChamCongView.Add(item);
            SelectedChamCongItem = item;
        }

        private void PayChamCong(object o)
        {
            if (SelectedChamCongItem == null || SelectedChamCongItem.IsPay) return;

            SelectedChamCongItem.IsPay = true;
            SelectedChamCongItem.SoTien = SelectedChamCongItem.SoGio * PayRatePerHour;

            if (db != null && !string.IsNullOrWhiteSpace(SelectedChamCongItem.MaNV))
            {
                var key = SelectedChamCongItem.MaNV;
                var dbItem = db.BangChamCongs.FirstOrDefault(b => b.MaNV == key && DbFunctions.TruncateTime(b.Ngay) == SelectedChamCongItem.Ngay.Date);
                if (dbItem != null)
                {
                    dbItem.IsPay = true;
                    db.SaveChanges();
                }
            }
            (PayChamCongCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void DeleteChamCong(object o)
        {
            if (SelectedChamCongItem == null) return;
            try
            {
                // try find matching DB record by MaNV + Ngay
                if (db != null && !string.IsNullOrWhiteSpace(SelectedChamCongItem.MaNV))
                {
                    var key = SelectedChamCongItem.MaNV;
                    var dbItem = db.BangChamCongs.FirstOrDefault(b => b.MaNV == key && DbFunctions.TruncateTime(b.Ngay) == SelectedChamCongItem.Ngay.Date);
                    if (dbItem != null)
                    {
                        db.BangChamCongs.Remove(dbItem);
                        db.SaveChanges();
                    }
                }
            }
            catch { }
            DsBangChamCongView.Remove(SelectedChamCongItem);
            SelectedChamCongItem = null;
            LoadData_BangChamCong();
        }

        private void SaveChamCong(object o)
        {
            if (DsBangChamCongView == null || db == null) return;
            try
            {
                foreach (var item in DsBangChamCongView)
                {
                    if (string.IsNullOrWhiteSpace(item.MaNV)) continue;
                    item.MaNV = item.MaNV.Trim();
                    // validate employee against database to prevent FOREIGN KEY constraint errors
                    var nv = db.NhanViens.FirstOrDefault(x => x.MaNV == item.MaNV);
                    if (nv == null) continue; // skip invalid
                    item.TenNV = nv.TenNV;
                    item.ChucVu = nv.ChucVu;
                    if (item.SoGio < 0) item.SoGio = 0;
                    // find existing record by employee + date
                    var exist = db.BangChamCongs.FirstOrDefault(b => b.MaNV == item.MaNV && DbFunctions.TruncateTime(b.Ngay) == item.Ngay.Date);
                    if (exist != null)
                    {
                        exist.Ngay = item.Ngay;
                        exist.GioVao = TimeSpan.Zero;
                        exist.GioRa = TimeSpan.FromHours(item.SoGio);
                        if (exist.IsPay == null) exist.IsPay = false;
                    }
                    else
                    {
                        var newBc = new BangChamCong
                        {
                            MaBCC = GenerateNextMaBCC(),
                            MaNV = item.MaNV,
                            Ngay = item.Ngay,
                            GioVao = TimeSpan.Zero,
                            GioRa = TimeSpan.FromHours(item.SoGio),
                            IsPay = false
                        };
                        db.BangChamCongs.Add(newBc);
                    }
                }
                db.SaveChanges();
                LoadData_BangChamCong();
                MessageBox.Show("Lưu bảng chấm công thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu chấm công: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void AddNV(object obj)
        {
            try
            {
                // Kiểm tra lương cơ bản trước
                if (InputLuongCB == null || InputLuongCB <= 0)
                {
                    MessageBox.Show("Vui lòng nhập lương cơ bản và phải lớn hơn 0!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mã nhân viên: tự động sinh mã (không cho người dùng nhập)
                int max = DsNhanVien
                    .Select(x =>
                    {
                        int n;
                        return int.TryParse(x.MaNV.Replace("NV", ""), out n) ? n : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                string maNV = "NV" + (max + 1).ToString("D3");

                // Kiểm tra trùng mã
                if (DsNhanVien != null && DsNhanVien.Any(x => string.Equals(x.MaNV, maNV, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Mã nhân viên đã tồn tại! Vui lòng nhập mã khác.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                NhanVien nv = new NhanVien()
                {
                    MaNV = maNV,
                    TenNV = InputTenNV,
                    ChucVu = InputChucVu,
                    DiaChi = InputDiaChi,
                    SDT = InputSDT,
                    NgaySinh = InputNgaySinh,
                    NgayVL = InputNgayVL,
                    TaiKhoan = InputNewTK,
                    MatKhau = InputNewMK,
                    LuongCB = InputLuongCB
                };

                // Kiểm tra hợp lệ
                if (!KiemTraHopLeNhanVien(nv, true))
                    return;

                db.NhanViens.Add(nv);
                db.SaveChanges();

                DsNhanVien.Add(nv);
                ApplyNhanVienFilter(); // Cập nhật danh sách lọc

                MessageBox.Show(
                    "Thêm nhân viên thành công",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Reset form
                InputMaNV = "";
                InputTenNV = "";
                InputChucVu = "";
                InputDiaChi = "";
                InputSDT = "";
                InputNgaySinh = null;
                InputNgayVL = null;
                InputNewTK = "";
                InputNewMK = "";
                InputLuongCB = null;

                IsAddNV = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        // ===================== LỊCH SỬ HÓA ĐƠN =====================
        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(nameof(StartDate)); }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(nameof(EndDate)); }
        }

        private NhanVien _selectedNhanVien;
        public NhanVien SelectedNhanVien
        {
            get => _selectedNhanVien;
            set { _selectedNhanVien = value; OnPropertyChanged(nameof(SelectedNhanVien)); }
        }

        private HoaDon _selectedHoaDon;
        public HoaDon SelectedHoaDon
        {
            get => _selectedHoaDon;
            set
            {
                _selectedHoaDon = value;
                OnPropertyChanged(nameof(SelectedHoaDon));
                (XuatBaoCaoHoaDonCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // COMMAND
        public ICommand LocHoaDonCommand { get; set; }
        public ICommand XuatExcelHoaDonCommand { get; set; }
        public ICommand XuatBaoCaoHoaDonCommand { get; set; }

        public void LocHoaDon()
        {
            if (DsHoaDon == null) LoadData_HoaDon();

            var q = DsHoaDon.AsEnumerable();

            if (StartDate.HasValue)
            {
                var sd = StartDate.Value.Date;
                q = q.Where(h => LayNgayHoaDon(h) >= sd);
            }

            if (EndDate.HasValue)
            {
                var ed = EndDate.Value.Date.AddDays(1).AddTicks(-1);
                q = q.Where(h => LayNgayHoaDon(h) <= ed);
            }

            if (SelectedNhanVien != null)
            {
                q = q.Where(h => h.MaNV == SelectedNhanVien.MaNV);
            }

            FilteredHoaDon = new ObservableCollection<HoaDon>(q);
            OnPropertyChanged(nameof(FilteredHoaDon));
        }

        private DateTime LayNgayHoaDon(HoaDon h)
        {
            return h.NgayHoaDon ?? DateTime.MinValue;
        }
        // Helper to sanitize values for CSV output:
        // - escape double quotes by doubling them
        // - wrap field in quotes if it contains comma, quote or newline
        private string XuLyCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var escaped = value.Replace("\"", "\"\"");
            if (escaped.Contains(",") || escaped.Contains("\"") || escaped.Contains("\r") || escaped.Contains("\n"))
            {
                return $"\"{escaped}\"";
            }
            return escaped;
        }

        public void XuatExcelHoaDon()
        {
            try
            {
                var list = FilteredHoaDon ?? new ObservableCollection<HoaDon>(DsHoaDon ?? new ObservableCollection<HoaDon>());

                var sb = new StringBuilder();

                sb.AppendLine("MaHoaDon,TenKhachHang,MaBan,TongTien,NgayHoaDon,TenNV");

                foreach (var h in list)
                {
                    var tenKH = h.KhachHang?.TenKhachHang ?? "";
                    var tenNV = h.NhanVien?.TenNV ?? "";
                    var ngay = LayNgayHoaDon(h).ToString("yyyy-MM-dd HH:mm:ss");

                    sb.AppendLine($"{h.MaHoaDon},{XuLyCsv(tenKH)},{h.MaBan},{h.TongTienHoaDon},{ngay},{XuLyCsv(tenNV)}");
                }

                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    FileName = "LichSuHoaDon",
                    DefaultExt = ".csv",
                    Filter = "CSV files (.csv)|*.csv"
                };

                bool? result = dlg.ShowDialog();

                if (result == true)
                {
                    System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Đã xuất file.",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất file: " + ex.Message,
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExecXuatBaoCaoHoaDon(object obj)
        {
            if (SelectedHoaDon == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn để xuất báo cáo!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var hd = SelectedHoaDon;
            var ctList = DsChiTietHoaDon?.Where(ct => ct.MaHoaDon == hd.MaHoaDon).ToList() ?? new List<ChiTietHoaDon>();

            var reportWindow = new InvoiceReportWindow(hd, ctList, DsSanPham ?? new ObservableCollection<SanPham>());
            reportWindow.Owner = Application.Current.MainWindow;
            reportWindow.ShowDialog();
        }

        public ICommand ShowAllHoaDonCommand { get; set; }
        private void ExecShowAllHoaDon()
        {
            if (DsHoaDon != null)
            {
                // Gán lại toàn bộ danh sách hóa đơn gốc vào danh sách hiển thị sau lọc
                FilteredHoaDon = new ObservableCollection<HoaDon>(DsHoaDon);

                // Thông báo cho giao diện (UI) cập nhật lại dữ liệu mới thay đổi
                OnPropertyChanged(nameof(FilteredHoaDon));

                // (Tùy chọn) Nếu bạn có TextBox tìm kiếm từ khóa, hãy reset nó về rỗng
                // SearchInvoiceText = string.Empty; 
            }
        }
        // ===================== SẢN PHẨM =====================

        // ===================== THỐNG KÊ =====================
        private double _tongDoanhThu;
        public double TongDoanhThu
        {
            get => _tongDoanhThu;
            set { _tongDoanhThu = value; OnPropertyChanged(nameof(TongDoanhThu)); }
        }

        private int _soHoaDonDaThanhToan;
        public int SoHoaDonDaThanhToan
        {
            get => _soHoaDonDaThanhToan;
            set { _soHoaDonDaThanhToan = value; OnPropertyChanged(nameof(SoHoaDonDaThanhToan)); }
        }

        private int _soKhachHang;
        public int SoKhachHang
        {
            get => _soKhachHang;
            set { _soKhachHang = value; OnPropertyChanged(nameof(SoKhachHang)); }
        }

        private int _soBanDangHoatDong;
        public int SoBanDangHoatDong
        {
            get => _soBanDangHoatDong;
            set { _soBanDangHoatDong = value; OnPropertyChanged(nameof(SoBanDangHoatDong)); }
        }

        private ObservableCollection<ThongKeSanPham> _dsTopSanPham;
        public ObservableCollection<ThongKeSanPham> DsTopSanPham
        {
            get => _dsTopSanPham;
            set { _dsTopSanPham = value; OnPropertyChanged(nameof(DsTopSanPham)); }
        }

        private ObservableCollection<ThongKeBan> _dsThongKeBan;
        public ObservableCollection<ThongKeBan> DsThongKeBan
        {
            get => _dsThongKeBan;
            set { _dsThongKeBan = value; OnPropertyChanged(nameof(DsThongKeBan)); }
        }

        private double _maxTongTienBan;
        public double MaxTongTienBan
        {
            get => _maxTongTienBan;
            set { _maxTongTienBan = value; OnPropertyChanged(nameof(MaxTongTienBan)); }
        }

        private string _debugLog;
        public string DebugLog
        {
            get => _debugLog;
            set { _debugLog = value; OnPropertyChanged(nameof(DebugLog)); }
        }

        private ObservableCollection<KhachHangThongKe> _dsTopKhachHang;
        public ObservableCollection<KhachHangThongKe> DsTopKhachHang
        {
            get => _dsTopKhachHang;
            set { _dsTopKhachHang = value; OnPropertyChanged(nameof(DsTopKhachHang)); }
        }

        public ICommand RefreshThongKeCommand { get; set; }
        // Thay nút xuất PDF thành overlay báo cáo
        public ICommand BaoCaoHieuSuatCommand { get; set; }
        public ICommand ExportReportToExcelCommand { get; set; }
        public ICommand CloseReportOverlayCommand { get; set; }

        private bool _isReportOverlayVisible;
        public bool IsReportOverlayVisible
        {
            get => _isReportOverlayVisible;
            set { _isReportOverlayVisible = value; OnPropertyChanged(nameof(IsReportOverlayVisible)); }
        }

        private ReportViewModel.ReportResult _currentReport;
        public ReportViewModel.ReportResult CurrentReport
        {
            get => _currentReport;
            set { _currentReport = value; OnPropertyChanged(nameof(CurrentReport)); }
        }

        public System.Collections.ObjectModel.ObservableCollection<ReportBanDTO> ReportPoolRows { get; set; } = new System.Collections.ObjectModel.ObservableCollection<ReportBanDTO>();
        public System.Collections.ObjectModel.ObservableCollection<ReportBanDTO> ReportLipRows { get; set; } = new System.Collections.ObjectModel.ObservableCollection<ReportBanDTO>();

        private int _reportPoolTotalLuot;
        public int ReportPoolTotalLuot { get => _reportPoolTotalLuot; set { _reportPoolTotalLuot = value; OnPropertyChanged(nameof(ReportPoolTotalLuot)); } }

        private double _reportPoolTotalGio;
        public double ReportPoolTotalGio { get => _reportPoolTotalGio; set { _reportPoolTotalGio = value; OnPropertyChanged(nameof(ReportPoolTotalGio)); } }

        private int _reportLipTotalLuot;
        public int ReportLipTotalLuot { get => _reportLipTotalLuot; set { _reportLipTotalLuot = value; OnPropertyChanged(nameof(ReportLipTotalLuot)); } }

        private double _reportLipTotalGio;
        public double ReportLipTotalGio { get => _reportLipTotalGio; set { _reportLipTotalGio = value; OnPropertyChanged(nameof(ReportLipTotalGio)); } }

        private double _reportPoolEfficiency;
        public double ReportPoolEfficiency { get => _reportPoolEfficiency; set { _reportPoolEfficiency = value; OnPropertyChanged(nameof(ReportPoolEfficiency)); } }

        private double _reportLipEfficiency;
        public double ReportLipEfficiency { get => _reportLipEfficiency; set { _reportLipEfficiency = value; OnPropertyChanged(nameof(ReportLipEfficiency)); } }

        public System.Collections.ObjectModel.ObservableCollection<ReportTypeEfficiency> ReportPoolTypeEfficiencies { get; set; } = new System.Collections.ObjectModel.ObservableCollection<ReportTypeEfficiency>();

        public class ReportTypeEfficiency
        {
            public string KieuBan { get; set; }
            public double HieuSuat { get; set; }
            public int SoBan { get; set; }
            public double TongGio { get; set; }
        }

        private DateTime _reportFromDate = DateTime.Today.AddDays(-30);
        public DateTime ReportFromDate
        {
            get => _reportFromDate;
            set { _reportFromDate = value; OnPropertyChanged(nameof(ReportFromDate)); }
        }

        private DateTime _reportToDate = DateTime.Today;
        public DateTime ReportToDate
        {
            get => _reportToDate;
            set { _reportToDate = value; OnPropertyChanged(nameof(ReportToDate)); }
        }

        public ReportViewModel ReportVM { get; set; }

        public void LoadThongKe()
        {
            try
            {
                var hoaDonDaThanhToan = DsHoaDon?
                    .Where(hd => hd.TrangThaiThanhToan == true)
                    .ToList()
                    ?? new List<HoaDon>();

                TongDoanhThu = hoaDonDaThanhToan.Sum(hd => hd.TongTienHoaDon ?? 0);
                SoHoaDonDaThanhToan = hoaDonDaThanhToan.Count;
                SoKhachHang = DsKhachHang?.Count ?? 0;
                SoBanDangHoatDong = DsBan?.Count(b => b.TrangThaiBan == true) ?? 0;

                var today = DateTime.Today;
                var hoaDonHomNay = hoaDonDaThanhToan
                    .Where(hd => hd.NgayHoaDon.HasValue && hd.NgayHoaDon.Value.Date == today)
                    .ToList();

                TongDoanhThuHomNay = hoaDonHomNay.Sum(hd => hd.TongTienHoaDon ?? 0);
                SoHoaDonHomNay = hoaDonHomNay.Count;

                var maHDSet = new HashSet<string>(hoaDonDaThanhToan.Select(hd => hd.MaHoaDon));
                var topSP = (DsChiTietHoaDon?
                    .Where(ct => maHDSet.Contains(ct.MaHoaDon))
                    .GroupBy(ct => ct.MaThucDon)
                    .Select(g =>
                    {
                        var sp = DsSanPham?.FirstOrDefault(s => s.Ma == g.Key);
                        return new ThongKeSanPham
                        {
                            MaSP = g.Key,
                            TenSP = sp?.Ten ?? g.Key,
                            TongSoLuong = g.Sum(ct => ct.SoLuongChiTiet ?? 0),
                            TongTien = g.Sum(ct => ct.GiaChiTiet ?? 0)
                        };
                    })
                    .OrderByDescending(x => x.TongSoLuong)
                    .Take(10)
                    .ToList()) ?? new List<ThongKeSanPham>();

                for (int i = 0; i < topSP.Count; i++)
                {
                    topSP[i].ViTri = i + 1;
                }
                DsTopSanPham = new ObservableCollection<ThongKeSanPham>(topSP);

                var tkBan = hoaDonDaThanhToan
                    .GroupBy(hd => hd.MaBan)
                    .Select(g =>
                    {
                        var ban = DsBan?.FirstOrDefault(b => string.Equals(b.MaBan?.Trim(), g.Key?.Trim(), StringComparison.OrdinalIgnoreCase));
                        return new ThongKeBan
                        {
                            MaBan = g.Key,
                            TenBan = ban?.TenBan ?? g.Key,
                            SoHoaDon = g.Count(),
                            TongTien = g.Sum(hd => hd.TongTienHoaDon ?? 0)
                        };
                    })
                    .OrderByDescending(x => x.TongTien)
                    .ToList();

                DsThongKeBan = new ObservableCollection<ThongKeBan>(tkBan);
                MaxTongTienBan = DsThongKeBan.Any() ? DsThongKeBan.Max(x => x.TongTien) : 0;

                var sourceKhach = _allKhachHang ?? DsKhachHang?.ToList() ?? new List<KhachHang>();
                var topKH = sourceKhach
                    .OrderByDescending(k => k.DiemTichLuy ?? 0)
                    .Take(10)
                    .Select((k, i) => new KhachHangThongKe
                    {
                        ViTri = i + 1,
                        MaKhachHang = k.MaKhachHang,
                        TenKhachHang = k.TenKhachHang,
                        SoDienThoai = k.SoDienThoai,
                        DiemTichLuy = k.DiemTichLuy ?? 0
                    })
                    .ToList();

                DsTopKhachHang = new ObservableCollection<KhachHangThongKe>(topKH);
            }
            catch (Exception ex)
            {
                DebugLog = $"LoadThongKe failed: {ex.Message}";
                MessageBox.Show($"LoadThongKe error: {ex.Message}\n{ex.StackTrace}", "Debug", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===================== LẤY DỮ LIỆU TỪ DATABASE =====================
        public void LoadData_Kho()
        {
            if (db == null) return;
            try
            {
                DsKho = new ObservableCollection<Kho>(db.Khoes.ToList());
            }
            catch
            {
                DsKho = new ObservableCollection<Kho>();
                throw;
            }
            RefreshKhoGroups();
            ApplyProductFilter();
        }
        
        public void LoadData_PhieuNhap()
        {
            if (db == null) return;
            try
            {
                DsPhieuNhap = new ObservableCollection<PhieuNhap>(db.PhieuNhaps.ToList());
            }
            catch
            {
                DsPhieuNhap = new ObservableCollection<PhieuNhap>();
                throw;
            }
            OnPropertyChanged(nameof(DsPhieuNhap));
        }

        private void EnsureRuntimeSchema()
        {
            if (db == null) return;

            EnsureTable("Ban");
            EnsureColumn("Ban", "MaBan", "NVARCHAR(50) NULL");
            EnsureColumn("Ban", "TenBan", "NVARCHAR(50) NULL");
            EnsureColumn("Ban", "KieuBan", "NVARCHAR(50) NULL");
            EnsureColumn("Ban", "TrangThaiBan", "BIT NULL");

            EnsureTable("BangChamCong");
            EnsureColumn("BangChamCong", "MaBCC", "NVARCHAR(50) NULL");
            EnsureColumn("BangChamCong", "MaNV", "NVARCHAR(50) NULL");
            EnsureColumn("BangChamCong", "Ngay", "DATETIME NULL");
            EnsureColumn("BangChamCong", "GioVao", "TIME NULL");
            EnsureColumn("BangChamCong", "GioRa", "TIME NULL");
            EnsureColumn("BangChamCong", "IsPay", "BIT NULL DEFAULT 0");

            EnsureTable("ChiTietHoaDon");
            EnsureColumn("ChiTietHoaDon", "MaHoaDon", "NVARCHAR(50) NULL");
            EnsureColumn("ChiTietHoaDon", "MaThucDon", "NVARCHAR(50) NULL");
            EnsureColumn("ChiTietHoaDon", "GiaChiTiet", "FLOAT NULL");
            EnsureColumn("ChiTietHoaDon", "SoLuongChiTiet", "INT NULL");

            EnsureTable("HoaDon");
            EnsureColumn("HoaDon", "MaHoaDon", "NVARCHAR(50) NULL");
            EnsureColumn("HoaDon", "MaBan", "NVARCHAR(50) NULL");
            EnsureColumn("HoaDon", "MaKhachHang", "NVARCHAR(50) NULL");
            EnsureColumn("HoaDon", "NgayHoaDon", "DATETIME NULL");
            EnsureColumn("HoaDon", "GioVaoHoaDon", "TIME NULL");
            EnsureColumn("HoaDon", "GioRaHoaDon", "TIME NULL");
            EnsureColumn("HoaDon", "TrangThaiThanhToan", "BIT NULL");
            EnsureColumn("HoaDon", "TongTienHoaDon", "FLOAT NULL");
            EnsureColumn("HoaDon", "MaNV", "NVARCHAR(50) NULL");

            EnsureTable("KhachHang");
            EnsureColumn("KhachHang", "MaKhachHang", "NVARCHAR(50) NULL");
            EnsureColumn("KhachHang", "TenKhachHang", "NVARCHAR(100) NULL");
            EnsureColumn("KhachHang", "SoDienThoai", "NVARCHAR(20) NULL");
            EnsureColumn("KhachHang", "DiemTichLuy", "INT NULL");

            EnsureTable("NhanVien");
            EnsureColumn("NhanVien", "MaNV", "NVARCHAR(50) NULL");
            EnsureColumn("NhanVien", "TenNV", "NVARCHAR(100) NULL");
            EnsureColumn("NhanVien", "NgaySinh", "DATETIME NULL");
            EnsureColumn("NhanVien", "GioiTinh", "BIT NULL");
            EnsureColumn("NhanVien", "SDT", "NVARCHAR(20) NULL");
            EnsureColumn("NhanVien", "DiaChi", "NVARCHAR(200) NULL");
            EnsureColumn("NhanVien", "ChucVu", "NVARCHAR(50) NULL");
            EnsureColumn("NhanVien", "LuongCB", "FLOAT NULL");
            EnsureColumn("NhanVien", "NgayVL", "DATETIME NULL");
            EnsureColumn("NhanVien", "TaiKhoan", "NVARCHAR(50) NULL");
            EnsureColumn("NhanVien", "MatKhau", "NVARCHAR(50) NULL");

            EnsureTable("PhieuNhap");
            EnsureColumn("PhieuNhap", "Ma", "NVARCHAR(50) NULL");
            EnsureColumn("PhieuNhap", "NgayNhap", "DATETIME NULL");

            EnsureTable("SanPham");
            EnsureColumn("SanPham", "Ma", "NVARCHAR(50) NULL");
            EnsureColumn("SanPham", "Ten", "NVARCHAR(100) NULL");
            EnsureColumn("SanPham", "DonGia", "FLOAT NULL");
            EnsureColumn("SanPham", "HinhAnh", "NVARCHAR(MAX) NULL");

            EnsureTable("Kho");
            EnsureColumn("Kho", "MaSP", "NVARCHAR(50) NULL");
            EnsureColumn("Kho", "TenSanPham", "NVARCHAR(100) NULL");
            EnsureColumn("Kho", "TonKho", "INT NULL DEFAULT 0");
            EnsureColumn("Kho", "DonViTinh", "NVARCHAR(20) NULL");

            CopyColumnIfExists("Kho", "DonVi", "DonViTinh");
        }

        private void EnsureTable(string tableName)
        {
            db.Database.ExecuteSqlCommand($@"
            IF OBJECT_ID(N'dbo.{tableName}', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.{tableName} (__SchemaPlaceholder INT NULL);
            END");
        }

        private void EnsureColumn(string tableName, string columnName, string definition)
        {
            db.Database.ExecuteSqlCommand($@"
            IF COL_LENGTH('dbo.{tableName}', '{columnName}') IS NULL
            BEGIN
                ALTER TABLE dbo.{tableName} ADD {columnName} {definition};
            END");
        }

        private void CopyColumnIfExists(string tableName, string sourceColumn, string targetColumn)
        {
            db.Database.ExecuteSqlCommand($@"
            IF COL_LENGTH('dbo.{tableName}', '{sourceColumn}') IS NOT NULL
            BEGIN
                EXEC sp_executesql N'UPDATE dbo.{tableName} SET {targetColumn} = {sourceColumn} WHERE {targetColumn} IS NULL';
            END");
        }

        private string GetExceptionMessage(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            return ex.Message;
        }

        private void UpdateNgayHoaDon(string maHoaDon, DateTime ngayHoaDon)
        {
            db.Database.ExecuteSqlCommand(
                "UPDATE dbo.HoaDon SET NgayHoaDon = @p0 WHERE MaHoaDon = @p1",
                ngayHoaDon, maHoaDon);
        }

        private void RefreshKhoGroups()
        {
            // No longer needed - database simplified without product groups
        }

        private string NormalizeString(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var formD = s.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var ch in formD)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            var cleaned = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
            return cleaned.ToLowerInvariant();
        }

        private void ApplyProductFilter()
        {
            var list = new List<ProductItem>();

            if (DsKho != null)
            {
                var qry = DsKho.AsEnumerable();
                
                // Filter by search text
                if (!string.IsNullOrWhiteSpace(SearchProductText))
                {
                    var key = SearchProductText.ToLower();
                    qry = qry.Where(k => (k.TenSanPham ?? string.Empty).ToLower().Contains(key));
                }

                foreach (var kho in qry)
                {
                    list.Add(new ProductItem
                    {
                        Ma = kho.MaSP,
                        TenSanPham = kho.TenSanPham,
                        TonDu = kho.TonKho ?? 0,
                        DonVi = kho.DonViTinh ?? "",
                        Source = kho.SanPham
                    });
                }
            }

            FilteredSanPham = new ObservableCollection<ProductItem>(list);
            OnPropertyChanged(nameof(FilteredSanPham));
        }

        private void ApplyMenuFilter()
        {
            if (DsSanPham == null) return;
            var q = DsSanPham.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(MenuSearchText))
            {
                var k = MenuSearchText.ToLower();
                q = q.Where(p => p.Ten != null && p.Ten.ToLower().Contains(k));
            }
            if (MenuSortOrder == "A -> Z") q = q.OrderBy(p => p.Ten);
            else if (MenuSortOrder == "Z -> A") q = q.OrderByDescending(p => p.Ten);
            else if (MenuSortOrder == "Giá tăng dần") q = q.OrderBy(p => p.DonGia ?? 0);
            else if (MenuSortOrder == "Giá giảm dần") q = q.OrderByDescending(p => p.DonGia ?? 0);
            MenuFilteredSanPham = new ObservableCollection<SanPham>(q);
            OnPropertyChanged(nameof(MenuFilteredSanPham));
        }

        private void ApplyNhanVienFilter()
        {
            if (DsNhanVien == null) return;
            var q = DsNhanVien.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchNhanVien))
            {
                var k = SearchNhanVien.ToLower();
                q = q.Where(nv => 
                    (nv.TenNV != null && nv.TenNV.ToLower().Contains(k)) ||
                    (nv.MaNV != null && nv.MaNV.ToLower().Contains(k)) ||
                    (nv.ChucVu != null && nv.ChucVu.ToLower().Contains(k))
                );
            }
            q = q.OrderBy(nv => nv.TenNV);
            FilteredNhanVien = new ObservableCollection<NhanVien>(q);
            OnPropertyChanged(nameof(FilteredNhanVien));
        }

        private int GetAvailableInventory(SanPham product)
        {
            if (product == null || string.IsNullOrWhiteSpace(product.Ma))
                return 0;
            var kho = DsKho?.FirstOrDefault(k => k.MaSP == product.Ma);
            return kho?.TonKho ?? 0;
        }

        private void AddToOrder(SanPham p)
        {
            if (p == null) return;

            int availableQty = GetAvailableInventory(p);
            if (availableQty <= 0)
            {
                MessageBox.Show($"Sản phẩm '{p.Ten}' hết hàng. Vui lòng chọn sản phẩm khác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var line = SelectedOrderItems.FirstOrDefault(x => x.Product.Ma == p.Ma);
            if (line == null)
            {
                line = new OrderLine { Product = p, Quantity = 1 };
                SelectedOrderItems.Add(line);
            }
            else
            {
                if (line.Quantity >= availableQty)
                {
                    MessageBox.Show($"Số lượng tồn kho không đủ. Tồn kho hiện tại: {availableQty}", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                line.Quantity++;
            }
            OnPropertyChanged(nameof(CurrentOrderTotal));
            (PlaceOrderCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void PlaceOrder()
        {
            // 1. Kiểm tra an toàn dữ liệu đầu vào
            if (SelectedOrderItems == null || SelectedOrderItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn món ăn trước khi đặt!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Giao diện MenuView sử dụng thuộc tính SelectedBan hoặc SelectedBanForOrder làm ComboBox chọn bàn
            // Ở đây ta dùng SelectedBan theo logic đồng bộ giao diện của bạn
            if (SelectedBan == null)
            {
                MessageBox.Show("Vui lòng chọn bàn cần thêm món ở Menu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (db == null) db = new QUANLYBIDAEntities1();

                // 2. Tìm hoặc Tự động mở bàn nếu bàn đang ở trạng thái trống (False)
                var banDb = db.Bans.FirstOrDefault(b => b.MaBan == SelectedBan.MaBan);
                if (banDb == null) return;

                // Tìm hóa đơn chưa thanh toán của bàn này (TrangThaiThanhToan == false)
                var hoaDon = DsHoaDon?.FirstOrDefault(h => h.MaBan == SelectedBan.MaBan && h.TrangThaiThanhToan == false);

                // NẾU BÀN TRỐNG HOẶC CHƯA CÓ HÓA ĐƠN ĐANG CHẠY
                if (banDb.TrangThaiBan != true || hoaDon == null)
                {
                    // Tự động sinh mã hóa đơn mới bằng hàm GenMaHD() có sẵn của bạn
                    string maHD = GenMaHD();

                    hoaDon = new HoaDon
                    {
                        MaHoaDon = maHD, // Khớp với DB của bạn
                        MaBan = SelectedBan.MaBan,
                        MaNV = CurrentAccount?.MaNV,
                        NgayHoaDon = DateTime.Now,
                        GioVaoHoaDon = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second),
                        TrangThaiThanhToan = false,
                        TongTienHoaDon = 0
                    };

                    db.HoaDons.Add(hoaDon);
                    DsHoaDon?.Add(hoaDon);

                    // Chuyển trạng thái bàn sang TRUE (Có khách)
                    banDb.TrangThaiBan = true;
                    SelectedBan.TrangThaiBan = true; // Cập nhật trạng thái hiển thị UI Local
                }

                // 3. Thêm các món ăn được chọn từ MenuView vào ChiTietHoaDon
                foreach (var orderLine in SelectedOrderItems)
                {
                    // Kiểm tra xem món ăn này đã tồn tại trong ChiTietHoaDon của hóa đơn hiện tại chưa
                    var chiTietTonTai = db.ChiTietHoaDons.FirstOrDefault(ct => ct.MaHoaDon == hoaDon.MaHoaDon && ct.MaThucDon == orderLine.Product.Ma);

                    if (chiTietTonTai != null)
                    {
                        // Nếu đã có món này rồi thì cộng dồn số lượng và cập nhật giá chi tiết
                        chiTietTonTai.SoLuongChiTiet += orderLine.Quantity;
                        chiTietTonTai.GiaChiTiet = chiTietTonTai.SoLuongChiTiet * (orderLine.Product.DonGia ?? 0);
                    }
                    else
                    {
                        // Nếu chưa có thì tạo mới bản ghi chi tiết hóa đơn
                        var ct = new ChiTietHoaDon
                        {
                            MaHoaDon = hoaDon.MaHoaDon,       // Khớp tên thuộc tính hệ thống của bạn
                            MaThucDon = orderLine.Product.Ma, // Mã món ăn (Product.Ma)
                            SoLuongChiTiet = orderLine.Quantity,
                            GiaChiTiet = orderLine.Quantity * (orderLine.Product.DonGia ?? 0)
                        };
                        db.ChiTietHoaDons.Add(ct);
                        DsChiTietHoaDon?.Add(ct);
                    }
                }

                // 4. TRỪ TỒN KHO NGAY KHI ĐẶT MÓN: giảm số lượng trong bảng Kho theo các món vừa thêm
                try
                {
                    foreach (var orderLine in SelectedOrderItems)
                    {
                        var sp = db.SanPhams.Find(orderLine.Product.Ma);
                        if (sp != null && !string.IsNullOrWhiteSpace(sp.Ma))
                        {
                            var khoDb = db.Khoes.Find(sp.Ma);
                            if (khoDb != null)
                            {
                                int qty = orderLine.Quantity;
                                var newTon = (khoDb.TonKho ?? 0) - qty;
                                khoDb.TonKho = newTon < 0 ? 0 : newTon;

                                var localKho = DsKho?.FirstOrDefault(k => k.MaSP == khoDb.MaSP);
                                if (localKho != null) localKho.TonKho = khoDb.TonKho;
                            }
                        }
                    }
                }
                catch { }

                // 4.5 CẬP NHẬT TỔNG TIỀN HÓA ĐƠN NGAY KHI ĐẶT MÓN
                // Tính tổng tiền từ tất cả chi tiết hóa đơn của bàn này
                var chiTietList = db.ChiTietHoaDons.Where(ct => ct.MaHoaDon == hoaDon.MaHoaDon).ToList();
                double tongTienMon = chiTietList.Sum(ct => ct.GiaChiTiet ?? 0);
                
                // Nếu có giờ vào thì cộng thêm tiền bàn
                double tongTienBan = 0;
                if (hoaDon.GioVaoHoaDon.HasValue)
                {
                    var gioHienTai = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    tongTienBan = BilliardCalculator.CalculateTableFee(hoaDon.GioVaoHoaDon, gioHienTai);
                }
                
                hoaDon.TongTienHoaDon = tongTienMon + tongTienBan;

                // 5. Lưu toàn bộ thay đổi xuống Database
                db.SaveChanges();

                // 5. Cập nhật lại danh sách hóa đơn mở công khai
                UpdateDsBanMo();

                // 6. Ép giao diện Tình trạng bàn nạp lại danh sách món và tổng tiền lập tức
                // Sửa lỗi: Hàm gốc LoadChiTietBan() không nhận tham số
                LoadChiTietBan();

                // Thông báo cập nhật thuộc tính cho UI cập nhật đầy đủ thông tin bàn
                OnPropertyChanged(nameof(DsMonAnCuaBan));
                OnPropertyChanged(nameof(TongTienHienTai));
                OnPropertyChanged(nameof(GioVaoBan));
                OnPropertyChanged(nameof(TrangThaiHienTai));

                // 7. Làm trống danh sách order tạm thời trên MenuView sau khi đặt xong
                SelectedOrderItems.Clear();
                OnPropertyChanged(nameof(CurrentOrderTotal));

                MessageBox.Show($"Đặt món thành công cho {banDb.TenBan}!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi đặt món: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanPlaceOrder(object obj)
        {
            // ĐIỀU KIỆN: Nút ĐẶT BÀN chỉ sáng lên khi:
            // 1. Danh sách món ăn được chọn có ít nhất 1 món (Count > 0)
            // 2. Nhân viên đã chọn một bàn cụ thể ở ComboBox
            return SelectedOrderItems != null && SelectedOrderItems.Count > 0 && SelectedBan != null;
        }
        private void CheckStock()
        {
            ApplyProductFilter();
            if (SelectedSanPhamKho != null)
            {
                MessageBox.Show($"Sản phẩm: {SelectedSanPhamKho.TenSanPham}\nTồn dư: {SelectedSanPhamKho.TonDu}", "Kiểm tra tồn kho", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var list = FilteredSanPham ?? new ObservableCollection<ProductItem>();
            int count = list.Count;
            int totalTonDu = list.Sum(p => p.TonDu);
            var groupName = string.IsNullOrWhiteSpace(SelectedNhom) ? "Tất cả" : SelectedNhom;
            MessageBox.Show($"Số sản phẩm: {count}\nTổng tồn dư: {totalTonDu}", "Kiểm tra tồn kho", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportProducts()
        {
            try
            {
                var dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "SanPham";
                dlg.DefaultExt = ".csv";
                dlg.Filter = "CSV files (.csv)|*.csv";
                if (dlg.ShowDialog() == true)
                {
                    using (var sw = new System.IO.StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8))
                    {
                        sw.WriteLine("Mã,Tên,Tồn dư,Đơn vị");
                        foreach (var item in FilteredSanPham ?? new ObservableCollection<ProductItem>())
                        {
                            sw.WriteLine($"{item.Ma},{item.TenSanPham},{item.TonDu},{item.DonVi}");
                        }
                    }
                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            { MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void DeleteProduct()
        {
            if (SelectedMenuProduct != null)
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc muốn xóa món '{SelectedMenuProduct.Ten}' khỏi menu không?",
                    "Xóa món",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                try
                {
                    db.Database.ExecuteSqlCommand("DELETE FROM dbo.SanPham WHERE Ma = @p0", SelectedMenuProduct.Ma);
                    var item = DsSanPham?.FirstOrDefault(x => x.Ma == SelectedMenuProduct.Ma);
                    if (item != null)
                    {
                        DsSanPham.Remove(item);
                    }

                    SelectedMenuProduct = null;
                    ApplyMenuFilter();
                    ApplyProductFilter();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa món này. Món có thể đã phát sinh hóa đơn.\n\n" + GetExceptionMessage(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return;
            }

            MessageBox.Show("Vui lòng chọn món cần xóa trong danh sách menu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewProductDetail()
        {
            if (SelectedMenuProduct != null)
            {
                EditProductMa = SelectedMenuProduct.Ma;
                EditProductTen = SelectedMenuProduct.Ten;
                EditProductDonGia = SelectedMenuProduct.DonGia;
                EditProductHinhAnh = SelectedMenuProduct.HinhAnh;
                IsEditingProduct = true;
                return;
            }

            if (SelectedSanPhamKho == null) return;
            MessageBox.Show($"Mã: {SelectedSanPhamKho.Ma}\nTên: {SelectedSanPhamKho.TenSanPham}\nTồn dư: {SelectedSanPhamKho.TonDu}\nĐơn vị: {SelectedSanPhamKho.DonVi}", "Chi tiết sản phẩm", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GenerateNextKhoId()
        {
            try
            {
                var ids = DsKho?.Select(x => x.MaSP).ToList();
                if (ids == null || ids.Count == 0)
                    return "K001";
                var max = ids.Select(x =>
                {
                    int n;
                    var digits = new string((x ?? "").Where(char.IsDigit).ToArray());
                    return int.TryParse(digits, out n) ? n : 0;
                }).Max();
                return $"K{(max + 1):D3}";
            }
            catch { return "K001"; }
        }

        private string GenerateNextProductId()
        {
            try
            {
                var ids = DsSanPham?.Select(x => x.Ma).ToList();
                if (ids == null || ids.Count == 0)
                    return "SP001";
                var max = ids.Select(x =>
                {
                    int n;
                    return int.TryParse(x?.Replace("SP", "") ?? "", out n) ? n : 0;
                }).Max();
                return $"SP{(max + 1):D3}";
            }
            catch { return "SP001"; }
        }

        private void ChangeEditImage()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                EditProductHinhAnh = dlg.FileName;
            }
        }

        private void AddImageForNewProduct()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                NewProductHinhAnh = dlg.FileName;
            }
        }

        private bool CanConfirmNewKho()
        {
            if (NewKhoSelectedSanPham == null) return false;
            if (NewKhoSoLuong <= 0) return false;
            if (string.IsNullOrWhiteSpace(NewKhoDonViTinh)) return false;
            if (!NewKhoNgayNhap.HasValue) return false;
            return true;
        }

        public void ConfirmNewKho()
        {
            try
            {
                // Parse GiaNhap from text input
                double giaNhap = 0;
                if (!string.IsNullOrWhiteSpace(NewKhoGiaNhapText) && double.TryParse(NewKhoGiaNhapText, out double parsedPrice))
                {
                    giaNhap = parsedPrice;
                }

                // Get the selected product
                if (NewKhoSelectedSanPham == null)
                    throw new InvalidOperationException("Vui lòng chọn sản phẩm.");

                var productCode = NewKhoSelectedSanPham.Ma;
                var sanPham = db.SanPhams.FirstOrDefault(s => s.Ma == productCode);
                
                if (sanPham == null)
                    throw new InvalidOperationException("Sản phẩm không tìm thấy trong cơ sở dữ liệu.");

                // Update SanPham's price if a price is provided
                if (giaNhap > 0)
                {
                    sanPham.DonGia = giaNhap;
                }


                // Generate next MaPN and ensure uniqueness (recheck against DB)
                var nextMaPN = GenMaPN();
                int safety = 0;
                while (db.PhieuNhaps.Any(p => p.MaPN == nextMaPN) && safety < 1000)
                {
                    // bump numeric suffix until unique
                    var numPart = nextMaPN.Substring(2);
                    if (!int.TryParse(numPart, out int n)) n = 0;
                    n++;
                    nextMaPN = $"PN{n:000}";
                    safety++;
                }

                // Add PhieuNhap entry
                var phieuNhap = new PhieuNhap
                {
                    MaPN = nextMaPN,
                    MaSP = productCode,
                    SoLuong = NewKhoSoLuong,
                    GiaNhap = giaNhap,
                    NgayNhap = NewKhoNgayNhap ?? DateTime.Now
                };
                db.PhieuNhaps.Add(phieuNhap);

                // Update Kho inventory or create new Kho record
                var kho = db.Khoes.FirstOrDefault(k => k.MaSP == productCode);
                if (kho != null)
                {
                    kho.TonKho = (kho.TonKho ?? 0) + NewKhoSoLuong;
                    kho.DonViTinh = NewKhoDonViTinh;
                }
                else
                {
                    db.Khoes.Add(new Kho
                    {
                        MaSP = productCode,
                        TenSanPham = sanPham.Ten,
                        DonViTinh = NewKhoDonViTinh,
                        TonKho = NewKhoSoLuong
                    });
                }

                db.SaveChanges();
                
                // Clear form fields
                NewKhoMaSPKho = "";
                NewKhoMaSP = "";
                NewKhoTenSanPham = "";
                NewKhoSoLuong = 0;
                NewKhoDonViTinh = "";
                NewKhoGiaNhapText = "";
                NewKhoNgayNhap = null;
                NewKhoSelectedSanPham = null;

                // Reload data
                LoadData_Kho();
                LoadData_PhieuNhap();
                UpdateSelectedMenuProductTonKho();
                IsAddingKho = false;
                RefreshKhoGroups();
                ApplyProductFilter();
            }
            catch (Exception ex)
            { MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private bool CanSaveEditProduct()
        {
            return !string.IsNullOrWhiteSpace(EditProductTen) && EditProductDonGia.HasValue && EditProductDonGia.Value > 0;
        }

        private void SaveEditProduct()
        {
            try
            {
                var entity = db.SanPhams.Find(EditProductMa);
                if (entity != null)
                {
                    entity.Ten = EditProductTen;
                    entity.DonGia = EditProductDonGia;
                    entity.HinhAnh = EditProductHinhAnh;
                    db.SaveChanges();

                    var sp = DsSanPham.FirstOrDefault(x => x.Ma == EditProductMa);
                    if (sp != null)
                    {
                        sp.Ten = EditProductTen;
                        sp.DonGia = EditProductDonGia;
                        sp.HinhAnh = EditProductHinhAnh;

                        if (!string.IsNullOrWhiteSpace(sp.Ma))
                        {
                            var kho = DsKho?.FirstOrDefault(x => x.MaSP == sp.Ma);
                            if (kho != null)
                            {
                                kho.TenSanPham = EditProductTen;
                            }

                            db.Database.ExecuteSqlCommand(
                                "UPDATE dbo.Kho SET TenSanPham = @p0 WHERE MaSP = @p1",
                                EditProductTen, sp.Ma);
                        }
                    }
                    ApplyMenuFilter();
                    ApplyProductFilter();
                    SelectedMenuProduct = MenuFilteredSanPham?.FirstOrDefault(x => x.Ma == EditProductMa);
                    IsEditingProduct = false;
                }
            }
            catch (Exception ex)
            { MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CancelEditProduct()
        {
            IsEditingProduct = false;
        }

        private void OpenAddKho()
        {
            // Generate receipt code
            string maPN = GenMaPN();
            
            NewKhoMaSPKho = maPN;
            NewKhoMaSP = GenerateNextKhoId();
            NewKhoTenSanPham = string.Empty;
            NewKhoSoLuong = 0;
            NewKhoDonViTinh = string.Empty;
            NewKhoGiaNhapText = string.Empty;
            NewKhoNgayNhap = DateTime.Now;
            NewKhoSelectedSanPham = null;

            var window = new AddKhoWindow
            {
                DataContext = this,
                Owner = Application.Current?.MainWindow
            };

            window.ShowDialog();
            IsAddingKho = false;
        }

        private bool CanConfirmAddProduct()
        {
            return !string.IsNullOrWhiteSpace(NewProductMa)
                && !string.IsNullOrWhiteSpace(NewProductTen)
                && NewProductDonGia.HasValue
                && NewProductDonGia.Value > 0;
        }

        private void ConfirmAddProduct()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewProductMa))
                    return;

                if (DsSanPham.Any(s => s.Ma == NewProductMa))
                {
                    MessageBox.Show($"Sản phẩm '{NewProductTen}' đã tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var sp = new SanPham
                {
                    Ma = NewProductMa,
                    Ten = NewProductTen,
                    DonGia = NewProductDonGia,
                    HinhAnh = NewProductHinhAnh,
                };
                db.Database.ExecuteSqlCommand(
                    @"INSERT INTO dbo.SanPham (Ma, Ten, DonGia, HinhAnh)
                      VALUES (@p0, @p1, @p2, @p3)",
                    sp.Ma, sp.Ten, sp.DonGia, sp.HinhAnh);
                DsSanPham.Add(sp);
                IsAddingProduct = false;
                ApplyMenuFilter();
                SelectedMenuProduct = MenuFilteredSanPham?.FirstOrDefault(x => x.Ma == sp.Ma);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelAddProduct()
        {
            IsAddingProduct = false;
        }

        private void OpenAddProduct()
        {
            NewProductMa = GenerateNextProductId();
            NewProductTen = string.Empty;
            NewProductDonGia = null;
            NewProductHinhAnh = null;
            SelectedKhoForNewProduct = null;
            SelectedMenuProduct = null;
            IsAddingProduct = true;
        }

        private void NewProduct()
        {
            MessageBox.Show("Chức năng thêm sản phẩm mới chưa triển khai giao diện. Vui lòng thêm trực tiếp vào cơ sở dữ liệu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        // ===================== LẤY DỮ LIỆU TỪ DATABASE =====================
        public void LoadData_Ban()
        {
            if (db == null) return;
            DsBan = new ObservableCollection<Ban>(db.Bans.ToList());
            UpdateDsBanMo();
            ApplyFilterBan(); //Ban-Tran Nguyen - cập nhật DsBanHienThi ngay sau load
        }

        private void UpdateDsBanMo()
        {
            if (DsBan == null) return;
            DsBanMo = new ObservableCollection<Ban>(DsBan.Where(b => b.TrangThaiBan == true).ToList());
        }
        public void LoadData_BangChamCong()
        {
            if (db == null) return;
            DsBangChamCong = new ObservableCollection<BangChamCong>(db.BangChamCongs.ToList());
            // create view model collection for UI
            var list = DsBangChamCong.Select(b => new ChamCongItem
            {
                MaBCC = b.MaBCC,
                MaNV = b.MaNV,
                TenNV = DsNhanVien?.FirstOrDefault(n => n.MaNV == b.MaNV)?.TenNV ?? b.MaNV,
                ChucVu = DsNhanVien?.FirstOrDefault(n => n.MaNV == b.MaNV)?.ChucVu ?? "",
                Ngay = b.Ngay ?? DateTime.Today,
                SoGio = CalculateSoGioFromBangChamCong(b),
                IsPay = b.IsPay ?? false,
                SoTien = (b.IsPay ?? false) ? CalculateSoTienFromBangChamCong(b) : 0
            }).ToList();
            DsBangChamCongView = new ObservableCollection<ChamCongItem>(list);
        }

        private double CalculateSoGioFromBangChamCong(BangChamCong b)
        {
            var start = b.GioVao ?? TimeSpan.Zero;
            var end = b.GioRa ?? TimeSpan.Zero;
            var duration = end - start;
            return duration.TotalHours > 0 ? duration.TotalHours : 0;
        }

        private double CalculateSoTienFromBangChamCong(BangChamCong b)
        {
            return CalculateSoGioFromBangChamCong(b) * PayRatePerHour;
        }

        public void LoadData_HoaDon()
        {
            if (db == null) return;
            var hoaDons = db.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .ToList();

            try
            {
                var dates = db.Database.SqlQuery<HoaDonNgayRow>(
                    "SELECT MaHoaDon, NgayHoaDon FROM dbo.HoaDon WHERE NgayHoaDon IS NOT NULL")
                    .ToDictionary(x => x.MaHoaDon, x => x.NgayHoaDon);

                foreach (var hoaDon in hoaDons)
                {
                    if (!string.IsNullOrWhiteSpace(hoaDon.MaHoaDon) && dates.TryGetValue(hoaDon.MaHoaDon, out var ngayHoaDon))
                    {
                        hoaDon.NgayHoaDon = ngayHoaDon;
                    }
                }
            }
            catch
            {
                throw;
            }

            DsHoaDon = new ObservableCollection<HoaDon>(hoaDons);
        }
        public void LoadData_ChiTietHoaDon()
        {
            if (db == null) return;
            DsChiTietHoaDon = new ObservableCollection<ChiTietHoaDon>(db.ChiTietHoaDons.ToList());
        }
        public void LoadData_KhachHang()
        {
            if (db == null) return;
            _allKhachHang = db.KhachHangs.ToList(); // ← thêm dòng này
            DsKhachHang = new ObservableCollection<KhachHang>(db.KhachHangs.ToList());
            var listKH = db.KhachHangs.Where(k => k.TenKhachHang != "Khách thường").ToList();
            DsKhachHang = new ObservableCollection<KhachHang>(listKH);
            // Khởi tạo danh sách lọc với toàn bộ danh sách
            FilteredDsKhachHang = new ObservableCollection<KhachHang>(DsKhachHang);
        }
        public void LoadData_SanPham()
        {
            if (db == null) return;
            DsSanPham = new ObservableCollection<SanPham>(db.SanPhams.ToList());
            MenuFilteredSanPham = new ObservableCollection<SanPham>(DsSanPham);
            RefreshKhoGroups();
            ApplyMenuFilter();
            ApplyProductFilter();
        }

        private class HoaDonNgayRow
        {
            public string MaHoaDon { get; set; }
            public DateTime NgayHoaDon { get; set; }
        }
        public void LoadData_NhanVien()
        {
            if (db == null) return;
            DsNhanVien = new ObservableCollection<NhanVien>(db.NhanViens.ToList());
            ApplyNhanVienFilter(); // Khởi tạo danh sách lọc
        }

        private void UpdateDebugLog(string note)
        {
            var banCount = DsBan?.Count ?? 0;
            var hoaDonCount = DsHoaDon?.Count ?? 0;
            var khCount = DsKhachHang?.Count ?? 0;
            var ctCount = DsChiTietHoaDon?.Count ?? 0;
            var spCount = DsSanPham?.Count ?? 0;
            var tkBanCount = DsThongKeBan?.Count ?? 0;
            var topKhCount = DsTopKhachHang?.Count ?? 0;
            DebugLog = $"{note}: Bàn={banCount}, Hóa đơn={hoaDonCount}, Khách hàng={khCount}, Chi tiết={ctCount}, SP={spCount}, Thống kê bàn={tkBanCount}, TopKH={topKhCount}";
        }
        public void LoadFullData()
        {
            LoadData_Ban();
            LoadData_BangChamCong();
            LoadData_HoaDon();
            LoadData_KhachHang();
            LoadData_NhanVien();
            LoadData_PhieuNhap();
            LoadData_Kho();
            LoadData_SanPham();
            LoadData_ChiTietHoaDon();
            LoadThongKe();
            UpdateDebugLog("LoadFullData completed");
        }
        // ===================== MAINVIEWMODEL =====================
        public MainViewModel()
        {
            _banVM = new BanView();
            _dnVM = new DangNhapView();
            _khVM = new KhachHangView();
            _khoVM = new KhoView();
            _lsVM = new LichSuHoaDonView();
            _nvVM = new NhanVienView();
            _tkVM = new ThongKeView();
            _menuVM = new MenuView();
            _adminMenuVM = new AdminMenuView();
            _tcVM = new TrangChuView();
            // ===================== LOAD DỮ LIỆU =====================
            DsChucVu = new ObservableCollection<string>()
            {
                "Quản lý",
                "Thu ngân",
                "Phục vụ",
                "Giữ xe"
            };
            FilteredSanPham = new ObservableCollection<ProductItem>();
            DsKho = new ObservableCollection<Kho>();
            DsNhomSanPham = new ObservableCollection<string>();
            MenuFilteredSanPham = new ObservableCollection<SanPham>();

            // ===================== THỐNG KÊ ========================
            DsTopSanPham = new ObservableCollection<ThongKeSanPham>();
            DsThongKeBan = new ObservableCollection<ThongKeBan>();
            DsTopKhachHang = new ObservableCollection<KhachHangThongKe>();
            RefreshThongKeCommand = new RelayCommand(o => LoadThongKe());
            BaoCaoHieuSuatCommand = new RelayCommand(o =>
            {
                try
                {
                    var result = ReportVM.GenerateReport(ReportFromDate, ReportToDate);
                    // populate collections grouped by KieuBan and calculate totals
                    ReportPoolRows.Clear();
                    ReportLipRows.Clear();
                    ReportPoolTotalLuot = 0;
                    ReportPoolTotalGio = 0.0;
                    ReportLipTotalLuot = 0;
                    ReportLipTotalGio = 0.0;
                    foreach (var r in result.Rows)
                    {
                        var k = (r.KieuBan ?? string.Empty).ToLower();
                        if (k.Contains("pool"))
                        {
                            ReportPoolRows.Add(r);
                            ReportPoolTotalLuot += r.SoLuotChoi;
                            ReportPoolTotalGio += r.TongGio;
                        }
                        else if (k.Contains("lip") || k.Contains("carom") || k.Contains("phăng") || k.Contains("phang") )
                        {
                            ReportLipRows.Add(r);
                            ReportLipTotalLuot += r.SoLuotChoi;
                            ReportLipTotalGio += r.TongGio;
                        }
                        else
                        {
                            // default to pool if unclear
                            ReportPoolRows.Add(r);
                            ReportPoolTotalLuot += r.SoLuotChoi;
                            ReportPoolTotalGio += r.TongGio;
                        }
                    }
                    // compute efficiencies per group
                    int days = (ReportToDate.Date - ReportFromDate.Date).Days + 1;
                    if (days <= 0) days = 1;
                    // count tables by KieuBan from db
                    int poolTableCount = db.Bans.Count(b => b.KieuBan != null && b.KieuBan.ToLower().Contains("pool"));
                    int lipTableCount = db.Bans.Count(b => b.KieuBan != null && (b.KieuBan.ToLower().Contains("lip") || b.KieuBan.ToLower().Contains("carom") || b.KieuBan.ToLower().Contains("phang") || b.KieuBan.ToLower().Contains("phăng")));
                    double TltPool = poolTableCount * days * 16.0;
                    double TltLip = lipTableCount * days * 16.0;
                    ReportPoolEfficiency = TltPool > 0 ? Math.Round((ReportPoolTotalGio / TltPool) * 100.0, 2) : 0.0;
                    ReportLipEfficiency = TltLip > 0 ? Math.Round((ReportLipTotalGio / TltLip) * 100.0, 2) : 0.0;

                    ReportPoolTypeEfficiencies.Clear();
                    var poolDays = days;
                    var poolTypeGroups = result.Rows
                        .Where(r => (r.KieuBan ?? string.Empty).ToLower().Contains("pool") || (r.KieuBan ?? string.Empty).ToLower().Contains("banlo") || (r.KieuBan ?? string.Empty).ToLower().Contains("bàn lỗ"))
                        .GroupBy(r => r.KieuBan ?? "Không xác định")
                        .ToList();

                    foreach (var typeGroup in poolTypeGroups)
                    {
                        var typeName = (typeGroup.Key ?? string.Empty).Trim().ToLower();
                        var typeTotalGio = typeGroup.Sum(r => r.TongGio);
                        var typeTotalSoBan = db.Bans.Count(b => b.KieuBan != null && b.KieuBan.Trim().ToLower() == typeName);
                        var typeTlt = typeTotalSoBan * poolDays * 16.0;
                        var typeEfficiency = typeTlt > 0 ? Math.Round((typeTotalGio / typeTlt) * 100.0, 2) : 0.0;

                        ReportPoolTypeEfficiencies.Add(new ReportTypeEfficiency
                        {
                            KieuBan = typeName,
                            HieuSuat = typeEfficiency,
                            SoBan = typeTotalSoBan,
                            TongGio = typeTotalGio
                        });
                    }

                    CurrentReport = result;
                    IsReportOverlayVisible = true;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Lỗi tạo báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            ExportReportToExcelCommand = new RelayCommand(o =>
            {
                try
                {
                    if (CurrentReport == null) { System.Windows.MessageBox.Show("Không có báo cáo để xuất.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                    var dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = $"BaoCaoHieuSuatBan_{DateTime.Now:ddMMyyyy_HHmm}.csv";
                    dlg.Filter = "CSV (Comma delimited)|*.csv|All files|*.*";
                    if (dlg.ShowDialog() == true)
                    {
                        ReportVM.ExportToCsv(CurrentReport, dlg.FileName);
                        System.Windows.MessageBox.Show("Đã xuất báo cáo (CSV).", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Lỗi xuất báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            CloseReportOverlayCommand = new RelayCommand(o => { IsReportOverlayVisible = false; });

            // Initialize database and load data only at runtime (not in Visual Studio designer)
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                try
                {
                    db = new QUANLYBIDAEntities1();
                    EnsureRuntimeSchema();
                    LoadFullData();
                    // Initialize report VM
                    ReportVM = new ReportViewModel(db);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Không thể tải dữ liệu từ database.\n\n" +
                        GetExceptionMessage(ex),
                        "Lỗi database",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            FilteredHoaDon = new ObservableCollection<HoaDon>(DsHoaDon ?? new ObservableCollection<HoaDon>());
            // ===================== BÀN ========================
            DsMonAnCuaBan = new ObservableCollection<MonAnChiTiet>();
            DsDatMon = new ObservableCollection<MonAnChiTiet>();
            DsBanHienThi = DsBan != null
                ? new ObservableCollection<Ban>(DsBan)
                : new ObservableCollection<Ban>();
            DatBanCommand = new RelayCommand(ExecDatBan);
            DongOverlayCommand = new RelayCommand(ExecDongOverlay);
            ThemMonCommand = new RelayCommand(ExecThemMon);
            XoaMonCommand = new RelayCommand(ExecXoaMon);
            XacNhanDatMonCommand = new RelayCommand(ExecXacNhanDatMon);
            XuatHoaDonCommand = new RelayCommand(ExecXuatHoaDon);
            ThanhToanCommand = new RelayCommand(ExecThanhToan);
            TangSoLuongCommand = new RelayCommand(o => SoLuongDat++);
            GiamSoLuongCommand = new RelayCommand(o => { if (SoLuongDat > 1) SoLuongDat--; });
            XoaMonTrenBanCommand = new RelayCommand(ExecXoaMonTrenBan);
            TangSoLuongMonTrenBanCommand = new RelayCommand(ExecTangSoLuongMonTrenBan);
            GiamSoLuongMonTrenBanCommand = new RelayCommand(ExecGiamSoLuongMonTrenBan);
            SetHoiVienCommand = new RelayCommand(o => { IsKhachThuong = false; });
            SetKhachThuongCommand = new RelayCommand(o => { IsKhachThuong = true; });
            SetTabKhachHangCommand = new RelayCommand(o => ActiveOverlayTab = 0);
            SetTabDatMonCommand = new RelayCommand(o => ActiveOverlayTab = 1);
            //Ban-Tran Nguyen - END
            // ===================== Nhân viên =====================
            IsAddNV = false;
            AddNVCommand = new RelayCommand(AddNV);

            DeleteNVCommand = new RelayCommand(
                DeleteNV, CanDeleteNV);

            EditNVCommand = new RelayCommand(
                EditNV, CanEditNV);
            ClearSelectionNVCommand = new RelayCommand(ClearSelectionNV);
            VisibileTableAddNVCommand = new RelayCommand(VisibileTableAddNV);
            ExitThemNVCommand = new RelayCommand(ExitThemNV);
            // Command để mở/đóng bảng chấm công
            VisibileTableChamCongCommand = new RelayCommand(o => {
                VisibileChamCong = Visibility.Visible;
                LoadData_BangChamCong();
            });
            ExitChamCongCommand = new RelayCommand(o => { VisibileChamCong = Visibility.Collapsed; });
            // Commands for attendance management
            AddChamCongCommand = new RelayCommand(AddChamCong);
            PayChamCongCommand = new RelayCommand(PayChamCong, CanPayChamCong);
            SaveChamCongCommand = new RelayCommand(SaveChamCong);
            DeleteChamCongCommand = new RelayCommand(DeleteChamCong, CanDeleteChamCong);
            //====================== KHOA (KHACH HANG - LICH SU HOA DON)  =============================
            MoThemKhachHangCommand = new RelayCommand(MoThemKhachHang);
            DongThemKhachHangCommand = new RelayCommand(DongThemKhachHang);
            XacNhanThemKhachHangCommand = new RelayCommand(XacNhanThemKhachHang);
            XoaKhachHangCommand = new RelayCommand(XoaKhachHang);
            MoSuaKhachHangCommand = new RelayCommand(MoSuaKhachHang);

            LocHoaDonCommand = new RelayCommand(o => LocHoaDon());
            XuatExcelHoaDonCommand = new RelayCommand(o => XuatExcelHoaDon());
            XuatBaoCaoHoaDonCommand = new RelayCommand(o => ExecXuatBaoCaoHoaDon(o), o => SelectedHoaDon != null);
            // ===================== KHO + MENU =====================
            FilteredSanPham = FilteredSanPham ?? new ObservableCollection<ProductItem>();
            DsKho = DsKho ?? new ObservableCollection<Kho>();
            DsNhomSanPham = DsNhomSanPham ?? new ObservableCollection<string>();
            MenuFilteredSanPham = MenuFilteredSanPham ?? new ObservableCollection<SanPham>();
            SelectedOrderItems = SelectedOrderItems ?? new ObservableCollection<OrderLine>();
            MenuSortOrder = "Z -> A"; // default
            
            CheckStockCommand = new RelayCommand(o => CheckStock());
            ExportProductsCommand = new RelayCommand(o => ExportProducts());
            DeleteProductCommand = new RelayCommand(o => DeleteProduct(), o => SelectedMenuProduct != null || SelectedSanPhamKho != null);
            ViewDetailProductCommand = new RelayCommand(o => ViewProductDetail(), o => SelectedMenuProduct != null || SelectedSanPhamKho != null);
            ViewKhoDetailCommand = new RelayCommand(o => ViewProductDetail(), o => SelectedSanPhamKho != null);
            
            NewKhoCommand = new RelayCommand(o => OpenAddKho());
            ConfirmNewKhoCommand = new RelayCommand(o => ConfirmNewKho(), o => CanConfirmNewKho());
            CancelNewKhoCommand = new RelayCommand(o => { IsAddingKho = false; });
            
            SaveEditProductCommand = new RelayCommand(o => SaveEditProduct(), o => CanSaveEditProduct());
            CancelEditProductCommand = new RelayCommand(o => CancelEditProduct());
            ChangeEditImageCommand = new RelayCommand(o => ChangeEditImage());
            
            NewProductCommand = new RelayCommand(o => OpenAddProduct());
            AddImageCommand = new RelayCommand(o => AddImageForNewProduct());
            ConfirmAddProductCommand = new RelayCommand(o => ConfirmAddProduct(), o => CanConfirmAddProduct());
            CancelAddProductCommand = new RelayCommand(o => CancelAddProduct());
            
            AddToOrderCommand = new RelayCommand(o => AddToOrder(o as SanPham), o => SelectedBanForOrder != null && o is SanPham);
            PlaceOrderCommand = new RelayCommand(o => PlaceOrder(), o => SelectedBanForOrder != null && SelectedOrderItems.Any());
            ClearOrderCommand = new RelayCommand(o => { SelectedOrderItems.Clear(); OnPropertyChanged(nameof(CurrentOrderTotal)); (PlaceOrderCommand as RelayCommand)?.RaiseCanExecuteChanged(); });
            IncreaseQtyCommand = new RelayCommand(o => {
                if (o is OrderLine ol)
                {
                    int availableQty = GetAvailableInventory(ol.Product);
                    if (ol.Quantity >= availableQty)
                    {
                        MessageBox.Show($"Số lượng tồn kho không đủ. Tồn kho hiện tại: {availableQty}", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    ol.Quantity++;
                    OnPropertyChanged(nameof(CurrentOrderTotal));
                }
            });
            DecreaseQtyCommand = new RelayCommand(o => { if (o is OrderLine ol) { if (ol.Quantity > 1) ol.Quantity--; else SelectedOrderItems.Remove(ol); OnPropertyChanged(nameof(CurrentOrderTotal)); } });
            RemoveOrderLineCommand = new RelayCommand(o => { if (o is OrderLine ol) { SelectedOrderItems.Remove(ol); OnPropertyChanged(nameof(CurrentOrderTotal)); } });
            
            SelectedOrderItems.CollectionChanged += (s, e) => { (PlaceOrderCommand as RelayCommand)?.RaiseCanExecuteChanged(); };
            // ===================== USERCONTROL =====================
            SwitchViewCommand = new RelayCommand(Switch);
            CurrentView = _dnVM;
            CurrentAppView = _tcVM;
            CurrentAccount = null;
            DangNhapCommand = new RelayCommand(DangNhap);
            BackLoginCommand = new RelayCommand(BackLogin);
            PlaceOrderCommand = new RelayCommand(o => PlaceOrder(), o => CanPlaceOrder(o));
            ShowAllHoaDonCommand = new RelayCommand(o => ExecShowAllHoaDon());
        }
    }
}
