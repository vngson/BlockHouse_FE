using BlockHouse.Api;
using BlockHouse.Helpers;
using BlockHouse.Models;
using BlockHouse.ViewModels.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BlockHouse.ViewModels.Popup.Service
{
    public class ServiceEditViewModel : ObservableObject, IDisposable
    {
        private readonly ServiceApi _serviceApi;
        private bool _disposed;
        private int _serviceId;
        private ServiceResponse _originalServiceData;

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
        public event Action<ServiceResponse> ServiceUpdated;

        public ServiceEditViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo ServiceEditViewModel");
            _serviceApi = new ServiceApi();
            SaveCommand = new AsyncRelayCommand(ExecuteSave, CanExecuteSave);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        public async Task LoadDataAsync(int serviceId)
        {
            AppLogger.Instance.LogInfo($"Bắt đầu LoadDataAsync cho serviceId={serviceId}");
            _serviceId = serviceId;
            try
            {
                IsLoading = true;
                var response = await _serviceApi.GetServiceById(serviceId);
                if (response.IsSuccess && response.Data != null)
                {
                    AppLogger.Instance.LogInfo($"Tải chi tiết dịch vụ thành công cho serviceId={serviceId}");
                    _originalServiceData = response.Data;

                    Name = _originalServiceData.Name ?? "Không xác định";
                    Description = _originalServiceData.Description ?? "Không có mô tả";
                    Price = Convert.ToDecimal(_originalServiceData.Price ?? 0);
                }
                else
                {
                    AppLogger.Instance.LogWarning($"Không tìm thấy dữ liệu dịch vụ cho serviceId={serviceId}: {response.Message}");
                    SetDefaultValues();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError($"Lỗi khi tải chi tiết dịch vụ serviceId={serviceId}", ex);
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
            Description = "N/A";
            Price = 0;
        }

        private bool CanExecuteSave()
        {
            return !IsLoading && _originalServiceData != null &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   Price >= 0;
        }

        private async Task ExecuteSave()
        {
            AppLogger.Instance.LogInfo($"Bắt đầu lưu thông tin dịch vụ serviceId={_serviceId}");
            try
            {
                IsLoading = true;
                SaveCommand.NotifyCanExecuteChanged();

                var updateData = new ServiceUpdateRequest
                {
                    Name = Name,
                    Description = Description,
                    Price = Price
                };

                var response = await _serviceApi.UpdateService(_serviceId, updateData);
                if (response.IsSuccess && response.Data != null)
                {
                    AppLogger.Instance.LogInfo($"Cập nhật dịch vụ thành công serviceId={_serviceId}");

                    _originalServiceData = response.Data;

                    CustomMessageBox.Show(
                        "Cập nhật thông tin dịch vụ thành công!",
                        new List<string> { "OK" },
                        MessageType.Information
                    );

                    ServiceUpdated?.Invoke(_originalServiceData);
                    RequestClose?.Invoke();
                }
                else
                {
                    AppLogger.Instance.LogError($"Lỗi khi cập nhật dịch vụ serviceId={_serviceId}: {response.Message}");
                    CustomMessageBox.Show(
                        $"Lỗi cập nhật thông tin dịch vụ: {response.Message}",
                        new List<string> { "OK" },
                        MessageType.Error
                    );
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError($"Lỗi khi lưu thông tin dịch vụ serviceId={_serviceId}", ex);
                CustomMessageBox.Show(
                    "Lỗi khi lưu thông tin dịch vụ!",
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
            AppLogger.Instance.LogInfo("Đóng ServiceEditViewModel");
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
                AppLogger.Instance.LogInfo("Dispose ServiceEditViewModel");
                if (_serviceApi is IDisposable disposableApi)
                {
                    disposableApi.Dispose();
                }
                RequestClose = null;
                ServiceUpdated = null;
            }

            _disposed = true;
        }

        ~ServiceEditViewModel()
        {
            Dispose(false);
        }
    }

    public class ServiceUpdateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}