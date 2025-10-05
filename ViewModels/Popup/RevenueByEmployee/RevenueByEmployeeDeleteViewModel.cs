using BlockHouse.Api;
using BlockHouse.ViewModels.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Input;
using static BlockHouse.ViewModels.MainWindowViewModel;
using static BlockHouse.ViewModels.RevenueViewModel;

namespace BlockHouse.ViewModels.Popup.RevenueByEmployee
{
    class RevenueByEmployeeDeleteViewModel : ObservableObject, IDisposable
    {
        private readonly RevenueApi _revenueApi;
        private bool _disposed;

        private string _dateTime;
        public string DateTime
        {
            get => _dateTime;
            set => SetProperty(ref _dateTime, value);
        }

        private List<ServiceSummary> _services;
        public List<ServiceSummary> Services
        {
            get => _services;
            set => SetProperty(ref _services, value);
        }

        private string _income;
        public string Income
        {
            get => _income;
            set => SetProperty(ref _income, value);
        }

        private string _employee;
        public string Employee
        {
            get => _employee;
            set => SetProperty(ref _employee, value);
        }

        public string ServicesDisplay
        {
            get
            {
                if (Services == null || Services.Count == 0)
                    return string.Empty;

                return string.Join(", ", Services.Select(s => $"{s.Name} x{s.Quantity}"));
            }
        }

        private int _orderId;
        public int OrderId
        {
            get => _orderId;
            set => SetProperty(ref _orderId, value);
        }

        public ICommand CloseCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }

        public event Action RequestClose;

        public RevenueByEmployeeDeleteViewModel()
        {
            _revenueApi = new RevenueApi();
            CloseCommand = new RelayCommand(ExecuteClose);
            DeleteCommand = new AsyncRelayCommand(ExecuteDelete);
        }

        public async Task LoadDataAsync(int orderId)
        {
            OrderId = orderId;
            try
            {
                var response = await _revenueApi.GetOrderById(orderId);
                if (response.IsSuccess && response.Data != null)
                {
                    var order = response.Data;
                    DateTime = order.DateTime.ToString("dd-MM-yyyy HH:mm:ss");
                    Services = order.Services.Select(s => new ServiceSummary
                    {
                        Name = s.Name,
                        Quantity = s.Quantity
                    }).ToList();
                    Income = order.Total?.ToString("N0") ?? "0";
                    Employee = order.EmployeeName ?? "Không xác định";
                }
                else
                {
                    CustomMessageBox.Show(
                            "Lỗi tải thông tin doanh thu!",
                            new List<string> { "OK" },
                            MessageType.Error
                            );
                    DateTime = "N/A";
                    Services = new List<ServiceSummary>();
                    Income = "N/A";
                    Employee = "N/A";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading order detail: {ex.Message}");
                CustomMessageBox.Show(
                        "Lỗi tải thông tin doanh thu!",
                        new List<string> { "OK" },
                        MessageType.Error
                        );
                DateTime = "N/A";
                Services = new List<ServiceSummary>();
                Income = "N/A";
                Employee = "N/A";
            }
        }

        private void ExecuteClose()
        {
            RequestClose?.Invoke();
            Dispose(); // Gọi Dispose khi đóng ViewModel
        }

        private async Task ExecuteDelete()
        {
            if (OrderId <= 0) return;

            // Show overlay
            WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });
            string result;

            try
            {
                var response = await _revenueApi.DeleteOrderById(OrderId);
                if (response.IsSuccess)
                {
                    result = CustomMessageBox.Show(
                            "Xóa đơn hàng thành công!",
                            new List<string> { "OK" },
                            MessageType.Information
                            );
                }
                else
                {
                    result = CustomMessageBox.Show(
                            "Xóa đơn hàng không thành công!",
                            new List<string> { "OK" },
                            MessageType.Error
                            );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading order detail: {ex.Message}");
                result = CustomMessageBox.Show(
                        "Xóa đơn hàng không thành công!",
                        new List<string> { "OK" },
                        MessageType.Error
                        );
            }

            if (result == "OK")
            {
                ExecuteClose();
            }
        }

        // Triển khai IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Ngăn GC gọi finalizer nếu đã dispose
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Giải phóng tài nguyên được quản lý
                if (_revenueApi is IDisposable disposableApi)
                {
                    disposableApi.Dispose();
                }

                // Giải phóng các tài nguyên khác nếu có
                // Ví dụ: Hủy đăng ký sự kiện, giải phóng danh sách, v.v.
                Services?.Clear();
                RequestClose = null; // Hủy đăng ký sự kiện
            }

            // Giải phóng tài nguyên không được quản lý (nếu có)
            // Ví dụ: Đóng kết nối mạng, file handler, v.v.

            _disposed = true;
        }

        // Finalizer (chỉ cần nếu có tài nguyên không được quản lý)
        ~RevenueByEmployeeDeleteViewModel()
        {
            Dispose(false);
        }
    }
}