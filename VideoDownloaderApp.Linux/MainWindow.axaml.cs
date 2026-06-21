using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace VideoDownloaderApp.Linux;

public partial class MainWindow : Window
{
    private readonly DownloadEngine _downloadEngine = new();

    public MainWindow()
    {
        InitializeComponent();
        OutputPathTextBox.Text = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        _downloadEngine.LogReceived += OnLogReceived;
        _downloadEngine.ProgressChanged += OnProgressChanged;
        Closing += MainWindow_Closing;
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        var startLocation = await TryGetStartLocationAsync(OutputPathTextBox.Text);
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "เลือกโฟลเดอร์ปลายทาง",
            AllowMultiple = false,
            SuggestedStartLocation = startLocation
        });

        if (folders.Count > 0)
        {
            OutputPathTextBox.Text = folders[0].TryGetLocalPath() ?? folders[0].Path.LocalPath;
        }
    }

    private async void DownloadButton_Click(object? sender, RoutedEventArgs e)
    {
        LogTextBox.Clear();
        DownloadProgressBar.Value = 0;

        var url = UrlTextBox.Text?.Trim() ?? string.Empty;
        var outputDirectory = ExpandHomePath(OutputPathTextBox.Text?.Trim() ?? string.Empty);
        var customOptions = CustomOptionsTextBox.Text?.Trim() ?? string.Empty;
        var format = Mp3RadioButton.IsChecked == true ? DownloadFormat.Mp3 : DownloadFormat.Mp4;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl) ||
            (parsedUrl.Scheme != Uri.UriSchemeHttp && parsedUrl.Scheme != Uri.UriSchemeHttps))
        {
            SetStatus("กรุณาป้อน URL แบบ http หรือ https ที่ถูกต้อง", true);
            AppendLog("[ERROR] URL ไม่ถูกต้อง");
            return;
        }

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            SetStatus("กรุณาระบุโฟลเดอร์ปลายทาง", true);
            return;
        }

        try
        {
            Directory.CreateDirectory(outputDirectory);
            OutputPathTextBox.Text = outputDirectory;
        }
        catch (Exception ex)
        {
            SetStatus($"สร้างโฟลเดอร์ปลายทางไม่ได้: {ex.Message}", true);
            AppendLog($"[ERROR] {ex.Message}");
            return;
        }

        SetBusy(true);
        SetStatus("กำลังดาวน์โหลด...", false);
        AppendLog($"เริ่มดาวน์โหลด: {url}");
        AppendLog($"โฟลเดอร์ปลายทาง: {outputDirectory}");

        try
        {
            var result = await _downloadEngine.DownloadAsync(url, outputDirectory, format, customOptions);
            if (result.WasStopped)
            {
                SetStatus("หยุดการดาวน์โหลดแล้ว", true);
                AppendLog("■ ผู้ใช้หยุดการดาวน์โหลด");
            }
            else if (result.ExitCode == 0)
            {
                DownloadProgressBar.Value = 100;
                SetStatus("✅ ดาวน์โหลดเสร็จสมบูรณ์", false);
                AppendLog("✅ ดาวน์โหลดเสร็จสมบูรณ์");
            }
            else
            {
                SetStatus($"❌ ดาวน์โหลดไม่สำเร็จ (Exit code: {result.ExitCode})", true);
                AppendLog("❌ ดาวน์โหลดไม่สำเร็จ กรุณาตรวจสอบ log");
            }
        }
        catch (Exception ex)
        {
            SetStatus("❌ เกิดข้อผิดพลาดระหว่างดาวน์โหลด", true);
            AppendLog($"[ERROR] {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void StopButton_Click(object? sender, RoutedEventArgs e)
    {
        StopButton.IsEnabled = false;
        SetStatus("กำลังหยุด...", true);
        _downloadEngine.Stop();
    }

    private void OnLogReceived(string line) => Dispatcher.UIThread.Post(() => AppendLog(line));

    private void OnProgressChanged(double progress) =>
        Dispatcher.UIThread.Post(() => DownloadProgressBar.Value = progress);

    private void AppendLog(string message)
    {
        LogTextBox.Text += message + Environment.NewLine;
        LogTextBox.CaretIndex = LogTextBox.Text?.Length ?? 0;
    }

    private void SetStatus(string message, bool isError)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = isError
            ? Avalonia.Media.Brushes.OrangeRed
            : Avalonia.Media.Brushes.LightGreen;
    }

    private void SetBusy(bool isBusy)
    {
        UrlTextBox.IsEnabled = !isBusy;
        Mp4RadioButton.IsEnabled = !isBusy;
        Mp3RadioButton.IsEnabled = !isBusy;
        OutputPathTextBox.IsEnabled = !isBusy;
        BrowseButton.IsEnabled = !isBusy;
        CustomOptionsTextBox.IsEnabled = !isBusy;
        DownloadButton.IsEnabled = !isBusy;
        StopButton.IsEnabled = isBusy;
    }

    private async Task<IStorageFolder?> TryGetStartLocationAsync(string? path)
    {
        var expandedPath = ExpandHomePath(path?.Trim() ?? string.Empty);
        return Directory.Exists(expandedPath)
            ? await StorageProvider.TryGetFolderFromPathAsync(expandedPath)
            : null;
    }

    private static string ExpandHomePath(string path)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (path == "~") return home;
        return path.StartsWith("~/", StringComparison.Ordinal) ? Path.Combine(home, path[2..]) : path;
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        _downloadEngine.Stop();
        _downloadEngine.Dispose();
    }
}
