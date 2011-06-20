using System;
using TlPlugin;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Web;
namespace CMS.Plugin
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
        { get { return "ColorMeSubbed"; } }
        private string[] listname = { "Beelzebub", "The World God Only Knows II" };
        private int[] ids = { 9513, 10080 };
        public int[] GetTranslatedAnimeListIds()
        {
            return ids;
        }
        public float GetStatus(int id)
        {
            throw new NotSupportedException();
        }
        public Type type { get { return m_type; } set { m_type = value; } }
        public string GetLink() { return "http://www.colormesubbed.com"; }
        public int LatestEpisode(int id)
        {
            //http://www.nyaa.eu/?page=rss&user=91833

            string anime = listname[Libs.IdOf(ids, id)];
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://xdcc.colormesubbed.com");
            req.CookieContainer = new CookieContainer();
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            CookieCollection cookies = resp.Cookies;
            req = (HttpWebRequest)WebRequest.Create("http://xdcc.colormesubbed.com/search.php?nick=[CMS]Anarchy");
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(cookies);
            resp = (HttpWebResponse)req.GetResponse();
            Stream st = resp.GetResponseStream();
            StreamReader sr2 = new StreamReader(st);
            string text = sr2.ReadToEnd();
            text = text.Replace("p.k", Environment.NewLine + "p.k");
            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 1; i < lines.Length; i++)
            {
                string work = lines[i];
                work = work.Remove(work.IndexOf("p.k"), work.IndexOf("f:\"") + 3);
                work = work.Replace("\"};", "");
                lines[i] = work;
            }
            for (int i = 1; i < lines.Length; i++)
            {
                string work = lines[i];
                do
                {
                    int start = work.IndexOf('[');
                    if (start < 0)
                        break;
                    work = work.Remove(start, work.IndexOf(']') - start + 1);
                } while (true);
                lines[i] = work;
            }
            List<string> corrects = new List<string>();
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Contains(anime)) // UNDONE
                    corrects.Add(lines[i]);
            }
            for (int i = 0; i < corrects.Count; i++)
            {
                string work = corrects[i];
                int end = work.IndexOfAny(new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' });
                work = work.Remove(0, end);
                int start = work.LastIndexOfAny(new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' });
                work = work.Remove(start + 1);
                if (work.Contains("v"))
                {
                    work = work.Remove(work.IndexOf('v'));
                }
                corrects[i] = work;
            }
            List<int> eps = corrects.ConvertAll<int>(new Converter<string, int>(ConvertToAnimeEp));
            eps.RemoveAll(new Predicate<int>(IsEp));
            eps.Sort(new Comparison<int>(SortComp));
            sr2.Close();
            resp.Close();
            return eps[0];
            /*
            string anime = listname[Libs.IdOf(ids, id)];
            WebClient wc = new WebClient();
            string link = "http://www.nyaa.eu/?page=rss&user=91833";
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
            */
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
        private int SortComp(int x, int y)
        {
            return y - x;
        }
        private bool IsEp(int num)
        {
            return num < 0;
        }
        private int ConvertToAnimeEp(string text)
        {
            try
            {
                return Convert.ToInt32(text);
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
