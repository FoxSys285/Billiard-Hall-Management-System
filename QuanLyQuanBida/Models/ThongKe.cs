using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyQuanBida.Models
{
    public class ThongKeSanPham
    {
        public int ViTri { get; set; }
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public int TongSoLuong { get; set; }
        public double TongTien { get; set; }
    }
    public class ThongKeBan
    {
        public string MaBan { get; set; }
        public string TenBan { get; set; }
        public int SoHoaDon { get; set; }
        public double TongTien { get; set; }
    }
    public class KhachHangThongKe
    {
        public int ViTri { get; set; }
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public int DiemTichLuy { get; set; }
        public bool HangThuNhat => ViTri == 1;
        public bool HangThuHai => ViTri == 2;
        public bool HangThuBa => ViTri == 3;
    }
}
