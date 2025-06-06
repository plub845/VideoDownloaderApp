using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoDownloaderApp
{
    public partial class Form1 : Form
    {
        // Paths to yt-dlp and ffmpeg (assumed to be in same folder as .exe)
        private string ytDlpPath = "yt-dlp.exe";
        private string ffmpegPath = "ffmpeg.exe";

        public Form1()
        {
            InitializeComponent();
            // Default output folder = Downloads
            txtOutputPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        // Clean URL: remove unnecessary query parameters etc.
        private string CleanYoutubeUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
            }
            catch
            {
                // If URL invalid, return original
                return url;
            }
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
                UpdateStatus($"ไม่พบ {ytDlpPath} กรุณาวางไฟล์ในโฟลเดอร์เดียวกับโปรแกรม หรือระบุพาธเต็ม", true);
                return;
            }

            if (outputFormat == "mp3" && !File.Exists(ffmpegPath))
            {
                UpdateStatus($"ไม่พบ {ffmpegPath} ซึ่งจำเป็นสำหรับการแปลงไฟล์ mp3", true);
                return;
            }

            SetUIEnabled(false);
            UpdateStatus("เริ่มต้นดาวน์โหลด...", false);
            progressBar.Value = 0;

            string outputFilenameTemplate = "%(upload_date)s_%(title)s.%(ext)s";
            string arguments = "";

            if (outputFormat == "mp3")
            {
                // Extract audio as mp3, no keep original video file (-k) removed
                arguments = $"--extract-audio --audio-format mp3 --audio-quality 0 --ffmpeg-location \"{ffmpegPath}\" --no-mtime";
            }
            else
            {
                // Download best video+audio mp4 or fallback best mp4
                arguments = $"-f bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best --ffmpeg-location \"{ffmpegPath}\" --no-mtime";
            }

            if (!string.IsNullOrEmpty(customOptions))
            {
                arguments += $" {customOptions}";
            }

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
                arguments += $" -o \"{Path.Combine(outputPath, outputFilenameTemplate)}\"";
            }
            else
            {
                arguments += $" -o \"{outputFilenameTemplate}\"";
            }

            arguments += $" \"{url}\"";

            UpdateStatus($"กำลังรันคำสั่ง: {ytDlpPath} {arguments}", false);

            await Task.Run(() => RunYtDlp(arguments));

            if (outputFormat == "mp3")
            {
                try
                {
                    string[] mp3Files = Directory.GetFiles(outputPath, "*.mp3", SearchOption.TopDirectoryOnly);
                    if (mp3Files.Length == 0)
                    {
                        UpdateStatus("⚠️ ไม่พบไฟล์ .mp3 ที่ถูกสร้างขึ้น", true);
                    }
                    else
                    {
                        UpdateStatus($"✅ พบไฟล์ .mp3: {Path.GetFileName(mp3Files[0])}", false);
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"เกิดข้อผิดพลาดในการตรวจสอบไฟล์ mp3: {ex.Message}", true);
                }
            }

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

        private void RunYtDlp(string arguments)
        {
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
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                UpdateStatus($"เกิดข้อผิดพลาดในการรัน yt-dlp: {ex.Message}", true);
            }
            finally
            {
                UpdateStatus("ดาวน์โหลดเสร็จสิ้น หรือมีข้อผิดพลาด โปรดตรวจสอบ Log.", false);
                process.Dispose();
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

            // ถ้าใช้ RichTextBox สามารถใส่สีข้อความได้
            // if (isError)
            // {
            //     txtStatus.SelectionColor = Color.Red;
            //     txtStatus.AppendText(message + Environment.NewLine);
            //     txtStatus.SelectionColor = txtStatus.ForeColor;
            // }
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
        }
    }
}
