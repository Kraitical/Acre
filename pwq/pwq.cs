﻿using System;
using TlPlugin;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;


namespace pwq.Plugin
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
        { get { return "pwq"; } }
        private string[] listname = { "A Channel", "Astarotte_no_Omocha!" };
        private int[] ids = { 9776, 9736 };
        public int[] GetTranslatedAnimeListIds()
        {
            return ids;
        }
        public float GetStatus(int id)
        {
            throw new NotSupportedException();
        }
        public string GetTorrentLink(int id)
        {
            throw new NotImplementedException();
        }
        public Type type { get { return m_type; } set { m_type = value; } }
        public string GetLink() { return "http://pwqsubs.com"; }
        public LatestResponse LatestEpisode(int id)
        {
            string anime = listname[Libs.IdOf(ids, id)];
            string link = "http://www.nyaa.eu/?page=rss&user=63729";
            byte[] buffer = Libs.DownloadData(link);
            string page = System.Text.Encoding.UTF8.GetString(buffer);
            Regex r = new Regex("(?<=(<item>)).*?(?=(</item>))", RegexOptions.Singleline);
            foreach (Match m in r.Matches(page))
            {
                Regex ir = new Regex("(?<=(<title>)).*?(?=(</title>))");
                string item = Libs.UnEscapeHtml(m.Value);
                if (ir.IsMatch(item))
                {
                    string title = ir.Match(item).Value;
                    if (title.Contains(anime))
                    {
                        int ep = AnimeTitleToEpNum(title);
                        if (ep > 0)
                        {
                            Regex tl = new Regex("(?<=(<link>)).*?(?=(</link>))");
                            if (tl.IsMatch(item))
                                return new LatestResponse(ep, tl.Match(item).Value);
                        }
                    }
                }
            }
            return new LatestResponse(-1, "");
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
