using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VideoDownloaderApp.Linux
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
            bool IsAvailable, string CurrentVersion, string LatestVersion,
            string DownloadUrl, string ReleaseNotes, string HtmlUrl, string ErrorMessage);

        public static async Task<UpdateInfo> CheckForUpdateAsync()
        {
            string currentVer = CurrentVersion;
            string latestVersion = "";
            string releaseNotes = "";
            string htmlUrl = "";
            string downloadUrl = "";

            try
            {
                var response = await _httpClient.GetStringAsync(GITHUB_API_URL);
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                string tagName = root.GetProperty("tag_name").GetString() ?? "";
                latestVersion = tagName.TrimStart('v', 'V');
                releaseNotes = root.TryGetProperty("body", out var b) ? (b.GetString() ?? "") : "";
                htmlUrl = root.TryGetProperty("html_url", out var h) ? (h.GetString() ?? "") : "";

                if (root.TryGetProperty("assets", out var assets))
                {
                    foreach (var asset in assets.EnumerateArray())
                    {
                        string name = asset.GetProperty("name").GetString() ?? "";
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
            catch
            {
                // Fallback to tags if releases/latest fails
            }

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
                        downloadUrl = $"https://github.com/plub845/VideoDownloaderApp/releases/download/{tagName}/install.sh";
                    }
                }
                catch (Exception ex)
                {
                    return new UpdateInfo(false, currentVer, "", "", "", "", $"ไม่สามารถเชื่อมต่อกับ GitHub ได้: {ex.Message}");
                }
            }

            if (string.IsNullOrEmpty(latestVersion))
            {
                return new UpdateInfo(false, currentVer, "", "", "", "", "ไม่พบข้อมูลเวอร์ชันบน GitHub");
            }

            if (string.IsNullOrEmpty(downloadUrl)) downloadUrl = !string.IsNullOrEmpty(htmlUrl) ? htmlUrl : "https://github.com/plub845/VideoDownloaderApp/releases";
            bool isNewer = IsNewerVersion(currentVer, latestVersion);

            return new UpdateInfo(isNewer, currentVer, latestVersion, downloadUrl, releaseNotes, htmlUrl, "");
        }

        public static void OpenReleasePage(string url)
        {
            try
            {
                Process.Start("xdg-open", url);
            }
            catch
            {
                try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
            }
        }

        public static async Task<(bool Success, string Message)> UpdateYtDlpAsync()
        {
            string ytDlpPath = FindExecutableInPath("yt-dlp");
            if (string.IsNullOrEmpty(ytDlpPath))
            {
                return (false, "ไม่พบ yt-dlp ใน system PATH\nติดตั้ง: pip install -U yt-dlp");
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

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                string combined = (output + "\n" + error).Trim();

                if (process.ExitCode == 0)
                {
                    if (combined.Contains("up to date", StringComparison.OrdinalIgnoreCase) ||
                        combined.Contains("is up-to-date", StringComparison.OrdinalIgnoreCase))
                        return (true, "yt-dlp เป็นเวอร์ชันล่าสุดแล้ว");
                    return (true, $"อัปเดต yt-dlp สำเร็จ\n{combined}");
                }
                return (false, $"อัปเดต yt-dlp ล้มเหลว (exit code {process.ExitCode}):\n{combined}");
            }
            catch (Exception ex)
            {
                return (false, $"เกิดข้อผิดพลาด: {ex.Message}\nลอง: pip install -U yt-dlp");
            }
        }

        private static string FindExecutableInPath(string executableName)
        {
            var pathValue = Environment.GetEnvironmentVariable("PATH") ?? "";
            var searchPaths = new List<string>(pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries));

            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(homeDir))
            {
                searchPaths.Add(Path.Combine(homeDir, ".local", "bin"));
            }
            searchPaths.Add("/usr/local/bin");
            searchPaths.Add("/usr/bin");
            searchPaths.Add("/bin");
            searchPaths.Add("/opt/io.github.plub845.VideoDownloaderApp/tools");
            searchPaths.Add(AppDomain.CurrentDomain.BaseDirectory);

            foreach (var directory in searchPaths)
            {
                if (string.IsNullOrWhiteSpace(directory)) continue;
                var candidate = Path.Combine(directory, executableName);
                if (File.Exists(candidate)) return candidate;
            }
            return "";
        }

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
            catch { return false; }
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
