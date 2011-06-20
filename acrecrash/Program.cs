using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace acrecrash
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                MessageBeep(0xFFFFFFFF);
                MessageBox.Show("This program should be called only by Arce, when it crashes!");
            }
            else if (File.Exists(args[0]))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new Form1(args[0]));
            }
            else
            {
                MessageBeep(0xFFFFFFFF);
                MessageBox.Show("This program should be called only by Arce, when it crashes!");
            }
        }
        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int MessageBeep(uint uType);
    }
}
