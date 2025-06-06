namespace VideoDownloaderApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            txtUrl = new TextBox();
            label2 = new Label();
            grpOutputFormat = new GroupBox();
            rbMp3 = new RadioButton();
            rbMp4 = new RadioButton();
            label3 = new Label();
            txtOutputPath = new TextBox();
            btnBrowse = new Button();
            label4 = new Label();
            txtCustomOptions = new TextBox();
            btnDownload = new Button();
            txtStatus = new TextBox();
            progressBar = new ProgressBar();
            grpOutputFormat.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(195, 30);
            label1.Name = "label1";
            label1.Size = new Size(78, 20);
            label1.TabIndex = 0;
            label1.Text = "Video URL";
            // 
            // txtUrl
            // 
            txtUrl.Location = new Point(124, 53);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(246, 27);
            txtUrl.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(185, 83);
            label2.Name = "label2";
            label2.Size = new Size(104, 20);
            label2.TabIndex = 2;
            label2.Text = "Output format";
            // 
            // grpOutputFormat
            // 
            grpOutputFormat.Controls.Add(rbMp3);
            grpOutputFormat.Controls.Add(rbMp4);
            grpOutputFormat.Location = new Point(124, 106);
            grpOutputFormat.Name = "grpOutputFormat";
            grpOutputFormat.Size = new Size(280, 62);
            grpOutputFormat.TabIndex = 3;
            grpOutputFormat.TabStop = false;
            grpOutputFormat.Text = "File format";
            // 
            // rbMp3
            // 
            rbMp3.AutoSize = true;
            rbMp3.Location = new Point(131, 27);
            rbMp3.Name = "rbMp3";
            rbMp3.Size = new Size(143, 24);
            rbMp3.TabIndex = 1;
            rbMp3.TabStop = true;
            rbMp3.Text = "MP3 (audio only)";
            rbMp3.UseVisualStyleBackColor = true;
            // 
            // rbMp4
            // 
            rbMp4.AutoSize = true;
            rbMp4.Checked = true;
            rbMp4.Location = new Point(15, 27);
            rbMp4.Name = "rbMp4";
            rbMp4.Size = new Size(110, 24);
            rbMp4.TabIndex = 0;
            rbMp4.TabStop = true;
            rbMp4.Text = "MP4 (video)";
            rbMp4.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(180, 171);
            label3.Name = "label3";
            label3.Size = new Size(129, 20);
            label3.TabIndex = 4;
            label3.Text = "Destination folder";
            // 
            // txtOutputPath
            // 
            txtOutputPath.Location = new Point(124, 194);
            txtOutputPath.Name = "txtOutputPath";
            txtOutputPath.Size = new Size(246, 27);
            txtOutputPath.TabIndex = 5;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(195, 227);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(94, 29);
            btnBrowse.TabIndex = 6;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = Color.SteelBlue;
            label4.Location = new Point(88, 259);
            label4.Name = "label4";
            label4.Size = new Size(338, 20);
            label4.TabIndex = 7;
            label4.Text = "Additional commands for advanced users (yt-dlp)";
            // 
            // txtCustomOptions
            // 
            txtCustomOptions.ForeColor = Color.Red;
            txtCustomOptions.Location = new Point(180, 282);
            txtCustomOptions.Name = "txtCustomOptions";
            txtCustomOptions.Size = new Size(134, 27);
            txtCustomOptions.TabIndex = 8;
            // 
            // btnDownload
            // 
            btnDownload.Location = new Point(195, 315);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(94, 29);
            btnDownload.TabIndex = 9;
            btnDownload.Text = "Download";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // txtStatus
            // 
            txtStatus.Location = new Point(432, 53);
            txtStatus.Multiline = true;
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.ScrollBars = ScrollBars.Vertical;
            txtStatus.Size = new Size(334, 359);
            txtStatus.TabIndex = 11;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(180, 350);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(125, 29);
            progressBar.TabIndex = 12;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(progressBar);
            Controls.Add(txtStatus);
            Controls.Add(btnDownload);
            Controls.Add(txtCustomOptions);
            Controls.Add(label4);
            Controls.Add(btnBrowse);
            Controls.Add(txtOutputPath);
            Controls.Add(label3);
            Controls.Add(grpOutputFormat);
            Controls.Add(label2);
            Controls.Add(txtUrl);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Form1";
            grpOutputFormat.ResumeLayout(false);
            grpOutputFormat.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtUrl;
        private Label label2;
        private GroupBox grpOutputFormat;
        private RadioButton rbMp4;
        private RadioButton rbMp3;
        private Label label3;
        private TextBox txtOutputPath;
        private Button btnBrowse;
        private Label label4;
        private TextBox txtCustomOptions;
        private Button btnDownload;
        private TextBox txtStatus;
        private ProgressBar progressBar;
    }
}
