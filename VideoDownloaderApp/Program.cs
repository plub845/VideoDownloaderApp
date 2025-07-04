using System;
using System.Windows.Forms;
// using System.Threading; // <-- บรรทัดนี้ถูกลบออกไปแล้ว เนื่องจาก Thread.Sleep ไม่จำเป็นแล้ว

namespace VideoDownloaderApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // The following line initializes application configuration for Windows Forms.
            // DO NOT REMOVE THIS LINE.
            ApplicationConfiguration.Initialize();

            // The 'Thread.Sleep(5000);' was a temporary line added for debugging purposes
            // to check if the program was starting. It has now been removed.
            // Application will now directly proceed to run the main form.

            // This line runs your main Form (Form1).
            // The application will stay open until Form1 is closed by the user.
            Application.Run(new Form1());
        }
    }
}