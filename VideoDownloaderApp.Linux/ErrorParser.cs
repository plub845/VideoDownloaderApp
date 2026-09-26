using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VideoDownloaderApp.Linux
{
    /// <summary>
    /// แปลง Error จาก yt-dlp ให้เป็นข้อความที่ผู้ใช้เข้าใจง่าย
    /// ไม่โยนให้ผู้ใช้ไปดู log เอง — แอปต้องรู้และบอกให้ชัด
    /// </summary>
    public static class ErrorParser
    {
        public sealed record ParsedError(string Title, string Detail, string Suggestion, ErrorSeverity Severity);

        public enum ErrorSeverity { Info, Warning, Error, Fatal }

        private static readonly List<(Regex Pattern, Func<Match, ParsedError> Handler)> Rules = new()
        {
            (new Regex(@"ERROR:.*Video unavailable", RegexOptions.IgnoreCase),
             m => new ParsedError("วิดีโอไม่สามารถเข้าถึงได้",
                 "วิดีโอนี้อาจถูกลบ ตั้งเป็นส่วนตัว หรือจำกัดสิทธิ์การเข้าถึง",
                 "ลองตรวจสอบลิงก์อีกครั้ง หรือลองเปิดลิงก์ในเบราว์เซอร์", ErrorSeverity.Error)),

            (new Regex(@"ERROR:.*Unsupported URL", RegexOptions.IgnoreCase),
             m => new ParsedError("ลิงก์ไม่รองรับ",
                 "yt-dlp ไม่รู้จักลิงก์รูปแบบนี้",
                 "ลองใช้ลิงก์จาก YouTube, Facebook, Twitter หรือเว็บที่ yt-dlp รองรับ", ErrorSeverity.Error)),

            (new Regex(@"ERROR:.*ffmpeg.*not found|ffmpeg.*is not installed", RegexOptions.IgnoreCase),
             m => new ParsedError("ไม่พบ ffmpeg",
                 "โปรแกรมต้องใช้ ffmpeg ในการรวมไฟล์ภาพกับเสียง และแปลงไฟล์เป็น MP3",
                 "ติดตั้ง ffmpeg ด้วย: sudo apt install ffmpeg", ErrorSeverity.Fatal)),

            (new Regex(@"ERROR:.*Requested format.*not available", RegexOptions.IgnoreCase),
             m => new ParsedError("ไม่มีคุณภาพวิดีโอที่เลือก",
                 "ไม่พบรูปแบบไฟล์ที่ขอบนเซิร์ฟเวอร์",
                 "ลองเปลี่ยนรูปแบบ (MP4/MP3) หรือลบ Custom Options แล้วลองใหม่", ErrorSeverity.Error)),

            (new Regex(@"ERROR:.*(Unable to download|Connection|timed?\s*out|Network|urlopen|socket|Errno|HTTPError\s+5\d\d)", RegexOptions.IgnoreCase),
             m => new ParsedError("ปัญหาการเชื่อมต่อเครือข่าย",
                 "ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์ได้",
                 "1. ตรวจสอบอินเทอร์เน็ต\n2. ลองใหม่อีกครั้ง\n3. ถ้าใช้ VPN ลองปิด VPN", ErrorSeverity.Error)),

            (new Regex(@"ERROR:.*(HTTP Error 403|Forbidden|HTTP Error 429|Too Many Requests|Sign in to confirm)", RegexOptions.IgnoreCase),
             m => new ParsedError("ถูกจำกัดการเข้าถึง",
                 "เว็บไซต์ปลายทางบล็อกการดาวน์โหลด",
                 "1. รอสักครู่แล้วลองใหม่\n2. อัปเดต yt-dlp\n3. ลองใช้ --cookies-from-browser firefox", ErrorSeverity.Error)),

            (new Regex(@"ERROR:.*(HTTP Error 404|not found|does not exist)", RegexOptions.IgnoreCase),
             m => new ParsedError("ไม่พบวิดีโอ (404)",
                 "ลิงก์นี้ไม่มีอยู่จริง หรือวิดีโอถูกลบไปแล้ว",
                 "ตรวจสอบลิงก์ให้ถูกต้อง", ErrorSeverity.Error)),

            (new Regex(@"ERROR:.*(not available in your country|geo.?restrict)", RegexOptions.IgnoreCase),
             m => new ParsedError("วิดีโอถูกจำกัดภูมิภาค",
                 "วิดีโอนี้ไม่สามารถเข้าถึงได้จากประเทศของคุณ",
                 "ลองใช้ VPN", ErrorSeverity.Error)),

            (new Regex(@"ERROR:.*(copyright|DMCA|claimed)", RegexOptions.IgnoreCase),
             m => new ParsedError("ถูกบล็อกเนื่องจากลิขสิทธิ์",
                 "วิดีโอนี้ถูกบล็อกเนื่องจากปัญหาลิขสิทธิ์",
                 "ไม่สามารถดาวน์โหลดได้", ErrorSeverity.Error)),

            (new Regex(@"ERROR:.*(age.?restrict|login required|Sign in|confirm your age)", RegexOptions.IgnoreCase),
             m => new ParsedError("ต้องล็อกอินเพื่อดูวิดีโอนี้",
                 "วิดีโอนี้จำกัดอายุผู้ชมหรือต้องมีบัญชีผู้ใช้",
                 "ลองใช้ Custom Options: --cookies-from-browser firefox", ErrorSeverity.Error)),

            (new Regex(@"ERROR:.*(m3u8|HLS|manifest)", RegexOptions.IgnoreCase),
             m => new ParsedError("ปัญหาการดาวน์โหลดสตรีม",
                 "ไม่สามารถดาวน์โหลดสตรีม m3u8/HLS ได้",
                 "ตรวจสอบว่าลิงก์ m3u8 ยังใช้งานได้ (ลิงก์อาจหมดอายุ)", ErrorSeverity.Error)),

            (new Regex(@"ERROR:.*(Postprocessing|[Mm]erge|muxing)", RegexOptions.IgnoreCase),
             m => new ParsedError("การรวมไฟล์ล้มเหลว",
                 "ดาวน์โหลดสำเร็จแล้วแต่รวมไฟล์ไม่ได้",
                 "1. ตรวจสอบว่ามี ffmpeg: which ffmpeg\n2. ตรวจสอบพื้นที่ว่างในดิสก์: df -h", ErrorSeverity.Error)),

            (new Regex(@"(No space left|disk full|not enough.*space)", RegexOptions.IgnoreCase),
             m => new ParsedError("พื้นที่เก็บข้อมูลเต็ม",
                 "ดิสก์ไม่มีพื้นที่ว่างเพียงพอ",
                 "ลบไฟล์ที่ไม่จำเป็น: df -h เพื่อตรวจสอบพื้นที่", ErrorSeverity.Fatal)),

            (new Regex(@"(Permission denied|Access.*denied|PermissionError)", RegexOptions.IgnoreCase),
             m => new ParsedError("ไม่มีสิทธิ์เข้าถึงโฟลเดอร์",
                 "ไม่สามารถบันทึกไฟล์ได้เนื่องจากไม่มีสิทธิ์เขียน",
                 "ลองเลือกโฟลเดอร์อื่น เช่น ~/Downloads", ErrorSeverity.Error)),

            (new Regex(@"(outdated version|please update|upgrade yt.?dlp)", RegexOptions.IgnoreCase),
             m => new ParsedError("yt-dlp เวอร์ชันเก่าเกินไป",
                 "เวอร์ชันของ yt-dlp ที่ใช้อยู่ล้าสมัยแล้ว",
                 "แอปจะอัปเดตให้อัตโนมัติเมื่อเปิดใหม่ หรือรัน: yt-dlp --update", ErrorSeverity.Warning)),

            (new Regex(@"ERROR:\s*(.+)", RegexOptions.IgnoreCase),
             m => new ParsedError("เกิดข้อผิดพลาด",
                 m.Groups[1].Value.Trim(),
                 "ลองดาวน์โหลดใหม่หรืออัปเดต yt-dlp", ErrorSeverity.Error)),
        };

        public static ParsedError? TryParse(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;
            foreach (var (pattern, handler) in Rules)
            {
                var match = pattern.Match(line);
                if (match.Success) return handler(match);
            }
            return null;
        }

        public static ParsedError AnalyzeResult(int exitCode, List<string> errorLines, List<string> warningLines)
        {
            if (exitCode == 0 && errorLines.Count == 0)
            {
                return new ParsedError("ดาวน์โหลดเสร็จสมบูรณ์",
                    warningLines.Count > 0 ? $"สำเร็จ (มีคำเตือน {warningLines.Count} รายการ)" : "ไฟล์ถูกบันทึกเรียบร้อยแล้ว",
                    "", ErrorSeverity.Info);
            }

            ParsedError? mostSevere = null;
            foreach (var line in errorLines)
            {
                var parsed = TryParse(line);
                if (parsed != null && (mostSevere == null || parsed.Severity > mostSevere.Severity))
                    mostSevere = parsed;
            }
            if (mostSevere != null) return mostSevere;

            string combinedErrors = errorLines.Count > 0
                ? string.Join("\n", errorLines.GetRange(0, Math.Min(3, errorLines.Count)))
                : "ไม่ทราบสาเหตุ";

            return new ParsedError("ดาวน์โหลดไม่สำเร็จ",
                $"Exit code: {exitCode}\n{combinedErrors}",
                "1. ตรวจสอบลิงก์\n2. ตรวจสอบอินเทอร์เน็ต\n3. อัปเดต yt-dlp",
                ErrorSeverity.Error);
        }

        public static string FormatForUser(ParsedError error)
        {
            string msg = $"[ข้อผิดพลาด] {error.Title}";
            if (!string.IsNullOrWhiteSpace(error.Detail))
                msg += $"\n\nรายละเอียด:\n{error.Detail}";
            if (!string.IsNullOrWhiteSpace(error.Suggestion))
                msg += $"\n\nแนวทางแก้ไข:\n{error.Suggestion}";
            return msg;
        }
    }
}
