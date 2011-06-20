using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms.Integration;
using System.Runtime.Serialization.Formatters.Binary;
using TlPlugin;
namespace Acre
{
    public class Misc
    {
        [STAThread]
        static void Main(string[] args)
        {
            AcreDef self = new AcreDef();
            self.Init();
            self.Start();
        }
        static void a_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((AS)sender).Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
    }
    class AcreDef
    {
        public void Init()
        {
            CheckDirs();
            System.Net.ServicePointManager.Expect100Continue = false;
            System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = Properties.Resources.loading;
            ni.Visible = true;
            ni.Text = "Acre";
            System.Windows.Forms.ContextMenuStrip cms = new System.Windows.Forms.ContextMenuStrip();
            System.Windows.Forms.ToolStripMenuItem set = new System.Windows.Forms.ToolStripMenuItem("Settings");
            System.Windows.Forms.ToolStripMenuItem quit = new System.Windows.Forms.ToolStripMenuItem("Quit");
            quit.Click += new EventHandler(quit_Click);
            set.Click += new EventHandler(set_Click);
            cms.Items.Add(set);
            cms.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            cms.Items.Add(quit);
            ni.ContextMenuStrip = cms;
            plugins = PluginLib.LoadPlugins();
            ids = ANS.GetAnimeIds(plugins);
            entries = MakeEntries(ids, plugins);
            a = new AS(plugins, new Delegates.AcreCall<AnimeEntry[]>(AcreSaveProcMethod), new Delegates.AcreCall<AnimeEntry>(ManualUpdate));
            a.InitializeComponent();
            ElementHost.EnableModelessKeyboardInterop(a);
        }

        private void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            WriteException(e.Exception);
        }
        private void CheckDirs()
        {
            if (!Directory.Exists(@".\Cache")) Directory.CreateDirectory(@".\Cache");
            if (!Directory.Exists(@".\Cache\Torrents")) Directory.CreateDirectory(@".\Cache\Torrents");
        }
        private int[] ids;
        private AnimeEntry[] entries;
        private Dictionary<string, ITlPlugin> plugins;
        void quit_Click(object sender, EventArgs e)
        {
            BinaryFormatter f = new BinaryFormatter();
            foreach (AnimeEntry ent in entries)
            {
                FileStream fs = new FileStream(@".\Cache\" + ent.AnimeId + ".acre", FileMode.Create);
                f.Serialize(fs, ent);
                fs.Close();
            }
            ni.Dispose();
            System.Windows.Forms.Application.Exit();
        }
        public AnimeEntry[] MakeEntries(int[] ids, Dictionary<string, ITlPlugin> plugins)
        {
            AnimeEntry[] ent = new AnimeEntry[ids.Length];
            BinaryFormatter form = new BinaryFormatter();
            for (int i = 0; i < ent.Length; i++)
            {
                if (File.Exists(@".\Cache\" + ids[i] + ".acre"))
                {
                    FileStream fs = new FileStream(@".\Cache\" + ids[i] + ".acre", FileMode.Open);
                    ent[i] = (AnimeEntry)form.Deserialize(fs);
                    fs.Close();
                    ent[i].Timer = new AnimeTimer(i);
                    ent[i].Timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
                }
                else
                {
                    ent[i] = new AnimeEntry(ids[i]);
                }
                ent[i].AnimeId = ids[i]; // WHUT?
                ent[i].ArrayId = i;
            }
            ni.Icon = Properties.Resources.main;
            return ent;
        }


        public void Start()
        {
            System.Windows.Forms.Application.Run();
        }
        private AS a;
        void set_Click(object sender, EventArgs e)
        {
            a.Show(entries);
        }
        private System.Windows.Forms.NotifyIcon ni;
        void main_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            WriteException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteException((Exception)e.ExceptionObject);
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            AnimeTimer self = (AnimeTimer)sender;
            UpdateEntry(self.ArrayId);
        }
        private void ManualUpdate(AnimeEntry ae)
        {
            AnimeStatus s2 = new AnimeStatus();
            if (plugins[ae.TLGroup].StatusSupported())
            {
                s2.Status = plugins[ae.TLGroup].GetStatus(ae.AnimeId);
            }
            s2.Latest = plugins[ae.TLGroup].LatestEpisode(ae.AnimeId);
            ni.ShowBalloonTip(1000, ae.Name, s2.ToString(), System.Windows.Forms.ToolTipIcon.None);
            entries[ae.ArrayId].Status = s2;
        }
        private void UpdateEntry(int id)
        {
            try
            {
                AnimeStatus s2 = new AnimeStatus();
                if (plugins[entries[id].TLGroup].StatusSupported())
                {
                    s2.Status = plugins[entries[id].TLGroup].GetStatus(entries[id].AnimeId);
                }
                s2.Latest = plugins[entries[id].TLGroup].LatestEpisode(entries[id].AnimeId);
                if (!s2.Equals(entries[id].Status))
                {
                    ni.ShowBalloonTip(1000, entries[id].Name, s2.ToString(), System.Windows.Forms.ToolTipIcon.None);
                    entries[id].Status = s2;
                }
            }
            catch
            {
            }
        }
        private void WriteException(Exception ex)
        {
            string filename = @".\Logs\exception-" + DateTime.Now.ToString("yyyy_mm_d_H_m_s") + ".log";
            if (!Directory.Exists(@".\Logs")) Directory.CreateDirectory(@".\Logs");
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(ex.ToString());
            sw.WriteLine("=======================");
            sw.WriteLine("Method: " + ex.TargetSite.Name);
            sw.WriteLine("=========DATA==========");
            foreach (DictionaryEntry de in ex.Data)
            {
                sw.WriteLine(de.Key + " = " + de.Value);
            }
            sw.Close();
            Process.Start(@".\acrecrash.exe", filename);
            System.Windows.Forms.Application.Exit();
        }

        public void AcreSaveProcMethod(AnimeEntry[] mod)
        {
            entries = mod;
        }
    }

}
