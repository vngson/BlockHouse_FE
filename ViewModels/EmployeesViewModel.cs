using BlockHouse.Api;
using BlockHouse.Helpers; // Thêm using cho AppLogger
using BlockHouse.ViewModels.Components;
using BlockHouse.ViewModels.Popup.Employee;
using BlockHouse.Views.Popup.Employee;
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
    public class EmployeesViewModel : ViewModelBase
    {
        public class EmployeeItem
        {
            public int Id { get; set; }               // STT
            public string Name { get; set; }          // Họ và Tên
            public string Phone { get; set; }         // Số điện thoại
            public string Status { get; set; }        // Trạng thái
            //public decimal Income { get; set; }


            private decimal _income;
            public decimal Income
            {
                get => _income;
                set
                {
                    if (_income != value)
                    {
                        _income = value;
                        OnPropertyChanged(nameof(Income));
                        OnPropertyChanged(nameof(IncomeDisplay));
                    }
                }
            }

            public string IncomeDisplay => string.Format("{0:N0} VNĐ", Income);

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<EmployeeItem> _employeeItems;

        public List<EmployeeItem> EmployeeItems
        {
            get => _employeeItems;
            set => SetProperty(ref _employeeItems, value);
        }


        public IAsyncRelayCommand AddCommand { get; }
        public IAsyncRelayCommand ViewCommand { get; }
        public IAsyncRelayCommand EditCommand { get; }
        public ICommand GoBackCommand { get; }


        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private readonly EmployeeApi _employeeApi;



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
        public ICommand FirstPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }

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

        public EmployeesViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo EmployeesViewModel");
            _employeeApi = new EmployeeApi();
            // Tạo dữ liệu giả
            EmployeeItems = new List<EmployeeItem>();

            // Khởi tạo các lệnh
            AddCommand = new AsyncRelayCommand(AddEmployeeAsync);
            ViewCommand = new AsyncRelayCommand<EmployeeItem>(ExecuteView);
            EditCommand = new AsyncRelayCommand<EmployeeItem>(ExecuteEdit);
            GoBackCommand = new RelayCommand(ExecuteGoBack);
            SearchCommand = new AsyncRelayCommand(ExecuteSearch);
            _totalPages = 1;
            _currentPage = 1;

            FirstPageCommand = new AsyncRelayCommand(GoToFirstPage);
            PrevPageCommand = new AsyncRelayCommand(GoToPrevPage);
            NextPageCommand = new AsyncRelayCommand(GoToNextPage);
            LastPageCommand = new AsyncRelayCommand(GoToLastPage);

            DatePickerLoadedCommand = new RelayCommand<DatePicker>(DatePicker_Loaded);

            StartDate = null;
            EndDate = null;
            _ = LoadData();
        }

        private async Task GoToFirstPage() => await LoadData(1, StartDate, EndDate, SearchKeyword);
        private async Task GoToPrevPage() => await LoadData(CurrentPage - 1, StartDate, EndDate, SearchKeyword);
        private async Task GoToNextPage() => await LoadData(CurrentPage + 1, StartDate, EndDate, SearchKeyword);
        private async Task GoToLastPage() => await LoadData(TotalPages, StartDate, EndDate, SearchKeyword);
        private void ExecuteGoBack()
        {
            AppLogger.Instance.LogInfo("Quay lại màn hình tổng quan từ EmployeesViewModel");
            WeakReferenceMessenger.Default.Send(new ChangeViewMessage("Overview"));
        }

        private async Task AddEmployeeAsync()
        {
            try
            {
                AppLogger.Instance.LogInfo("Mở form thêm nhân viên");
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });

                var addViewModel = new EmployeeAddViewModel();

                var dialog = new EmployeeAddView
                {
                    DataContext = addViewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // Đăng ký sự kiện thêm thành công
                addViewModel.EmployeeAdded += (newEmployee) =>
                {
                    // Có thể xử lý logic sau khi thêm thành công, ví dụ refresh danh sách
                };

                addViewModel.RequestClose += () =>
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
                AppLogger.Instance.LogError("Error opening add employee dialog", ex);
                CustomMessageBox.Show(
                    "Lỗi mở form thêm nhân viên!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
            }
        }


        private async Task ExecuteView(EmployeeItem item)
        {
            if (item == null)
            {
                AppLogger.Instance.LogWarning("Tham số item null khi xem chi tiết nhân viên");
                return;
            }

            AppLogger.Instance.LogInfo($"Xem chi tiết nhân viên Id={item.Id}");

            try
            {
                // Show overlay
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });

                var detailVm = new EmployeeDetailViewModel();
                await detailVm.LoadDataAsync(item.Id);

                var dialog = new EmployeeDetailView
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
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Error opening employee detail dialog", ex);
                CustomMessageBox.Show(
                    "Lỗi mở form chi tiết nhân viên!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
            }
        }

        private async Task ExecuteEdit(EmployeeItem item)
        {
            if (item == null)
            {
                AppLogger.Instance.LogWarning("Tham số item null khi sửa nhân viên");
                return;
            }

            AppLogger.Instance.LogInfo($"Sửa nhân viên Id={item.Id}");

            try
            {
                // Show overlay
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });

                var editVm = new EmployeeEditViewModel();
                await editVm.LoadDataAsync(item.Id); // Giả sử item.Id là employeeId

                var dialog = new EmployeeEditView
                {
                    DataContext = editVm,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                dialog.Closed += async (s, e) =>
                {
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                    // Refresh data after editing
                    await LoadData(CurrentPage, StartDate, EndDate, SearchKeyword);
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Error opening employee edit dialog", ex);
                CustomMessageBox.Show(
                    "Lỗi mở form sửa nhân viên!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
            }
        }

        private async Task ExecuteSearch()
        {
            AppLogger.Instance.LogInfo($"Thực hiện tìm kiếm nhân viên: StartDate={StartDate}, EndDate={EndDate}, Keyword={SearchKeyword}");
            if (StartDate > EndDate)
            {
                AppLogger.Instance.LogWarning("Ngày bắt đầu lớn hơn ngày kết thúc khi tìm kiếm nhân viên");
                CustomMessageBox.Show(
                    "Ngày bắt đầu phải nhỏ hơn ngày kết thúc!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                return;
            }
            await LoadData(1, StartDate, EndDate, SearchKeyword, true);
        }

        private async Task LoadData(int? page = 1,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? keyword = null,
            bool? isShowMessage = false)
        {
            AppLogger.Instance.LogInfo($"Bắt đầu tải dữ liệu nhân viên: page={page}, startDate={startDate}, endDate={endDate}, keyword={keyword}");
            try
            {
                IsLoading = true;
                CurrentPage = (int)page;

                List<EmployeeItem> items;
                var response = await _employeeApi.GetEmployees(page, PageSize, startDate, endDate, keyword);
                if (response.IsSuccess && response.Data != null)
                {
                    AppLogger.Instance.LogInfo($"Tải dữ liệu nhân viên thành công. Số lượng: {response.Data.Employees.Count}");
                    items = response.Data.Employees.Select((o, index) => new EmployeeItem
                    {
                        Id = index + 1,
                        Name = o.Name ?? "",
                        Phone = o.Phone ?? "",
                        Status = o.Status == 0 ? "Nghỉ" : "Hoạt động",
                        Income = (decimal)(o.Income ?? 0)
                    }).ToList();

                    var totaEmployees = response.Data.Total_Employees;
                    TotalPages = (int)Math.Ceiling((double)totaEmployees / PageSize);
                    OnPropertyChanged(ResultText);

                }
                else
                {
                    AppLogger.Instance.LogError($"Lỗi tải dữ liệu nhân viên: {response.Message}");
                    CustomMessageBox.Show(
                        "Lỗi tải dữ liệu nhân viên!",
                        new List<string> { "OK" },
                        MessageType.Error
                    );
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                    items = new List<EmployeeItem>();
                }

                EmployeeItems = items;
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Exception khi tải dữ liệu nhân viên", ex);
                CustomMessageBox.Show(
                    "Lỗi tải dữ liệu nhân viên!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                EmployeeItems = new List<EmployeeItem>();
            }
            finally
            {
                IsLoading = false;
            }
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
                    popup.Child is Calendar calendar)
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
    }

    public class ChangeViewMessage
    {
        public string Destination { get; }

        public ChangeViewMessage(string destination)
        {
            Destination = destination;
        }
    }
}
