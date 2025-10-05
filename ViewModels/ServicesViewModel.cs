// [file name]: ServicesViewModel.cs
// [file content begin]
using BlockHouse.Api;
using BlockHouse.Helpers;
using BlockHouse.Models.Responses;
using BlockHouse.ViewModels.Components;
using BlockHouse.ViewModels.Popup.Service;
using BlockHouse.Views.Popup.Service;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using static BlockHouse.ViewModels.MainWindowViewModel;

namespace BlockHouse.ViewModels
{
    class ServicesViewModel : ViewModelBase
    {
        public class ServiceItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int Service_Id { get; set; }
            private decimal _price;
            public decimal Price
            {
                get => _price;
                set
                {
                    if (_price != value)
                    {
                        _price = value;
                        OnPropertyChanged(nameof(Price));
                        OnPropertyChanged(nameof(PriceDisplay));
                    }
                }
            }

            public string PriceDisplay => string.Format("{0:N0} VNĐ", Price);

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<ServiceItem> _serviceItems;

        public List<ServiceItem> ServiceItems
        {
            get => _serviceItems;
            set => SetProperty(ref _serviceItems, value);
        }

        public ICommand ViewCommand { get; }
        public IAsyncRelayCommand AddCommand { get; }
        public IAsyncRelayCommand EditCommand { get; }
        public ICommand GoBackCommand { get; }
        public AsyncRelayCommand SearchCommand { get; }  // Thêm SearchCommand

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private readonly ServiceApi _serviceApi;

        private bool _isFirstPageEnabled;
        public bool IsFirstPageEnabled
        {
            get => _isFirstPageEnabled;
            set => SetProperty(ref _isFirstPageEnabled, value);
        }

        private bool _isPrevPageEnabled;
        public bool IsPrevPageEnabled
        {
            get => _isPrevPageEnabled;
            set => SetProperty(ref _isPrevPageEnabled, value);
        }

        private bool _isNextPageEnabled;
        public bool IsNextPageEnabled
        {
            get => _isNextPageEnabled;
            set => SetProperty(ref _isNextPageEnabled, value);
        }

        private bool _isLastPageEnabled;
        public bool IsLastPageEnabled
        {
            get => _isLastPageEnabled;
            set => SetProperty(ref _isLastPageEnabled, value);
        }

