using BlockHouse.Api;
using BlockHouse.Helpers; // Thêm using cho AppLogger
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using static BlockHouse.ViewModels.RevenueViewModel;

namespace BlockHouse.ViewModels.Popup.RevenueByEmployee
{
    class RevenueByEmployeeDetailViewModel : ObservableObject, IDisposable
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

                return string.Join(", ", Services.Select(s => $"{s.Name} ({s.Quantity})"));
            }
        }

        public ICommand CloseCommand { get; }

        public event Action RequestClose;

        public RevenueByEmployeeDetailViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo RevenueByEmployeeDetailViewModel");
            _revenueApi = new RevenueApi();
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        public async Task LoadDataAsync(int orderId)
        {
            AppLogger.Instance.LogInfo($"Bắt đầu LoadDataAsync cho orderId={orderId}");
            try
            {
                var response = await _revenueApi.GetOrderById(orderId);
                if (response.IsSuccess && response.Data != null)
                {
                    AppLogger.Instance.LogInfo($"Tải chi tiết đơn hàng thành công cho orderId={orderId}");
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
                    AppLogger.Instance.LogWarning($"Không tìm thấy dữ liệu đơn hàng cho orderId={orderId}: {response.Message}");
                    DateTime = "N/A";
                    Services = new List<ServiceSummary>();
                    Income = "N/A";
                    Employee = "N/A";
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError($"Lỗi khi tải chi tiết đơn hàng orderId={orderId}", ex);
                DateTime = "N/A";
                Services = new List<ServiceSummary>();
                Income = "N/A";
                Employee = "N/A";
            }
        }

        private void ExecuteClose()
        {
            AppLogger.Instance.LogInfo("Đóng RevenueByEmployeeDetailViewModel");
            RequestClose?.Invoke();
            Dispose(); // Gọi Dispose khi đóng ViewModel
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
                AppLogger.Instance.LogInfo("Dispose RevenueByEmployeeDetailViewModel");
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
        ~RevenueByEmployeeDetailViewModel()
        {
            Dispose(false);
        }
    }
}