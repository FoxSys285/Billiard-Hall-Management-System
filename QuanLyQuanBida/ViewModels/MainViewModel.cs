using QuanLyQuanBida.Models;
using QuanLyQuanBida.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        public ObservableCollection<HoaDon> DsHoaDon { get; set; }
        public ObservableCollection<ChiTietHoaDon> DsChiTietHoaDon { get; set; }
        public ObservableCollection<PhieuNhap> DsPhieuNhap { get; set; }
        public ObservableCollection<HoaDon> FilteredHoaDon { get; set; }
        public ObservableCollection<ChiTietPhieuNhap> DsChiTietPhieuNhap { get; set; }
        public ObservableCollection<KhachHang> DsKhachHang { get; set; }
        public ObservableCollection<NhanVien> DsNhanVien { get; set; }
        public ObservableCollection<SanPham> DsSanPham { get; set; }
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
            set { _datBan_SDT = value; OnPropertyChanged(nameof(DatBan_SDT)); TimKhachHangBySdt(); }
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
            if (DsBan == null || db == null) return;
            var updated = db.Bans.Find(maBan);
            if (updated != null)
            {
                for (int i = 0; i < DsBan.Count; i++)
                    if (DsBan[i].MaBan == maBan) { DsBan[i] = updated; break; }
            }
            ApplyFilterBan();
            var reselect = DsBanHienThi.FirstOrDefault(b => b.MaBan == maBan);
            if (reselect != null)
            {
                _selectedBan = reselect;
                OnPropertyChanged(nameof(SelectedBan));
                OnPropertyChanged(nameof(TrangThaiHienTai));
            }
        }

        private void LoadChiTietBan()
        {
            if (SelectedBan == null || db == null)
            {
                DsMonAnCuaBan = new ObservableCollection<MonAnChiTiet>();
                TongTienHienTai = 0; GioVaoBan = "--:--"; return;
            }
            var hd = DsHoaDon?.FirstOrDefault(h =>
                h.MaBan == SelectedBan.MaBan && h.TrangThaiThanhToan == false);
            if (hd == null)
            {
                DsMonAnCuaBan = new ObservableCollection<MonAnChiTiet>();
                TongTienHienTai = 0; GioVaoBan = "--:--"; return;
            }
            GioVaoBan = hd.GioVaoHoaDon.HasValue
                ? hd.GioVaoHoaDon.Value.ToString(@"hh\:mm") : "--:--";
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
            TongTienHienTai = DsMonAnCuaBan.Sum(m => m.ThanhTien);
        }

        private void TimKhachHangBySdt()
        {
            if (string.IsNullOrWhiteSpace(DatBan_SDT)) { ThongBaoTimKH = ""; return; }
            var kh = DsKhachHang?.FirstOrDefault(k => k.SoDienThoai == DatBan_SDT);
            if (kh != null)
            {
                DatBan_MaKH = kh.MaKhachHang; DatBan_TenKH = kh.TenKhachHang;
                DatBan_DiemTichLuy = kh.DiemTichLuy ?? 0;
                ThongBaoTimKH = $"✅ Khách cũ: {kh.TenKhachHang}  |  Điểm TL: {kh.DiemTichLuy}";
            }
            else
            {
                DatBan_MaKH = GenMaKH(); DatBan_TenKH = ""; DatBan_DiemTichLuy = 0;
                ThongBaoTimKH = "🆕 Khách mới — sẽ được tạo khi xác nhận";
            }
        }

        private string GenMaKH()
        {
            int max = DsKhachHang?
                .Select(k => { int n; return int.TryParse(k.MaKhachHang?.Replace("KH", ""), out n) ? n : 0; })
                .DefaultIfEmpty(0).Max() ?? 0;
            return $"KH{(max + 1):D2}";
        }

        private string GenMaHD()
        {
            int max = DsHoaDon?
                .Select(h => { int n; return int.TryParse(h.MaHoaDon?.Replace("HD", ""), out n) ? n : 0; })
                .DefaultIfEmpty(0).Max() ?? 0;
            return $"HD{(max + 1):D2}";
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
            IsShowDatBanOverlay = true;
        }

        private void ExecDongOverlay(object obj) => IsShowDatBanOverlay = false;

        private void ExecThemMon(object obj)
        {
            if (SelectedSanPham == null)
            { MessageBox.Show("Vui lòng chọn sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            double donGia = SelectedSanPham.DonGia ?? 0;
            var existing = DsDatMon.FirstOrDefault(m => m.MaMon == SelectedSanPham.Ma);
            if (existing != null)
            {
                int idx = DsDatMon.IndexOf(existing);
                existing.SoLuong += SoLuongDat; existing.ThanhTien = existing.SoLuong * donGia;
                DsDatMon[idx] = existing;
            }
            else
                DsDatMon.Add(new MonAnChiTiet { MaMon = SelectedSanPham.Ma, TenMon = SelectedSanPham.Ten, SoLuong = SoLuongDat, ThanhTien = SoLuongDat * donGia });
            TinhTongDatMon(); SoLuongDat = 1;
        }

        private void ExecXoaMon(object obj)
        {
            if (SelectedDatMon == null)
            { MessageBox.Show("Vui lòng chọn món cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            DsDatMon.Remove(SelectedDatMon); SelectedDatMon = null; TinhTongDatMon();
        }

        private void ExecXacNhanDatMon(object obj)
        {
            if (string.IsNullOrWhiteSpace(DatBan_TenKH) || string.IsNullOrWhiteSpace(DatBan_SDT))
            { MessageBox.Show("Vui lòng nhập đủ thông tin khách hàng!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); ActiveOverlayTab = 0; return; }
            if (DsDatMon == null || DsDatMon.Count == 0)
            { MessageBox.Show("Chưa có món nào trong đơn!", "Thiếu món", MessageBoxButton.OK, MessageBoxImage.Warning); ActiveOverlayTab = 1; return; }
            if (db == null) return;
            try
            {
                var kh = DsKhachHang?.FirstOrDefault(k => k.SoDienThoai == DatBan_SDT);
                if (kh == null)
                {
                    kh = new KhachHang { MaKhachHang = DatBan_MaKH, TenKhachHang = DatBan_TenKH, SoDienThoai = DatBan_SDT, DiemTichLuy = 0 };
                    db.KhachHangs.Add(kh); DsKhachHang?.Add(kh);
                }
                string maHD = GenMaHD();
                var hd = new HoaDon
                {
                    MaHoaDon = maHD,
                    MaBan = SelectedBan.MaBan,
                    MaNV = CurrentAccount?.MaNV,
                    MaKhachHang = kh.MaKhachHang,
                    GioVaoHoaDon = DateTime.Now.TimeOfDay,
                    TrangThaiThanhToan = false,
                    TongTienHoaDon = TongTienDatMon
                };
                db.HoaDons.Add(hd); DsHoaDon?.Add(hd);
                foreach (var mon in DsDatMon)
                {
                    var ct = new ChiTietHoaDon { MaHoaDon = maHD, MaThucDon = mon.MaMon, SoLuongChiTiet = mon.SoLuong, GiaChiTiet = mon.ThanhTien };
                    db.ChiTietHoaDons.Add(ct); DsChiTietHoaDon?.Add(ct);
                }
                var banDb = db.Bans.Find(SelectedBan.MaBan);
                if (banDb != null) { banDb.TrangThaiBan = true; SelectedBan.TrangThaiBan = true; }
                db.SaveChanges();
                string maBan = SelectedBan.MaBan;
                IsShowDatBanOverlay = false;
                RefreshBanRealtime(maBan);
                LoadChiTietBan();
                LoadThongKe();
                MessageBox.Show($"✅ Đặt {SelectedBan?.TenBan ?? maBan} thành công!\nKhách: {kh.TenKhachHang} | Giờ vào: {DateTime.Now:HH:mm}",
                    "Đặt bàn thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            { MessageBox.Show("Lỗi: " + (ex.InnerException?.Message ?? ex.Message), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void ExecXuatHoaDon(object obj)
        {
            if (SelectedBan == null) { MessageBox.Show("Vui lòng chọn bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var hd = DsHoaDon?.FirstOrDefault(h => h.MaBan == SelectedBan.MaBan && h.TrangThaiThanhToan == false);
            if (hd == null) { MessageBox.Show("Bàn này chưa có hóa đơn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var kh = DsKhachHang?.FirstOrDefault(k => k.MaKhachHang == hd.MaKhachHang);
            var ctList = DsChiTietHoaDon?.Where(ct => ct.MaHoaDon == hd.MaHoaDon).ToList();
            double tong = ctList?.Sum(ct => ct.GiaChiTiet ?? 0) ?? 0;
            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════╗");
            sb.AppendLine("║     HÓA ĐƠN QUÁN BIDA           ║");
            sb.AppendLine("╚══════════════════════════════════╝");
            sb.AppendLine($"  Mã HĐ   : {hd.MaHoaDon}");
            sb.AppendLine($"  Bàn     : {SelectedBan.TenBan}");
            sb.AppendLine($"  Khách   : {kh?.TenKhachHang ?? "Khách lẻ"}  |  SĐT: {kh?.SoDienThoai ?? "---"}");
            sb.AppendLine($"  Điểm TL : {kh?.DiemTichLuy ?? 0} điểm");
            sb.AppendLine($"  Giờ vào : {hd.GioVaoHoaDon?.ToString(@"hh\:mm") ?? "--:--"}");
            sb.AppendLine("──────────────────────────────────");
            sb.AppendLine($"  {"Tên món",-20} {"SL",3} {"Thành tiền",12}");
            sb.AppendLine("──────────────────────────────────");
            ctList?.ForEach(ct => {
                var sp = DsSanPham?.FirstOrDefault(s => s.Ma == ct.MaThucDon);
                sb.AppendLine($"  {sp?.Ten ?? ct.MaThucDon,-20} {ct.SoLuongChiTiet,3} {ct.GiaChiTiet,12:N0} đ");
            });
            sb.AppendLine("══════════════════════════════════");
            sb.AppendLine($"  TỔNG TIỀN: {tong:N0} VNĐ");
            MessageBox.Show(sb.ToString(), $"Hóa đơn — {SelectedBan.TenBan}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecThanhToan(object obj)
        {
            if (SelectedBan == null) { MessageBox.Show("Vui lòng chọn bàn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (SelectedBan.TrangThaiBan != true) { MessageBox.Show("Bàn đang trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var hd = DsHoaDon?.FirstOrDefault(h => h.MaBan == SelectedBan.MaBan && h.TrangThaiThanhToan == false);
            if (hd == null) { MessageBox.Show("Không tìm thấy hóa đơn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            double tong = DsChiTietHoaDon?.Where(ct => ct.MaHoaDon == hd.MaHoaDon).Sum(ct => ct.GiaChiTiet ?? 0) ?? 0;
            if (MessageBox.Show($"Xác nhận thanh toán {SelectedBan.TenBan}?\nTổng tiền: {tong:N0} VNĐ",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            if (db == null) return;
            try
            {
                hd.TrangThaiThanhToan = true; hd.GioRaHoaDon = DateTime.Now.TimeOfDay; hd.TongTienHoaDon = tong;
                if (!string.IsNullOrEmpty(hd.MaKhachHang))
                {
                    var kh = DsKhachHang?.FirstOrDefault(k => k.MaKhachHang == hd.MaKhachHang);
                    if (kh != null) kh.DiemTichLuy = (kh.DiemTichLuy ?? 0) + (int)(tong / 10000);
                }
                var banDb = db.Bans.Find(SelectedBan.MaBan);
                if (banDb != null) { banDb.TrangThaiBan = false; SelectedBan.TrangThaiBan = false; }
                db.SaveChanges();
                string maBan = SelectedBan.MaBan;
                DsMonAnCuaBan = new ObservableCollection<MonAnChiTiet>(); TongTienHienTai = 0; GioVaoBan = "--:--";
                RefreshBanRealtime(maBan);
                LoadThongKe();
                MessageBox.Show($"✅ Thanh toán thành công!\nTổng tiền: {tong:N0} VNĐ | Điểm cộng: {(int)(tong / 10000)} điểm",
                    "Thanh toán thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            { MessageBox.Show("Lỗi: " + (ex.InnerException?.Message ?? ex.Message), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        //Ban-Tran Nguyen - END
        // ===================== HÓA ĐƠN =====================
        // ===================== PHIẾU NHẬP =====================
        // ===================== KHÁCH HÀNG =====================
        // Biến điều khiển ẩn/hiện Overlay
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
        public ICommand OpenEditCommand { get; set; }
        private bool _isEditingCustomer = false;
        public bool IsEditingCustomer
        {
            get => _isEditingCustomer;
            set { _isEditingCustomer = value; OnPropertyChanged(nameof(IsEditingCustomer)); }
        }
        private void ExecuteSearch()
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

        // ===================== KHÁCH HÀNG CRUD =====================
        public void OpenAdd(object obj)
        {
            IsEditingCustomer = false;
            try
            {
                // Lấy danh sách mã hiện có
                var codes = db.KhachHangs.Select(k => k.MaKhachHang).ToList();
                int max = 0;

                foreach (var c in codes)
                {
                    if (string.IsNullOrWhiteSpace(c) || c.Length <= 2) continue;

                    // Lấy tất cả các ký tự số sau chữ "KH"
                    var numPart = c.Substring(2);
                    if (int.TryParse(numPart, out int n))
                    {
                        if (n > max) max = n;
                    }
                }

                int next = max + 1;
                AddMaKH = "KH" + next.ToString("D2");
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

        public void CloseAdd(object obj)
        {
            IsShowAdd = false;
        }

        public void ConfirmAdd(object obj)
        {
            if (string.IsNullOrWhiteSpace(AddTenKH) || string.IsNullOrWhiteSpace(AddSDT))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin khách hàng", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (IsEditingCustomer)
            {
                // update existing
                var existing = _allKhachHang.FirstOrDefault(x => x.MaKhachHang == AddMaKH) ?? SelectedKhachHang;
                if (existing != null)
                {
                    existing.TenKhachHang = AddTenKH;
                    existing.SoDienThoai = AddSDT;
                    existing.DiemTichLuy = AddDiemTichLuy;
                    try
                    {
                        // attach and save
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
                    // refresh collection (replace item)
                    var idxAll = _allKhachHang.FindIndex(x => x.MaKhachHang == existing.MaKhachHang);
                    if (idxAll >= 0) _allKhachHang[idxAll] = existing;
                    var idx = DsKhachHang.IndexOf(DsKhachHang.FirstOrDefault(x => x.MaKhachHang == existing.MaKhachHang));
                    if (idx >= 0)
                    {
                        DsKhachHang[idx] = existing;
                    }
                }
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
            // add to DB
            try
            {
                db.KhachHangs.Add(kh);
                db.SaveChanges();
            }
            catch
            {
                // ignore DB errors for now
            }
            _allKhachHang.Add(kh);
            DsKhachHang.Add(kh);
            IsShowAdd = false;
        }

        public void OpenEdit(object obj)
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
            IsShowAdd = true;
        }

        public void DeleteKhachHang(object obj)
        {
            if (SelectedKhachHang == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng để xóa", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var res = MessageBox.Show($"Xác nhận xóa khách hàng {SelectedKhachHang.TenKhachHang}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            // check for related invoices - do not allow delete if exists
            bool hasInvoices = db.HoaDons.Any(h => h.MaKhachHang == SelectedKhachHang.MaKhachHang);
            if (hasInvoices)
            {
                MessageBox.Show("Không thể xóa khách hàng này vì đã có lịch sử hóa đơn.", "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Xóa khách hàng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa khách hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
        public ICommand FilterCommand { get; set; }
        public ICommand ExportExcelCommand { get; set; }

        public void ApplyFilter()
        {
            if (DsHoaDon == null) LoadData_HoaDon();
            var q = DsHoaDon.AsEnumerable();
            if (StartDate.HasValue)
            {
                var sd = StartDate.Value.Date;
                q = q.Where(h => GetHoaDonDate(h) >= sd);
            }
            if (EndDate.HasValue)
            {
                var ed = EndDate.Value.Date.AddDays(1).AddTicks(-1);
                q = q.Where(h => GetHoaDonDate(h) <= ed);
            }
            if (SelectedNhanVien != null)
            {
                q = q.Where(h => h.MaNV == SelectedNhanVien.MaNV);
            }
            FilteredHoaDon = new ObservableCollection<HoaDon>(q);
            OnPropertyChanged(nameof(FilteredHoaDon));
        }

        private DateTime GetHoaDonDate(HoaDon h)
        {
            // HoaDon model does not have explicit date property; infer from NhanVien or other fields.
            // If database has DateTime in related entity, adjust accordingly. For now use DateTime.Now as fallback.
            // Try to read from ChiTietHoaDon timestamp if present - not available. So we approximate with DateTime.Now.
            return DateTime.Now;
        }

        public void ExportToExcel()
        {
            try
            {
                // Export FilteredHoaDon to CSV which Excel can open
                var list = FilteredHoaDon ?? new ObservableCollection<HoaDon>(DsHoaDon ?? new ObservableCollection<HoaDon>());
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("MaHoaDon,TenKhachHang,MaBan,TongTien,NgayHoaDon,TenNV");
                foreach (var h in list)
                {
                    var tenKH = h.KhachHang?.TenKhachHang ?? "";
                    var tenNV = h.NhanVien?.TenNV ?? "";
                    var ngay = GetHoaDonDate(h).ToString("yyyy-MM-dd HH:mm:ss");
                    sb.AppendLine($"{h.MaHoaDon},{EscapeCsv(tenKH)},{h.MaBan},{h.TongTienHoaDon},{ngay},{EscapeCsv(tenNV)}");
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
                    System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Đã xuất file.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string EscapeCsv(string s)
        {
            if (s == null) return "";
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
            {
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }
            return s;
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

        private ObservableCollection<KhachHangThongKe> _dsTopKhachHang;
        public ObservableCollection<KhachHangThongKe> DsTopKhachHang
        {
            get => _dsTopKhachHang;
            set { _dsTopKhachHang = value; OnPropertyChanged(nameof(DsTopKhachHang)); }
        }

        public ICommand RefreshThongKeCommand { get; set; }

        public void LoadThongKe()
        {
            var hoaDonDaThanhToan = DsHoaDon?
                .Where(hd => hd.TrangThaiThanhToan == true).ToList()
                ?? new List<HoaDon>();

            TongDoanhThu = hoaDonDaThanhToan.Sum(hd => hd.TongTienHoaDon ?? 0);
            SoHoaDonDaThanhToan = hoaDonDaThanhToan.Count;
            SoKhachHang = DsKhachHang?.Count ?? 0;
            SoBanDangHoatDong = DsBan?.Count(b => b.TrangThaiBan == true) ?? 0;


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
            for (int i = 0; i < topSP.Count; i++) topSP[i].ViTri = i + 1;
            DsTopSanPham = new ObservableCollection<ThongKeSanPham>(topSP);


            var tkBan = hoaDonDaThanhToan
                .GroupBy(hd => hd.MaBan)
                .Select(g =>
                {
                    var ban = DsBan?.FirstOrDefault(b => b.MaBan == g.Key);
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
            // set max for chart normalization
            MaxTongTienBan = DsThongKeBan.Any() ? DsThongKeBan.Max(x => x.TongTien) : 0;


            var topKH = (DsKhachHang?
                .OrderByDescending(k => k.DiemTichLuy)
                .Take(10)
                .Select((k, i) => new KhachHangThongKe
                {
                    ViTri = i + 1,
                    MaKhachHang = k.MaKhachHang,
                    TenKhachHang = k.TenKhachHang,
                    SoDienThoai = k.SoDienThoai,
                    DiemTichLuy = k.DiemTichLuy ?? 0
                })
                .ToList()) ?? new List<KhachHangThongKe>();
            DsTopKhachHang = new ObservableCollection<KhachHangThongKe>(topKH);
        }
        // ===================== LẤY DỮ LIỆU TỪ DATABASE =====================
        public void LoadData_Ban()
        {
            if (db == null) return;
            DsBan = new ObservableCollection<Ban>(db.Bans.ToList());
            ApplyFilterBan(); //Ban-Tran Nguyen - cập nhật DsBanHienThi ngay sau load
        }
        public void LoadData_BangChamCong()
        {
            if (db == null) return;
            DsBangChamCong = new ObservableCollection<BangChamCong>(db.BangChamCongs.ToList());
        }
        public void LoadData_HoaDon()
        {
            if (db == null) return;
            DsHoaDon = new ObservableCollection<HoaDon>(db.HoaDons.ToList());
        }
        public void LoadData_ChiTietHoaDon()
        {
            if (db == null) return;
            DsChiTietHoaDon = new ObservableCollection<ChiTietHoaDon>(db.ChiTietHoaDons.ToList());
        }
        public void LoadData_PhieuNhap()
        {
            if (db == null) return;
            DsPhieuNhap = new ObservableCollection<PhieuNhap>(db.PhieuNhaps.ToList());
        }
        public void LoadData_ChiTietPhieuNhap()
        {
            if (db == null) return;
            DsChiTietPhieuNhap = new ObservableCollection<ChiTietPhieuNhap>(db.ChiTietPhieuNhaps.ToList());
        }
        public void LoadData_KhachHang()
        {
            if (db == null) return;
            _allKhachHang = db.KhachHangs.ToList(); // ← thêm dòng này
            DsKhachHang = new ObservableCollection<KhachHang>(db.KhachHangs.ToList());
        }
        public void LoadData_SanPham()
        {
            if (db == null) return;
            DsSanPham = new ObservableCollection<SanPham>(db.SanPhams.ToList());
        }
        public void LoadData_NhanVien()
        {
            if (db == null) return;
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
            LoadThongKe();
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

            // Initialize database and load data only at runtime (not in Visual Studio designer)
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                try
                {
                    db = new QUANLYBIDAEntities1();
                    LoadFullData();
                }
                catch (Exception)
                {
                    // swallow exceptions at startup to avoid crashing designer; consider logging
                }
            }
            FilteredHoaDon = new ObservableCollection<HoaDon>(DsHoaDon ?? new ObservableCollection<HoaDon>());
            // ===================== THỐNG KÊ ========================
            DsTopSanPham = new ObservableCollection<ThongKeSanPham>();
            DsThongKeBan = new ObservableCollection<ThongKeBan>();
            DsTopKhachHang = new ObservableCollection<KhachHangThongKe>();
            RefreshThongKeCommand = new RelayCommand(o => LoadThongKe());
            //Ban-Tran Nguyen - BEGIN
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
            //Ban-Tran Nguyen - END
            // ===================== USERCONTROL =====================
            SwitchViewCommand = new RelayCommand(Switch);
            CurrentView = _dnVM;
            CurrentAccount = null;
            DangNhapCommand = new RelayCommand(DangNhap);
            BackLoginCommand = new RelayCommand(BackLogin);
        }
    }
}