using System;
using TlPlugin;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Web;
namespace CommieSubs.Plugin
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
        public string Name
        { get { return "CommieSubs"; } }
        private string[] htmlname = { "Deadman Wonderland", "Denpa Onna to Seishun Otoko",
                              "Hanasaku Iroha", "Nichijou", "The Tatami Galaxy", "Tiger & Bunny", "Steins;Gate",
                              "Maria†Holic Alive", "The World God Only Knows S2", "X-Men"
                           };
        private int[] ids = { 6880, 9379, 9289, 10165, 7785, 9941, 9253, 9712, 10080, 6919 };
        public int[] GetTranslatedAnimeListIds()
        {

            return ids;
        }

        public float GetStatus(int id)
        {
            throw new NotSupportedException();
        }
        public Type type { get { return m_type; } set { m_type = value; } }
        public string GetLink() { return "http://commiesubs.com/"; }
        public int LatestEpisode(int id)
        {
            string anime = htmlname[Libs.IdOf(ids, id)];
            WebClient wc = new WebClient();
            string link = "http://www.nyaa.eu/?page=rss&user=76430";
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
                    eps.Add(ep);
            } while (true);
            List<int> epnums = eps.ConvertAll<int>(new Converter<string, int>(AnimeTitleToEpNum));
            //epnums.RemoveAll(new Predicate<int>(IsEp));
            epnums.Sort(new Comparison<int>(SortComp));
            return epnums[0];

        }
        private int SortComp(int x, int y)
        {
            return y - x;
        }
        private int AnimeTitleToEpNum(string line)
        {
            //line = HttpUtility.HtmlDecode(line); // shouldn't be needed
            do
            {
                int st = line.IndexOf('[');
                int end = line.IndexOf(']') + 1;
                if (st < 0 || end < 0)
                    break;
                line = line.Remove(st, end - st);
            } while (true);
            int nst = line.LastIndexOf("- ") + 2;
            //int nend = line.LastIndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) +1;

            if (nst < 2)
                return -1;
            int nend = line.IndexOf(' ', nst);
            if (nend < 0)
                return -1;
            line = line.Substring(nst, nend - nst); // still contains v0s v1s v2s etc
            nst = line.IndexOf('v');
            if (nst < 0)
            {
                try
                {
                    return Convert.ToInt32(line);
                }
                catch
                {
                    return -1;
                }

            }
            line = line.Remove(nst);
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
