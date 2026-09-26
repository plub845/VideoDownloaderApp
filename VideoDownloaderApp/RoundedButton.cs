using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VideoDownloaderApp
{
    /// <summary>
    /// ปุ่มสไตล์โมเดิร์น มินิมอล มุมมน นุ่มนวล ไม่เหมือนโปรแกรมเก่า
    /// </summary>
    public class RoundedButton : Button
    {
        private int _borderRadius = 10;
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        private Color _normalColor = Color.FromArgb(217, 119, 87);
        public Color NormalColor
        {
            get => _normalColor;
            set { _normalColor = value; Invalidate(); }
        }

        private Color _hoverColor = Color.FromArgb(226, 138, 103);
        public Color HoverColor
        {
            get => _hoverColor;
            set { _hoverColor = value; Invalidate(); }
        }

        private Color _pressedColor = Color.FromArgb(196, 104, 73);
        public Color PressedColor
        {
            get => _pressedColor;
            set { _pressedColor = value; Invalidate(); }
        }

        public Color BorderLineColor { get; set; } = Color.Transparent;
        public int BorderLineWidth { get; set; } = 0;

        private bool _isHovered = false;
        private bool _isPressed = false;

        public RoundedButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            UseVisualStyleBackColor = false;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (mevent.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // ไม่เรียก base เพื่อป้องกันการวาดพื้นหลังสี่เหลี่ยมดำจาก Windows
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // เติมพื้นหลังทั้งหมดด้วยสีของ Parent เสมอ เพื่อให้มุมทั้ง 4 ที่อยู่นอกส่วนมน
            // กลืนเป็นเนื้อเดียวกับพื้นหลังของหน้าต่าง 100% ไม่มีขอบดำหรือเงาดำรองปุ่ม
            Color parentBg = Parent?.BackColor ?? Color.FromArgb(255, 248, 240);
            using (var parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);

            Color currentBg = !Enabled ? Color.FromArgb(235, 225, 210) :
                              _isPressed ? PressedColor :
                              _isHovered ? HoverColor : NormalColor;

            Color currentText = !Enabled ? Color.FromArgb(185, 170, 150) : ForeColor;

            using (var path = IconHelper.GetRoundedRectanglePath(rect, BorderRadius))
            {
                // วาดพื้นหลังมุมมน
                using (var brush = new SolidBrush(currentBg))
                {
                    g.FillPath(brush, path);
                }

                // เส้นขอบ (ถ้ามี)
                if (BorderLineWidth > 0 && BorderLineColor != Color.Transparent)
                {
                    using var borderPen = new Pen(BorderLineColor, BorderLineWidth);
                    g.DrawPath(borderPen, path);
                }

                // วาดไอคอนและข้อความ
                int iconWidth = (Image != null) ? Image.Width : 0;
                int spacing = (iconWidth > 0 && !string.IsNullOrEmpty(Text)) ? 10 : 0;

                var textSize = TextRenderer.MeasureText(Text, Font);
                int totalContentWidth = iconWidth + spacing + textSize.Width;

                int startX = (Width - totalContentWidth) / 2;
                if (startX < 8) startX = 8;

                if (Image != null)
                {
                    int iconY = (Height - Image.Height) / 2;
                    g.DrawImage(Image, startX, iconY, Image.Width, Image.Height);
                    startX += Image.Width + spacing;
                }

                if (!string.IsNullOrEmpty(Text))
                {
                    var textRect = new Rectangle(startX, 0, Width - startX - 8, Height);
                    TextRenderer.DrawText(g, Text, Font, textRect, currentText,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.WordEllipsis);
                }
            }
        }
    }
}
