using QuanLyQuanBida.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyQuanBida.ViewModels
{
    public class ReportViewModel
    {
        private QUANLYBIDAEntities1 db;

        public ReportViewModel(QUANLYBIDAEntities1 context)
        {
            db = context;
        }

        public class ReportResult
        {
            public List<ReportBanDTO> Rows { get; set; }
            public double TotalGio { get; set; }
            public double TotalDoanhThu { get; set; }
            public int TotalLuotChoi { get; set; }
            public double HieuSuat { get; set; }
            public double Tlt { get; set; }
            public int SoBan { get; set; }
            public int SoNgay { get; set; }
        }

        public ReportResult GenerateReport(DateTime fromDate, DateTime toDate)
        {
            // Normalize dates (include whole day)
            DateTime from = fromDate.Date;
            DateTime to = toDate.Date.AddDays(1).AddTicks(-1);

            // Query invoices in range
            var invoices = db.HoaDons
                .Where(h => h.NgayHoaDon.HasValue && h.NgayHoaDon.Value >= from && h.NgayHoaDon.Value <= to && h.TrangThaiThanhToan == true)
                .Select(h => new
                {
                    h.MaBan,
                    h.Ban.TenBan,
                    h.Ban.KieuBan,
                    h.GioVaoHoaDon,
                    h.GioRaHoaDon,
                    h.TongTienHoaDon
                })
                .ToList();

            var grouped = invoices.GroupBy(x => x.MaBan)
                .Select(g => new ReportBanDTO
                {
                    MaBan = g.Key,
                    TenBan = g.FirstOrDefault()?.TenBan ?? g.Key,
                    KieuBan = g.FirstOrDefault()?.KieuBan ?? "",
                    SoLuotChoi = g.Count(),
                    TongGio = g.Sum(i =>
                    {
                        if (i.GioVaoHoaDon.HasValue && i.GioRaHoaDon.HasValue)
                        {
                            var diff = (i.GioRaHoaDon.Value - i.GioVaoHoaDon.Value).TotalHours;
                            return diff > 0 ? diff : 0;
                        }
                        return 0.0;
                    }),
                    TongTien = g.Sum(i => i.TongTienHoaDon ?? 0)
                })
                .ToList();

            var totalGio = grouped.Sum(r => r.TongGio);
            var totalTien = grouped.Sum(r => r.TongTien);
            var totalLuot = grouped.Sum(r => r.SoLuotChoi);
            var soBan = db.Bans.Count();
            var soNgay = (toDate.Date - fromDate.Date).Days + 1;
            var Tlt = soBan * soNgay * 16.0; // 16 hours per day
            var hieuSuat = Tlt > 0 ? (totalGio / Tlt) * 100.0 : 0.0;

            return new ReportResult
            {
                Rows = grouped,
                TotalGio = totalGio,
                TotalDoanhThu = totalTien,
                TotalLuotChoi = totalLuot,
                HieuSuat = Math.Round(hieuSuat, 2),
                Tlt = Math.Round(Tlt, 2),
                SoBan = soBan,
                SoNgay = soNgay
            };
        }

        // Export CSV (Excel-friendly)
        public void ExportToCsv(ReportResult report, string filePath)
        {
            using (var sw = new StreamWriter(filePath))
            {
                sw.WriteLine("KieuBan,MaBan,TenBan,SoLuotChoi,TongGio,TongTien");
                foreach (var r in report.Rows)
                {
                    sw.WriteLine($"{Escape(r.KieuBan)},{Escape(r.MaBan)},{Escape(r.TenBan)},{r.SoLuotChoi},{r.TongGio:F2},{r.TongTien:F2}");
                }
                sw.WriteLine();
                sw.WriteLine($",Tổng,{report.TotalLuotChoi},{report.TotalGio:F2},{report.TotalDoanhThu:F2}");
                sw.WriteLine($",Hiệu suất:,{report.HieuSuat:F2}%");
            }
        }

        private string Escape(string s)
        {
            if (s == null) return "";
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
            {
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }
            return s;
        }
    }
}
