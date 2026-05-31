using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyQuanBida.Models
{
    public partial class HoaDon
    {
        /// <summary>
        /// Tính tiền bàn dựa trên giờ vào và giờ ra
        /// </summary>
        public double GetTableFee()
        {
            return BilliardCalculator.CalculateTableFee(GioVaoHoaDon, GioRaHoaDon);
        }

        /// <summary>
        /// Tính tổng tiền đồ uống (từ chi tiết hóa đơn)
        /// </summary>
        public double GetFoodDrinkTotal()
        {
            if (ChiTietHoaDons == null || ChiTietHoaDons.Count == 0)
                return 0;
            
            return ChiTietHoaDons.Sum(ct => (ct.GiaChiTiet ?? 0) * (ct.SoLuongChiTiet ?? 1));
        }

        /// <summary>
        /// Tính tổng tiền trước giảm giá (tiền bàn + tiền đồ uống)
        /// </summary>
        public double GetSubtotal()
        {
            return GetTableFee() + GetFoodDrinkTotal();
        }

        /// <summary>
        /// Tính số tiền giảm giá dựa trên điểm tích lũy của khách hàng
        /// </summary>
        public double GetDiscount()
        {
            if (KhachHang == null)
                return 0;

            int loyaltyPoints = KhachHang.DiemTichLuy ?? 0;
            return BilliardCalculator.CalculateDiscount(GetSubtotal(), loyaltyPoints);
        }

        /// <summary>
        /// Tính tổng tiền cuối cùng (sau giảm giá)
        /// </summary>
        public double GetFinalTotal()
        {
            if (KhachHang == null)
            {
                // Khách lẻ: không có giảm giá
                return GetSubtotal();
            }

            int loyaltyPoints = KhachHang.DiemTichLuy ?? 0;
            return BilliardCalculator.CalculateFinalTotal(
                GetFoodDrinkTotal(), 
                GetTableFee(), 
                loyaltyPoints
            );
        }

        /// <summary>
        /// Tính điểm tích lũy mà khách hàng sẽ nhận được từ hóa đơn này
        /// </summary>
        public int GetEarnedLoyaltyPoints()
        {
            // Tính điểm dựa trên tổng tiền trước giảm giá
            return BilliardCalculator.CalculateLoyaltyPoints(GetSubtotal());
        }

        /// <summary>
        /// Lấy loại hội viên của khách hàng
        /// </summary>
        public string GetMembershipTier()
        {
            if (KhachHang == null)
                return "Regular";

            int loyaltyPoints = KhachHang.DiemTichLuy ?? 0;
            return BilliardCalculator.GetMembershipTier(loyaltyPoints);
        }
    }
}
