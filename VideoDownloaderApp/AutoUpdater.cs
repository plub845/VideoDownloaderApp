using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VideoDownloaderApp
{
    /// <summary>
    /// ตรวจสอบอัปเดตจาก GitHub Releases โดยอัตโนมัติ
    /// รองรับทั้ง Windows และ Linux
    /// </summary>
    public sealed class AutoUpdater
    {
        private const string GITHUB_API_URL = "https://api.github.com/repos/plub845/VideoDownloaderApp/releases/latest";
        private const string GITHUB_TAGS_URL = "https://api.github.com/repos/plub845/VideoDownloaderApp/tags";
        public const string DEFAULT_VERSION = "1.1.0";

        public static string CurrentVersion => GetAppVersion();

        private static string GetAppVersion()
        {
            var ver = typeof(AutoUpdater).Assembly.GetName().Version;
            if (ver != null && !(ver.Major == 1 && ver.Minor == 0 && ver.Build == 0 && ver.Revision == 0))
            {
                return $"{ver.Major}.{ver.Minor}.{ver.Build}";
            }
            return DEFAULT_VERSION;
        }

        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        static AutoUpdater()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "VideoDownloaderApp-AutoUpdater");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        }

        public sealed record UpdateInfo(
            bool IsAvailable,
            string CurrentVersion,
            string LatestVersion,
            string DownloadUrl,
            string ReleaseNotes,
            string HtmlUrl,
            string ErrorMessage
        );

        /// <summary>
        /// ตรวจสอบว่ามีเวอร์ชันใหม่หรือไม่ (ตรวจทั้ง Releases และ Tags)
        /// </summary>
        public static async Task<UpdateInfo> CheckForUpdateAsync()
        {
            string currentVer = CurrentVersion;
            string latestVersion = "";
            string releaseNotes = "";
            string htmlUrl = "";
            string downloadUrl = "";

            // 1. ลองอ่านจาก releases/latest ก่อน
            try
            {
                var response = await _httpClient.GetStringAsync(GITHUB_API_URL);
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                string tagName = root.GetProperty("tag_name").GetString() ?? "";
                latestVersion = tagName.TrimStart('v', 'V');
                releaseNotes = root.TryGetProperty("body", out var b) ? (b.GetString() ?? "") : "ไม่มีรายละเอียด";
                htmlUrl = root.TryGetProperty("html_url", out var h) ? (h.GetString() ?? "") : "";

                if (root.TryGetProperty("assets", out var assets))
                {
                    foreach (var asset in assets.EnumerateArray())
                    {
                        string name = asset.GetProperty("name").GetString() ?? "";
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            if (name.Contains("Installer", StringComparison.OrdinalIgnoreCase) &&
                                name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            {
                                downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                                break;
                            }
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            if (name.EndsWith(".deb", StringComparison.OrdinalIgnoreCase) ||
                                name.EndsWith(".AppImage", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains("install.sh", StringComparison.OrdinalIgnoreCase))
                            {
                                downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                // ถ้าดึง releases/latest ไม่ได้ ให้ลอง fallback ไปที่ tags
            }

            // 2. ถ้ายังไม่ได้ version หรือไม่มี release ให้ดูจาก tags โดยตรง
            if (string.IsNullOrEmpty(latestVersion))
            {
                try
                {
                    var tagsResponse = await _httpClient.GetStringAsync(GITHUB_TAGS_URL);
                    using var tagsDoc = JsonDocument.Parse(tagsResponse);
                    if (tagsDoc.RootElement.ValueKind == JsonValueKind.Array && tagsDoc.RootElement.GetArrayLength() > 0)
                    {
                        var firstTag = tagsDoc.RootElement[0];
                        string tagName = firstTag.GetProperty("name").GetString() ?? "";
                        latestVersion = tagName.TrimStart('v', 'V');
                        htmlUrl = $"https://github.com/plub845/VideoDownloaderApp/releases/tag/{tagName}";
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            downloadUrl = $"https://github.com/plub845/VideoDownloaderApp/releases/download/{tagName}/VideoDownloaderApp_Installer.exe";
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            downloadUrl = $"https://github.com/plub845/VideoDownloaderApp/releases/download/{tagName}/install.sh";
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new UpdateInfo(false, currentVer, "", "", "", "", $"ไม่สามารถตรวจสอบเวอร์ชันจาก GitHub ได้: {ex.Message}");
                }
            }

            if (string.IsNullOrEmpty(latestVersion))
            {
                return new UpdateInfo(false, currentVer, "", "", "", "", "ไม่พบข้อมูลเวอร์ชันบน GitHub");
            }

            if (string.IsNullOrEmpty(downloadUrl))
                downloadUrl = !string.IsNullOrEmpty(htmlUrl) ? htmlUrl : "https://github.com/plub845/VideoDownloaderApp/releases";

            bool isNewer = IsNewerVersion(currentVer, latestVersion);
            return new UpdateInfo(
                IsAvailable: isNewer,
                CurrentVersion: currentVer,
                LatestVersion: latestVersion,
                DownloadUrl: downloadUrl,
                ReleaseNotes: releaseNotes,
                HtmlUrl: htmlUrl,
                ErrorMessage: ""
            );
        }

        /// <summary>
        /// ดาวน์โหลดตัวอัปเดตแล้วรัน (สำหรับ Windows)
        /// </summary>
        public static async Task<(bool Success, string Message)> DownloadAndInstallAsync(
            string downloadUrl, Action<int>? onProgress = null)
        {
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "VideoDownloaderApp_Update");
                Directory.CreateDirectory(tempDir);

                string rawName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
                if (string.IsNullOrEmpty(rawName)) rawName = "VideoDownloaderApp_Installer.exe";
                string baseName = Path.GetFileNameWithoutExtension(rawName);
                string ext = Path.GetExtension(rawName);
                if (string.IsNullOrEmpty(ext)) ext = ".exe";

                string uniqueFileName = $"{baseName}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                string filePath = Path.Combine(tempDir, uniqueFileName);

                using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long? totalBytes = response.Content.Headers.ContentLength;
                    long downloadedBytes = 0;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 16384, true))
                    {
                        var buffer = new byte[16384];
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;
                            if (totalBytes.HasValue && totalBytes.Value > 0)
                            {
                                int progress = (int)(downloadedBytes * 100 / totalBytes.Value);
                                onProgress?.Invoke(progress);
                            }
                        }

                        await fileStream.FlushAsync();
                    }
                }

                // รอให้ OS ปลดการล็อกไฟล์อย่างสมบูรณ์
                await Task.Delay(150);

                if (!File.Exists(filePath))
                {
                    return (false, "ไม่พบไฟล์ตัวติดตั้งหลังดาวน์โหลดเสร็จสิ้น");
                }

                // รันตัวติดตั้งในโฟลเดอร์ temp เพื่อไม่ให้ล็อก directory ของแอป
                var startInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true,
                    WorkingDirectory = tempDir
                };

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    startInfo.Verb = "runas";
                }

                Process.Start(startInfo);

                return (true, "กำลังเปิดตัวติดตั้ง กรุณาปิดโปรแกรมเพื่อดำเนินการอัปเดต");
            }
            catch (Exception ex)
            {
                return (false, $"ดาวน์โหลดอัปเดตล้มเหลว: {ex.Message}");
            }
        }

        /// <summary>
        /// เปิดหน้า Release บน GitHub ในเบราว์เซอร์
        /// </summary>
        public static void OpenReleasePage(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AutoUpdater] ไม่สามารถเปิดเบราว์เซอร์ได้: {ex.Message}");
            }
        }

        /// <summary>
        /// อัปเดต yt-dlp เป็นเวอร์ชันล่าสุดโดยตรง (yt-dlp --update)
        /// </summary>
        public static async Task<(bool Success, string Message)> UpdateYtDlpAsync(string ytDlpPath)
        {
            if (!File.Exists(ytDlpPath))
            {
                return (false, $"ไม่พบ yt-dlp ที่ {ytDlpPath}");
            }

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ytDlpPath,
                        Arguments = "--update",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                string output = "";
                string error = "";

                process.Start();
                output = await process.StandardOutput.ReadToEndAsync();
                error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                string combined = output + "\n" + error;

                if (process.ExitCode == 0)
                {
                    if (combined.Contains("up to date", StringComparison.OrdinalIgnoreCase) ||
                        combined.Contains("is up-to-date", StringComparison.OrdinalIgnoreCase))
                    {
                        return (true, "yt-dlp เป็นเวอร์ชันล่าสุดแล้ว");
                    }
                    else if (combined.Contains("Updated", StringComparison.OrdinalIgnoreCase) ||
                             combined.Contains("Updating to", StringComparison.OrdinalIgnoreCase))
                    {
                        return (true, "อัปเดต yt-dlp สำเร็จ");
                    }
                    return (true, $"yt-dlp อัปเดตเสร็จสิ้น\n{combined.Trim()}");
                }
                else
                {
                    return (false, $"อัปเดต yt-dlp ล้มเหลว (exit code {process.ExitCode}):\n{combined.Trim()}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"เกิดข้อผิดพลาดในการอัปเดต yt-dlp: {ex.Message}");
            }
        }

        /// <summary>
        /// เปรียบเทียบเวอร์ชัน semver
        /// </summary>
        private static bool IsNewerVersion(string currentVersion, string latestVersion)
        {
            if (string.IsNullOrWhiteSpace(latestVersion)) return false;

            try
            {
                var current = ParseVersion(currentVersion);
                var latest = ParseVersion(latestVersion);

                if (latest.major > current.major) return true;
                if (latest.major == current.major && latest.minor > current.minor) return true;
                if (latest.major == current.major && latest.minor == current.minor && latest.patch > current.patch) return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static (int major, int minor, int patch) ParseVersion(string version)
        {
            version = version.TrimStart('v', 'V');
            var parts = version.Split('.');
            int major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
            int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
            int patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;
            return (major, minor, patch);
        }
    }
}
