using BlockHouse.Api;
using BlockHouse.Helpers;
using BlockHouse.Models;
using BlockHouse.Models.Responses;
using BlockHouse.ViewModels.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BlockHouse.ViewModels.Popup.RevenueByEmployee
{
    class RevenueByEmployeeAddViewModel : ObservableObject, IDisposable
    {
        private bool _disposed;
        private readonly RevenueApi _revenueApi;
        private readonly EmployeeApi _employeeApi;
        private readonly ServiceApi _serviceApi;

        public DateTime CurrentDateTime => DateTime.Now;

        public ObservableCollection<EmployeeResponse> Employees { get; } = new();
        private int? _selectedEmployeeId;
        public int? SelectedEmployeeId
        {
            get => _selectedEmployeeId;
            set => SetProperty(ref _selectedEmployeeId, value);
        }

        public ObservableCollection<ServiceResponse> Services { get; } = new();
        public ObservableCollection<OrderServiceItem> OrderServices { get; } = new();

        private decimal _total;
        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public string TotalDisplay => $"{Total:N0} VNĐ";

        public IAsyncRelayCommand SaveCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand AddServiceCommand { get; }
        public ICommand RemoveServiceCommand { get; }


        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }

        public event Action RequestClose;

        public RevenueByEmployeeAddViewModel()
        {
            _revenueApi = new RevenueApi();
            _employeeApi = new EmployeeApi();
            _serviceApi = new ServiceApi();

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CloseCommand = new RelayCommand(ExecuteClose);
            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new RelayCommand<OrderServiceItem>(RemoveService);

            IncreaseQuantityCommand = new RelayCommand<OrderServiceItem>(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand<OrderServiceItem>(DecreaseQuantity);

            OrderServices.CollectionChanged += (s, e) =>
            {
                CalculateTotal();

                // Đăng ký PropertyChanged cho các item mới được thêm
                if (e.NewItems != null)
                {
                    foreach (OrderServiceItem newItem in e.NewItems)
                    {
                        newItem.PropertyChanged += OrderServiceItem_PropertyChanged;
                    }
                }

                // Hủy đăng ký PropertyChanged cho các item bị xóa
                if (e.OldItems != null)
                {
                    foreach (OrderServiceItem oldItem in e.OldItems)
                    {
                        oldItem.PropertyChanged -= OrderServiceItem_PropertyChanged;
                    }
                }
            };
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentDateTime))
                    OnPropertyChanged(nameof(CurrentDateTime));
            };
        }

        private void IncreaseQuantity(OrderServiceItem item)
        {
            if (item != null)
            {
                item.Quantity++;
                CalculateTotal();
            }
        }

        private void DecreaseQuantity(OrderServiceItem item)
        {
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                CalculateTotal();
            }
        }

        private void OrderServiceItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderServiceItem.ServiceId) ||
                e.PropertyName == nameof(OrderServiceItem.Quantity))
            {
                CalculateTotal();
            }
        }

        public async Task LoadDataAsync()
        {
            try
            {
                // Load employees
                var empResponse = await _employeeApi.GetEmployees();
                if (empResponse.IsSuccess && empResponse.Data != null)
                {
                    Employees.Clear();
                    foreach (var emp in empResponse.Data.Employees)
                    {
                        if (emp.Status == 1) Employees.Add(emp);
                    }
                }

                // Load services
                var svcResponse = await _serviceApi.GetAllServicesNoPagination();
                if (svcResponse.IsSuccess && svcResponse.Data != null)
                {
                    Services.Clear();
                    foreach (var svc in svcResponse.Data.Services)
                    {
                        Services.Add(svc);
                    }
                }

                // Add initial empty service row
                AddService();
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Error loading data for add order", ex);
                CustomMessageBox.Show("Lỗi tải dữ liệu!", new List<string> { "OK" }, MessageType.Error);
            }
        }

        private void AddService()
        {
            // Check if there are any existing OrderServiceItems
            if (OrderServices.Any())
            {
                // Get the last added OrderServiceItem
                var lastItem = OrderServices.Last();

                // Validate that the last item has a valid ServiceId
                if (!lastItem.ServiceId.HasValue)
                {
                    CustomMessageBox.Show("Vui lòng chọn dịch vụ cho mục hiện tại trước khi thêm mới!",
                        new List<string> { "OK" }, MessageType.Warning);
                    return;
                }
            }

            // If validation passes, add a new OrderServiceItem
            var newItem = new OrderServiceItem { Quantity = 1 };
            newItem.PropertyChanged += (s, e) => CalculateTotal();
            OrderServices.Add(newItem);
        }

        private void RemoveService(OrderServiceItem item)
        {
            if (item != null)
            {
                OrderServices.Remove(item);
            }
        }

        private void CalculateTotal()
        {
            decimal subtotal = 0;
            foreach (var item in OrderServices)
            {
                if (item.ServiceId.HasValue)
                {
                    var service = Services.FirstOrDefault(s => s.Id == item.ServiceId);
                    if (service != null)
                    {
                        subtotal += (decimal)service.Price * item.Quantity;
                    }
                }
            }

            Total = subtotal;

            // Đảm bảo TotalDisplay được cập nhật
            OnPropertyChanged(nameof(TotalDisplay));
        }

        private async Task SaveAsync()
        {
            try
            {
                // Validate dữ liệu
                if (!SelectedEmployeeId.HasValue)
                {
                    CustomMessageBox.Show("Vui lòng chọn nhân viên!", new List<string> { "OK" }, MessageType.Warning);
                    return;
                }

                var servicesData = OrderServices
                    .Where(os => os.ServiceId.HasValue && os.Quantity > 0)
                    .Select(os => new { service_id = os.ServiceId.Value, quantity = os.Quantity })
                    .ToList();

                if (servicesData.Count == 0)
                {
                    CustomMessageBox.Show("Vui lòng thêm ít nhất một dịch vụ hợp lệ!", new List<string> { "OK" }, MessageType.Warning);
                    return;
                }

                // Tính toán tổng tiền chính xác từ dịch vụ
                decimal calculatedTotal = 0;
                foreach (var item in OrderServices)
                {
                    if (item.ServiceId.HasValue)
                    {
                        var service = Services.FirstOrDefault(s => s.Id == item.ServiceId);
                        if (service != null)
                        {
                            calculatedTotal += (decimal)service.Price * item.Quantity;
                        }
                    }
                }

                if (calculatedTotal <= 0)
                {
                    CustomMessageBox.Show("Tổng tiền phải lớn hơn 0!", new List<string> { "OK" }, MessageType.Warning);
                    return;
                }

                // Chuẩn bị dữ liệu gửi lên server
                var data = new
                {
                    datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    employee_id = SelectedEmployeeId.Value, // Sử dụng .Value vì đã validate
                    total = calculatedTotal,
                    services = servicesData
                };

                var response = await _revenueApi.CreateOrder(data);
                if (response.IsSuccess && response.Data != null)
                {
                    CustomMessageBox.Show("Thêm đơn hàng thành công!", new List<string> { "OK" }, MessageType.Information);
                    ExecuteClose();
                }
                else
                {
                    CustomMessageBox.Show($"Lỗi: {response.Message}", new List<string> { "OK" }, MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Error saving order", ex);
                CustomMessageBox.Show("Lỗi lưu đơn hàng!", new List<string> { "OK" }, MessageType.Error);
            }
        }

        private void ExecuteClose()
        {
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
            if (_disposed) return;

            if (disposing)
            {
                OrderServices.Clear();
                Employees.Clear();
                Services.Clear();
                RequestClose = null;
            }

            _disposed = true;
        }

        ~RevenueByEmployeeAddViewModel()
        {
            Dispose(false);
        }
    }

    public class OrderServiceItem : ObservableObject
    {
        private int? _serviceId;
        public int? ServiceId
        {
            get => _serviceId;
            set
            {
                SetProperty(ref _serviceId, value);
                OnPropertyChanged(nameof(ServiceId));
            }
        }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                SetProperty(ref _quantity, value < 1 ? 1 : value);
                OnPropertyChanged(nameof(Quantity));
            }
        }
    }
}