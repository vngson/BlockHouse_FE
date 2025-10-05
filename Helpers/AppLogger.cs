using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BlockHouse.Helpers
{
    public class AppLogger : INotifyPropertyChanged
    {
        private static readonly Lazy<AppLogger> _instance = new Lazy<AppLogger>(() => new AppLogger());
        public static AppLogger Instance => _instance.Value;

        private string _logFilePath;
        private readonly object _lock = new object();
        private ObservableCollection<string> _logMessages;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB in bytes

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> LogMessages
        {
            get => _logMessages;
            private set
            {
                _logMessages = value;
                OnPropertyChanged();
            }
        }

        private AppLogger()
        {
            UpdateLogFilePath();
            _logMessages = new ObservableCollection<string>();
            EnsureLogDirectoryExists();
        }

        private void EnsureLogDirectoryExists()
        {
            var directory = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void UpdateLogFilePath()
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string baseFileName = Path.Combine(basePath, $"app_{datePart}.log");
            int fileIndex = 0;
            _logFilePath = baseFileName;

            // Check if file exists and is over 5MB
            while (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length >= MaxFileSize)
            {
                fileIndex++;
                _logFilePath = Path.Combine(basePath, $"app_{datePart}_{fileIndex}.log");
            }
        }

        public void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        public void LogError(string message, Exception ex = null)
        {
            string fullMessage = ex != null ? $"{message}: {ex.Message}\n{ex.StackTrace}" : message;
            Log("ERROR", fullMessage);
        }

        private void Log(string level, string message)
        {
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {message}";

            lock (_lock)
            {
                // Check if date has changed or file size exceeds 5MB
                string newDatePart = DateTime.Now.ToString("yyyyMMdd");
                bool fileSizeExceeded = File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length >= MaxFileSize;
                if (fileSizeExceeded || !Path.GetFileName(_logFilePath).Contains(newDatePart))
                {
                    UpdateLogFilePath();
                }

                // Add to UI collection
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    _logMessages.Add(formattedMessage);
                });

                // Write to console
                Console.WriteLine(formattedMessage);

                // Write to file asynchronously
                Task.Run(() =>
                {
                    try
                    {
                        File.AppendAllText(_logFilePath, formattedMessage + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to write to log file: {ex.Message}");
                    }
                });
            }
        }

        public void ClearLogs()
        {
            lock (_lock)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    _logMessages.Clear();
                });

                try
                {
                    if (File.Exists(_logFilePath))
                    {
                        File.WriteAllText(_logFilePath, string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to clear log file: {ex.Message}");
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}