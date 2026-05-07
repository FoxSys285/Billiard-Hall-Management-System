using QuanLyQuanBida.Models;
using QuanLyQuanBida.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuanLyQuanBida.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // ===================== DATABASE =====================
        public QUANLYBIDAEntities1 db = new QUANLYBIDAEntities1();
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
        // ===================== ĐĂNG NHẬP =====================
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
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
                    CurrentView = _tcVM;
                    break;
                case "Menu":
                    CurrentView = _menuVM;
                    break;
                case "Ban":
                    CurrentView = _banVM;
                    break;
                case "LichSu":
                    CurrentView = _lsVM;
                    break;
                case "KhachHang":
                    CurrentView = _khVM;
                    break;
                case "Kho":
                    CurrentView = _khoVM;
                    break;
                case "NhanVien":
                    CurrentView = _nvVM;
                    break;
                case "ThongKe":
                    CurrentView = _tkVM;
                    break;
                case "DangNhap":
                    CurrentView = _dnVM;
                    break;
            }
        }
        // Đăng xuất
        public void BackLogin(Object obj)
        {
            CurrentView = _dnVM;
            CurrentAccount = null;
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
                InputMK = "";
                InputTK = "";
                CurrentView = new TrangChuView();
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
                if (CurrentAccount.ChucVu == "Quản lý")
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        // ===================== DANH SÁCH =====================
        public ObservableCollection<string> DsChucVu { get; set; }
        public ObservableCollection<Ban> DsBan { get; set; }
        public ObservableCollection<BangChamCong> DsBangChamCong { get; set; }
        public ObservableCollection<HoaDon> DsHoaDon { get; set;  }
        public ObservableCollection<ChiTietHoaDon> DsChiTietHoaDon { get; set; }
        public ObservableCollection<PhieuNhap> DsPhieuNhap { get; set; }
        public ObservableCollection<ChiTietPhieuNhap> DsChiTietPhieuNhap { get; set; }
        public ObservableCollection<KhachHang> DsKhachHang { get; set; }
        public ObservableCollection<NhanVien> DsNhanVien { get; set; }
        public ObservableCollection<SanPham> DsSanPham { get; set; }
        // ===================== BÀN =====================
        private Ban _selectedBan;
        public Ban SelectedBan
        {
            get => _selectedBan;
            set
            {
                _selectedBan = value;
                OnPropertyChanged(nameof(SelectedBan));
            }
        }
        // ===================== HÓA ĐƠN =====================
        // ===================== PHIẾU NHẬP =====================
        // ===================== KHÁCH HÀNG =====================
        // Biến điều khiển ẩn/hiện Overlay
        private string _addTenKH;
        private string _addSDT;
        private string _addMaKH;
        private bool _isShowAdd;
        public bool IsShowAdd
        {
            get => _isShowAdd;
            set { _isShowAdd = value; OnPropertyChanged(nameof(IsShowAdd)); }
        }

        public string AddTenKH { get => _addTenKH; set { _addTenKH = value; OnPropertyChanged(nameof(AddTenKH)); } }
        public string AddSDT { get => _addSDT; set { _addSDT = value; OnPropertyChanged(nameof(AddSDT)); } }
        public string AddMaKH { get => _addMaKH; set { _addMaKH = value; OnPropertyChanged(nameof(AddMaKH)); } }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ExecuteSearch();
            }
        }

        private KhachHang _selectedKhachHang;
        public KhachHang SelectedKhachHang
        {
            get => _selectedKhachHang;
            set { _selectedKhachHang = value; OnPropertyChanged(nameof(SelectedKhachHang)); }
        }


        public ICommand OpenAddCommand { get; set; } // Nút "Thêm" ở danh sách
        public ICommand CloseAddCommand { get; set; }       // Nút "X" để đóng bảng nhỏ
        public ICommand ConfirmAddCommand { get; set; }     // Nút "THÊM KHÁCH HÀNG" trong bảng nhỏ
        public ICommand DeleteKhachHangCommand { get; set; }
        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DsKhachHang = new ObservableCollection<KhachHang>(DsKhachHang);
            }
            else
            {
                var filtered = DsKhachHang.Where(x =>
                    x.SoDienThoai.Contains(SearchText) ||
                    (x.TenKhachHang != null && x.TenKhachHang.ToLower().Contains(SearchText.ToLower()))
                ).ToList();

                DsKhachHang = new ObservableCollection<KhachHang>(filtered);
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
        public string InputNewTK
        {
            get { return _inputNewTK; }
            set
            {
                _inputNewTK = value;
                OnPropertyChanged(nameof(InputNewTK));
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
            }
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
        // Chức năng hiện bảng thêm nhân viên
        public ICommand VisibileTableAddNVCommand { get; set; }
        public void VisibileTableAddNV(Object obj)
        {
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
                        MatKhau = _isSelectedNV.MatKhau
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
            DsNhanVien.Remove(IsSelectedNV);
            OnPropertyChanged(nameof(DsNhanVien));
        }
        // Chức năgn sửa nhân viên
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
            if (IsSelectedNV != null && EditingNV != null)
            {
                // Cập nhật dữ liệu từ đối tượng tạm vào đối tượng chính trong danh sách
                IsSelectedNV.TenNV = EditingNV.TenNV;
                IsSelectedNV.ChucVu = EditingNV.ChucVu;
                IsSelectedNV.DiaChi = EditingNV.DiaChi;
                IsSelectedNV.SDT = EditingNV.SDT;
                IsSelectedNV.NgaySinh = EditingNV.NgaySinh;
                IsSelectedNV.NgayVL = EditingNV.NgayVL;
                IsSelectedNV.TaiKhoan = EditingNV.TaiKhoan;
                IsSelectedNV.MatKhau = EditingNV.MatKhau;
                int index = DsNhanVien.IndexOf(IsSelectedNV);
                if (index != -1)
                {
                    DsNhanVien[index] = IsSelectedNV;
                    IsSelectedNV = DsNhanVien[index];
                }
                MessageBox.Show($"Đã cập nhật thông tin nhân viên: {IsSelectedNV.TenNV}",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        // ===================== SẢN PHẨM =====================

        // ===================== LẤY DỮ LIỆU TỪ DATABASE =====================
        public void LoadData_Ban()
        {
            DsBan = new ObservableCollection<Ban>(db.Bans.ToList());
        }
        public void LoadData_BangChamCong()
        {
            DsBangChamCong = new ObservableCollection<BangChamCong>(db.BangChamCongs.ToList());
        }
        public void LoadData_HoaDon()
        {
            DsHoaDon = new ObservableCollection<HoaDon>(db.HoaDons.ToList());
        }
        public void LoadData_ChiTietHoaDon()
        {
            DsChiTietHoaDon = new ObservableCollection<ChiTietHoaDon>(db.ChiTietHoaDons.ToList());
        }
        public void LoadData_PhieuNhap()
        {
            DsPhieuNhap = new ObservableCollection<PhieuNhap>(db.PhieuNhaps.ToList());
        }
        public void LoadData_ChiTietPhieuNhap()
        {
            DsChiTietPhieuNhap = new ObservableCollection<ChiTietPhieuNhap>(db.ChiTietPhieuNhaps.ToList());
        }
        public void LoadData_KhachHang()
        {
            DsKhachHang = new ObservableCollection<KhachHang>(db.KhachHangs.ToList());
        }
        public void LoadData_SanPham()
        {
            DsSanPham = new ObservableCollection<SanPham>(db.SanPhams.ToList());
        }
        public void LoadData_NhanVien()
        {
            DsNhanVien = new ObservableCollection<NhanVien>(db.NhanViens.ToList());
        }
        public void LoadFullData()
        {
            LoadData_Ban();
            LoadData_BangChamCong();
            LoadData_HoaDon();
            LoadData_KhachHang();
            LoadData_NhanVien();
            LoadData_PhieuNhap();
            LoadData_SanPham();
            LoadData_ChiTietPhieuNhap();
            LoadData_ChiTietHoaDon();
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
            _tcVM = new TrangChuView();
            // ===================== LOAD DỮ LIỆU =====================
            DsChucVu = new ObservableCollection<string>()
            {
                "Quản lý",
                "Thu ngân",
                "Phục vụ",
                "Giữ xe"
            };
            LoadFullData();
            // ===================== USERCONTROL =====================
            SwitchViewCommand = new RelayCommand(Switch);
            CurrentView = _dnVM;
            CurrentAccount = new NhanVien { MaNV = "NV001", TenNV = "Nguyễn Văn Nam", NgaySinh = new DateTime(1998, 5, 15), ChucVu = "Quản lý", LuongCB = 12000000, TaiKhoan = "admin", MatKhau = "123", NgayVL = new DateTime(2024, 1, 1), GioiTinh = true };
            DangNhapCommand = new RelayCommand(DangNhap);
            BackLoginCommand = new RelayCommand(BackLogin);
            


        }
    }
}
