using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Net;
using TlPlugin;
using System.Runtime.Serialization;
using System.Web;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Diagnostics;
namespace Acre
{
    [Serializable]
    public class AnimeEntry : ISerializable
    {
        /// <summary>
        /// Creates a new AnimeEnty based on its ID
        /// </summary>
        /// <param name="id">the AnimeEntry's MAL ID</param>
        public AnimeEntry(int id)
        {
            AnimeId = id;
        }
        public AnimeEntry(SerializationInfo info, StreamingContext context)
        {
            _image = (byte[])info.GetValue("Image", typeof(byte[]));
            Status = (AnimeStatus)info.GetValue("Status", typeof(AnimeStatus));
            _name = (string)info.GetValue("Name", typeof(string));
            TLGroup = (string)info.GetValue("TLGroup", typeof(string));
        }
        private string _name;
        private bool NameLock = false;
        private bool ImageLock = false;
        public string Name
        {
            get
            {
                if (_name == null && !NameLock)
                {
                    ANS.GetName(AnimeId, new Delegates.AcreCall<string>(NameCallback));
                    NameLock = true;
                    return "Loading";
                }
                return _name;
            }
        }
        /// <summary>
        /// Index in entries[]
        /// </summary>
        public int ArrayId { get; set; }
        /// <summary>
        /// MAL ID
        /// </summary>
        public int AnimeId { get; set; }
        private byte[] _image;
        public byte[] Image
        {
            get
            {
                if (_image == null && !ImageLock)
                {
                    ANS.GetImage(AnimeId, new Delegates.AcreCall<byte[]>(ImageCallback));
                    ImageLock = true;
                    return null;
                }
                return _image;
            }
        }
        public AnimeTimer Timer { get; set; }
        public string TLGroup { get; set; }
        public AnimeStatus Status { get; set; }
        public bool Notify { get; set; }
        public string TorrentPath { get; set; }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Image", Image);
            info.AddValue("Status", Status);
            info.AddValue("Name", _name);
            info.AddValue("TLGroup", TLGroup);
            info.AddValue("Notifiy", Notify);
            info.AddValue("TorrentPath", TorrentPath);
        }
        private void NameCallback(string name)
        {
            _name = name;
        }

