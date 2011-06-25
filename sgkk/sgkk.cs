using System;
using TlPlugin;
using System.Net;
using System.IO;
using System.Drawing;
using System.Net.Cache;

namespace sgkk.Plugin
{
    class PlugIn : ITlPlugin
    {
        private ITlPluginHost m_Host;
        internal Type m_type = null;
        public PlugIn()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
        }
        public bool StatusSupported() { return true; }
        public string Name
        { get { return "sgkk"; } }
        private string[] phpname = { "bleach", "deadman", "beelzebub",
                              "toriko"
                           };
        private string[] torrentname = { "Bleach", "Deadman_Wonderland", "Beelzebub",
                              "Toriko"
                           };
        private int[] ids = { 269, 6880, 9513, 10033 };
        public int[] GetTranslatedAnimeListIds()
        {

            return ids;
        }

        public string GetTorrentLink(int id)
        {
            throw new NotImplementedException();
        }
        private Color sgkkgreen = Color.FromArgb(54, 198, 3);
        public float GetStatus(int id)
        {
            string anime = phpname[Libs.IdOf(ids, id)];
            WebClient wc = new WebClient();
            string link = "http://www.sgkkfansubs.com/status/"+anime+".php";
            byte[] buffer = wc.DownloadData(link);
            MemoryStream ms = new MemoryStream(buffer);          
            Bitmap bmp = new Bitmap(ms);
            int green = 0;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    if (sgkkgreen == bmp.GetPixel(i, j))
                    {
                        green++;
                    }
                }
            }
            if (green > 130)
            {
                return 100;
            }
            else
            {
                return ((float)green / 138 * 100);
            }    
        }
        public Type type { get { return m_type; } set { m_type = value; } }
        public string GetLink() { return "http://www.sgkkfansubs.com/"; }
        public LatestResponse LatestEpisode(int id)
        {
            string anime = phpname[Libs.IdOf(ids, id)];
            WebClient wc = new WebClient();
            byte[] buffer = wc.DownloadData("http://www.sgkkfansubs.com/projects/"+anime+".php");
            MemoryStream ms = new MemoryStream(buffer);
            StreamReader sr = new StreamReader(ms);
            string page = sr.ReadToEnd();
            anime = torrentname[Libs.IdOf(phpname, anime)] + '_';
            int start = page.LastIndexOf(anime, StringComparison.CurrentCultureIgnoreCase) + anime.Length + 2;
            int end = page.IndexOfAny(new char[] {'_',' ','v'}, start);
            string ss = page.Substring(start, end - start);
            return new LatestResponse(Convert.ToInt32(ss), "");
        }
        public ITlPluginHost Host
        {
            get { return m_Host; }
            set
            {
                m_Host = value;
                m_Host.Register(this);
            }
        }
    }
}
