using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace VideoDownloaderApp.Linux;

public partial class MainWindow : Window
{
    private readonly DownloadEngine _downloadEngine = new();
    private AutoUpdater.UpdateInfo? _latestUpdateInfo;

    public MainWindow()
    {
        InitializeComponent();
        OutputPathTextBox.Text = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        _downloadEngine.LogReceived += OnLogReceived;
        _downloadEngine.ProgressChanged += OnProgressChanged;
        Closing += MainWindow_Closing;
        VersionLabel.Text = $"Version {AutoUpdater.CurrentVersion}";

        // เช็คอัปเดตอัตโนมัติตอนเปิดแอป
        _ = CheckUpdateOnStartupAsync();
    }

    private async Task CheckUpdateOnStartupAsync()
    {
        try
        {
            // 1. ตรวจสอบอัปเดตของตัวแอปอัตโนมัติ
            var updateInfo = await AutoUpdater.CheckForUpdateAsync();
            if (updateInfo.IsAvailable)
            {
                _latestUpdateInfo = updateInfo;
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UpdateBadge.Text = $"มีเวอร์ชันใหม่ v{updateInfo.LatestVersion} (คลิกเพื่อเปิด)";
                    HeaderUpdateBadge.Text = $"v{updateInfo.LatestVersion} พร้อมอัปเดต";
                    AppendLog($"[Auto-Update] มีเวอร์ชันใหม่ v{updateInfo.LatestVersion} (ปัจจุบัน v{updateInfo.CurrentVersion})");
                    AppendLog($"ลิงก์ดาวน์โหลด: {updateInfo.HtmlUrl}");
                });
            }

