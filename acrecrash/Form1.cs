using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace acrecrash
{
    public partial class Form1 : Form
    {
        public Form1(string file)
        {
            InitializeComponent();
            FileStream fs = new FileStream(file, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            this.textBox1.Text = sr.ReadToEnd();
            Program.MessageBeep(0xFFFFFFFF);
             
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
    }
}
