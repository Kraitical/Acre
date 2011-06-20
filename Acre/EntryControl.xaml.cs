using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TlPlugin;
using System.IO;

namespace Acre
{
    /// <summary>
    /// Interaction logic for EntryControl.xaml
    /// </summary>
    public partial class EntryControl : UserControl
    {
        public EntryControl(AnimeEntry entry, Dictionary<string, ITlPlugin> plugins, Delegates.AcreVariable<bool> torrentstate, Delegates.AcreCall<AnimeEntry> update)
        {
            InitializeComponent();
            torrent = torrentstate;
            manualupdate = update;
            _entry = entry;
            foreach (KeyValuePair<string, ITlPlugin> pair in plugins)
            {
                if (pair.Value.GetTranslatedAnimeListIds().Contains<int>(entry.AnimeId))
                    tlist.Items.Add(pair.Key);
            }
            tlist.SelectedIndex = 0;
            
            TryPicUpdate(entry.Image);
        }
        private Delegates.AcreCall<AnimeEntry> manualupdate;
        private Delegates.AcreVariable<bool> torrent;
        private AnimeEntry _entry;
        System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
        private void path_browse_Click(object sender, RoutedEventArgs e)
        {
            dialog.ShowNewFolderButton = true;
            dialog.ShowDialog();
            t_path.Text = dialog.SelectedPath;
            _entry.TorrentPath = t_path.Text;
        }
        public void TryPicUpdate(byte[] image)
        {
            if (image != null)
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.StreamSource = new MemoryStream(image);
                img.EndInit();
                pic.Source = img;
                pic.Visibility = System.Windows.Visibility.Visible;
            }

        }
        private void c_torrent_Checked(object sender, RoutedEventArgs e)
        {
            s_path.IsEnabled = true;
            t_path.IsEnabled = true;
            path_browse.IsEnabled = true;
        }
        public AnimeEntry GetEntry()
        {
            return _entry;
        }
        private void c_torrent_Unchecked(object sender, RoutedEventArgs e)
        {
            s_path.IsEnabled = false;
            t_path.IsEnabled = false;
            t_path.Text = "";
            path_browse.IsEnabled = false;
            _entry.TorrentPath = t_path.Text;
        }

        private void notify_Checked(object sender, RoutedEventArgs e)
        {
            _entry.Notify = true;
            tlist.IsEnabled = true;
            s_tlist.IsEnabled = true;
            s_int.IsEnabled = true;
            interval.IsEnabled = true;
            if (torrent.Invoke())
                c_torrent.IsEnabled = true;
        }

        private void notify_Unchecked(object sender, RoutedEventArgs e)
        {
            _entry.Notify = false;
            tlist.IsEnabled = false;
            s_tlist.IsEnabled = false;
            c_torrent.IsEnabled = false;
            s_int.IsEnabled = false;
            interval.IsEnabled = false;
        }
        private void t_path_TextChanged(object sender, TextChangedEventArgs e)
        {
            _entry.TorrentPath = t_path.Text;
        }

        private void tlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _entry.Status = new AnimeStatus();
            _entry.TLGroup = tlist.SelectedItem.ToString();
        }

        private void t_path_GotFocus(object sender, RoutedEventArgs e)
        {
            _entry.TorrentPath = t_path.Text;
        }

        private void manual_update_Click(object sender, RoutedEventArgs e)
        {
            manualupdate.Invoke(this._entry);
        }
        private void interval_TextChanged(object sender, TextChangedEventArgs e)
        {
            _entry.Timer.Interval = Convert.ToInt32(interval.Text) * 10;
        }
    }
}
