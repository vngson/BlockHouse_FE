using BlockHouse.Api;
using BlockHouse.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BlockHouse.ViewModels.Popup.Employee
{
    public class EmployeeDetailViewModel : ObservableObject, IDisposable
    {
        private readonly EmployeeApi _employeeApi;
        private bool _disposed;

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _income;
        public string Income
        {
            get => _income;
            set => SetProperty(ref _income, value);
        }

        private string _joinDate;
        public string JoinDate
        {
            get => _joinDate;
            set => SetProperty(ref _joinDate, value);
        }

        public ICommand CloseCommand { get; }

        public event Action RequestClose;

        public EmployeeDetailViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo EmployeeDetailViewModel");
            _employeeApi = new EmployeeApi();
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        public async Task LoadDataAsync(int employeeId)
        {
            AppLogger.Instance.LogInfo($"Bắt đầu LoadDataAsync cho employeeId={employeeId}");
            try
            {
                var response = await _employeeApi.GetEmployeeById(employeeId);
                if (response.IsSuccess && response.Data != null)
                {
                    AppLogger.Instance.LogInfo($"Tải chi tiết nhân viên thành công cho employeeId={employeeId}");
                    var employee = response.Data;



                    Name = employee.Name ?? "Không xác định";
                    Phone = employee.Phone ?? "Không xác định";
                    Status = employee.Status == 0 ? "Nghỉ" : "Hoạt động";
                    Income = (employee.Income ?? 0).ToString("N0") + " VNĐ";
                    JoinDate = employee.CreatedAt.ToString("dd-MM-yyyy");
                }
                else
                {
                    AppLogger.Instance.LogWarning($"Không tìm thấy dữ liệu nhân viên cho employeeId={employeeId}: {response.Message}");
                    SetDefaultValues();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError($"Lỗi khi tải chi tiết nhân viên employeeId={employeeId}", ex);
                SetDefaultValues();
            }
        }

        private void SetDefaultValues()
        {
            Name = "N/A";
            Phone = "N/A";
            Status = "N/A";
            Income = "N/A";
            JoinDate = DateTime.Now.ToString("dd-MM-yyyy");
        }

        private void ExecuteClose()
        {
            AppLogger.Instance.LogInfo("Đóng EmployeeDetailViewModel");
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
                AppLogger.Instance.LogInfo("Dispose EmployeeDetailViewModel");
                if (_employeeApi is IDisposable disposableApi)
                {
                    disposableApi.Dispose();
                }
                RequestClose = null;
            }

            _disposed = true;
        }

        ~EmployeeDetailViewModel()
        {
            Dispose(false);
        }
    }
}