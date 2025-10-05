using BlockHouse.Api;
using BlockHouse.Helpers; // Thêm using cho AppLogger
using BlockHouse.ViewModels.Components;
using BlockHouse.ViewModels.Popup;
using BlockHouse.ViewModels.Popup.RevenueByEmployee;
using BlockHouse.ViewModels.Popup.RevenueByEmployeeAndDate;
using BlockHouse.Views.Popup.RevenueByDate;
using BlockHouse.Views.Popup.RevenueByEmployee;
using BlockHouse.Views.Popup.RevenueByEmployeeAndDate;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static BlockHouse.ViewModels.MainWindowViewModel;

namespace BlockHouse.ViewModels
{
    public class RevenueViewModel : ViewModelBase
    {

        public class ServiceSummary
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
        }

        public class OrderItem
        {
            public string Datetime { get; set; }
            public string EmployeeName { get; set; }
            public int Id { get; set; }
            public int OrderId { get; set; }
            public int? PromotionId { get; set; }
            public List<ServiceSummary> Services { get; set; }


            private decimal _total;

            public decimal Total
            {
                get => _total;
                set
                {
                    if (_total != value)
                    {
                        _total = value;
                        OnPropertyChanged(nameof(Total));
                        OnPropertyChanged(nameof(TotalDisplay));
                    }
                }
            }

            public string TotalDisplay => string.Format("{0:N0} VNĐ", Total);

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public string ServicesDisplay
            {
                get
                {
                    if (Services == null || Services.Count == 0)
                        return string.Empty;

                    return string.Join(", ", Services.Select(s => $"{s.Name} ({s.Quantity})"));
                }
            }
        }
        private List<OrderItem> _revenueItems;

        public List<OrderItem> RevenueItems
        {
            get => _revenueItems;
            set => SetProperty(ref _revenueItems, value);
        }


        public ICommand GoBackCommand { get; }
        public IAsyncRelayCommand ViewCommand { get; }
        public IAsyncRelayCommand EditCommand { get; }

        public ICommand EmployeeCommand { get; }
        public ICommand DateCommand { get; }

