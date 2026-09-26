using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VideoDownloaderApp.Linux;

public enum DownloadFormat { Mp4, Mp3 }
public sealed record DownloadResult(int ExitCode, bool WasStopped, List<string> ErrorLines, List<string> WarningLines);

public sealed partial class DownloadEngine : IDisposable
{
    private readonly object _processLock = new();
    private Process? _activeProcess;
    private bool _stopRequested;

    // Error tracking สำหรับ error parsing ที่ชาญฉลาด
    private readonly List<string> _errorLines = new();
    private readonly List<string> _warningLines = new();

    public event Action<string>? LogReceived;
    public event Action<double>? ProgressChanged;

    public async Task<DownloadResult> DownloadAsync(
        string url, string outputDirectory, DownloadFormat format, string customOptions,
        CancellationToken cancellationToken = default)
    {
        lock (_processLock)
        {
            if (_activeProcess is not null)
                throw new InvalidOperationException("มีการดาวน์โหลดกำลังทำงานอยู่");
            _stopRequested = false;
        }

        _errorLines.Clear();
        _warningLines.Clear();

        var startInfo = BuildStartInfo(url, outputDirectory, format, customOptions);
        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        lock (_processLock) _activeProcess = process;

        try
        {
            LogReceived?.Invoke($"🔧 กำลังรัน: {BuildDisplayCommand(startInfo)}");
            if (!process.Start()) throw new InvalidOperationException("ไม่สามารถเริ่ม process yt-dlp ได้");

            using var registration = cancellationToken.Register(Stop);
            var stdoutTask = ReadStreamAsync(process.StandardOutput, cancellationToken);
            var stderrTask = ReadStreamAsync(process.StandardError, cancellationToken);
            await process.WaitForExitAsync(CancellationToken.None);
            await Task.WhenAll(stdoutTask, stderrTask);
            return new DownloadResult(process.ExitCode, _stopRequested,
                new List<string>(_errorLines), new List<string>(_warningLines));
        }
        catch (OperationCanceledException) when (_stopRequested || cancellationToken.IsCancellationRequested)
        {
            return new DownloadResult(-1, true, new List<string>(_errorLines), new List<string>(_warningLines));
        }
        finally
        {
            lock (_processLock)
            {
                if (ReferenceEquals(_activeProcess, process)) _activeProcess = null;
            }
        }
    }

    public void Stop()
    {
        Process? process;
        lock (_processLock)
        {
            _stopRequested = true;
            process = _activeProcess;
        }

        if (process is null) return;
        try
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException) { }
        catch (Exception ex) { LogReceived?.Invoke($"[ERROR] หยุด process ไม่สำเร็จ: {ex.Message}"); }
    }

    private static ProcessStartInfo BuildStartInfo(
        string url, string outputDirectory, DownloadFormat format, string customOptions)
    {
        var ytDlpPath = FindExecutableInPath("yt-dlp");
        var ffmpegPath = FindExecutableInPath("ffmpeg");

        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("--newline");
        foreach (var argument in ParseArguments(customOptions)) startInfo.ArgumentList.Add(argument);

        if (format == DownloadFormat.Mp4)
        {
            AddArguments(startInfo,
                "-f", "bv*[vcodec^=avc1]+ba[acodec^=mp4a]/b[ext=mp4]/bv*+ba/b",
                "--merge-output-format", "mp4",
                "--recode-video", "mp4",
                "--no-keep-video");
        }
        else
        {
            AddArguments(startInfo,
                "-f", "ba/b",
                "-x",
                "--audio-format", "mp3",
                "--audio-quality", "0",
                "--no-keep-video");
        }

        AddArguments(startInfo, "--ffmpeg-location", ffmpegPath, "-o",
            Path.Combine(outputDirectory, "%(title)s.%(ext)s"));
        startInfo.ArgumentList.Add(url);
        return startInfo;
    }

    private static string FindExecutableInPath(string executableName)
    {
        var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
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

        throw new FileNotFoundException($"ไม่พบ {executableName} ในระบบ\n\nแนวทางแก้ไข:\n" +
            (executableName == "yt-dlp"
                ? "ติดตั้ง yt-dlp: pip install -U yt-dlp\nหรือ: sudo apt install yt-dlp"
                : "ติดตั้ง ffmpeg: sudo apt install ffmpeg"));
    }

    private static void AddArguments(ProcessStartInfo startInfo, params string[] arguments)
    {
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);
    }

    private async Task ReadStreamAsync(System.IO.StreamReader reader, CancellationToken cancellationToken)
    {
        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;
            LogReceived?.Invoke(line);
            TryReportProgress(line);

            // Track errors and warnings สำหรับ error parsing
            if (line.Contains("ERROR:", StringComparison.Ordinal))
            {
                lock (_errorLines) { _errorLines.Add(line); }
            }
            else if (line.Contains("WARNING:", StringComparison.Ordinal))
            {
                lock (_warningLines) { _warningLines.Add(line); }
            }
        }
    }

    private void TryReportProgress(string line)
    {
        var match = ProgressRegex().Match(line);
        if (!match.Success)
        {
            if (line.Contains("[Merger]", StringComparison.Ordinal) ||
                line.Contains("[ExtractAudio]", StringComparison.Ordinal))
                ProgressChanged?.Invoke(100);
            return;
        }

        if (double.TryParse(match.Groups[1].Value, NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var progress))
            ProgressChanged?.Invoke(Math.Clamp(progress, 0, 100));
    }

    private static IReadOnlyList<string> ParseArguments(string commandLine)
    {
        var arguments = new List<string>();
        var current = new StringBuilder();
        var inSingleQuotes = false;
        var inDoubleQuotes = false;
        var escaping = false;

        foreach (var character in commandLine)
        {
            if (escaping) { current.Append(character); escaping = false; continue; }
            if (character == '\\' && !inSingleQuotes) { escaping = true; continue; }
            if (character == '\'' && !inDoubleQuotes) { inSingleQuotes = !inSingleQuotes; continue; }
            if (character == '"' && !inSingleQuotes) { inDoubleQuotes = !inDoubleQuotes; continue; }
            if (char.IsWhiteSpace(character) && !inSingleQuotes && !inDoubleQuotes)
            {
                if (current.Length > 0) { arguments.Add(current.ToString()); current.Clear(); }
                continue;
            }
            current.Append(character);
        }

        if (escaping) current.Append('\\');
        if (inSingleQuotes || inDoubleQuotes)
            throw new ArgumentException("Custom Options มีเครื่องหมาย quote ที่ปิดไม่ครบ");
        if (current.Length > 0) arguments.Add(current.ToString());
        return arguments;
    }

    private static string BuildDisplayCommand(ProcessStartInfo startInfo)
    {
        var command = new StringBuilder(startInfo.FileName);
        foreach (var argument in startInfo.ArgumentList)
        {
            command.Append(" \"");
            command.Append(argument.Replace("\"", "\\\"", StringComparison.Ordinal));
            command.Append('"');
        }
        return command.ToString();
    }

    public void Dispose() { Stop(); GC.SuppressFinalize(this); }

    [GeneratedRegex(@"\[download\]\s+(\d+(?:\.\d+)?)%", RegexOptions.CultureInvariant)]
    private static partial Regex ProgressRegex();
}
