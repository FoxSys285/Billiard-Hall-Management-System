# 🎱 Hệ Thống Quản Lý Quán Bida

> Ứng dụng quản lý quán bida hiện đại, giúp tối ưu hóa quy trình kinh doanh và nâng cao chất lượng dịch vụ

![Version](https://img.shields.io/badge/version-1.0-blue)
![Platform](https://img.shields.io/badge/platform-.NET%206.0-brightgreen)
![License](https://img.shields.io/badge/license-MIT-green)

---

## 📋 Mục Lục

- [Giới Thiệu](#giới-thiệu)
- [Tính Năng](#tính-năng)
- [Công Nghệ](#công-nghệ)
- [Cài Đặt](#cài-đặt)
- [Cấu Trúc Dự Án](#cấu-trúc-dự-án)
- [Hướng Dẫn Sử Dụng](#hướng-dẫn-sử-dụng)
- [Nhóm Phát Triển](#nhóm-phát-triển)

---

## 🎯 Giới Thiệu

**QuanLyQuanBida** là một ứng dụng desktop toàn diện được xây dựng trên nền tảng WPF (.NET), thiết kế đặc biệt để quản lý hoạt động kinh doanh của các quán bida.

Ứng dụng cung cấp các công cụ mạnh mẽ để quản lý bàn chơi, khách hàng, nhân viên, hóa đơn, kho hàng và báo cáo chi tiết.

---

## ✨ Tính Năng

### 👥 Quản Lý Khách Hàng
- Thêm/sửa/xóa thông tin khách hàng
- Quản lý lịch sử giao dịch
- Theo dõi khách hàng VIP

### 🎱 Quản Lý Bàn Chơi
- Quản lý trạng thái các bàn
- Ghi nhận thời gian sử dụng
- Theo dõi doanh thu từng bàn

### 💰 Quản Lý Hóa Đơn
- Tạo hóa đơn nhanh chóng
- Quản lý chi tiết sản phẩm/dịch vụ
- In báo cáo hóa đơn
- Lịch sử hóa đơn chi tiết

### 📦 Quản Lý Kho Hàng
- Quản lý sản phẩm (đồ ăn, đồ uống, dụng cụ)
- Phiếu nhập hàng
- Theo dõi tồn kho
- Cảnh báo hết hàng

### 👔 Quản Lý Nhân Viên
- Quản lý thông tin nhân viên
- Chấm công tự động
- Bảng chấm công chi tiết

### 📊 Báo Cáo & Thống Kê
- Báo cáo doanh thu
- Thống kê bàn chơi
- Phân tích doanh thu theo thời gian
- Xuất báo cáo định kỳ

### 🔐 Bảo Mật
- Hệ thống đăng nhập bảo mật
- Phân quyền admin/nhân viên
- Mã hóa dữ liệu

---

## 🛠️ Công Nghệ

| Thành Phần | Công Nghệ |
|-----------|-----------|
| **Nền Tảng** | .NET 6.0 Framework |
| **UI Framework** | WPF (Windows Presentation Foundation) |
| **Database** | SQL Server |
| **ORM** | Entity Framework 6.5.1 |
| **XAML** | WPF XAML với Data Binding |
| **Pattern** | MVVM (Model-View-ViewModel) |

### Thư Viện Chính
- Entity Framework 6.5.1 - ORM cho SQL Server
- WPF Components - UI tiên tiến
- System.Windows - Windows Desktop API

---

## 📁 Cấu Trúc Dự Án

```
QuanLyQuanBida/
├── Models/                      # Các lớp mô hình dữ liệu
│   ├── Ban.cs                  # Model bàn chơi
│   ├── HoaDon.cs               # Model hóa đơn
│   ├── KhachHang.cs            # Model khách hàng
│   ├── NhanVien.cs             # Model nhân viên
│   ├── SanPham.cs              # Model sản phẩm
│   ├── Kho.cs                  # Model kho hàng
│   ├── Model1.Context.cs       # DbContext Entity Framework
│   └── ...
├── Views/                       # Các cửa sổ/màn hình giao diện
│   ├── MainWindow.xaml         # Cửa sổ chính
│   ├── AdminMenuView.xaml      # Menu admin
│   ├── BanView.xaml            # Quản lý bàn
│   ├── HoaDonView.xaml         # Quản lý hóa đơn
│   ├── NhanVienView.xaml       # Quản lý nhân viên
│   ├── KhoView.xaml            # Quản lý kho
│   └── ...
├── ViewModels/                  # Logic xử lý giao diện
│   ├── MainViewModel.cs        # ViewModel chính
│   ├── BaseViewModel.cs        # Base class cho ViewModels
│   ├── RelayCommand.cs         # Lệnh relay
│   └── ...
├── Converters/                  # Value Converters XAML
│   ├── BoolToVisibilityConverter.cs
│   ├── ImagePathConverter.cs
│   └── ...
├── App.xaml                     # Cấu hình ứng dụng
├── App.config                   # File cấu hình
└── packages.config              # Quản lý NuGet packages
```

---

## 🚀 Cài Đặt

### Yêu Cầu Hệ Thống
- **OS**: Windows 7+ (khuyến nghị Windows 10/11)
- **.NET Framework**: .NET 6.0 trở lên
- **SQL Server**: 2016+ hoặc SQL Server Express
- **RAM**: Tối thiểu 4GB
- **Ổ cứng**: Tối thiểu 500MB

### Các Bước Cài Đặt

#### 1. Chuẩn Bị Database
```sql
-- Chạy script QUANLYBIDA.sql để tạo database
-- Từ SQL Server Management Studio hoặc Command Prompt
sqlcmd -S your_server -U sa -P your_password -i QUANLYBIDA.sql
```

#### 2. Clone/Tải Dự Án
```bash
# Clone từ repository (nếu có)
git clone https://github.com/yourusername/QuanLyQuanBida.git
cd QuanLyQuanBida
```

#### 3. Mở Dự Án
```bash
# Mở với Visual Studio
start QuanLyQuanBida.slnx
```

#### 4. Cấu Hình Kết Nối Database
- Chỉnh sửa `App.config`
- Cập nhật connection string:
```xml
<connectionStrings>
    <add name="Model1" 
         connectionString="Server=YOUR_SERVER;Database=QUANLYBIDA;User Id=sa;Password=YOUR_PASSWORD;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

#### 5. Cài Đặt Dependencies
```bash
# Restore NuGet packages
nuget restore QuanLyQuanBida.sln
```

#### 6. Build & Run
```bash
# Build dự án
dotnet build

# Chạy ứng dụng
dotnet run
```

---

## 📖 Hướng Dẫn Sử Dụng

### Đăng Nhập
1. Nhập tên đăng nhập (admin)
2. Nhập mật khẩu
3. Nhấn **Đăng Nhập**

### Quản Lý Hóa Đơn
1. Vào menu **Hóa Đơn**
2. Chọn **Tạo Hóa Đơn Mới**
3. Chọn bàn chơi
4. Nhập thông tin khách hàng
5. Chọn sản phẩm/dịch vụ
6. Tính toán và in hóa đơn

### Xem Báo Cáo
1. Vào menu **Báo Cáo**
2. Chọn loại báo cáo (doanh thu, bàn, nhân viên)
3. Chọn khoảng thời gian
4. Xuất file PDF/Excel

### Quản Lý Kho
1. Vào menu **Kho**
2. Thêm/sửa sản phẩm
3. Nhập hàng qua **Phiếu Nhập**
4. Kiểm tra tồn kho

---

## 🔧 Cấu Hình

### File App.config
```xml
<configuration>
  <connectionStrings>
    <!-- Cấu hình SQL Server -->
    <add name="Model1" connectionString="..." />
  </connectionStrings>
  <appSettings>
    <!-- Cài đặt ứng dụng -->
  </appSettings>
</configuration>
```

### Tùy Chỉnh Giao Diện
- Sửa XAML files trong thư mục `Views/`
- Cập nhật style trong `App.xaml`
- Thay đổi theme color nếu cần

---

## 🐛 Troubleshooting

### Lỗi Kết Nối Database
```
Error: A network-related or instance-specific error occurred while establishing a connection to SQL Server.
```
**Giải Pháp**: 
- Kiểm tra SQL Server đang chạy
- Xác nhận connection string
- Kiểm tra firewall

### Lỗi Entity Framework
```
Error: The database does not exist
```
**Giải Pháp**:
- Chạy lại script QUANLYBIDA.sql
- Xóa migration và tạo mới

### Lỗi XAML
**Giải Pháp**:
- Rebuild Solution
- Xóa thư mục `bin` và `obj`
- Clean Solution rồi Build lại

---

## 📝 Notes cho Developer

### Kiến Trúc MVVM
- **Model**: Các lớp dữ liệu trong thư mục `Models/`
- **View**: Các file XAML trong thư mục `Views/`
- **ViewModel**: Các lớp logic trong thư mục `ViewModels/`

### Thêm Tính Năng Mới
1. Tạo Model nếu cần
2. Tạo View (XAML)
3. Tạo ViewModel
4. Binding View ↔ ViewModel
5. Implement logic trong ViewModel

### Database Migration
```bash
# Nếu cần update database schema
Add-Migration MigrationName
Update-Database
```

---

## 📞 Hỗ Trợ & Liên Hệ

Nếu có câu hỏi hoặc báo cáo lỗi, vui lòng:
- Tạo Issue trên GitHub
- Liên hệ nhóm phát triển
- Gửi email: support@quanlybida.com

---

## 👥 Nhóm Phát Triển

**Nhóm 04 - Lớp Công Nghệ Phần Mềm**
- HUIT (Đại Học Công Nghệ TP.HCM)
- Năm: 2026

---

## 📄 Giấy Phép

Dự án này được cấp phép dưới MIT License. Chi tiết xem tại [LICENSE](LICENSE) file.

---

## 🎉 Cảm Ơn

Cảm ơn các bạn đã sử dụng hệ thống QuanLyQuanBida!

**Phiên Bản**: 1.0  
**Cập Nhật Lần Cuối**: Tháng 5, 2026  
**Trạng Thái**: Đang Phát Triển ✨

---

<div align="center">

**Made with ❤️ by HUIT Group 04**

⭐ Nếu bạn thích dự án này, hãy give a star!

</div>
