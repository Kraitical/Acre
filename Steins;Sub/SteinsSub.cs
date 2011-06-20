using TlPlugin;
using System.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
namespace SteinsSub.Plugin
{
    class PlugIn : ITlPlugin
    {
        private ITlPluginHost m_Host;
        internal Type m_type = null;
        public PlugIn()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
        }
        public bool StatusSupported() { return false; }
        public string Name { get { return "Stein;Sub"; } }
        private string[] listname = { "Steins;Gate" };
        private int[] ids = { 9253 };
        public int[] GetTranslatedAnimeListIds()
        {
            return ids;
        }
        public float GetStatus(int id)
        {
            throw new NotSupportedException();
        }
        public Type type { get { return m_type; } set { m_type = value; } }
        public string GetLink() { return ""; }
        public int LatestEpisode(int id)
        {
            string anime = listname[Libs.IdOf(ids, id)];
            WebClient wc = new WebClient();
            string link = "http://www.nyaa.eu/?page=rss&user=107733";
            byte[] buffer = wc.DownloadData(link);
            MemoryStream ms = new MemoryStream(buffer);
            StreamReader sr = new StreamReader(ms);
            string page = sr.ReadToEnd();
            List<string> eps = new List<string>();
            int start = page.IndexOf("<item>");
            do
            {
                start = page.IndexOf("<title>", start) + 7;
                if (start < 7)
                {
                    break;
                }
                int end = page.IndexOf("</title>", start);
                string ep = page.Substring(start, end - start);
                ep = HttpUtility.HtmlDecode(ep);
                if (ep.Contains(anime))
                {
                    ep = ep.Replace(anime, "");
                    ep = ep.Replace("_", "");
                    eps.Add(ep);
                }
            } while (true);
            List<int> epnums = eps.ConvertAll<int>(new Converter<string, int>(AnimeTitleToEpNum));
            epnums.Sort(new Comparison<int>(SortComp));
            try
            {
                return epnums[0];
            }
            catch
            {
                throw new Exception("problem in:" + Name + ":" + epnums.Count + " page: " + page);
            }
        }
        private int SortComp(int x, int y)
        {
            return y - x;
        }
        private int AnimeTitleToEpNum(string line)
        {
            //[Shikkaku]_Oretachi_ni_Tsubasa_wa_Nai_08_[1280x720][AAC][220E86D7].mkv
            //line = HttpUtility.HtmlDecode(line); // shouldn't be needed
            do
            {
                int st = line.IndexOf('[');
                int end = line.IndexOf(']') + 1;
                if (st < 0 || end < 0)
                    break;
                line = line.Remove(st, end - st);
            } while (true);
            int dot = line.LastIndexOf('.');
            if (dot < 0)
                return -1;
            line = line.Remove(dot);
            line = line.Replace("_", "");
            try
            {
                return Convert.ToInt32(line);
            }
            catch
            {
                return -1;
            }
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
