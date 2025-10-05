using BlockHouse.Api;
using BlockHouse.Helpers;
using BlockHouse.Models.Responses;
using BlockHouse.ViewModels.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BlockHouse.ViewModels.Popup.Employee
{
    public class EmployeeAddViewModel : ObservableObject, IDisposable
    {
        private readonly EmployeeApi _employeeApi;
        private bool _disposed;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set
            {
                SetProperty(ref _phone, value);
                SaveCommand.NotifyCanExecuteChanged();
            }
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
        public event Action<EmployeeResponse> EmployeeAdded;

        public EmployeeAddViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo EmployeeAddViewModel");
            _employeeApi = new EmployeeApi();
            SaveCommand = new AsyncRelayCommand(ExecuteSave, CanExecuteSave);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        private bool CanExecuteSave()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Phone);
        }

        private async Task ExecuteSave()
        {
            AppLogger.Instance.LogInfo("Bắt đầu thêm nhân viên mới");
            try
            {
                IsLoading = true;
                SaveCommand.NotifyCanExecuteChanged();

                // Tạo object thêm mới chỉ với name và phone, các trường khác mặc định
                var newEmployee = new EmployeeAddRequest
                {
                    Name = Name.Trim(),
                    Phone = Phone.Trim()
                };

                // Gọi API để thêm nhân viên
                var response = await _employeeApi.AddEmployee(newEmployee);
                if (response.IsSuccess && response.Data != null)
                {
                    AppLogger.Instance.LogInfo("Thêm nhân viên thành công");

                    CustomMessageBox.Show(
                        "Thêm nhân viên thành công!",
                        new List<string> { "OK" },
                        MessageType.Information
                    );

                    // Gửi sự kiện thêm thành công
                    EmployeeAdded?.Invoke(response.Data);
                    RequestClose?.Invoke();
                }
                else
                {
                    AppLogger.Instance.LogError($"Lỗi khi thêm nhân viên: {response.Message}");
                    CustomMessageBox.Show(
                        $"Lỗi thêm nhân viên: {response.Message}",
                        new List<string> { "OK" },
                        MessageType.Error
                    );
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Lỗi khi thêm nhân viên", ex);
                CustomMessageBox.Show(
                    "Lỗi khi thêm nhân viên!",
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
            AppLogger.Instance.LogInfo("Đóng EmployeeAddViewModel");
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
                AppLogger.Instance.LogInfo("Dispose EmployeeAddViewModel");
                if (_employeeApi is IDisposable disposableApi)
                {
                    disposableApi.Dispose();
                }
                RequestClose = null;
                EmployeeAdded = null;
            }

            _disposed = true;
        }

        ~EmployeeAddViewModel()
        {
            Dispose(false);
        }
    }

    // Model cho request thêm mới
    public class EmployeeAddRequest
    {
        public string Name { get; set; }
        public string Phone { get; set; }
    }
}