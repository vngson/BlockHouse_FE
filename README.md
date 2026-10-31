# BlockHouse Desktop 2

Bản chạy mới là WPF + WebView2, đóng gói sẵn giao diện React từ source BLH_website. Hoạt động offline với cùng BE/tài khoản/database như web. Mở/build trong Visual Studio dùng .NET 8 để tương thích IDE hiện có. Build bộ cài bằng `build-desktop.ps1` dùng CLI .NET 10 LTS SDK và chỉ định target .NET 10 riêng; source UI cũ bên dưới chỉ giữ để tham chiếu và không biên dịch trong bản mới.

Hướng dẫn đầy đủ: `../Blockhouse_BE/installer/RELEASE_GUIDE.md`. Cài lần đầu phải chọn DB cũ nếu muốn tiếp tục dữ liệu; đóng Desktop không dừng BE.

---

# BlockHouse

A barbershop management desktop application built with WPF and .NET 8.

## Features

- **Dashboard** - Revenue charts, employee rankings, and business metrics overview
- **Revenue Management** - Track revenue by date, employee, and service
- **Employee Management** - Add, edit, and manage barbershop staff
- **Service Management** - Manage services with pricing and descriptions

## Tech Stack

- **Framework**: WPF on .NET 8.0
- **Architecture**: MVVM (Model-View-ViewModel)
- **UI**: Material Design in XAML
- **Charts**: LiveCharts.Wpf
- **Backend API**: Runs locally on `http://localhost:5000`

## Project Structure

```
BlockHouse_FE/
├── Api/                    # API client classes
├── Assets/                 # Images and icons
├── Converters/             # XAML value converters
├── Helpers/                # Utility classes
├── Models/                 # Data models
├── Services/               # Backend process management
├── ViewModels/             # MVVM ViewModels
│   ├── Components/         # Reusable component VMs
│   └── Popup/              # Dialog VMs
├── Views/                  # XAML views
│   ├── Components/         # Reusable UI components
│   └── Popup/              # Dialog views
├── App.xaml                # App resources and theme
├── MainWindow.xaml         # Main window
└── BlockHouse.csproj       # Project configuration
```

## Prerequisites

- Visual Studio 2022 (v17.10+)
- .NET 8.0 SDK

## Getting Started

1. Clone the repository
2. Open `BlockHouse.sln` in Visual Studio
3. Restore NuGet packages
4. Build and run the solution

The application automatically starts the backend process (`app.exe`) on launch and connects to it via REST API.

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.4.0 | MVVM framework |
| MaterialDesignThemes | 5.2.1 | Material Design UI |
| MaterialDesignColors | 5.2.1 | Material Design colors |
| LiveCharts.Wpf | 0.9.7 | Data visualization charts |
| Newtonsoft.Json | 13.0.3 | JSON serialization |
| Haley.MVVM | 6.5.2 | MVVM utilities |
| System.Drawing.Common | 9.0.8 | Image handling |

## Chạy F5 trong Visual Studio

Build tự chép BE.exe đã chuẩn bị và giao diện offline vào output. Nếu thiếu dependency, build báo rõ cần chạy build-desktop.ps1. Debug tái sử dụng BE API v2 đang chạy; nếu cổng 5000 bị BE cũ chiếm, cần dừng BE cũ trước. Khi cần tự mở BE, Debug chạy BE riêng ngoài thư mục bin để không khóa DLL/EXE WPF lúc rebuild. DB Debug dùng BLOCKHOUSE_DB_PATH nếu có, tiếp đến D:\Release_BlackHouse\BlockHouse.db, rồi DB trong source BE; không tạo shop mới trong AppData. Bản Release vẫn dùng launcher và DB AppData của bộ cài.

## Nhớ tài khoản WPF
Bản mới lưu %LOCALAPPDATA%\BlockHouse\data\remembered-login.json sau khi đăng nhập thành công và tick Nhớ tài khoản. Tên/mật khẩu được mã hóa DPAPI CurrentUser; tự điền ở màn hình đăng nhập, giữ sau đăng xuất; bỏ tick rồi đăng nhập để xóa. Web Chrome/Edge dùng autocomplete riêng. Khi đổi mật khẩu, tài khoản đang nhớ được cập nhật nếu trùng tên. Cầu nối chỉ nhận đúng trang bundle nội bộ và API 127.0.0.1:5050, không nhận đường dẫn file từ JavaScript.

Kiểm thử native (DB/tài khoản tạm): dotnet run --project tests/RememberedLoginStore.Tests/RememberedLoginStore.Tests.csproj. Bộ kiểm thử xác nhận mã hóa JSON, đọc lại qua tiến trình khác, file hỏng, ghi đè mật khẩu, xóa lựa chọn nhớ và chặn origin/API sai.

## Thanh tiêu đề riêng
Caption cao 40 DIP với WindowChrome, logo và Bebas Neue offline. Nút SystemCommands thu nhỏ/phóng to-thu về/đóng có tên truy cập và phím hệ thống vẫn dùng được. Theme chỉ nhận kênh blockhouse-theme từ index.html của host nội bộ. Các file giao diện: DesktopWindow.xaml, DesktopWindow.Chrome.cs; kiểm tra thông điệp theme trong Services/DesktopTheme.cs. Đóng Desktop không dừng BE.

Kiểm tra render: build bản Debug vào bin/Debug/ui-check, rồi build tests/DesktopCaption.Tests; chạy DLL của project test với tham số thư mục output để xuất ảnh sáng/tối 360/1200px. Bộ render không Show cửa sổ hoặc khởi động BE. Cần thử kéo, double click, resize và work area/taskbar trên máy thực tế.
