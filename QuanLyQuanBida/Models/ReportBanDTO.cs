using System;

namespace QuanLyQuanBida.Models
{
    public class ReportBanDTO
    {
        public string MaBan { get; set; }
        public string TenBan { get; set; }
        public int SoLuotChoi { get; set; }
        public double TongGio { get; set; } // hours
        public double TongTien { get; set; }
        public string KieuBan { get; set; }
    }
}