        public ICommand EmployeeAndDateCommand { get; }
        public ICommand DeleteCommand { get; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private readonly RevenueApi _revenueApi;

        private Brush _employeeBtnBackground;

        public Brush EmployeeBtnBackground
        {
            get => _employeeBtnBackground;
            set
            {
                _employeeBtnBackground = value;
                OnPropertyChanged(nameof(EmployeeBtnBackground));
            }
        }

        private Brush _dateBtnBackground;

        public Brush DateBtnBackground
        {
            get => _dateBtnBackground;
            set
            {
                _dateBtnBackground = value;
                OnPropertyChanged(nameof(DateBtnBackground));
            }
        }

        private Brush _empADateBackground;

        public Brush EmpADateBackground
        {
            get => _empADateBackground;
            set
            {
                _empADateBackground = value;
                OnPropertyChanged(nameof(EmpADateBackground));
            }
        }

        public static readonly SolidColorBrush ActiveBackground
        = new SolidColorBrush(Colors.Black);

        public static readonly SolidColorBrush InactiveBackground
            = new SolidColorBrush(Color.FromArgb(204, 20, 20, 20));

        private bool _byOrderGridVisibility;
        public bool ByOrderGridVisibility
        {
            get => _byOrderGridVisibility;
            set
            {
                if (SetProperty(ref _byOrderGridVisibility, value))
                {
                    // Khi bool thay đổi thì cập nhật Visibility
                    AddBtnVisibility = value ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }

        private bool _byEmployeeGridVisibility;
        public bool ByEmployeeGridVisibility
        {
            get => _byEmployeeGridVisibility;
            set => SetProperty(ref _byEmployeeGridVisibility, value);
        }

        private Visibility _addBtnVisibility = Visibility.Visible;
        public Visibility AddBtnVisibility
        {
            get => _addBtnVisibility;
            set => SetProperty(ref _addBtnVisibility, value);
        }

        private bool _byDateGridVisibility;
        public bool ByDateGridVisibility
        {
            get => _byDateGridVisibility;
            set => SetProperty(ref _byDateGridVisibility, value);
        }



        private GridLength _employeeColumnWidth = new GridLength(1, GridUnitType.Star);
        public GridLength EmployeeColumnWidth
        {
            get => _employeeColumnWidth;
            set
            {
                _employeeColumnWidth = value;
                OnPropertyChanged(); // INotifyPropertyChanged
            }
        }

        private double _serviceColumnMaxWidth;
        public double ServiceColumnMaxWidth
        {
            get => _serviceColumnMaxWidth;
            set => SetProperty(ref _serviceColumnMaxWidth, value);
        }

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

        private void UpdatePagingButtons()
        {
            IsFirstPageEnabled = CurrentPage > 1;
            IsPrevPageEnabled = CurrentPage > 1;
            IsNextPageEnabled = CurrentPage < TotalPages;
            IsLastPageEnabled = CurrentPage < TotalPages;
        }


        public string ResultText => $"Trang {CurrentPage}/{TotalPages}";

        // Commands
        public IAsyncRelayCommand FirstPageCommand { get; }
        public IAsyncRelayCommand PrevPageCommand { get; }
        public IAsyncRelayCommand NextPageCommand { get; }
        public IAsyncRelayCommand LastPageCommand { get; }

        // Add these properties for date binding
        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }

        // Add this command for DatePicker loaded event
        public ICommand DatePickerLoadedCommand { get; }
        public AsyncRelayCommand SearchCommand { get; }
        public IAsyncRelayCommand AddCommand { get; }

        public RevenueViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo RevenueViewModel");
            _revenueApi = new RevenueApi();
            // Tạo dữ liệu giả
            RevenueItems = new List<OrderItem>();

            // Khởi tạo các lệnh
            GoBackCommand = new RelayCommand(ExecuteGoBack);
            ViewCommand = new AsyncRelayCommand<OrderItem>(ExecuteView);
            EditCommand = new AsyncRelayCommand<OrderItem>(ExecuteEdit);
            DeleteCommand = new AsyncRelayCommand<OrderItem>(ExecuteDelete);
            SearchCommand = new AsyncRelayCommand(ExecuteSearch);
            AddCommand = new AsyncRelayCommand(AddOrderAsync);
            EmployeeCommand = new RelayCommand(
    async () => await ExecuteGetLayoutByEmployee()
);
            _totalPages = 1;
            _currentPage = 1;

            FirstPageCommand = new AsyncRelayCommand(GoToFirstPage);
            PrevPageCommand = new AsyncRelayCommand(GoToPrevPage);
            NextPageCommand = new AsyncRelayCommand(GoToNextPage);
            LastPageCommand = new AsyncRelayCommand(GoToLastPage);

            DateCommand = new RelayCommand(
                async () => await ExecuteGetLayoutByDate()
            );

            EmployeeAndDateCommand = new RelayCommand(
                async () => await ExecuteGetLayoutByEmployeeAndDate()
            );

            EmployeeBtnBackground = ActiveBackground;
            DateBtnBackground = InactiveBackground;
            EmpADateBackground = InactiveBackground;
            ByOrderGridVisibility = true;
            ByDateGridVisibility = false;

            ServiceColumnMaxWidth = 200;
            EmployeeColumnWidth = new GridLength(1, GridUnitType.Star);

            DatePickerLoadedCommand = new RelayCommand<DatePicker>(DatePicker_Loaded);
            SearchCommand = new AsyncRelayCommand(ExecuteSearch);

            // Initialize dates
            //StartDate = new DateTime(DateTime.Today.AddMonths(-1).Year,
            //                      DateTime.Today.AddMonths(-1).Month,
            //                      1);
            //EndDate = DateTime.Today;

            StartDate = null;
            EndDate = null;
            _ = LoadData(1, StartDate, EndDate, SearchKeyword, true);
        }

        private async Task AddOrderAsync()
        {
            try
            {
                var viewModel = new RevenueByEmployeeAddViewModel();

                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });

                var dialog = new RevenueAddView
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                viewModel.RequestClose += () =>
                {
                    dialog.Close();

                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                };

                dialog.ShowDialog();

                // Refresh data after adding
                await LoadData(CurrentPage, StartDate, EndDate, SearchKeyword, true);
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Error opening add order dialog", ex);
                CustomMessageBox.Show(
                    "Lỗi mở form thêm đơn hàng!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
            }
        }
        private async Task ExecuteGetLayoutByEmployee()
        {
            AppLogger.Instance.LogInfo("Chuyển sang layout doanh thu theo đơn hàng");
            EmployeeBtnBackground = ActiveBackground;
            DateBtnBackground = InactiveBackground;
            EmpADateBackground = InactiveBackground;
            ByOrderGridVisibility = true;
            ByDateGridVisibility = false;
            ByEmployeeGridVisibility = false;

            await LoadData(1, StartDate, EndDate, SearchKeyword, true);
        }