        private void ImageCallback(byte[] img)
        {
            _image = img;
        }
    }
    [Serializable]
    public class AnimeStatus : ISerializable
    {
        public AnimeStatus(SerializationInfo info, StreamingContext context)
        {
            Latest = (int)info.GetValue("Latest", typeof(int));
            Status = (float)info.GetValue("Status", typeof(float));
        }
        public AnimeStatus()
        {
            Latest = -1;
            Status = -1;
        }
        public int Latest { get; set; }
        public float Status { get; set; }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Latest", Latest);
            info.AddValue("Status", Status);
        }
        public override string ToString()
        {
            return string.Format("Updated!\r\nLatest Episode: {0}{1}", this.Latest, (this.Status == -1 ? "" :
                        string.Format("\r\nStatus({0}):{1}%", (this.Status == 100 ? this.Latest.ToString() : (this.Latest + 1).ToString()), this.Status)));
        }
    }
    public class Delegates
    {
        public delegate void AcreCall<in T>(T data);
        public delegate T AcreVariable<out T>();
    }
    public class AnimeTimer : System.Timers.Timer
    {
        public AnimeTimer(int arrayid)
        {
            this.ArrayId = arrayid;
            this.AutoReset = true;
        }
        public int ArrayId { get; set; }
    }
    public class ANS
    {
        private static int[] ids = null;
        public static void GetName(int id, Delegates.AcreCall<string> cb)
        {
            WebClient wc = new WebClient();
            wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(wc_DownloadDataCompleted);
            wc.DownloadDataAsync(new Uri("http://mal-api.com/anime/" + id + "?format=xml"), cb);
        }

        private static void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            string page = System.Text.Encoding.UTF8.GetString(e.Result);
            int st = page.IndexOf("<title>") + 7;
            string name = page.Substring(st, page.IndexOf("</title>", st) - st);
            name = UnEscapeHtml(name);
            ((Delegates.AcreCall<string>)e.UserState).Invoke(name);
        }
        public static int[] GetAnimeIds(Dictionary<string, ITlPlugin> plugindir)
        {
            if (ids != null)
            {
                return ids;
            }
            else
            {
                List<int> list = new List<int>();
                foreach (KeyValuePair<string, ITlPlugin> pair in plugindir)
                {
                    foreach (int id in pair.Value.GetTranslatedAnimeListIds())
                    {
                        if (!list.Contains(id))
                            list.Add(id);
                    }
                }
                ids = new int[list.Count];
                list.CopyTo(ids);
                return ids;
            }
        }
        public static string UnEscapeHtml(string input)
        {
            string ret = HttpUtility.HtmlDecode(input);
            do
            {
                ret = HttpUtility.HtmlDecode(ret);
            } while (HttpUtility.HtmlDecode(ret) != ret);
            return ret;

        }
        internal static void GetImage(int id, Delegates.AcreCall<byte[]> cb)
        {
            WebClient wc = new WebClient();
            wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(wc_DownloadDataCompleted2);
            wc.DownloadDataAsync(new Uri("http://mal-api.com/anime/" + id + "?format=xml"), cb);
        }

        static void wc_DownloadDataCompleted2(object sender, DownloadDataCompletedEventArgs e)
        {
            string page = System.Text.Encoding.UTF8.GetString(e.Result);
            int st = page.IndexOf("<image_url>") + 11;
            string link = page.Substring(st, page.IndexOf("</image_url>", st) - st);
            WebClient img = new WebClient();
            img.DownloadDataCompleted += new DownloadDataCompletedEventHandler(img_DownloadDataCompleted);
            img.DownloadDataAsync(new Uri(link), e.UserState);
        }

        static void img_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            ((Delegates.AcreCall<byte[]>)e.UserState).Invoke(e.Result);
        }
    }
    public class uTorrent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Username</param>
        /// <param name="pass">Password</param>
        /// <param name="authcallback">The callback used for returning the succesful autentication's token</param>
        public static void Auth(string name, string pass, string link, string port, Delegates.AcreCall<bool> authcallback)
        {
            loc = link + ':' + port;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(loc + "/gui/token.html");
            creds = new NetworkCredential(name, pass);
            req.Credentials = creds;
            req.BeginGetResponse(new AsyncCallback(AuthPrivateCallback), new AuthState(req, authcallback));
            return;
        }
        private static string loc;
        private static NetworkCredential creds;
        [Flags]
        public enum Statuses
        {
            Started = 1,
            Checking = 2,
            StartAfterCheck = 4,
            Checked = 8,
            Error = 16,
            Paused = 32,
            Queued = 64,
            Loaded = 128
        }
        private static void AuthPrivateCallback(IAsyncResult res)
        {
            if (res.IsCompleted)
            {
                AuthState stat = (AuthState)res.AsyncState;
                try
                {
                    StreamReader s = new StreamReader(stat.req.GetResponse().GetResponseStream());
                    string page = s.ReadToEnd();
                    Regex r = new Regex("(?<=('>)).*(?=</d)");
                    token = "";
                    if (r.IsMatch(page))
                    {
                        token = r.Match(page).Value;
                        stat.authcallback.Invoke(true);
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(loc + "/gui/?token=" + token + "&action=getsettings");
                        req.Credentials = creds;
                        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                        StreamReader sr = new StreamReader(resp.GetResponseStream());
                        string page2 = sr.ReadToEnd();
                        //(?<=("exe_path",2,")).*
                        r = new Regex("(?<=(exe_path\",2,\")).*(?=\"])");
                        if (r.IsMatch(page2))
                        {
                            exe_path = r.Match(page2).Value;
                        }
                    }
                    else
                    {
                        stat.authcallback.Invoke(false);
                    }
                }
                catch { stat.authcallback.Invoke(false); }

            }
        }
        private static string exe_path;
        private static string token = "";
        private class AuthState
        {
            public AuthState(HttpWebRequest r, Delegates.AcreCall<bool> a) { req = r; authcallback = a; }
            public HttpWebRequest req;
            public Delegates.AcreCall<bool> authcallback;
        }
        public class Torrent
        {
            public Torrent(Uri uri)
            {
                WebClient wc = new WebClient();
                string path = @".\Cache\Torrents\" + Guid.NewGuid().ToString("N") + ".torrent";
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
                wc.DownloadFileAsync(uri, path, path);
            }
            public Torrent(string path)
            {
                this.path = path;
                FileStream torrentfile = new FileStream(path, FileMode.Open, FileAccess.Read);
                filearray = new byte[torrentfile.Length];
                torrentfile.Read(filearray, 0, filearray.Length);
                torrentfile.Close();
                string file = System.Text.Encoding.UTF8.GetString(filearray);
                int ind = file.IndexOf("4:name") + 6;
                int i = ind;
                bool f = false;
                string l = "";
                do
                {
                    if (f)
                    {
                        if (char.IsDigit(file[i]))
                        {
                            l += file[i];
                            i++;
                            continue;
                        }
                        else
                            break;
                    }
                    if (char.IsDigit(file[i]))
                    {
                        f = true;
                        i--;
                    }
                    if (i == file.Length)
                        break;
                    i++;
                } while (true);
                Name = file.Substring(ind + l.Length + 1, Convert.ToInt32(l));
            }
            void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
            {
                string path = (string)e.UserState;
                this.path = path;
                FileStream torrentfile = new FileStream(path, FileMode.Open, FileAccess.Read);
                filearray = new byte[torrentfile.Length];
                torrentfile.Read(filearray, 0, filearray.Length);
                torrentfile.Close();
                string file = System.Text.Encoding.UTF8.GetString(filearray);
                int ind = file.IndexOf("4:name") + 6;
                int i = ind;
                bool f = false;
                string l = "";
                do
                {
                    if (f)
                    {
                        if (char.IsDigit(file[i]))
                        {
                            l += file[i];
                            i++;
                            continue;
                        }
                        else
                            break;
                    }
                    if (char.IsDigit(file[i]))
                    {
                        f = true;
                        i--;
                    }
                    if (i == file.Length)
                        break;
                    i++;
                } while (true);
                Name = file.Substring(ind + l.Length + 1, Convert.ToInt32(l));
            }
            private string path;
            private byte[] filearray;
            private string hash;
            /// <summary>
            /// NO trailing backslash
            /// </summary>
            public string DestinationFolder { get; set; }
            public float Percentage
            {
                get
                {
                    if (Name != null)
                    {
                        string page2 = GetPage();
                        if (hash == null)
                        {
                            Regex r = new Regex("(?<=(\\[\")).*?(?=(\",[0-256].*" + Name + "))");
                            if (r.IsMatch(page2))
                                hash = r.Match(page2).Value;
                        }
                        Regex r2 = new Regex("(?<=(\\[\")).*" + hash + ".*(?=\\])");
                        if (r2.IsMatch(page2))
                        {
                            string[] line = r2.Match(page2).Value.Split(new char[] { ',' });
                            return Convert.ToInt32(line[line.Length - 15]);
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            private string GetPage()
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(loc + "/gui/?token=" + token + "&list=1");
                req.Credentials = creds;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                return sr.ReadToEnd();
            }
            public int Eta
            {
                get {
                    if (Name != null)
                    {
                        string page2 = GetPage();
                        if (hash == null)
                        {
                            Regex r = new Regex("(?<=(\\[\")).*?(?=(\",[0-256].*" + Name + "))");
                            if (r.IsMatch(page2))
                                hash = r.Match(page2).Value;
                        }
                        Regex r2 = new Regex("(?<=(\\[\")).*"+hash+".*(?=\\])");
                        if (r2.IsMatch(page2))
                        {
                            string[] line = r2.Match(page2).Value.Split(new char[] { ',' });
                            return Convert.ToInt32(line[line.Length - 9]);
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            public string Name { get; set; }
            public byte Status
            {
                get
                {
                    if (Name != null)
                    {
                        string page2 = GetPage();
                        if (hash == null)
                        {
                            Regex r = new Regex("(?<=(\\[\")).*?(?=(\",[0-256].*" + Name + "))");
                            if (r.IsMatch(page2))
                                hash = r.Match(page2).Value;
                        }
                        Regex r2 = new Regex("(?<=(" + hash + "\",))[0-256].*?(?=,)");
                        if (r2.IsMatch(page2))
                        {
                            string r = r2.Match(page2).Value;
                            try
                            {
                                return Convert.ToByte(r);
                            }
                            catch { }
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            public void Add()
            {
                if (this.path == null || this.path == "")
                    throw new NotSupportedException();
                ProcessStartInfo psi = new ProcessStartInfo(exe_path, "/DIRECTORY \"" + DestinationFolder + "\" \"" + path + "\"");
                Process.Start(psi);
            }
        }
    }
}