            // 2. ตรวจสอบและอัปเดต yt-dlp ในพื้นหลังอัตโนมัติ
            var (ytSuccess, ytMsg) = await AutoUpdater.UpdateYtDlpAsync();
            if (ytSuccess)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    string clean = ytMsg.Replace("\r", "").Replace("\n", " ").Trim();
                    AppendLog($"[Auto-Update yt-dlp] {clean}");
                });
            }
        }
        catch { /* ไม่ต้องรบกวนผู้ใช้ถ้าเช็คอัปเดตไม่ได้ */ }
    }

    private void HeaderUpdateBadge_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_latestUpdateInfo != null && !string.IsNullOrEmpty(_latestUpdateInfo.HtmlUrl))
        {
            AutoUpdater.OpenReleasePage(_latestUpdateInfo.HtmlUrl);
        }
        else
        {
            AutoUpdater.OpenReleasePage("https://github.com/plub845/VideoDownloaderApp/releases");
        }
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        var startLocation = await TryGetStartLocationAsync(OutputPathTextBox.Text);
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "เลือกโฟลเดอร์สำหรับบันทึกไฟล์",
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

        // === Validation ที่ชัดเจน ===
        if (string.IsNullOrWhiteSpace(url))
        {
            ShowUserError("กรุณาวาง URL", "ยังไม่ได้ใส่ลิงก์วิดีโอ",
                "คัดลอกลิงก์วิดีโอจาก YouTube หรือเว็บไซต์อื่นมาวางในช่อง Video URL");
            return;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl) ||
            (parsedUrl.Scheme != Uri.UriSchemeHttp && parsedUrl.Scheme != Uri.UriSchemeHttps))
        {
            ShowUserError("ลิงก์ไม่ถูกต้อง",
                $"ลิงก์ \"{(url.Length > 60 ? url[..60] + "..." : url)}\" ไม่ใช่ URL ที่ถูกต้อง",
                "ลิงก์ต้องขึ้นต้นด้วย http:// หรือ https://\nตัวอย่าง: https://www.youtube.com/watch?v=...");
            return;
        }

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            ShowUserError("ไม่ได้ระบุโฟลเดอร์", "กรุณาเลือกโฟลเดอร์สำหรับบันทึกไฟล์",
                "กดปุ่ม Browse เพื่อเลือกโฟลเดอร์ หรือพิมพ์ path โดยตรง เช่น ~/Downloads");
            return;
        }

        try
        {
            Directory.CreateDirectory(outputDirectory);
            OutputPathTextBox.Text = outputDirectory;
        }
        catch (Exception ex)
        {
            ShowUserError("สร้างโฟลเดอร์ไม่ได้",
                $"ไม่สามารถสร้างโฟลเดอร์:\n{outputDirectory}\n\nสาเหตุ: {ex.Message}",
                "ลองเลือกโฟลเดอร์อื่น เช่น ~/Downloads หรือ ~/Desktop");
            return;
        }

        SetBusy(true);
        SetStatus("กำลังเริ่มดาวน์โหลด...", false);
        AppendLog($"เริ่มดาวน์โหลด: {url}");
        AppendLog($"โฟลเดอร์ปลายทาง: {outputDirectory}");

        try
        {
            var result = await _downloadEngine.DownloadAsync(url, outputDirectory, format, customOptions);

            if (result.WasStopped)
            {
                SetStatus("ผู้ใช้หยุดการดาวน์โหลดแล้ว", false);
                AppendLog("การดาวน์โหลดถูกหยุดโดยผู้ใช้");
            }
            else
            {
                // ใช้ ErrorParser วิเคราะห์ผลลัพธ์ — ไม่โยนให้ดู log เอง
                var analysis = ErrorParser.AnalyzeResult(result.ExitCode, result.ErrorLines, result.WarningLines);
                string formattedMsg = ErrorParser.FormatForUser(analysis);

                if (analysis.Severity >= ErrorParser.ErrorSeverity.Error)
                {
                    SetStatus(analysis.Title, true);
                    AppendLog(formattedMsg);
                    // แสดง inline error ที่ชัดเจน ไม่ใช่แค่ "ดู log"
                    AppendLog("═══════════════════════════════════");
                    AppendLog($"สาเหตุ: {analysis.Detail}");
                    if (!string.IsNullOrWhiteSpace(analysis.Suggestion))
                        AppendLog($"แก้ไข: {analysis.Suggestion}");
                    AppendLog("═══════════════════════════════════");
                }
                else if (result.ExitCode == 0)
                {
                    DownloadProgressBar.Value = 100;
                    SetStatus("ดาวน์โหลดเสร็จสมบูรณ์", false);
                    AppendLog("ดาวน์โหลดเสร็จสมบูรณ์");

                    if (result.WarningLines.Count > 0)
                    {
                        AppendLog($"มีคำเตือน {result.WarningLines.Count} รายการ (ไม่กระทบการดาวน์โหลด)");
                    }
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            // yt-dlp หรือ ffmpeg ไม่เจอใน PATH
            ShowUserError("ไม่พบโปรแกรมที่จำเป็น", ex.Message, "");
        }
        catch (ArgumentException ex)
        {
            // Custom Options มี quote ไม่ครบ
            ShowUserError("Custom Options ไม่ถูกต้อง", ex.Message,
                "ตรวจสอบว่าเครื่องหมาย quote (\" หรือ ') เปิดและปิดครบถ้วน");
        }
        catch (InvalidOperationException ex)
        {
            ShowUserError("ไม่สามารถเริ่มดาวน์โหลดได้", ex.Message,
                "ลองรอสักครู่แล้วลองใหม่ หรือปิดโปรแกรมแล้วเปิดใหม่");
        }
        catch (Exception ex)
        {
            ShowUserError("เกิดข้อผิดพลาดที่ไม่คาดคิด", ex.Message,
                "ลองดาวน์โหลดใหม่อีกครั้ง หากยังไม่ได้ ลองอัปเดต yt-dlp");
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



    private void OnLogReceived(string line) => Dispatcher.UIThread.Post(() =>
    {
        AppendLog(line);

        // ตรวจจับ error แบบ real-time แล้วแสดงข้อมูลเพิ่มเติมทันที
        var parsed = ErrorParser.TryParse(line);
        if (parsed != null && parsed.Severity >= ErrorParser.ErrorSeverity.Error)
        {
            SetStatus(parsed.Title, true);
        }
    });

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
            ? Avalonia.Media.Brushes.Coral
            : Avalonia.Media.Brushes.LightGreen;
    }

    /// <summary>
    /// แสดง Error ที่ชัดเจน — ไม่โยนให้ดู log เอง
    /// </summary>
    private void ShowUserError(string title, string detail, string suggestion)
    {
        SetStatus(title, true);
        AppendLog("═══════════════════════════════════");
        AppendLog($"[ข้อผิดพลาด] {title}");
        if (!string.IsNullOrWhiteSpace(detail))
            AppendLog($"สาเหตุ: {detail}");
        if (!string.IsNullOrWhiteSpace(suggestion))
            AppendLog($"แก้ไข: {suggestion}");
        AppendLog("═══════════════════════════════════");
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