        private async Task ExecuteGetLayoutByDate()
        {
            AppLogger.Instance.LogInfo("Chuyển sang layout doanh thu theo ngày");
            EmployeeBtnBackground = InactiveBackground;
            DateBtnBackground = ActiveBackground;
            EmpADateBackground = InactiveBackground;
            ByOrderGridVisibility = false;
            ByEmployeeGridVisibility = false;
            ByDateGridVisibility = true;

            await LoadData(1, StartDate, EndDate, SearchKeyword, true);
        }

        private async Task ExecuteGetLayoutByEmployeeAndDate()
        {
            AppLogger.Instance.LogInfo("Chuyển sang layout doanh thu theo nhân viên");
            EmployeeBtnBackground = InactiveBackground;
            DateBtnBackground = InactiveBackground;
            EmpADateBackground = ActiveBackground;
            ByOrderGridVisibility = false;
            ByDateGridVisibility = false;
            ByEmployeeGridVisibility = true;

            await LoadData(1, StartDate, EndDate, SearchKeyword, true);
        }
        private void ExecuteGoBack()
        {
            WeakReferenceMessenger.Default.Send(new ChangeViewMessage("Overview"));
        }
        private void DatePicker_Loaded(DatePicker dp)
        {
            if (dp.Template.FindName("PART_TextBox", dp) is DatePickerTextBox textBox)
            {
                var binding = new Binding("SelectedDate")
                {
                    Source = dp,
                    StringFormat = "yyyy-MM-dd",
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                textBox.SetBinding(TextBox.TextProperty, binding);

                textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                textBox.VerticalContentAlignment = VerticalAlignment.Center;
                textBox.FontSize = 16;

                textBox.GotKeyboardFocus -= DatePickerTextBox_GotKeyboardFocus;
                textBox.GotKeyboardFocus += DatePickerTextBox_GotKeyboardFocus;

                textBox.FocusVisualStyle = null;
                textBox.BorderThickness = new Thickness(0);
                textBox.BorderBrush = Brushes.Transparent;
                textBox.Background = Brushes.Transparent;
                textBox.CaretBrush = Brushes.Black;
                textBox.TextDecorations = null;

                textBox.PreviewMouseDown -= DatePickerTextBox_PreviewMouseDown;
                textBox.PreviewMouseDown += DatePickerTextBox_PreviewMouseDown;
            }

            if (dp.Template.FindName("PART_Button", dp) is Button calendarButton)
            {
                calendarButton.Background = Brushes.Transparent;
                calendarButton.BorderBrush = Brushes.Transparent;
                calendarButton.BorderThickness = new Thickness(0);
                calendarButton.Width = 0;
                calendarButton.Height = 0;
                calendarButton.Content = null;
            }

            dp.CalendarOpened += DatePicker_CalendarOpened;
        }

        private void DatePicker_CalendarOpened(object sender, EventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
                // Đợi để calendar render hoàn toàn
                datePicker.Dispatcher.BeginInvoke(new Action(() =>
                {
                    FindAndColorCalendarHeader(datePicker);
                }), DispatcherPriority.Loaded);
            }
        }
        private void FindAndColorCalendarHeader(DatePicker datePicker)
        {
            try
            {
                if (datePicker.Template.FindName("PART_Popup", datePicker) is System.Windows.Controls.Primitives.Popup popup &&
                    popup.Child is System.Windows.Controls.Calendar calendar)
                {
                    // Tìm CalendarItem
                    var calendarItem = FindVisualChild<CalendarItem>(calendar);
                    if (calendarItem != null)
                    {
                        // Cách 1: Tìm bằng template part names
                        ProcessCalendarItemWithExactSearch(calendarItem);

                        // Cách 2: Fallback nếu cách 1 không hoạt động
                        if (!IsHeaderColored(calendarItem))
                        {
                            ProcessCalendarItemHeaderOnly(calendarItem);
                            // Đổi màu select và hover của các ngày
                            ChangeDayButtonStyles(calendarItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing calendar styles: {ex.Message}");
            }
        }

        private void ChangeDayButtonStyles(CalendarItem calendarItem)
        {
            // Tìm tất cả CalendarDayButton (các ô ngày)
            var dayButtons = FindVisualChildren<CalendarDayButton>(calendarItem);

            foreach (var dayButton in dayButtons)
            {
                // Xử lý sự kiện loaded của mỗi button
                dayButton.Loaded -= DayButton_Loaded;
                dayButton.Loaded += DayButton_Loaded;

                // Áp dụng style ngay lập tức nếu button đã loaded
                if (dayButton.IsLoaded)
                {
                    ApplyDayButtonStyle(dayButton);
                }
            }
        }

        private void DayButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CalendarDayButton dayButton)
            {
                ApplyDayButtonStyle(dayButton);
            }
        }

        private void ApplyDayButtonStyle(CalendarDayButton dayButton)
        {
            // Đổi màu cho các trạng thái bằng cách sử dụng triggers thông qua Style
            var style = new Style(typeof(CalendarDayButton));

            // Background mặc định
            style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.Black));

            // Trigger cho selected state
            var selectedTrigger = new Trigger()
            {
                Property = CalendarDayButton.IsSelectedProperty,
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.DarkBlue));
            selectedTrigger.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
            style.Triggers.Add(selectedTrigger);

