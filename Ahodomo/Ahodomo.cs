using System;
using TlPlugin;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;
namespace Ahodomo.Plugin
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
        { get { return "Ahodomo"; } }
        private string[] listname = { "Nichijou" };
        private int[] ids = { 10165 };
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
            string link = "http://www.nyaa.eu/?page=rss&user=87309";
            byte[] buffer = wc.DownloadData(link);
            string page = System.Text.Encoding.UTF8.GetString(buffer);
            Regex r = new Regex("(?<=(<title>)).*?(?=</title>)", RegexOptions.Singleline);
            List<string> eps = new List<string>();
            foreach (Match m in r.Matches(page))
            {
                string line = m.Value;
                if (line.Contains(anime))
                    eps.Add(line);
            }
            List<int> epnums = eps.ConvertAll<int>(new Converter<string, int>(AnimeTitleToEpNum));
            epnums.RemoveAll(new Predicate<int>(NotEp));
            return epnums[0];
        }
        private bool NotEp(int num)
        {
            return num < 0;
        }
        private int AnimeTitleToEpNum(string line)
        {
            string[] ls = line.Split(new char[] { ' ' });
            foreach (string lin in ls)
            {
                try
                {
                    return Convert.ToInt32(lin);
                }
                catch { }
            }
            return -1;
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