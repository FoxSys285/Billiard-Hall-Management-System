using QuanLyQuanBida.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace QuanLyQuanBida.Views
{
    public partial class InvoiceReportWindow : Window
    {
        public string TableName { get; }
        public string TimeIn { get; }
        public string TimeOut { get; }
        public string TotalText { get; }
        public string AmountInWords { get; }
        public string ThankYouText { get; } = "Cảm ơn quý khách, hẹn gặp lại!";
        public ObservableCollection<InvoiceItem> Items { get; }

        public InvoiceReportWindow(HoaDon invoice, IList<ChiTietHoaDon> details, IEnumerable<SanPham> products)
        {
            InitializeComponent();

            TableName = invoice.MaBan;
            TimeIn = invoice.GioVaoHoaDon?.ToString(@"hh\:mm") ?? "--:--";
            TimeOut = invoice.GioRaHoaDon?.ToString(@"hh\:mm") ?? DateTime.Now.ToString(@"hh\:mm");

            Items = new ObservableCollection<InvoiceItem>();
            var index = 1;
            foreach (var ct in details)
            {
                var product = products?.FirstOrDefault(p => p.Ma == ct.MaThucDon);
                var unitPrice = 0.0;
                var quantity = ct.SoLuongChiTiet ?? 0;
                if (quantity > 0)
                {
                    unitPrice = (ct.GiaChiTiet ?? 0) / quantity;
                }

                Items.Add(new InvoiceItem
                {
                    Stt = index++,
                    Name = product?.Ten ?? ct.MaThucDon,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                });
            }

            var totalAmount = details.Sum(ct => ct.GiaChiTiet ?? 0);
            TotalText = $"Tổng cộng tiền cần thanh toán: {totalAmount:N0} đ";
            AmountInWords = $"Bằng chữ: {ToVietnameseWords((long)Math.Round(totalAmount))} đồng chẵn";
            DataContext = this;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static string ToVietnameseWords(long number)
        {
            if (number == 0) return "Không";
            if (number < 0) return "âm " + ToVietnameseWords(Math.Abs(number));

            string[] units = { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ" };
            var parts = new List<string>();
            int unitIndex = 0;

            while (number > 0)
            {
                int segment = (int)(number % 1000);
                if (segment > 0)
                {
                    var segmentText = ReadSegment(segment, number >= 1000);
                    if (!string.IsNullOrWhiteSpace(segmentText))
                    {
                        parts.Insert(0, string.IsNullOrWhiteSpace(units[unitIndex]) ? segmentText : $"{segmentText} {units[unitIndex]}");
                    }
                }
                number /= 1000;
                unitIndex++;
            }

            return string.Join(" ", parts).Replace("  ", " ").Trim();
        }

        private static string ReadSegment(int value, bool full)
        {
            string[] digits = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            int hundreds = value / 100;
            int tens = (value % 100) / 10;
            int ones = value % 10;

            var result = new StringBuilder();
            if (hundreds > 0)
            {
                result.Append(digits[hundreds]);
                result.Append(" trăm");
            }
            else if (full)
            {
                result.Append("không trăm");
            }

            if (tens > 1)
            {
                if (result.Length > 0) result.Append(" ");
                result.Append(digits[tens]);
                result.Append(" mươi");
                if (ones == 1) result.Append(" mốt");
                else if (ones == 4) result.Append(" tư");
                else if (ones == 5) result.Append(" lăm");
                else if (ones > 0) result.Append($" {digits[ones]}");
            }
            else if (tens == 1)
            {
                if (result.Length > 0) result.Append(" ");
                result.Append("mười");
                if (ones == 1) result.Append(" một");
                else if (ones == 4) result.Append(" bốn");
                else if (ones == 5) result.Append(" lăm");
                else if (ones > 0) result.Append($" {digits[ones]}");
            }
            else if (tens == 0 && ones > 0)
            {
                if (result.Length > 0) result.Append(" lẻ");
                if (ones == 5 && result.Length > 0)
                {
                    result.Append(" lăm");
                }
                else
                {
                    result.Append($" {digits[ones]}");
                }
            }

            return result.ToString().Trim();
        }
    }

    public class InvoiceItem
    {
        public int Stt { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
    }
}