            // Trigger cho today state
            var todayTrigger = new Trigger()
            {
                Property = CalendarDayButton.IsTodayProperty,
                Value = true
            };
            todayTrigger.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.LightBlue));
            todayTrigger.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.Black));
            style.Triggers.Add(todayTrigger);

            // Trigger cho disabled state
            var disabledTrigger = new Trigger()
            {
                Property = UIElement.IsEnabledProperty,
                Value = false
            };
            disabledTrigger.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.LightGray));
            disabledTrigger.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.DarkGray));
            style.Triggers.Add(disabledTrigger);

            // Áp dụng style
            dayButton.Style = style;

            // Xử lý hover (không thể dùng trigger trong code-behind nên dùng event)
            dayButton.MouseEnter -= DayButton_MouseEnter;
            dayButton.MouseEnter += DayButton_MouseEnter;

            dayButton.MouseLeave -= DayButton_MouseLeave;
            dayButton.MouseLeave += DayButton_MouseLeave;
        }

        private void DayButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is CalendarDayButton dayButton && dayButton.IsEnabled)
            {
                // Lưu màu gốc trước khi đổi
                if (dayButton.Tag == null)
                {
                    dayButton.Tag = dayButton.Background;
                }

                // Màu khi hover
                dayButton.Background = Brushes.LightGray;
            }
        }

        private void DayButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is CalendarDayButton dayButton)
            {
                // Khôi phục màu gốc
                if (dayButton.Tag is Brush originalBrush)
                {
                    dayButton.Background = originalBrush;
                }
                else
                {
                    // Fallback: khôi phục dựa trên trạng thái
                    if (dayButton.IsSelected)
                    {
                        dayButton.Background = Brushes.DarkBlue;
                    }
                    else if (dayButton.IsToday)
                    {
                        dayButton.Background = Brushes.LightBlue;
                    }
                    else
                    {
                        dayButton.Background = Brushes.Transparent;
                    }
                }
            }
        }

        private bool IsHeaderColored(CalendarItem calendarItem)
        {
            // Kiểm tra xem header đã được đổi màu chưa
            var headerButton = FindVisualChild<Button>(calendarItem);
            return headerButton != null && headerButton.Foreground == Brushes.White;
        }

        private void ProcessCalendarItemWithExactSearch(CalendarItem calendarItem)
        {
            // Danh sách các tên template part có thể có của header
            var headerPartNames = new[]
            {
        "PART_HeaderBackground",
        "HeaderBackground",
        "PART_Header",
        "CalendarHeader",
        "HeaderBorder"
    };

            foreach (var partName in headerPartNames)
            {
                var headerElement = calendarItem.Template.FindName(partName, calendarItem) as Border;
                if (headerElement != null)
                {
                    headerElement.Background = Brushes.Black;

                    // Tìm và đổi màu chữ trong header
                    ChangeTextColorInElement(headerElement, Brushes.White);
                    break;
                }
            }

            // Tìm header button
            var headerButton = calendarItem.Template.FindName("PART_HeaderButton", calendarItem) as Button;
            if (headerButton != null)
            {
                headerButton.Foreground = Brushes.White;
                headerButton.Background = Brushes.Transparent;
            }
        }

        private void ProcessCalendarItemHeaderOnly(CalendarItem calendarItem)
        {
            // Tìm tất cả border và xác định cái nào là header
            var borders = FindVisualChildren<Border>(calendarItem);

            foreach (var border in borders)
            {
                // Header thường có đặc điểm: chiều cao 25-50px, nằm ở trên cùng
                var position = border.TransformToVisual(calendarItem).Transform(new Point(0, 0));

                if (position.Y < 10 && border.ActualHeight >= 25 && border.ActualHeight <= 50)
                {
                    border.Background = Brushes.Black;
                    ChangeTextColorInElement(border, Brushes.White);
                    break;
                }
            }
        }

        private void ChangeTextColorInElement(DependencyObject parent, Brush color)
        {
            var children = FindAllVisualChildren(parent);
            foreach (var child in children)
            {
                if (child is Button button)
                {
                    button.Foreground = color;
                    button.Background = Brushes.Transparent;
                }
                else if (child is TextBlock textBlock)
                {
                    textBlock.Foreground = color;
                }
                else if (child is Label label)
                {
                    label.Foreground = color;
                }
                else if (child is AccessText accessText)
                {
                    accessText.Foreground = color;
                }
            }
        }

        // Hàm tìm tất cả visual children
        private List<DependencyObject> FindAllVisualChildren(DependencyObject parent)
        {
            var children = new List<DependencyObject>();
            if (parent == null) return children;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                children.Add(child);
                children.AddRange(FindAllVisualChildren(child));
            }
            return children;
        }

        // Hàm tìm tất cả children của một type cụ thể
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void DatePickerTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Select(textBox.Text.Length, 0);
                e.Handled = true;
            }
        }

        private void DatePickerTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DatePickerTextBox textBox)
            {
                var datePicker = FindVisualParent<DatePicker>(textBox);
                if (datePicker != null)
                {
                    datePicker.IsDropDownOpen = true;
                    e.Handled = true;
                }
            }
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            return parentObject is T parent ? parent : FindVisualParent<T>(parentObject);
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }

        private async Task ExecuteSearch()
        {
            AppLogger.Instance.LogInfo($"Thực hiện tìm kiếm doanh thu: StartDate={StartDate}, EndDate={EndDate}, Keyword={SearchKeyword}");
            if (StartDate > EndDate)
            {
                AppLogger.Instance.LogWarning("Ngày bắt đầu lớn hơn ngày kết thúc khi tìm kiếm doanh thu");
                CustomMessageBox.Show(
                    "Ngày bắt đầu phải nhỏ hơn ngày kết thúc!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
                return;
            }

            await LoadData(1, StartDate, EndDate, SearchKeyword, true);
        }


        private async Task GoToFirstPage() => await LoadData(1, StartDate, EndDate, SearchKeyword);
        private async Task GoToPrevPage() => await LoadData(CurrentPage - 1, StartDate, EndDate, SearchKeyword);
        private async Task GoToNextPage() => await LoadData(CurrentPage + 1, StartDate, EndDate, SearchKeyword);
        private async Task GoToLastPage() => await LoadData(TotalPages, StartDate, EndDate, SearchKeyword);

        private async Task ExecuteView(OrderItem item)
        {
            if (item == null)
            {
                AppLogger.Instance.LogWarning("Tham số item null khi xem chi tiết doanh thu");
                return;
            }

            AppLogger.Instance.LogInfo($"Xem chi tiết doanh thu cho OrderId={item.OrderId}, Date={item.Datetime}");

            // Show overlay
            WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });

            if (ByOrderGridVisibility)
            {
                if (item.OrderId == 0) return;
                // Existing code for employee view
                var detailVm = new RevenueByEmployeeDetailViewModel();
                await detailVm.LoadDataAsync(item.OrderId);

                var dialog = new RevenueByEmployeeDetailView
                {
                    DataContext = detailVm,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                dialog.Closed += (s, e) =>
                {
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                };

                dialog.ShowDialog();
            }
            else if (ByDateGridVisibility)
            {
                // New code for date view
                var detailVm = new RevenueByDateDetailViewModel();
                var date = DateTime.ParseExact(item.Datetime, "dd/MM/yyyy",
                                    new System.Globalization.CultureInfo("vi-VN"));
                string order_date = date.ToString("yyyy-MM-dd");
                await detailVm.LoadDataAsync(date.ToString("yyyy-MM-dd"));

                var dialog = new RevenueByDateDetailView
                {
                    DataContext = detailVm,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                dialog.Closed += (s, e) =>
                {
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                };


                dialog.ShowDialog();
            }
            else if (ByEmployeeGridVisibility)
            {
                // New code for date view
                var detailVm = new RevenueByEmployeeAndDateDetailViewModel();
                var date = DateTime.ParseExact(item.Datetime, "dd/MM/yyyy",
                                    new System.Globalization.CultureInfo("vi-VN"));
                string order_date = date.ToString("yyyy-MM-dd");
                await detailVm.LoadDataAsync(item.OrderId, date.ToString("yyyy-MM-dd"));

                var dialog = new RevenueByEmployeeAndDateDetailView
                {
                    DataContext = detailVm,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                dialog.Closed += (s, e) =>
                {
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                };


                dialog.ShowDialog();
            }
        }

        private async Task ExecuteEdit(OrderItem item)
        {
            if (item == null || item.OrderId == 0)
            {
                AppLogger.Instance.LogWarning("Tham số item null hoặc OrderId=0 khi sửa doanh thu");
                return;
            }

            AppLogger.Instance.LogInfo($"Sửa đơn hàng OrderId={item.OrderId}");

            try
            {
                // Show overlay
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });

                // Tạo view model cho popup update
                var updateViewModel = new RevenueByEmployeeUpdateViewModel();
                updateViewModel.SetOrderId(item.OrderId);

                var dialog = new RevenueUpdateView(item.OrderId)
                {
                    DataContext = updateViewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // Handle window closed event to hide overlay
                updateViewModel.RequestClose += () =>
                {
                    dialog.Close();
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                };

                dialog.ShowDialog();

                // Refresh data after editing
                await LoadData(CurrentPage, StartDate, EndDate, SearchKeyword);
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Error opening edit order dialog", ex);
                CustomMessageBox.Show(
                    "Lỗi mở form sửa đơn hàng!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
            }
        }

        private async Task ExecuteDelete(OrderItem item)
        {
            if (item == null || item.OrderId == 0)
            {
                AppLogger.Instance.LogWarning("Tham số item null hoặc OrderId=0 khi xóa doanh thu");
                return;
            }

            AppLogger.Instance.LogInfo($"Xóa doanh thu OrderId={item.OrderId}");

            // Show overlay
            WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });

            var detailVm = new RevenueByEmployeeDeleteViewModel();
            await detailVm.LoadDataAsync(item.OrderId);

            var dialog = new RevenueByEmployeeDeleteView
            {
                DataContext = detailVm,
                Owner = Application.Current.MainWindow
            };

            // Handle window closed event to hide overlay
            dialog.Closed += async (s, e) =>
            {
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                await LoadData(CurrentPage, StartDate, EndDate, SearchKeyword);
            };

            dialog.ShowDialog();
        }

        private async Task LoadData(int? page = 1,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? keyword = null,
            bool? isShowMessage = false)
        {
            try
            {
                AppLogger.Instance.LogInfo($"Bắt đầu tải dữ liệu doanh thu: page={page}, startDate={startDate}, endDate={endDate}, keyword={keyword}");
                IsLoading = true;

                CurrentPage = (int)page;

                List<OrderItem> items;
                if (EmployeeBtnBackground == ActiveBackground)
                {
                    var response = await _revenueApi.GetOrdersByEmployee(page, 9, startDate, endDate, keyword);
                    if (response.IsSuccess && response.Data != null)
                    {
                        AppLogger.Instance.LogInfo($"Tải dữ liệu doanh thu theo nhân viên thành công. Số lượng: {response.Data.Orders.Count}");
                        items = response.Data.Orders.Select((o, index) => new OrderItem
                        {
                            Id = PageSize * (CurrentPage - 1) + index + 1,
                            OrderId = o.Id,
                            Datetime = o.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                            EmployeeName = o.EmployeeName ?? "",
                            PromotionId = o.PromotionId,
                            Services = o.Services.Select(s => new ServiceSummary
                            {
                                Name = s.Name,
                                Quantity = s.Quantity
                            }).ToList(),
                            Total = (decimal)(o.Total ?? 0)
                        }).ToList();

                        var totaOrders = response.Data.Total_Orders;
                        TotalPages = (int)Math.Ceiling((double)totaOrders / PageSize);
                        OnPropertyChanged(ResultText);
                        if (isShowMessage == true) AppLogger.Instance.LogInfo(
                            "Tải dữ liệu doanh thu thành công!");
                    }
                    else
                    {
                        AppLogger.Instance.LogError($"Lỗi tải dữ liệu doanh thu theo nhân viên: {response.Message}");
                        CustomMessageBox.Show(
                            "Lỗi tải dữ liệu doanh thu theo nhân viên!",
                            new List<string> { "OK" },
                            MessageType.Error
                            );
                        items = new List<OrderItem>();
                    }
                }
                else if (DateBtnBackground == ActiveBackground)
                {
                    var response = await _revenueApi.GetOrdersByDate(page, 9, startDate, endDate, keyword);
                    if (response.IsSuccess && response.Data != null)
                    {
                        AppLogger.Instance.LogInfo($"Tải dữ liệu doanh thu theo ngày thành công. Số lượng: {response.Data.OrdersByDate.Count}");
                        items = response.Data.OrdersByDate.Select((o, index) => new OrderItem
                        {
                            Id = index + 1,
                            Datetime = o.Date.ToString("dd/MM/yyyy"),
                            OrderId = o.Id,
                            EmployeeName = "", // No employee name in OrdersByDateResponse
                            PromotionId = o.PromotionId,
                            Services = o.Services.Select(s => new ServiceSummary
                            {
                                Name = s.Name,
                                Quantity = s.Quantity
                            }).ToList(),
                            Total = o.Total
                        }).ToList();

                        var totaOrders = response.Data.TotalDays;
                        TotalPages = (int)Math.Ceiling((double)totaOrders / PageSize);
                        OnPropertyChanged(ResultText);
                        if (isShowMessage == true) AppLogger.Instance.LogInfo(
                            "Tải dữ liệu doanh thu thành công!");
                    }
                    else
                    {
                        AppLogger.Instance.LogError($"Lỗi tải dữ liệu doanh thu theo ngày: {response.Message}");
                        CustomMessageBox.Show(
                            "Lỗi tải dữ liệu doanh thu theo ngày!",
                            new List<string> { "OK" },
                            MessageType.Error
                            );
                        items = new List<OrderItem>();
                    }
                }
                else
                {
                    var response = await _revenueApi.GetOrdersByEmployeeAndDate(page, 9, startDate, endDate, keyword);
                    if (response.IsSuccess && response.Data != null)
                    {
                        AppLogger.Instance.LogInfo($"Tải dữ liệu doanh thu theo nhân viên thành công. Số lượng: {response.Data.OrdersByEmployeeAndDate.Count}");
                        items = response.Data.OrdersByEmployeeAndDate.Select((o, index) => new OrderItem
                        {
                            Id = index + 1,
                            Datetime = o.Date.ToString("dd/MM/yyyy"),
                            OrderId = o.EmployeeId,
                            EmployeeName = o.EmployeeName,
                            PromotionId = 0,
                            Services = o.Services.Select(s => new ServiceSummary
                            {
                                Name = s.Name,
                                Quantity = s.Quantity
                            }).ToList(),
                            Total = o.Total
                        }).ToList();

                        var totaOrders = response.Data.TotalDays;
                        TotalPages = (int)Math.Ceiling((double)totaOrders / PageSize);
                        OnPropertyChanged(ResultText);
                        if (isShowMessage == true) AppLogger.Instance.LogInfo(
                            "Tải dữ liệu doanh thu thành công!");
                    }
                    else
                    {
                        AppLogger.Instance.LogError($"Lỗi tải dữ liệu doanh thu theo nhân viên: {response.Message}");
                        CustomMessageBox.Show(
                            "Lỗi tải dữ liệu doanh thu theo nhân viên!",
                            new List<string> { "OK" },
                            MessageType.Error
                            );
                        items = new List<OrderItem>();
                    }
                }

                RevenueItems = items;
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Exception khi tải dữ liệu doanh thu", ex);
                CustomMessageBox.Show(
                    "Lỗi tải dữ liệu doanh thu!",
                    new List<string> { "OK" },
                    MessageType.Error
                    );
                RevenueItems = new List<OrderItem>();
            }
            finally
            {
                IsLoading = false;
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
            }
        }
    }
}