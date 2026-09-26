using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VideoDownloaderApp
{
    /// <summary>
    /// ตัวสร้างไอคอนเวกเตอร์สไตล์มินิมอล ลายเส้นโมเดิร์น (Feather / Lucide Style)
    /// ไม่ติดลิขสิทธิ์ 100% ไม่ใช้อิโมจิ คมชัดทุกขนาดหน้าจอ
    /// </summary>
    public static class IconHelper
    {
        public static Bitmap CreateDownloadIcon(int size, Color color, float strokeWidth = 2.4f)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var pen = new Pen(color, strokeWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            float cx = size / 2f;
            float pad = size * 0.22f;
            float topY = pad;
            float arrowTipY = size * 0.62f;
            float headSize = size * 0.20f;

            // ก้านลูกศร
            g.DrawLine(pen, cx, topY, cx, arrowTipY);

            // หัวลูกศร
            g.DrawLine(pen, cx - headSize, arrowTipY - headSize, cx, arrowTipY);
            g.DrawLine(pen, cx + headSize, arrowTipY - headSize, cx, arrowTipY);

            // ถาดรับด้านล่าง
            float trayLeft = pad;
            float trayRight = size - pad;
            float trayTop = size * 0.68f;
            float trayBottom = size - pad;

            using var trayPath = new GraphicsPath();
            trayPath.AddLine(trayLeft, trayTop, trayLeft, trayBottom);
            trayPath.AddLine(trayLeft, trayBottom, trayRight, trayBottom);
            trayPath.AddLine(trayRight, trayBottom, trayRight, trayTop);
            g.DrawPath(pen, trayPath);

            return bmp;
        }

        public static Bitmap CreateFolderIcon(int size, Color color, float strokeWidth = 2.2f)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var pen = new Pen(color, strokeWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            float padX = size * 0.18f;
            float padY = size * 0.24f;
            float w = size - (padX * 2);
            float h = size - (padY * 2);

            using var path = new GraphicsPath();
            float r = size * 0.08f;
            float tabW = w * 0.45f;
            float tabH = h * 0.28f;

            // รูปร่างโฟลเดอร์มินิมอล
            path.AddLine(padX + r, padY, padX + tabW, padY);
            path.AddLine(padX + tabW + (r * 1.2f), padY + tabH, padX + w - r, padY + tabH);
            path.AddArc(padX + w - (2 * r), padY + tabH, 2 * r, 2 * r, 270, 90);
            path.AddLine(padX + w, padY + tabH + r, padX + w, padY + h - r);
            path.AddArc(padX + w - (2 * r), padY + h - (2 * r), 2 * r, 2 * r, 0, 90);
            path.AddLine(padX + w - r, padY + h, padX + r, padY + h);
            path.AddArc(padX, padY + h - (2 * r), 2 * r, 2 * r, 90, 90);
            path.AddLine(padX, padY + h - r, padX, padY + r);
            path.AddArc(padX, padY, 2 * r, 2 * r, 180, 90);

            g.DrawPath(pen, path);
            return bmp;
        }

        public static Bitmap CreateStopIcon(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float pad = size * 0.28f;
            float boxSize = size - (pad * 2);
            float radius = size * 0.10f;

            using var brush = new SolidBrush(color);
            using var path = GetRoundedRectanglePath(new RectangleF(pad, pad, boxSize, boxSize), radius);
            g.FillPath(brush, path);

            return bmp;
        }

        public static Bitmap CreateUpdateIcon(int size, Color color, float strokeWidth = 2.4f)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(color, strokeWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            float cx = size / 2f;
            float cy = size / 2f;
            float r = size * 0.32f;
            var rect = new RectangleF(cx - r, cy - r, r * 2, r * 2);

            // ส่วนโค้งบน
            g.DrawArc(pen, rect, 45, 140);
            // ส่วนโค้งล่าง
            g.DrawArc(pen, rect, 225, 140);

            // หัวลูกศรบน
            float a1x = cx + r * 0.707f;
            float a1y = cy - r * 0.707f;
            g.DrawLine(pen, a1x - size * 0.12f, a1y, a1x, a1y);
            g.DrawLine(pen, a1x, a1y - size * 0.12f, a1x, a1y);

            // หัวลูกศรล่าง
            float a2x = cx - r * 0.707f;
            float a2y = cy + r * 0.707f;
            g.DrawLine(pen, a2x + size * 0.12f, a2y, a2x, a2y);
            g.DrawLine(pen, a2x, a2y + size * 0.12f, a2x, a2y);

            return bmp;
        }

        public static Bitmap CreateCheckIcon(int size, Color color, float strokeWidth = 2.4f)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(color, strokeWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            g.DrawLine(pen, size * 0.22f, size * 0.52f, size * 0.42f, size * 0.72f);
            g.DrawLine(pen, size * 0.42f, size * 0.72f, size * 0.78f, size * 0.28f);

            return bmp;
        }

        public static Bitmap CreateAlertIcon(int size, Color color, float strokeWidth = 2.4f)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(color, strokeWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            float cx = size / 2f;
            float pad = size * 0.15f;
            var circleRect = new RectangleF(pad, pad, size - pad * 2, size - pad * 2);
            g.DrawEllipse(pen, circleRect);

            // ขีดตกใจ
            g.DrawLine(pen, cx, size * 0.32f, cx, size * 0.56f);
            // จุดตกใจ
            using var brush = new SolidBrush(color);
            float dotSize = strokeWidth * 1.2f;
            g.FillEllipse(brush, cx - dotSize / 2f, size * 0.68f, dotSize, dotSize);

            return bmp;
        }

        public static GraphicsPath GetRoundedRectanglePath(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
