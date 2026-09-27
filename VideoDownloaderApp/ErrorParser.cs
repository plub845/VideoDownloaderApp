using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VideoDownloaderApp
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
            // Video ไม่สามารถเข้าถึงได้ / ลบแล้ว / ส่วนตัว
            (new Regex(@"ERROR:.*Video unavailable", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "วิดีโอไม่สามารถเข้าถึงได้",
                 "วิดีโอนี้อาจถูกลบ ตั้งเป็นส่วนตัว หรือจำกัดสิทธิ์การเข้าถึง",
                 "ลองตรวจสอบลิงก์อีกครั้ง หรือลองเปิดลิงก์ในเบราว์เซอร์เพื่อเช็คว่าวิดีโอยังอยู่หรือไม่",
                 ErrorSeverity.Error)),

            // ลิงก์ไม่รองรับ
            (new Regex(@"ERROR:.*Unsupported URL", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ลิงก์ไม่รองรับ",
                 "yt-dlp ไม่รู้จักลิงก์รูปแบบนี้ อาจเป็นเว็บไซต์ที่ยังไม่รองรับ",
                 "ลองใช้ลิงก์จาก YouTube, Facebook, Twitter หรือเว็บที่ yt-dlp รองรับ\nดูรายชื่อเว็บไซต์ที่รองรับได้ที่: https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md",
                 ErrorSeverity.Error)),

            // ไม่พบ ffmpeg
            (new Regex(@"ERROR:.*ffmpeg.*not found|ffmpeg.*is not installed", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ไม่พบ ffmpeg",
                 "โปรแกรมต้องใช้ ffmpeg ในการรวมไฟล์ภาพกับเสียง และแปลงไฟล์เป็น MP3",
                 "ลองติดตั้งโปรแกรมใหม่อีกครั้ง หรือดาวน์โหลด ffmpeg จาก https://ffmpeg.org/ แล้ววางไว้ในโฟลเดอร์เดียวกับโปรแกรม",
                 ErrorSeverity.Fatal)),

            // ไม่พบ format ที่ต้องการ
            (new Regex(@"ERROR:.*Requested format.*not available", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ไม่มีคุณภาพวิดีโอที่เลือก",
                 "ไม่พบรูปแบบไฟล์ที่ขอบนเซิร์ฟเวอร์ วิดีโอบางตัวอาจมีความละเอียดจำกัด",
                 "ลองเปลี่ยนรูปแบบ (MP4/MP3) หรือลบ Custom Options แล้วลองใหม่",
                 ErrorSeverity.Error)),

            // ปัญหาการเชื่อมต่อ / เครือข่าย
            (new Regex(@"ERROR:.*(Unable to download|Connection|timed?\s*out|Network|urlopen|socket|Errno|HTTPError\s+5\d\d|HTTP Error 503|HTTP Error 500)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ปัญหาการเชื่อมต่อเครือข่าย",
                 "ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์ได้ อาจเป็นเพราะอินเทอร์เน็ตขัดข้อง หรือเซิร์ฟเวอร์ปลายทางมีปัญหา",
                 "1. ตรวจสอบการเชื่อมต่ออินเทอร์เน็ตของคุณ\n2. ลองใหม่อีกครั้งในอีกสักครู่\n3. ถ้าใช้ VPN ลองปิด VPN แล้วลองใหม่",
                 ErrorSeverity.Error)),

            // HTTP 403 / 429 ถูกบล็อก / Rate limit
            (new Regex(@"ERROR:.*(HTTP Error 403|Forbidden|HTTP Error 429|Too Many Requests|Sign in to confirm)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ถูกจำกัดการเข้าถึง",
                 "เว็บไซต์ปลายทางบล็อกการดาวน์โหลด อาจเป็นเพราะส่งคำขอมากเกินไป หรือต้องล็อกอินก่อน",
                 "1. รอสักครู่แล้วลองใหม่ (ประมาณ 5-10 นาที)\n2. ลองอัปเดต yt-dlp เป็นเวอร์ชันล่าสุด\n3. หากวิดีโอต้องล็อกอิน ลองใช้ Custom Options: --cookies-from-browser chrome",
                 ErrorSeverity.Error)),

            // HTTP 404 ไม่พบ
            (new Regex(@"ERROR:.*(HTTP Error 404|not found|does not exist)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ไม่พบวิดีโอ (404)",
                 "ลิงก์นี้ไม่มีอยู่จริง หรือวิดีโอถูกลบไปแล้ว",
                 "ตรวจสอบลิงก์ให้ถูกต้อง ลองเปิดในเบราว์เซอร์ดูว่ายังเข้าถึงได้หรือไม่",
                 ErrorSeverity.Error)),

            // Geo-restricted
            (new Regex(@"ERROR:.*(not available in your country|geo.?restrict|geo.?block)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "วิดีโอถูกจำกัดภูมิภาค",
                 "วิดีโอนี้ไม่สามารถเข้าถึงได้จากประเทศของคุณ",
                 "ลองใช้ VPN เชื่อมต่อไปยังประเทศที่วิดีโอนั้นสามารถเข้าถึงได้",
                 ErrorSeverity.Error)),

            // ปัญหาลิขสิทธิ์
            (new Regex(@"ERROR:.*(copyright|DMCA|claimed)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ถูกบล็อกเนื่องจากลิขสิทธิ์",
                 "วิดีโอนี้ถูกบล็อกเนื่องจากปัญหาลิขสิทธิ์",
                 "ไม่สามารถดาวน์โหลดได้เนื่องจากข้อจำกัดด้านลิขสิทธิ์",
                 ErrorSeverity.Error)),

            // Playlist ว่าง
            (new Regex(@"ERROR:.*(no video|playlist.*is empty|no entries)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ไม่พบวิดีโอในเพลย์ลิสต์",
                 "เพลย์ลิสต์นี้อาจว่างเปล่า หรือวิดีโอทั้งหมดถูกลบแล้ว",
                 "ตรวจสอบเพลย์ลิสต์ในเบราว์เซอร์ หรือลองใช้ลิงก์วิดีโอโดยตรง",
                 ErrorSeverity.Warning)),

            // อายุ / ต้องยืนยันตัวตน
            (new Regex(@"ERROR:.*(age.?restrict|age.?gate|login required|Sign in|confirm your age)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ต้องล็อกอินเพื่อดูวิดีโอนี้",
                 "วิดีโอนี้จำกัดอายุผู้ชม หรือต้องมีบัญชีผู้ใช้เพื่อเข้าถึง",
                 "ลองใช้ Custom Options: --cookies-from-browser chrome\n(เปิด Chrome ล็อกอิน YouTube ก่อน แล้วลองใหม่)",
                 ErrorSeverity.Error)),

            // Live stream ยังไม่เริ่ม / จบแล้ว
            (new Regex(@"ERROR:.*(live.*has not.*start|live.*end|premieres? in)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ถ่ายทอดสดยังไม่เริ่มหรือจบแล้ว",
                 "วิดีโอนี้เป็นการถ่ายทอดสดที่ยังไม่เริ่ม หรือจบไปแล้วและยังไม่มี replay",
                 "รอจนกว่าการถ่ายทอดสดจะเริ่ม หรือรอจนมี replay ปกติ",
                 ErrorSeverity.Info)),

            // yt-dlp ล้าสมัย
            (new Regex(@"(outdated version|please update|upgrade yt.?dlp)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "yt-dlp เวอร์ชันเก่าเกินไป",
                 "เวอร์ชันของ yt-dlp ที่ใช้อยู่ล้าสมัยแล้ว อาจทำให้ดาวน์โหลดไม่ได้",
                 "กดปุ่ม 'ตรวจสอบอัปเดต' เพื่ออัปเดตโปรแกรมเป็นเวอร์ชันล่าสุด",
                 ErrorSeverity.Warning)),

            // m3u8 / HLS ปัญหา
            (new Regex(@"ERROR:.*(m3u8|HLS|manifest)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ปัญหาการดาวน์โหลดสตรีม",
                 "ไม่สามารถดาวน์โหลดสตรีม m3u8/HLS ได้ อาจเป็นเพราะลิงก์หมดอายุ หรือสตรีมถูกปิด",
                 "1. ตรวจสอบว่าลิงก์ m3u8 ยังใช้งานได้\n2. ลองเปิดลิงก์ในเบราว์เซอร์ก่อน\n3. ลิงก์ m3u8 บางตัวมีอายุจำกัด ต้องคัดลอกใหม่",
                 ErrorSeverity.Error)),

            // Merge / Post-processing ล้มเหลว
            (new Regex(@"ERROR:.*(Postprocessing|[Mm]erge|muxing|Conversion)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "การรวมไฟล์ล้มเหลว",
                 "ดาวน์โหลดสำเร็จแล้ว แต่ไม่สามารถรวมไฟล์ภาพกับเสียงเข้าด้วยกันได้",
                 "1. ตรวจสอบว่ามี ffmpeg ในโฟลเดอร์โปรแกรม\n2. ตรวจสอบพื้นที่ว่างในดิสก์\n3. ลองดาวน์โหลดใหม่อีกครั้ง",
                 ErrorSeverity.Error)),

            // พื้นที่ไม่พอ
            (new Regex(@"(No space left|disk full|not enough.*space|IOException.*space)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "พื้นที่เก็บข้อมูลเต็ม",
                 "ดิสก์ไม่มีพื้นที่ว่างเพียงพอสำหรับบันทึกไฟล์",
                 "ลบไฟล์ที่ไม่จำเป็นออกเพื่อเพิ่มพื้นที่ แล้วลองดาวน์โหลดใหม่",
                 ErrorSeverity.Fatal)),

            // Permission denied
            (new Regex(@"(Permission denied|Access.*denied|PermissionError)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ไม่มีสิทธิ์เข้าถึงโฟลเดอร์",
                 "ไม่สามารถบันทึกไฟล์ไปยังโฟลเดอร์ที่เลือกได้ เนื่องจากไม่มีสิทธิ์ในการเขียน",
                 "ลองเลือกโฟลเดอร์อื่น เช่น Desktop หรือ Downloads หรือเปิดโปรแกรมด้วยสิทธิ์ Administrator",
                 ErrorSeverity.Error)),

            // WinError 448 (Untrusted Mount Point)
            (new Regex(@"WinError\s+448|untrusted mount point", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "ข้อจำกัดความปลอดภัยของ Windows (WinError 448)",
                 "ตรวจพบโฟลเดอร์ที่ไม่ปลอดภัยหรือเป็น Junction Link ใน Environment Variable PATH (เช่น .dotnet\\tools)",
                 "1. ลบโฟลเดอร์ .dotnet\\tools ออกจากตัวแปร PATH ของ Windows ใน Environment Variables\n2. หรือเปิดโปรแกรมที่อัปเดตแล้ว ซึ่งจะตัดโฟลเดอร์นี้ออกให้อัตโนมัติ",
                 ErrorSeverity.Error)),

            // Catch-all ERROR:
            (new Regex(@"ERROR:\s*(.+)", RegexOptions.IgnoreCase),
             m => new ParsedError(
                 "เกิดข้อผิดพลาด",
                 m.Groups[1].Value.Trim(),
                 "ลองดาวน์โหลดใหม่อีกครั้ง หรืออัปเดต yt-dlp เป็นเวอร์ชันล่าสุด",
                 ErrorSeverity.Error)),
        };

        /// <summary>
        /// วิเคราะห์บรรทัด log จาก yt-dlp แล้วคืน ParsedError ถ้าเจอข้อผิดพลาด
        /// </summary>
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

        /// <summary>
        /// วิเคราะห์ exit code ของ yt-dlp รวมกับ error log ทั้งหมด
        /// เพื่อสร้างข้อความสรุปผลที่ผู้ใช้เข้าใจง่าย
        /// </summary>
        public static ParsedError AnalyzeResult(int exitCode, List<string> errorLines, List<string> warningLines)
        {
            if (exitCode == 0 && errorLines.Count == 0)
            {
                return new ParsedError(
                    "ดาวน์โหลดเสร็จสมบูรณ์",
                    warningLines.Count > 0
                        ? $"สำเร็จ (มีคำเตือน {warningLines.Count} รายการ)"
                        : "ไฟล์ถูกบันทึกเรียบร้อยแล้ว",
                    "",
                    ErrorSeverity.Info);
            }

            // ลองหา error ที่สำคัญที่สุดจาก error lines
            ParsedError? mostSevere = null;
            foreach (var line in errorLines)
            {
                var parsed = TryParse(line);
                if (parsed != null)
                {
                    if (mostSevere == null || parsed.Severity > mostSevere.Severity)
                    {
                        mostSevere = parsed;
                    }
                }
            }

            if (mostSevere != null) return mostSevere;

            // ถ้าไม่สามารถวิเคราะห์ error ได้เลย ให้สรุปรวม
            string combinedErrors = errorLines.Count > 0
                ? string.Join("\n", errorLines.GetRange(0, Math.Min(3, errorLines.Count)))
                : "ไม่ทราบสาเหตุ";

            return new ParsedError(
                "ดาวน์โหลดไม่สำเร็จ",
                $"Exit code: {exitCode}\n{combinedErrors}",
                "1. ตรวจสอบลิงก์ให้ถูกต้อง\n2. ตรวจสอบการเชื่อมต่ออินเทอร์เน็ต\n3. ลองอัปเดต yt-dlp เป็นเวอร์ชันล่าสุด",
                ErrorSeverity.Error);
        }

        /// <summary>
        /// สร้างข้อความ formatted สำหรับแสดงใน ModernDialog หรือ UI
        /// </summary>
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
