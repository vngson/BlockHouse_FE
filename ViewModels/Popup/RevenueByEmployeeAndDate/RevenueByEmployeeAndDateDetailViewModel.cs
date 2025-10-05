using BlockHouse.Api;
using BlockHouse.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using static BlockHouse.ViewModels.RevenueViewModel;

namespace BlockHouse.ViewModels.Popup.RevenueByEmployeeAndDate
{
    public class RevenueByEmployeeAndDateDetailViewModel : ObservableObject, IDisposable
    {
        private readonly RevenueApi _revenueApi;
        private bool _disposed;

        private string _datetime;
        public string Datetime
        {
            get => _datetime;
            set => SetProperty(ref _datetime, value);
        }

        private int _employeeId;
        public int EmployeeId
        {
            get => _employeeId;
            set => SetProperty(ref _employeeId, value);
        }

        private string _employee;
        public string Employee
        {
            get => _employee;
            set => SetProperty(ref _employee, value);
        }

        private List<ServiceSummary> _services;
        public List<ServiceSummary> Services
        {
            get => _services;
            set
            {
                if (SetProperty(ref _services, value))
                {
                    OnPropertyChanged(nameof(ServicesDisplay));
                }
            }
        }

        private decimal _total;
        public decimal Total
        {
            get => _total;
            set
            {
                if (SetProperty(ref _total, value))
                {
                    OnPropertyChanged(nameof(TotalDisplay));
                }
            }
        }

        public string TotalDisplay => string.Format("{0:N0} VNĐ", Total);

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

        public RevenueByEmployeeAndDateDetailViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo RevenueByEmployeeAndDateDetailViewModel");
            _revenueApi = new RevenueApi();
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        public async Task LoadDataAsync(int employeeId, string dateTime)
        {
            AppLogger.Instance.LogInfo($"Bắt đầu LoadDataAsync cho ngày {dateTime} và nhân viên ID {employeeId}");
            try
            {
                var response = await _revenueApi.GetOrdersByDateAndEmployee(dateTime, employeeId);
                if (response.IsSuccess && response.Data != null && response.Data.OrderData != null)
                {
                    AppLogger.Instance.LogInfo($"Tải chi tiết doanh thu theo ngày và nhân viên thành công: {dateTime}, EmployeeId: {employeeId}");
                    var order = response.Data.OrderData;

                    Datetime = order.Date.ToString("dd-MM-yyyy");
                    EmployeeId = order.EmployeeId;
                    Employee = order.EmployeeName ?? "Không xác định";
                    Services = order.Services.Select(s => new ServiceSummary
                    {
                        Name = s.Name,
                        Quantity = s.Quantity
                    }).ToList();
                    Total = (decimal)order.Total;
                }
                else
                {
                    AppLogger.Instance.LogWarning($"Không tìm thấy dữ liệu doanh thu cho ngày {dateTime} và nhân viên ID {employeeId}: {response.Message}");
                    Datetime = "N/A";
                    EmployeeId = employeeId;
                    Employee = "Không xác định";
                    Services = new List<ServiceSummary>();
                    Total = 0;
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError($"Lỗi khi tải chi tiết doanh thu cho ngày {dateTime} và nhân viên ID {employeeId}", ex);
                Datetime = "N/A";
                EmployeeId = employeeId;
                Employee = "Không xác định";
                Services = new List<ServiceSummary>();
                Total = 0;
            }
        }

        private void ExecuteClose()
        {
            AppLogger.Instance.LogInfo("Đóng RevenueByEmployeeAndDateDetailViewModel");
            RequestClose?.Invoke();
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                AppLogger.Instance.LogInfo("Dispose RevenueByEmployeeAndDateDetailViewModel");
                if (_revenueApi is IDisposable disposableApi)
                {
                    disposableApi.Dispose();
                }
                Services?.Clear();
                RequestClose = null;
            }

            _disposed = true;
        }

        ~RevenueByEmployeeAndDateDetailViewModel()
        {
            Dispose(false);
        }
    }
}