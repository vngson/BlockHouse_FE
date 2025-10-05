using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace BlockHouse.Services
{
    public class BackendProcessService
    {
        private Process _backendProcess;
        private readonly string _backendExePath = "app.exe";
        private readonly string _healthCheckUrl = "http://localhost:5000/api/v1/health";
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        public async Task<bool> StartBackendWithHealthCheck(CancellationToken cancellationToken = default)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string backendExePath = Path.Combine(baseDir, _backendExePath);

                // Check if app.exe exists in the current directory (for .exe deployment)
                if (!File.Exists(backendExePath))
                {
                    // Fallback to project root (for development mode)
                    string projectDir = Directory.GetParent(baseDir)?.FullName;
                    if (projectDir != null)
                    {
                        backendExePath = Path.Combine(projectDir, _backendExePath);
                    }
                }

                backendExePath = Path.GetFullPath(backendExePath);

                if (!File.Exists(backendExePath))
                {
                    MessageBox.Show($"Backend executable not found: {backendExePath}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                Debug.WriteLine($"Starting backend process at: {backendExePath}");
                var startInfo = new ProcessStartInfo
                {
                    FileName = backendExePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                _backendProcess = Process.Start(startInfo);
                if (_backendProcess == null)
                {
                    MessageBox.Show("Failed to start backend process.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                Debug.WriteLine($"Backend process started, ID: {_backendProcess.Id}");

                using var client = new HttpClient();
                var startTime = DateTime.Now;

                while (DateTime.Now - startTime < _timeout)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var response = await client.GetAsync(_healthCheckUrl, cancellationToken);
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // Ignore errors and retry
                    }
                    await Task.Delay(500, cancellationToken);
                }

                // Clean up if health check fails
                if (_backendProcess != null && !_backendProcess.HasExited)
                {
                    _backendProcess.Kill();
                    _backendProcess.WaitForExit(5000);
                    _backendProcess.Dispose();
                    _backendProcess = null;
                }
                MessageBox.Show("Backend failed to start or respond to health check.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting backend: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void KillBackendProcess()
        {
            try
            {
                if (_backendProcess != null && !_backendProcess.HasExited)
                {
                    Debug.WriteLine($"Killing backend process, ID: {_backendProcess.Id}");
                    _backendProcess.Kill();
                    _backendProcess.WaitForExit(5000);
                    _backendProcess.Dispose();
                    _backendProcess = null;
                }
                else if (_backendProcess == null)
                {
                    Debug.WriteLine("Backend process is null, nothing to kill.");
                }

                string[] processNames = { "app", "run_api" };
                foreach (var name in processNames)
                {
                    var processes = Process.GetProcessesByName(name);
                    foreach (var process in processes)
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                Debug.WriteLine($"Killing process {name}, ID: {process.Id}");
                                process.Kill();
                                process.WaitForExit(3000);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error killing process {name}: {ex.Message}");
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in KillBackendProcess: {ex.Message}");
            }
        }
    }
}