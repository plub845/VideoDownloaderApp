using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoDownloaderApp
{
    public partial class Form1 : Form
    {
        private static readonly string EngineDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VideoDownloaderApp");
        private readonly string ytDlpPath = Path.Combine(EngineDir, "yt-dlp.exe");
        private readonly string ffmpegPath = Path.Combine(EngineDir, "ffmpeg.exe");

        private CheckBox chkPlaylist;
        private RoundedButton btnStop;
        private Label lblVersion;
        private AutoUpdater.UpdateInfo? _latestUpdateInfo;
        private CancellationTokenSource? _cts;
        private Process? _activeProcess;
        private readonly object _processLock = new();

        // Error tracking สำหรับ error parsing ที่ชาญฉลาด
        private readonly List<string> _errorLines = new();
        private readonly List<string> _warningLines = new();

        private void ApplyModernTheme()
        {
            // === Warm Cream Theme (สีเนื้อนวล) ===
            this.BackColor = Color.FromArgb(255, 248, 240);
            this.ForeColor = Color.FromArgb(80, 60, 45);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.Size = new Size(960, 545);
            this.MinimumSize = new Size(960, 545);

            // ไม่มีแบนเนอร์ — เริ่ม layout จากด้านบนเลย

            // 2. จัดตำแหน่งคอนโทรล
            label1.Text = "URL วิดีโอ (Video URL):";
            label1.Location = new Point(20, 18);
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(90, 65, 45);

            txtUrl.Location = new Point(20, 43);
            txtUrl.Size = new Size(440, 30);
            txtUrl.BackColor = Color.FromArgb(255, 252, 245);
            txtUrl.ForeColor = Color.FromArgb(70, 50, 35);
            txtUrl.BorderStyle = BorderStyle.FixedSingle;

            chkPlaylist.Location = new Point(20, 80);
            chkPlaylist.AutoSize = true;
            chkPlaylist.ForeColor = Color.FromArgb(120, 95, 70);

            label2.Location = new Point(20, 112);
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label2.ForeColor = Color.FromArgb(90, 65, 45);
            label2.Text = "รูปแบบไฟล์ที่ต้องการ (Format):";

            grpOutputFormat.Location = new Point(20, 137);
            grpOutputFormat.Size = new Size(440, 60);
            grpOutputFormat.ForeColor = Color.FromArgb(130, 105, 80);
            grpOutputFormat.Text = "ตัวเลือกไฟล์";
            rbMp4.Location = new Point(30, 23);
            rbMp4.Text = "MP4 (วิดีโอพร้อมเสียง)";
            rbMp4.ForeColor = Color.FromArgb(80, 60, 45);
            rbMp3.Location = new Point(220, 23);
            rbMp3.Text = "MP3 (เสียงเท่านั้น)";
            rbMp3.ForeColor = Color.FromArgb(80, 60, 45);

            label3.Location = new Point(20, 208);
            label3.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label3.ForeColor = Color.FromArgb(90, 65, 45);
            label3.Text = "โฟลเดอร์ปลายทาง (Destination):";

            txtOutputPath.Location = new Point(20, 233);
            txtOutputPath.Size = new Size(330, 30);
            txtOutputPath.BackColor = Color.FromArgb(255, 252, 245);
            txtOutputPath.ForeColor = Color.FromArgb(70, 50, 35);
            txtOutputPath.BorderStyle = BorderStyle.FixedSingle;

            btnBrowse.Location = new Point(360, 232);
            btnBrowse.Size = new Size(100, 32);
            btnBrowse.Text = "Browse";
            btnBrowse.NormalColor = Color.FromArgb(228, 210, 185);
            btnBrowse.HoverColor = Color.FromArgb(240, 222, 198);
            btnBrowse.PressedColor = Color.FromArgb(215, 195, 170);
            btnBrowse.ForeColor = Color.FromArgb(90, 65, 45);
            btnBrowse.BorderRadius = 8;
            btnBrowse.Image = IconHelper.CreateFolderIcon(16, Color.FromArgb(140, 100, 65));
            btnBrowse.TextImageRelation = TextImageRelation.ImageBeforeText;

            label4.Location = new Point(20, 273);
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            label4.ForeColor = Color.FromArgb(160, 135, 110);
            label4.Text = "คำสั่งเสริม yt-dlp (เช่น --no-playlist หรือ --write-sub):";

            txtCustomOptions.Location = new Point(20, 298);
            txtCustomOptions.Size = new Size(440, 30);
            txtCustomOptions.BackColor = Color.FromArgb(255, 252, 245);
            txtCustomOptions.ForeColor = Color.FromArgb(70, 50, 35);
            txtCustomOptions.BorderStyle = BorderStyle.FixedSingle;

            progressBar.Location = new Point(20, 342);
            progressBar.Size = new Size(440, 18);

            // ปุ่ม Download + Stop
            btnDownload.Location = new Point(20, 375);
            btnDownload.Size = new Size(325, 50);
            btnDownload.Text = "DOWNLOAD NOW";
            btnDownload.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnDownload.NormalColor = Color.FromArgb(200, 140, 95);
            btnDownload.HoverColor = Color.FromArgb(215, 158, 115);
            btnDownload.PressedColor = Color.FromArgb(185, 125, 82);
            btnDownload.ForeColor = Color.White;
            btnDownload.BorderRadius = 10;
            btnDownload.Image = IconHelper.CreateDownloadIcon(20, Color.White);
            btnDownload.TextImageRelation = TextImageRelation.ImageBeforeText;

            btnStop.Location = new Point(355, 375);
            btnStop.Size = new Size(105, 50);
            btnStop.Text = "STOP";
            btnStop.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnStop.NormalColor = Color.FromArgb(220, 205, 185);
            btnStop.HoverColor = Color.FromArgb(232, 218, 198);
            btnStop.PressedColor = Color.FromArgb(208, 192, 172);
            btnStop.ForeColor = Color.FromArgb(120, 85, 55);
            btnStop.BorderRadius = 10;
            btnStop.Image = IconHelper.CreateStopIcon(14, Color.FromArgb(160, 110, 70));
            btnStop.TextImageRelation = TextImageRelation.ImageBeforeText;

            lblVersion.Location = new Point(20, 440);
            lblVersion.Size = new Size(440, 24);
            lblVersion.ForeColor = Color.FromArgb(175, 155, 130);

            // ช่อง Log ด้านขวา
            txtStatus.Location = new Point(480, 18);
            txtStatus.Size = new Size(450, 455);
            txtStatus.BackColor = Color.FromArgb(250, 245, 235);
            txtStatus.ForeColor = Color.FromArgb(110, 90, 65);
            txtStatus.BorderStyle = BorderStyle.None;
            txtStatus.Font = new Font("Consolas", 9.5F);

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        public Form1()
        {
            InitializeComponent();

            this.Text = "Video Downloader Pro";

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VDapp.icon.ico");
                if (File.Exists(iconPath))
                    this.Icon = new System.Drawing.Icon(iconPath);
                else if (File.Exists("VDapp.icon.ico"))
                    this.Icon = new System.Drawing.Icon("VDapp.icon.ico");
            }
            catch { }

            // CheckBox เลือกว่าจะโหลดเพลย์ลิสต์หรือไม่
            chkPlaylist = new CheckBox
            {
                Text = "ดาวน์โหลดแบบ Playlist (สร้างโฟลเดอร์ให้อัตโนมัติ)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            this.Controls.Add(chkPlaylist);

            // ปุ่ม Stop
            btnStop = new RoundedButton
            {
                Name = "btnStop",
                Text = "STOP",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Enabled = false,
                BorderRadius = 10
            };
            btnStop.Click += btnStop_Click;
            this.Controls.Add(btnStop);

            // Label เวอร์ชัน (คลิกเพื่ออัปเดตได้เมื่อมีเวอร์ชันใหม่)
            lblVersion = new Label
            {
                Name = "lblVersion",
                Text = $"Version {AutoUpdater.CurrentVersion}",
                Font = new Font("Segoe UI", 8.5F),
                AutoSize = false
            };
            lblVersion.Click += async (s, e) =>
            {
                if (_latestUpdateInfo != null && _latestUpdateInfo.IsAvailable)
                {
                    await PromptAndInstallUpdateAsync(_latestUpdateInfo);
                }
            };
            this.Controls.Add(lblVersion);

            txtOutputPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            ApplyModernTheme();

            // เช็คอัปเดตแอปและ yt-dlp อัตโนมัติตอนเปิดแอป (ทำงานในพื้นหลัง ไม่บล็อก UI)
            _ = CheckUpdateOnStartupAsync();
            _ = AutoUpdateYtDlpOnStartupAsync();
        }

        private async Task CheckUpdateOnStartupAsync()
        {
            try
            {
                var updateInfo = await AutoUpdater.CheckForUpdateAsync();
                if (updateInfo.IsAvailable)
                {
                    _latestUpdateInfo = updateInfo;
                    if (!IsDisposed)
                    {
                        Invoke(() =>
                        {
                            UpdateStatus($"ตรวจพบเวอร์ชันใหม่ v{updateInfo.LatestVersion} (ปัจจุบัน v{updateInfo.CurrentVersion})", false);
                            lblVersion.Text = $"Version {updateInfo.CurrentVersion} (พบเวอร์ชันใหม่ v{updateInfo.LatestVersion} - คลิกเพื่ออัปเดต)";
                            lblVersion.ForeColor = Color.FromArgb(180, 120, 60);
                            lblVersion.Cursor = Cursors.Hand;
                        });

                        await PromptAndInstallUpdateAsync(updateInfo);
                    }
                }
            }
            catch { }
        }

        private async Task AutoUpdateYtDlpOnStartupAsync()
        {
            try
            {
                if (File.Exists(ytDlpPath))
                {
                    var (success, message) = await AutoUpdater.UpdateYtDlpAsync(ytDlpPath);
                    if (!IsDisposed && success)
                    {
                        Invoke(() =>
                        {
                            string cleanMsg = message.Replace("\r", "").Replace("\n", " ").Trim();
                            UpdateStatus(cleanMsg, false);
                        });
                    }
                }
            }
            catch { }
        }

        private async Task PromptAndInstallUpdateAsync(AutoUpdater.UpdateInfo updateInfo)
        {
            DialogResult result = DialogResult.No;
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    result = ModernDialog.ShowDialog(
                        this,
                        "พบเวอร์ชันใหม่พร้อมใช้งาน",
                        $"ตรวจพบเวอร์ชันใหม่ v{updateInfo.LatestVersion} (ปัจจุบัน v{updateInfo.CurrentVersion})\nต้องการดาวน์โหลดและติดตั้งตัวอัปเดตอัตโนมัติทันทีเลยหรือไม่?",
                        ModernDialogType.Question,
                        confirmText: "อัปเดตทันที",
                        cancelText: "ไว้ภายหลัง",
                        details: updateInfo.ReleaseNotes);
                });
            }
            else
            {
                result = ModernDialog.ShowDialog(
                    this,
                    "พบเวอร์ชันใหม่พร้อมใช้งาน",
                    $"ตรวจพบเวอร์ชันใหม่ v{updateInfo.LatestVersion} (ปัจจุบัน v{updateInfo.CurrentVersion})\nต้องการดาวน์โหลดและติดตั้งตัวอัปเดตอัตโนมัติทันทีเลยหรือไม่?",
                    ModernDialogType.Question,
                    confirmText: "อัปเดตทันที",
                    cancelText: "ไว้ภายหลัง",
                    details: updateInfo.ReleaseNotes);
            }

            if (result == DialogResult.Yes)
            {
                UpdateStatus($"กำลังดาวน์โหลดตัวอัปเดต v{updateInfo.LatestVersion}...", false);
                progressBar.Value = 0;
                var (success, message) = await AutoUpdater.DownloadAndInstallAsync(
                    updateInfo.DownloadUrl,
                    progress =>
                    {
                        if (!IsDisposed)
                        {
                            Invoke(() =>
                            {
                                progressBar.Value = Math.Clamp(progress, 0, 100);
                                UpdateStatus($"กำลังดาวน์โหลดตัวอัปเดต... {progress}%", false);
                            });
                        }
                    }
                );

                if (success)
                {
                    ModernDialog.ShowDialog(
                        this,
                        "เริ่มติดตั้งอัปเดต",
                        "ดาวน์โหลดตัวอัปเดตเรียบร้อยแล้ว กำลังเริ่มโปรแกรมติดตั้ง\nโปรแกรมจะปิดตัวเองเพื่อดำเนินการอัปเดต",
                        ModernDialogType.Information,
                        confirmText: "ตกลง");
                    Application.Exit();
                }
                else
                {
                    ShowUserError("ดาวน์โหลดอัปเดตล้มเหลว", message, "กำลังเปิดหน้าดาวน์โหลดบน GitHub ให้แทน");
                    AutoUpdater.OpenReleasePage(updateInfo.HtmlUrl);
                }
            }
        }

        private string CleanYoutubeUrl(string url) => url.Trim();

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();
            _errorLines.Clear();
            _warningLines.Clear();

            string url = CleanYoutubeUrl(txtUrl.Text.Trim());
            txtUrl.Text = url;

            string outputFormat = rbMp4.Checked ? "mp4" : "mp3";
            string outputPath = txtOutputPath.Text.Trim();
            string customOptions = txtCustomOptions.Text.Trim();

            if (string.IsNullOrEmpty(url))
            {
                ShowUserError("กรุณากรอก URL",
                    "ช่อง URL ว่างเปล่า กรุณาวางลิงก์วิดีโอที่ต้องการดาวน์โหลด",
                    "คัดลอกลิงก์จากเบราว์เซอร์ แล้วกด Ctrl+V วางในช่อง URL");
                return;
            }

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                ShowUserError("ลิงก์ไม่ถูกต้อง",
                    $"ลิงก์ \"{(url.Length > 60 ? url[..60] + "..." : url)}\" ไม่ใช่ URL ที่ถูกต้อง",
                    "ลิงก์ต้องขึ้นต้นด้วย http:// หรือ https://\nตัวอย่าง: https://www.youtube.com/watch?v=...");
                return;
            }

            if (!File.Exists(ytDlpPath))
            {
                ShowUserError("ไม่พบ yt-dlp.exe",
                    $"ไม่พบไฟล์ yt-dlp.exe ที่:\n{ytDlpPath}",
                    "1. ติดตั้งโปรแกรมใหม่ผ่าน Installer\n2. หรือดาวน์โหลด yt-dlp.exe จาก https://github.com/yt-dlp/yt-dlp/releases แล้ววางที่โฟลเดอร์ด้านบน");
                return;
            }

            if (!File.Exists(ffmpegPath))
            {
                ShowUserError("ไม่พบ ffmpeg.exe",
                    $"ไม่พบไฟล์ ffmpeg.exe ที่:\n{ffmpegPath}\n\nffmpeg จำเป็นสำหรับทั้ง MP4 และ MP3",
                    "1. ติดตั้งโปรแกรมใหม่ผ่าน Installer\n2. หรือดาวน์โหลด ffmpeg จาก https://ffmpeg.org/download.html แล้ววางที่โฟลเดอร์ด้านบน");
                return;
            }

            SetUIEnabled(false);
            UpdateStatus("กำลังเริ่มดาวน์โหลด...", false);
            progressBar.Value = 0;

            _cts = new CancellationTokenSource();

            string outputFilenameTemplate = "%(upload_date)s_%(title)s.%(ext)s";
            string arguments = "";

            if (outputFormat == "mp3")
            {
                arguments = $"--extract-audio --audio-format mp3 --audio-quality 0 --ffmpeg-location \"{ffmpegPath}\" --no-mtime --newline";
            }
            else
            {
                arguments = $"-f bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best --ffmpeg-location \"{ffmpegPath}\" --no-mtime --newline";
            }

            if (!string.IsNullOrEmpty(customOptions))
            {
                arguments += $" {customOptions}";
            }

            bool isPlaylist = chkPlaylist.Checked;
            arguments += isPlaylist ? " --yes-playlist" : " --no-playlist";

            if (!string.IsNullOrEmpty(outputPath))
            {
                try
                {
                    Directory.CreateDirectory(outputPath);
                }
                catch (Exception ex)
                {
                    ShowUserError("สร้างโฟลเดอร์ไม่ได้",
                        $"ไม่สามารถสร้างโฟลเดอร์:\n{outputPath}\n\nสาเหตุ: {ex.Message}",
                        "ลองเลือกโฟลเดอร์อื่น เช่น Desktop หรือ Downloads");
                    SetUIEnabled(true);
                    return;
                }

                arguments += isPlaylist
                    ? $" -o \"{Path.Combine(outputPath, "%(playlist_title)s", outputFilenameTemplate)}\""
                    : $" -o \"{Path.Combine(outputPath, outputFilenameTemplate)}\"";
            }

            arguments += $" \"{url}\"";

            UpdateStatus("กำลังรันคำสั่ง yt-dlp...", false);

            int exitCode = await Task.Run(() => RunYtDlp(arguments, _cts.Token));

            ShowFinalResult(exitCode);

            _cts?.Dispose();
            _cts = null;
            SetUIEnabled(true);
        }

        private void btnStop_Click(object? sender, EventArgs e)
        {
            btnStop.Enabled = false;
            UpdateStatus("กำลังหยุดการดาวน์โหลด...", true);

            _cts?.Cancel();

            lock (_processLock)
            {
                if (_activeProcess != null && !_activeProcess.HasExited)
                {
                    try
                    {
                        _activeProcess.Kill(true);
                    }
                    catch { }
                }
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var folderBrowserDialog = new FolderBrowserDialog();
            if (Directory.Exists(txtOutputPath.Text))
            {
                folderBrowserDialog.SelectedPath = txtOutputPath.Text;
            }
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtOutputPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private int RunYtDlp(string arguments, CancellationToken ct)
        {
            var process = new Process();
            process.StartInfo.FileName = ytDlpPath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UpdateStatus(e.Data, false);
                    UpdateProgressBar(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UpdateStatus(e.Data, true);

                    if (e.Data.Contains("ERROR:"))
                    {
                        lock (_errorLines) { _errorLines.Add(e.Data); }
                    }
                    else if (e.Data.Contains("WARNING:"))
                    {
                        lock (_warningLines) { _warningLines.Add(e.Data); }
                    }
                }
            };

            int exitCode = -1;
            try
            {
                process.Start();
                lock (_processLock) { _activeProcess = process; }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.WaitForExit(500))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try { if (!process.HasExited) process.Kill(true); } catch { }
                        return -2;
                    }
                }

                process.WaitForExit();
                exitCode = process.ExitCode;
            }
            catch (OperationCanceledException)
            {
                return -2;
            }
            catch (Exception ex)
            {
                UpdateStatus($"ไม่สามารถรัน yt-dlp ได้: {ex.Message}", true);
            }
            finally
            {
                lock (_processLock) { _activeProcess = null; }
                process.Dispose();
            }

            if (exitCode == 0 && _errorLines.Count == 0)
                return 0;

            return exitCode != 0 ? exitCode : 1;
        }

        private void ShowFinalResult(int exitCode)
        {
            if (exitCode == -2)
            {
                UpdateStatus("ผู้ใช้หยุดการดาวน์โหลดแล้ว", false);
                ModernDialog.ShowDialog(this, "หยุดการทำงาน", "การดาวน์โหลดถูกหยุดโดยผู้ใช้เรียบร้อยแล้ว", ModernDialogType.Information);
                return;
            }

            var result = ErrorParser.AnalyzeResult(exitCode, _errorLines, _warningLines);
            UpdateStatus(ErrorParser.FormatForUser(result), result.Severity >= ErrorParser.ErrorSeverity.Error);

            if (result.Severity >= ErrorParser.ErrorSeverity.Error)
            {
                ModernDialog.ShowDialog(
                    this,
                    result.Title,
                    result.Detail,
                    result.Severity == ErrorParser.ErrorSeverity.Fatal ? ModernDialogType.Error : ModernDialogType.Warning,
                    confirmText: "รับทราบ",
                    details: result.Detail,
                    suggestion: result.Suggestion);
            }
            else if (exitCode == 0)
            {
                UpdateStatus("ดาวน์โหลดเสร็จสมบูรณ์", false);
                progressBar.Value = 100;

                string outputPath = txtOutputPath.Text.Trim();
                if (Directory.Exists(outputPath))
                {
                    var openFolder = ModernDialog.ShowDialog(
                        this,
                        "ดาวน์โหลดเสร็จสมบูรณ์",
                        chkPlaylist.Checked
                            ? "ดาวน์โหลดเพลย์ลิสต์เสร็จเรียบร้อยแล้ว ต้องการเปิดโฟลเดอร์ดูไฟล์หรือไม่?"
                            : "ดาวน์โหลดไฟล์เสร็จเรียบร้อยแล้ว ต้องการเปิดโฟลเดอร์ดูไฟล์หรือไม่?",
                        ModernDialogType.Question,
                        confirmText: "เปิดโฟลเดอร์",
                        cancelText: "ปิด");

                    if (openFolder == DialogResult.Yes)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = $"\"{outputPath}\"",
                                UseShellExecute = true
                            });
                        }
                        catch { }
                    }
                }
                else
                {
                    ModernDialog.ShowDialog(this, "สำเร็จ", "ดาวน์โหลดเสร็จสมบูรณ์แล้ว", ModernDialogType.Information);
                }
            }
        }

        private void ShowUserError(string title, string detail, string suggestion)
        {
            var error = new ErrorParser.ParsedError(title, detail, suggestion, ErrorParser.ErrorSeverity.Error);
            UpdateStatus(ErrorParser.FormatForUser(error), true);
            ModernDialog.ShowDialog(this, title, detail, ModernDialogType.Error, confirmText: "รับทราบ", details: detail, suggestion: suggestion);
        }

        private void UpdateStatus(string message, bool isError)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, bool>(UpdateStatus), message, isError);
                return;
            }

            txtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtStatus.SelectionStart = txtStatus.Text.Length;
            txtStatus.ScrollToCaret();
        }

        private void UpdateProgressBar(string logLine)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateProgressBar), logLine);
                return;
            }

            if (logLine.Contains("[download]") && logLine.Contains("%"))
            {
                try
                {
                    int percentIndex = logLine.IndexOf('%');
                    int startIndex = logLine.LastIndexOf(' ', percentIndex - 1);
                    if (startIndex != -1)
                    {
                        string percentString = logLine.Substring(startIndex, percentIndex - startIndex).Trim();
                        if (float.TryParse(percentString, out float percent))
                        {
                            int progress = (int)percent;
                            if (progress >= 0 && progress <= 100)
                            {
                                progressBar.Value = progress;
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private void SetUIEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(SetUIEnabled), enabled);
                return;
            }

            txtUrl.Enabled = enabled;
            rbMp4.Enabled = enabled;
            rbMp3.Enabled = enabled;
            txtOutputPath.Enabled = enabled;
            btnBrowse.Enabled = enabled;
            txtCustomOptions.Enabled = enabled;
            btnDownload.Enabled = enabled;
            if (chkPlaylist != null) chkPlaylist.Enabled = enabled;
            if (btnStop != null) btnStop.Enabled = !enabled;
        }
    }
}