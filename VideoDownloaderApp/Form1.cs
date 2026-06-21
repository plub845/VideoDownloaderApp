using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoDownloaderApp
{
    public partial class Form1 : Form
    {
        // yt-dlp / ffmpeg ถูกดาวน์โหลดโดยตัว Installer (.iss) ไปไว้ที่
        // %LOCALAPPDATA%\VideoDownloaderApp\ ต้องชี้พาธให้ตรงกันเสมอ
        private static readonly string EngineDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VideoDownloaderApp");
        private readonly string ytDlpPath = Path.Combine(EngineDir, "yt-dlp.exe");
        private readonly string ffmpegPath = Path.Combine(EngineDir, "ffmpeg.exe");
        private CheckBox chkPlaylist;

        private void ApplyModernTheme()
        {
            this.BackColor = System.Drawing.Color.FromArgb(20, 20, 25);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.Size = new Size(900, 680);
            this.MinimumSize = new Size(900, 680);

            // 1. แบนเนอร์ด้านบน
            PictureBox picBanner = new PictureBox();
            picBanner.Dock = DockStyle.Top;
            picBanner.Height = 150;
            picBanner.SizeMode = PictureBoxSizeMode.StretchImage;
            string bannerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "banner.png");
            try
            {
                if (File.Exists(bannerPath))
                {
                    picBanner.Image = System.Drawing.Image.FromFile(bannerPath);
                }
                else
                {
                    Console.WriteLine($"[Theme] ไม่พบไฟล์ banner ที่ {bannerPath} (เช็ค .csproj ว่า copy โฟลเดอร์ Assets ไป output หรือยัง)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Theme] โหลด banner ไม่สำเร็จ: {ex.Message}");
            }
            this.Controls.Add(picBanner);

            // 2. จัดตำแหน่งคอนโทรล (ซ้าย: ฟอร์ม, ขวา: Log)
            label1.Text = "🎵 Video URL:";
            label1.Location = new Point(20, 170);
            label1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);

            txtUrl.Location = new Point(20, 195);
            txtUrl.Size = new Size(420, 30);

            chkPlaylist.Location = new Point(20, 235);
            chkPlaylist.AutoSize = true;

            label2.Location = new Point(20, 275);
            label2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);

            grpOutputFormat.Location = new Point(20, 300);
            grpOutputFormat.Size = new Size(420, 65);
            rbMp4.Location = new Point(30, 25);
            rbMp3.Location = new Point(180, 25);

            label3.Location = new Point(20, 380);
            label3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);

            txtOutputPath.Location = new Point(20, 405);
            txtOutputPath.Size = new Size(310, 30);

            btnBrowse.Location = new Point(340, 404);
            btnBrowse.Size = new Size(100, 32);
            btnBrowse.Text = "📁 Browse";

            label4.Location = new Point(20, 445);

            txtCustomOptions.Location = new Point(20, 470);
            txtCustomOptions.Size = new Size(420, 30);

            progressBar.Location = new Point(20, 520);
            progressBar.Size = new Size(420, 20);

            btnDownload.Location = new Point(20, 555);
            btnDownload.Size = new Size(420, 55);
            btnDownload.Text = " DOWNLOAD NOW";
            btnDownload.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            btnDownload.TextImageRelation = TextImageRelation.ImageBeforeText;

            string iconBtnPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "download_icon.png");
            try
            {
                if (File.Exists(iconBtnPath))
                {
                    using (System.Drawing.Image rawIcon = System.Drawing.Image.FromFile(iconBtnPath))
                    {
                        btnDownload.Image = new System.Drawing.Bitmap(rawIcon, new Size(32, 32));
                    }
                }
                else
                {
                    Console.WriteLine($"[Theme] ไม่พบไฟล์ icon ที่ {iconBtnPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Theme] โหลด download_icon ไม่สำเร็จ: {ex.Message}");
            }

            txtStatus.Location = new Point(460, 170);
            txtStatus.Size = new Size(400, 440);

            // 3. ปรับสีสัน
            foreach (Control c in this.Controls)
            {
                if (c is TextBox)
                {
                    c.BackColor = System.Drawing.Color.FromArgb(40, 42, 50);
                    c.ForeColor = System.Drawing.Color.Cyan;
                    ((TextBox)c).BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is Button)
                {
                    Button btn = (Button)c;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    if (btn.Name == "btnDownload")
                    {
                        btn.BackColor = System.Drawing.Color.FromArgb(239, 45, 86); // Vibrant pinkish-red
                        btn.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        btn.BackColor = System.Drawing.Color.FromArgb(56, 110, 204); // Vibrant Blue
                        btn.ForeColor = System.Drawing.Color.White;
                    }
                    btn.Cursor = Cursors.Hand;
                }
                else if (c is Label)
                {
                    c.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
                }
            }

            grpOutputFormat.ForeColor = System.Drawing.Color.Orange;
            rbMp4.ForeColor = System.Drawing.Color.White;
            rbMp3.ForeColor = System.Drawing.Color.White;
            chkPlaylist.ForeColor = System.Drawing.Color.SpringGreen;

            txtStatus.BackColor = System.Drawing.Color.FromArgb(15, 15, 20);
            txtStatus.ForeColor = System.Drawing.Color.LimeGreen;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        public Form1()
        {
            InitializeComponent();

            this.Text = "Video Downloader Pro v1.0.2";

            // บังคับใช้ไอคอน VDapp.icon.ico
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VDapp.icon.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new System.Drawing.Icon(iconPath);
                }
                else if (File.Exists("VDapp.icon.ico"))
                {
                    this.Icon = new System.Drawing.Icon("VDapp.icon.ico");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Init] โหลดไอคอนแอปไม่สำเร็จ: {ex.Message}");
            }

            // CheckBox เลือกว่าจะโหลดเพลย์ลิสต์หรือไม่
            chkPlaylist = new CheckBox();
            chkPlaylist.Text = " ดาวน์โหลดแบบ Playlist (สร้างโฟลเดอร์ให้อัตโนมัติ)";
            chkPlaylist.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.Controls.Add(chkPlaylist);

            // Default output folder = Downloads
            txtOutputPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            ApplyModernTheme();
        }

        // yt-dlp จัดการ query string เช่น ?list=... ได้เอง ไม่ต้องตัดออก
        private string CleanYoutubeUrl(string url)
        {
            return url.Trim();
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();

            string url = CleanYoutubeUrl(txtUrl.Text.Trim());
            txtUrl.Text = url;

            string outputFormat = rbMp4.Checked ? "mp4" : "mp3";
            string outputPath = txtOutputPath.Text.Trim();
            string customOptions = txtCustomOptions.Text.Trim();

            if (string.IsNullOrEmpty(url))
            {
                UpdateStatus("กรุณาป้อน URL ของวิดีโอ", true);
                return;
            }

            if (!File.Exists(ytDlpPath))
            {
                UpdateStatus($"ไม่พบ yt-dlp.exe ที่ {ytDlpPath}\nลองเปิดโปรแกรมใหม่ (ติดตั้งใหม่) หรือดาวน์โหลด yt-dlp.exe มาวางในโฟลเดอร์นี้เอง", true);
                return;
            }

            // mp4 ก็ต้องใช้ ffmpeg merge เสียง+วิดีโอด้วยเหมือนกัน ไม่ใช่แค่ mp3
            if (!File.Exists(ffmpegPath))
            {
                UpdateStatus($"ไม่พบ ffmpeg.exe ที่ {ffmpegPath}\nจำเป็นสำหรับทั้งโหมด MP4 (merge เสียง/วิดีโอ) และ MP3 (แปลงไฟล์)", true);
                return;
            }

            SetUIEnabled(false);
            UpdateStatus("เริ่มต้นดาวน์โหลด...", false);
            progressBar.Value = 0;

            string outputFilenameTemplate = "%(upload_date)s_%(title)s.%(ext)s";
            string arguments = "";

            if (outputFormat == "mp3")
            {
                arguments = $"--extract-audio --audio-format mp3 --audio-quality 0 --ffmpeg-location \"{ffmpegPath}\" --no-mtime";
            }
            else
            {
                arguments = $"-f bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best --ffmpeg-location \"{ffmpegPath}\" --no-mtime";
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
                    UpdateStatus($"ไม่สามารถสร้างโฟลเดอร์ปลายทางได้: {ex.Message}", true);
                    SetUIEnabled(true);
                    return;
                }

                arguments += isPlaylist
                    ? $" -o \"{Path.Combine(outputPath, "%(playlist_title)s", outputFilenameTemplate)}\""
                    : $" -o \"{Path.Combine(outputPath, outputFilenameTemplate)}\"";
            }
            else
            {
                arguments += isPlaylist
                    ? $" -o \"%(playlist_title)s\\{outputFilenameTemplate}\""
                    : $" -o \"{outputFilenameTemplate}\"";
            }

            arguments += $" \"{url}\"";

            UpdateStatus($"กำลังรันคำสั่ง: {ytDlpPath} {arguments}", false);

            int exitCode = await Task.Run(() => RunYtDlp(arguments));

            ShowFinalResult(exitCode);

            SetUIEnabled(true);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (Directory.Exists(txtOutputPath.Text))
                {
                    folderBrowserDialog.SelectedPath = txtOutputPath.Text;
                }
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        // คืนค่า exit code ของ yt-dlp (0 = สำเร็จ) เพื่อใช้ตัดสินผลลัพธ์ที่แม่นยำกว่าการเดาจาก log text
        private int RunYtDlp(string arguments)
        {
            System.Collections.Generic.List<string> realErrors = new System.Collections.Generic.List<string>();
            Process process = new Process();
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
                    // นับเฉพาะ ERROR: จริงๆ ว่าล้มเหลว ส่วน WARNING: ปกติของ yt-dlp ไม่ถือว่า fail
                    if (e.Data.Contains("ERROR:"))
                    {
                        lock (realErrors) { realErrors.Add(e.Data); }
                    }
                }
            };

            int exitCode = -1;
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }
            catch (Exception ex)
            {
                UpdateStatus($"เกิดข้อผิดพลาดในการรัน yt-dlp: {ex.Message}", true);
            }
            finally
            {
                process.Dispose();
            }

            // ใช้ exit code เป็นตัวตัดสินหลัก ถ้า exit code = 0 แต่ดันมี ERROR: หลุดมาก็ยังถือว่าไม่ผ่าน
            if (exitCode == 0 && realErrors.Count == 0)
            {
                return 0;
            }
            return exitCode != 0 ? exitCode : 1;
        }

        private void ShowFinalResult(int exitCode)
        {
            if (exitCode == 0)
            {
                UpdateStatus("✅ ดาวน์โหลดเสร็จสมบูรณ์!", false);
                if (chkPlaylist.Checked)
                {
                    MessageBox.Show("ดาวน์โหลดเพลย์ลิสต์เสร็จสมบูรณ์แล้ว!", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                UpdateStatus("❌ ดาวน์โหลดไม่สำเร็จ กรุณาดู Log ด้านบน", true);
                MessageBox.Show("มีข้อผิดพลาดระหว่างดาวน์โหลด กรุณาตรวจสอบ Log ด้านขวาของโปรแกรม", "แจ้งเตือนข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateStatus(string message, bool isError)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, bool>(UpdateStatus), message, isError);
                return;
            }

            txtStatus.AppendText(message + Environment.NewLine);
            txtStatus.SelectionStart = txtStatus.Text.Length;
            txtStatus.ScrollToCaret();
        }

        private void UpdateProgressBar(string line)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateProgressBar), line);
                return;
            }

            if (line.Contains("[download]") && line.Contains("%"))
            {
                try
                {
                    int startIndex = line.IndexOf("[download]") + "[download]".Length;
                    string progressPart = line.Substring(startIndex).Trim();
                    int percentIndex = progressPart.IndexOf('%');
                    if (percentIndex != -1)
                    {
                        string percentString = progressPart.Substring(0, percentIndex).Trim();
                        if (percentString.Contains(","))
                            percentString = percentString.Replace(",", ".");
                        if (double.TryParse(percentString, out double percentage))
                        {
                            progressBar.Value = Math.Min(100, (int)percentage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing progress: {ex.Message} - Line: {line}");
                }
            }
            else if (line.Contains("[ExtractAudio]") || line.Contains("[Merger]"))
            {
                progressBar.Value = 100;
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
        }
    }
}