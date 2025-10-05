using BlockHouse.Api;
using BlockHouse.Helpers;
using BlockHouse.ViewModels.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BlockHouse.ViewModels.Popup.Employee
{
    public class EmployeeEditViewModel : ObservableObject, IDisposable
    {
        private readonly EmployeeApi _employeeApi;
        private bool _disposed;
        private int _employeeId;

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

        private int _status; // 0: Nghỉ, 1: Hoạt động
        public int Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        // Properties cho radio button binding
        public bool IsActive
        {
            get => Status == 1;
            set
            {
                if (value) Status = 1;
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(IsInactive));
            }
        }

        public bool IsInactive
        {
            get => Status == 0;
            set
            {
                if (value) Status = 0;
                OnPropertyChanged(nameof(IsInactive));
                OnPropertyChanged(nameof(IsActive));
            }
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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Commands
        public IAsyncRelayCommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action RequestClose;

        public EmployeeEditViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo EmployeeEditViewModel");
            _employeeApi = new EmployeeApi();
            SaveCommand = new AsyncRelayCommand(ExecuteSave, CanExecuteSave);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        public async Task LoadDataAsync(int employeeId)
        {
            AppLogger.Instance.LogInfo($"Bắt đầu LoadDataAsync cho employeeId={employeeId}");
            _employeeId = employeeId;
            try
            {
                IsLoading = true;
                var response = await _employeeApi.GetEmployeeById(employeeId);
                if (response.IsSuccess && response.Data != null)
                {
                    AppLogger.Instance.LogInfo($"Tải chi tiết nhân viên thành công cho employeeId={employeeId}");
                    var employee = response.Data;

                    Name = employee.Name ?? "Không xác định";
                    Phone = employee.Phone ?? "Không xác định";
                    Status = employee.Status; // 0 hoặc 1
                    Income = (employee.Income ?? 0).ToString("N0") + " VNĐ";
                    JoinDate = employee.CreatedAt.ToString("dd-MM-yyyy") ?? DateTime.Now.ToString();

                    // Trigger property change for radio buttons
                    OnPropertyChanged(nameof(IsActive));
                    OnPropertyChanged(nameof(IsInactive));
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
            finally
            {
                IsLoading = false;
            }
        }

        private void SetDefaultValues()
        {
            Name = "N/A";
            Phone = "N/A";
            Status = 1; // Mặc định là hoạt động
            Income = "N/A";
            JoinDate = DateTime.Now.ToString();
        }

        private bool CanExecuteSave()
        {
            return !IsLoading;
        }

        private async Task ExecuteSave()
        {
            AppLogger.Instance.LogInfo($"Bắt đầu lưu thông tin nhân viên employeeId={_employeeId}");
            try
            {
                IsLoading = true;
                SaveCommand.NotifyCanExecuteChanged();

                // Gọi API để cập nhật trạng thái
                var response = await _employeeApi.UpdateEmployee(_employeeId, Status);
                if (response.IsSuccess)
                {
                    AppLogger.Instance.LogInfo($"Cập nhật trạng thái nhân viên thành công employeeId={_employeeId}");
                    CustomMessageBox.Show(
                        "Cập nhật trạng thái nhân viên thành công!",
                        new List<string> { "OK" },
                        MessageType.Information
                    );
                    RequestClose?.Invoke();
                }
                else
                {
                    AppLogger.Instance.LogError($"Lỗi khi cập nhật trạng thái nhân viên employeeId={_employeeId}: {response.Message}");
                    CustomMessageBox.Show(
                        "Lỗi cập nhật trạng thái nhân viên!",
                        new List<string> { "OK" },
                        MessageType.Error
                    );
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError($"Lỗi khi lưu thông tin nhân viên employeeId={_employeeId}", ex);
                CustomMessageBox.Show(
                    "Lỗi khi lưu thông tin nhân viên!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
            }
            finally
            {
                IsLoading = false;
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        private void ExecuteClose()
        {
            AppLogger.Instance.LogInfo("Đóng EmployeeEditViewModel");
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
                AppLogger.Instance.LogInfo("Dispose EmployeeEditViewModel");
                if (_employeeApi is IDisposable disposableApi)
                {
                    disposableApi.Dispose();
                }
                RequestClose = null;
            }

            _disposed = true;
        }

        ~EmployeeEditViewModel()
        {
            Dispose(false);
        }
    }
}