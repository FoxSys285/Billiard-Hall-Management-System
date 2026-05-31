using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyQuanBida.Models
{
    /// <summary>
    /// Helper class để tính tiền bàn, điểm tích lũy và chiết khấu
    /// </summary>
    public static class BilliardCalculator
    {
        // Hằng số giá tiền bàn
        private const double RATE_MORNING = 40000;   // 8g - 16g: 40k/giờ
        private const double RATE_EVENING = 50000;   // > 16g: 50k/giờ
        private const int MORNING_START = 8;         // 8:00
        private const int EVENING_START = 16;        // 16:00

        // Hằng số điểm tích lũy
        private const int POINTS_PER_10K = 1;        // 10k tiền => 1 điểm
        private const int SILVER_POINTS = 50;        // 50 điểm => Silver (5% giảm)
        private const int GOLD_POINTS = 80;          // 80 điểm => Gold (8% giảm)
        private const int PLATINUM_POINTS = 100;     // 100+ điểm => Platinum (10% giảm)

        private const double SILVER_DISCOUNT = 0.05;     // 5%
        private const double GOLD_DISCOUNT = 0.08;       // 8%
        private const double PLATINUM_DISCOUNT = 0.10;   // 10%

        /// <summary>
        /// Tính tiền bàn dựa trên giờ vào và giờ ra
        /// Tính theo phút thực tế: 40k/giờ = 666.67đ/phút (8g-16g), 50k/giờ = 833.33đ/phút (>16g)
        /// </summary>
        /// <param name="timeIn">Giờ vào (TimeSpan)</param>
        /// <param name="timeOut">Giờ ra (TimeSpan)</param>
        /// <returns>Tiền bàn (đơn vị: đ)</returns>
        public static double CalculateTableFee(TimeSpan? timeIn, TimeSpan? timeOut)
        {
            if (!timeIn.HasValue || !timeOut.HasValue)
                return 0;

            try
            {
                DateTime now = DateTime.Now;
                DateTime inTime = now.Date.Add(timeIn.Value);
                DateTime outTime = now.Date.Add(timeOut.Value);

                // Nếu giờ ra < giờ vào, cộng thêm 1 ngày
                if (outTime < inTime)
                    outTime = outTime.AddDays(1);

                TimeSpan duration = outTime - inTime;
                double totalMinutes = duration.TotalMinutes;

                if (totalMinutes <= 0)
                    return 0;

                double fee = 0;
                double ratePerMinuteMorning = RATE_MORNING / 60.0;  // 666.67 đ/phút
                double ratePerMinuteEvening = RATE_EVENING / 60.0;  // 833.33 đ/phút

                // Thời điểm chuyển từ sáng sang tối
                DateTime transitionTime = inTime.Date.AddHours(EVENING_START);

                // Trường hợp 1: Toàn bộ là giờ sáng (8g-16g)
                if (outTime <= transitionTime)
                {
                    fee = totalMinutes * ratePerMinuteMorning;
                }
                // Trường hợp 2: Toàn bộ là giờ tối (từ 16g trở đi hoặc trước 8g)
                else if (inTime >= transitionTime)
                {
                    fee = totalMinutes * ratePerMinuteEvening;
                }
                // Trường hợp 3: Kéo dài từ sáng sang tối
                else
                {
                    // Tính tiền phần sáng (từ giờ vào đến 16g)
                    TimeSpan morningDuration = transitionTime - inTime;
                    fee += morningDuration.TotalMinutes * ratePerMinuteMorning;

                    // Tính tiền phần tối (từ 16g đến giờ ra)
                    TimeSpan eveningDuration = outTime - transitionTime;
                    fee += eveningDuration.TotalMinutes * ratePerMinuteEvening;
                }

                return Math.Round(fee, 0);  // Làm tròn đến đồng
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Tính điểm tích lũy dựa trên số tiền hóa đơn (trước giảm giá)
        /// </summary>
        /// <param name="amount">Số tiền (đơn vị: VNĐ)</param>
        /// <returns>Số điểm tích lũy</returns>
        public static int CalculateLoyaltyPoints(double amount)
        {
            if (amount <= 0)
                return 0;

            // Cứ 10k = 1 điểm
            return (int)(amount / 10000) * POINTS_PER_10K;
        }

        /// <summary>
        /// Lấy tên loại hội viên dựa trên điểm tích lũy
        /// </summary>
        public static string GetMembershipTier(int points)
        {
            if (points >= PLATINUM_POINTS)
                return "Platinum";
            else if (points >= GOLD_POINTS)
                return "Gold";
            else if (points >= SILVER_POINTS)
                return "Silver";
            else
                return "Regular";
        }

        /// <summary>
        /// Tính tỷ lệ giảm giá dựa trên điểm tích lũy
        /// </summary>
        public static double GetDiscountRate(int points)
        {
            if (points >= PLATINUM_POINTS)
                return PLATINUM_DISCOUNT;
            else if (points >= GOLD_POINTS)
                return GOLD_DISCOUNT;
            else if (points >= SILVER_POINTS)
                return SILVER_DISCOUNT;
            else
                return 0;
        }

        /// <summary>
        /// Tính số tiền giảm giá
        /// </summary>
        public static double CalculateDiscount(double amount, int loyaltyPoints)
        {
            double discountRate = GetDiscountRate(loyaltyPoints);
            return amount * discountRate;
        }

        /// <summary>
        /// Tính tổng tiền cuối cùng (tiền bàn + tiền đồ uống - giảm giá)
        /// </summary>
        public static double CalculateFinalTotal(double foodDrinkAmount, double tableFee, int loyaltyPoints)
        {
            double subtotal = foodDrinkAmount + tableFee;
            double discount = CalculateDiscount(subtotal, loyaltyPoints);
            return subtotal - discount;
        }
    }
}
