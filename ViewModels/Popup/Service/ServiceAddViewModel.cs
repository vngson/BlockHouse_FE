using BlockHouse.Api;
using BlockHouse.Helpers;
using BlockHouse.Models;
using BlockHouse.ViewModels.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BlockHouse.ViewModels.Popup.Service
{
    public class ServiceAddViewModel : ObservableObject, IDisposable
    {
        private readonly ServiceApi _serviceApi;
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

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                SetProperty(ref _description, value);
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                SetProperty(ref _price, value);
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
        public event Action<ServiceResponse> ServiceAdded;

        public ServiceAddViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo ServiceAddViewModel");
            _serviceApi = new ServiceApi();
            SaveCommand = new AsyncRelayCommand(ExecuteSave, CanExecuteSave);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        private bool CanExecuteSave()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Name) && Price >= 0;
        }

        private async Task ExecuteSave()
        {
            AppLogger.Instance.LogInfo("Bắt đầu thêm dịch vụ mới");
            try
            {
                IsLoading = true;
                SaveCommand.NotifyCanExecuteChanged();

                var newService = new ServiceAddRequest
                {
                    Name = Name.Trim(),
                    Description = Description.Trim(),
                    Price = Price
                };

                var response = await _serviceApi.CreateService(newService);
                if (response.IsSuccess && response.Data != null)
                {
                    AppLogger.Instance.LogInfo("Thêm dịch vụ thành công");

                    CustomMessageBox.Show(
                        "Thêm dịch vụ thành công!",
                        new List<string> { "OK" },
                        MessageType.Information
                    );

                    ServiceAdded?.Invoke(response.Data);
                    RequestClose?.Invoke();
                }
                else
                {
                    AppLogger.Instance.LogError($"Lỗi khi thêm dịch vụ: {response.Message}");
                    CustomMessageBox.Show(
                        $"Lỗi thêm dịch vụ: {response.Message}",
                        new List<string> { "OK" },
                        MessageType.Error
                    );
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Lỗi khi thêm dịch vụ", ex);
                CustomMessageBox.Show(
                    "Lỗi khi thêm dịch vụ!",
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
            AppLogger.Instance.LogInfo("Đóng ServiceAddViewModel");
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
                AppLogger.Instance.LogInfo("Dispose ServiceAddViewModel");
                if (_serviceApi is IDisposable disposableApi)
                {
                    disposableApi.Dispose();
                }
                RequestClose = null;
                ServiceAdded = null;
            }

            _disposed = true;
        }

        ~ServiceAddViewModel()
        {
            Dispose(false);
        }
    }

    public class ServiceAddRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}