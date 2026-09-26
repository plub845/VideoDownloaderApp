using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VideoDownloaderApp
{
    public enum ModernDialogType
    {
        Information,
        Question,
        Warning,
        Error
    }

    /// <summary>
    /// หน้าต่างแจ้งเตือนสไตล์มินิมอล สีนวล มุมมน ไม่ใช้อิโมจิ ไม่เหมือนหน้าต่าง Windows รุ่นเก่า
    /// </summary>
    public sealed class ModernDialog : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public static DialogResult ShowDialog(
            IWin32Window? owner,
            string title,
            string message,
            ModernDialogType type = ModernDialogType.Information,
            string confirmText = "ตกลง",
            string? cancelText = null,
            string? details = null,
            string? suggestion = null)
        {
            using var dlg = new ModernDialog(title, message, type, confirmText, cancelText, details, suggestion);
            return dlg.ShowDialog(owner);
        }

        private ModernDialog(
            string title,
            string message,
            ModernDialogType type,
            string confirmText,
            string? cancelText,
            string? details,
            string? suggestion)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(255, 250, 242);
            ForeColor = Color.FromArgb(80, 60, 45);
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            DoubleBuffered = true;

            int dialogWidth = 480;
            int dialogHeight = 220;

            if (!string.IsNullOrEmpty(details) || !string.IsNullOrEmpty(suggestion))
            {
                dialogHeight += 120;
            }

            Size = new Size(dialogWidth, dialogHeight);
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 16, 16));

            // Drag support
            MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            // Icon
            Color accentColor = type switch
            {
                ModernDialogType.Question => Color.FromArgb(226, 138, 103),
                ModernDialogType.Warning => Color.FromArgb(232, 168, 86),
                ModernDialogType.Error => Color.FromArgb(224, 102, 102),
                _ => Color.FromArgb(114, 176, 142)
            };

            Bitmap iconBmp = type switch
            {
                ModernDialogType.Question => IconHelper.CreateUpdateIcon(36, accentColor),
                ModernDialogType.Error => IconHelper.CreateAlertIcon(36, accentColor),
                ModernDialogType.Warning => IconHelper.CreateAlertIcon(36, accentColor),
                _ => IconHelper.CreateCheckIcon(36, accentColor)
            };

            var picIcon = new PictureBox
            {
                Image = iconBmp,
                Size = new Size(38, 38),
                Location = new Point(24, 24),
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.Transparent
            };
            Controls.Add(picIcon);

            // Title
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 55, 35),
                Location = new Point(72, 24),
                Size = new Size(dialogWidth - 96, 28),
                BackColor = Color.Transparent
            };
            Controls.Add(lblTitle);

            // Message
            var lblMessage = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(110, 90, 65),
                Location = new Point(72, 58),
                Size = new Size(dialogWidth - 96, 65),
                BackColor = Color.Transparent
            };
            Controls.Add(lblMessage);

            int currentY = 130;

            // Details / Suggestion Box (if any)
            if (!string.IsNullOrEmpty(details) || !string.IsNullOrEmpty(suggestion))
            {
                var txtDetails = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BackColor = Color.FromArgb(250, 245, 235),
                    ForeColor = Color.FromArgb(100, 80, 60),
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Segoe UI", 8.5F),
                    Location = new Point(24, currentY),
                    Size = new Size(dialogWidth - 48, 100),
                    Text = (string.IsNullOrEmpty(details) ? "" : $"รายละเอียด:\n{details}\n\n") +
                           (string.IsNullOrEmpty(suggestion) ? "" : $"แนวทางแก้ไข:\n{suggestion}")
                };
                Controls.Add(txtDetails);
                currentY += 115;
            }

            // Buttons panel
            int btnY = Height - 52;
            int btnWidth = 110;
            int btnHeight = 36;

            var btnConfirm = new RoundedButton
            {
                Text = confirmText,
                NormalColor = accentColor,
                HoverColor = ControlPaint.Light(accentColor, 0.15f),
                PressedColor = ControlPaint.Dark(accentColor, 0.15f),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BorderRadius = 8,
                Size = new Size(btnWidth, btnHeight)
            };
            btnConfirm.Click += (s, e) => { DialogResult = DialogResult.Yes; Close(); };

            if (!string.IsNullOrEmpty(cancelText))
            {
                var btnCancel = new RoundedButton
                {
                    Text = cancelText,
                    NormalColor = Color.FromArgb(228, 215, 195),
                    HoverColor = Color.FromArgb(238, 225, 208),
                    PressedColor = Color.FromArgb(218, 205, 185),
                    ForeColor = Color.FromArgb(100, 75, 50),
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                    BorderRadius = 8,
                    Size = new Size(btnWidth, btnHeight),
                    Location = new Point(dialogWidth - 24 - (btnWidth * 2) - 10, btnY)
                };
                btnCancel.Click += (s, e) => { DialogResult = DialogResult.No; Close(); };
                Controls.Add(btnCancel);

                btnConfirm.Location = new Point(dialogWidth - 24 - btnWidth, btnY);
            }
            else
            {
                btnConfirm.Text = confirmText;
                btnConfirm.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
                btnConfirm.Location = new Point(dialogWidth - 24 - btnWidth, btnY);
            }

            Controls.Add(btnConfirm);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // เส้นขอบนุ่มนวล
            using var pen = new Pen(Color.FromArgb(220, 205, 180), 1.2f);
            using var path = IconHelper.GetRoundedRectanglePath(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), 16);
            g.DrawPath(pen, path);
        }
    }
}