        private int _currentPage;
        private int _totalPages;

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ResultText));
                    UpdatePagingButtons();
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (_totalPages != value)
                {
                    _totalPages = value > 0 ? value : 1;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ResultText));
                    UpdatePagingButtons();
                }
            }
        }

        public int PageSize = 9;

        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }

        private void UpdatePagingButtons()
        {
            IsFirstPageEnabled = CurrentPage > 1;
            IsPrevPageEnabled = CurrentPage > 1;
            IsNextPageEnabled = CurrentPage < TotalPages;
            IsLastPageEnabled = CurrentPage < TotalPages;
        }

        public string ResultText => $"Trang {CurrentPage}/{TotalPages}";

        // Commands
        public ICommand FirstPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }

        public ServicesViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo ServicesViewModel");
            _serviceApi = new ServiceApi();
            ServiceItems = new List<ServiceItem>();

            ViewCommand = new RelayCommand<OrderResponse>(ExecuteView);
            AddCommand = new AsyncRelayCommand(AddServiceAsync);
            EditCommand = new AsyncRelayCommand<ServiceItem>(ExecuteEdit);
            GoBackCommand = new RelayCommand(ExecuteGoBack);
            SearchCommand = new AsyncRelayCommand(ExecuteSearch);  // Khởi tạo SearchCommand

            _totalPages = 1;
            _currentPage = 1;

            FirstPageCommand = new AsyncRelayCommand(GoToFirstPage);
            PrevPageCommand = new AsyncRelayCommand(GoToPrevPage);
            NextPageCommand = new AsyncRelayCommand(GoToNextPage);
            LastPageCommand = new AsyncRelayCommand(GoToLastPage);
            _ = LoadData();
        }

        private async Task GoToFirstPage() => await LoadData(1, SearchKeyword);
        private async Task GoToPrevPage() => await LoadData(CurrentPage - 1, SearchKeyword);
        private async Task GoToNextPage() => await LoadData(CurrentPage + 1, SearchKeyword);
        private async Task GoToLastPage() => await LoadData(TotalPages, SearchKeyword);

        private void ExecuteGoBack()
        {
            AppLogger.Instance.LogInfo("Quay lại màn hình tổng quan từ ServicesViewModel");
            WeakReferenceMessenger.Default.Send(new ChangeViewMessage("Overview"));
        }

        private async Task ExecuteSearch()
        {
            AppLogger.Instance.LogInfo($"Thực hiện tìm kiếm dịch vụ: Keyword={SearchKeyword}");
            await LoadData(1, SearchKeyword, true);
        }

        private void ExecuteView(OrderResponse item)
        {
            AppLogger.Instance.LogInfo($"Xem chi tiết dịch vụ Id={item?.Id}");
            // Hàm xử lý lệnh Xem
        }

        private async Task ExecuteEdit(ServiceItem item)
        {
            if (item == null)
            {
                AppLogger.Instance.LogWarning("Tham số item null khi sửa dịch vụ");
                return;
            }

            AppLogger.Instance.LogInfo($"Sửa dịch vụ Id={item.Service_Id}");

            try
            {
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });

                var editVm = new ServiceEditViewModel();
                await editVm.LoadDataAsync(item.Service_Id);

                var dialog = new ServiceEditView
                {
                    DataContext = editVm,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                editVm.RequestClose += () =>
                {
                    dialog.Close();
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                };

                dialog.Closed += async (s, e) =>
                {
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                    // Refresh data after editing
                    await LoadData(CurrentPage, SearchKeyword);
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Error opening edit service dialog", ex);
                CustomMessageBox.Show(
                    "Lỗi mở form sửa dịch vụ!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
            }
        }

        private async Task AddServiceAsync()
        {
            try
            {
                AppLogger.Instance.LogInfo("Mở form thêm dịch vụ");
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });

                var addViewModel = new ServiceAddViewModel();

                var dialog = new ServiceAddView
                {
                    DataContext = addViewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                addViewModel.RequestClose += () =>
                {
                    dialog.Close();
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                };

                dialog.Closed += async (s, e) =>
                {
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                    // Refresh data after adding
                    await LoadData(CurrentPage, SearchKeyword);
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Error opening add service dialog", ex);
                CustomMessageBox.Show(
                    "Lỗi mở form thêm dịch vụ!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
            }
        }

        private async Task LoadData(int? page = 1, string? keyword = null, bool? isShowMessage = false)
        {
            AppLogger.Instance.LogInfo($"Bắt đầu tải dữ liệu dịch vụ: page={page}, keyword={keyword}");
            try
            {
                IsLoading = true;
                CurrentPage = (int)page;

                List<ServiceItem> items;
                var response = await _serviceApi.GetAllServices(page, PageSize, keyword);
                if (response.IsSuccess && response.Data != null)
                {
                    AppLogger.Instance.LogInfo($"Tải dữ liệu dịch vụ thành công. Số lượng: {response.Data.Services.Count}");
                    items = response.Data.Services.Select((o, index) => new ServiceItem
                    {
                        Id = index + 1,
                        Name = o.Name ?? "",
                        Description = o.Description ?? "",
                        Service_Id = o.Id,
                        Price = (decimal)(o.Price ?? 0)
                    }).ToList();

                    var totaServices = response.Data.TotalServices;
                    TotalPages = (int)Math.Ceiling((double)totaServices / PageSize);
                    OnPropertyChanged(nameof(ResultText));

                }
                else
                {
                    AppLogger.Instance.LogError($"Lỗi tải dữ liệu dịch vụ: {response.Message}");
                    CustomMessageBox.Show(
                        "Lỗi tải dữ liệu dịch vụ!",
                        new List<string> { "OK" },
                        MessageType.Error
                    );
                    items = new List<ServiceItem>();
                }

                ServiceItems = items;
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Exception khi tải dữ liệu dịch vụ", ex);
                CustomMessageBox.Show(
                    "Lỗi tải dữ liệu dịch vụ!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
                ServiceItems = new List<ServiceItem>();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}