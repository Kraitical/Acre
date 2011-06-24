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
using System.Windows.Shapes;
using TlPlugin;
using System.Web;

namespace Acre
{
    /// <summary>
    /// Interaction logic for AS.xaml
    /// </summary>
    public partial class AS : Window
    {
        public AS(Dictionary<string, ITlPlugin> plugins, Delegates.AcreCall<AnimeEntry[]> saveproc, Delegates.AcreCall<AnimeEntry> manualupdate)
        {
            this.plugins = plugins;
            this.manualupdate = manualupdate;
            proc = saveproc;
            InitializeComponent();
        }
        private AnimeEntry[] entries;
        private Delegates.AcreCall<AnimeEntry[]> proc;
        private Delegates.AcreCall<AnimeEntry> manualupdate;
        Dictionary<string, ITlPlugin> plugins;
        void AS_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        public void Show(AnimeEntry[] entries)
        {
            this.entries = entries;
            this.Show();
        }
        private void save_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement elem in animes.Children)
            {
                EntryControl c = (EntryControl)((Expander)elem).Content;
                AnimeEntry ae = c.GetEntry();
                entries[ae.ArrayId] = ae;
            }
            proc.Invoke(entries);
            this.Visibility = Visibility.Hidden;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                animes.Children.Clear();
                for (int i = 0; i < entries.Length; i++)
                {
                    Expander ex = new Expander();
                    ex.Width = _main.Width - 20;
                    ex.Margin = new Thickness(2);
                    ex.Header = entries[i].Name;
                    ex.Tag = entries[i].ArrayId;
                    ex.Expanded += new RoutedEventHandler(ex_Expanded);
                    ex.Content = new EntryControl(entries[i], plugins, new Delegates.AcreVariable<bool>(TorrentState), manualupdate);
                    entries[i].Parent = ex;
                    animes.Children.Add(ex);
                }
            }
        }
        private bool TorrentState()
        {
            return _ts;
        }
        private bool _ts = false;
        void ex_Expanded(object sender, RoutedEventArgs e)
        {
            foreach (UIElement elem in animes.Children)
            {
                if (elem != (UIElement)sender)
                {
                    ((Expander)elem).IsExpanded = false;
                }
            }
            Expander self = (Expander)sender;
            self.Header = entries[(int)self.Tag].Name;
            ((EntryControl)self.Content).TryPicUpdate(entries[(int)self.Tag].Image);
        }

        private void utorrent_Checked(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult r = MessageBox.Show("This option can cause unnecessarily downloaded files!", "Warning!", MessageBoxButton.OKCancel);
            //if (r == MessageBoxResult.OK)
            //{
            _ts = true;
            s_address.IsEnabled = true;
            s_pass.IsEnabled = true;
            s_port.IsEnabled = true;
            s_username.IsEnabled = true;
            address.IsEnabled = true;
            pass.IsEnabled = true;
            port.IsEnabled = true;
            username.IsEnabled = true;
            logintry.IsEnabled = true;

            //}
            //else
            //{
            //    no.IsChecked = true;
            //}
        }

        private void utorrent_Unchecked(object sender, RoutedEventArgs e)
        {
            logintry.Content = "Try";
            username.Text = "admin";
            port.Text = "8080";
            pass.Password = "admin";
            address.Text = "http://localhost";
            _ts = false;
            s_address.IsEnabled = false;
            s_pass.IsEnabled = false;
            s_port.IsEnabled = false;
            s_username.IsEnabled = false;
            address.IsEnabled = false;
            pass.IsEnabled = false;
            port.IsEnabled = false;
            logintry.IsEnabled = false;
            username.IsEnabled = false;
        }

        private void _misc_Expanded(object sender, RoutedEventArgs e)
        {
            _animes.IsExpanded = false;
        }

        private void _animes_Expanded(object sender, RoutedEventArgs e)
        {
            _misc.IsExpanded = false;
        }

        private void logintry_Click(object sender, RoutedEventArgs e)
        {
            uTorrent.Auth(username.Text, pass.Password, address.Text, port.Text, new Delegates.AcreCall<bool>(LoginCallback));

        }
        private void torrentcredChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = false;
            if (logintry != null)
                logintry.Content = "Try";
        }
        private void LoginCallback(bool a)
        {
            if (a)
            {
                logintry.Dispatcher.Invoke(new Delegates.AcreCall<string>(LogintryContentInvoke), new string[] { "Succeded" });
            }
            else
            {
                logintry.Dispatcher.Invoke(new Delegates.AcreCall<string>(LogintryContentInvoke), new string[] { "Failed" });
            }
        }
        private void LogintryContentInvoke(string text)
        {
            logintry.Content = text;
        }
        private void pass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            e.Handled = false;
            if (logintry != null)
                logintry.Content = "Try";
        }
    }
}
