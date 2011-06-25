using System;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace TlPlugin
{
    /// <summary>
    /// Translator Group plugin interface
    /// </summary>
    public interface ITlPlugin
    {
        string Name { get; }
        ITlPluginHost Host { get; set; }
        int[] GetTranslatedAnimeListIds();
        LatestResponse LatestEpisode(int id);
        Type type { get; set; }
        float GetStatus(int id); //(sgkk)
        bool StatusSupported();
        string GetLink();
        string GetTorrentLink(int id);
    }
    /// <summary>
    /// The host
    /// </summary>
    public interface ITlPluginHost
    {
        bool Register(ITlPlugin ipi);
    }
    public class LatestResponse
    {
        public LatestResponse(int ep, string tlink)
        {
            Ep = ep;
            TLink = tlink;
        }
        public int Ep { get; set; }
        public string TLink { get; set; }
    }
    public class Libs
    {
        public static string UnEscapeHtml(string input)
        {
            string ret = HttpUtility.HtmlDecode(input);
            do
            {
                ret = HttpUtility.HtmlDecode(ret);
            } while (HttpUtility.HtmlDecode(ret) != ret);
            return ret;

        }
        public static byte[] DownloadData(string link)
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(link);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            List<byte> bl = new List<byte>();
            Stream s = resp.GetResponseStream();
            do
            {
                int n = s.ReadByte();
                if (n < 0)
                    break;
                bl.Add((byte)n);
            } while (true);
            byte[] ret = new byte[bl.Count];
            bl.CopyTo(ret);
            return ret;
        }
        public static int IdOf(string[] where, string what)
        {
            int ret = -1;
            for (int i = 0; i < where.Length; i++)
            {
                if (where[i] == what)
                {
                    ret = i;
                }
            }
            return ret;
        }
        public static int IdOf(int[] where, int what)
        {
            int ret = -1;
            for (int i = 0; i < where.Length; i++)
            {
                if (where[i] == what)
                {
                    ret = i;
                }
            }
            return ret;
        }

        public static bool IsIn(int[] where, int what)
        {
            for (int i = 0; i < where.Length; i++)
            {
                if (where[i] == what)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public class PluginLib
    {
        public static Dictionary<string, ITlPlugin> LoadPlugins()
        {
            string path = Application.StartupPath + "\\Plugins";
            string[] pluginFiles = Directory.GetFiles(path, "*.Plugin.dll");
            Dictionary<string, ITlPlugin> ret = new Dictionary<string, ITlPlugin>(pluginFiles.Length);
            Type[] types = new Type[pluginFiles.Length];
            for (int i = 0; i < pluginFiles.Length; i++)
            {
                string args = pluginFiles[i].Substring(
                    pluginFiles[i].LastIndexOf("\\") + 1,
                    pluginFiles[i].IndexOf(".dll") -
                    pluginFiles[i].LastIndexOf("\\") - 1);

                Assembly ass = null;
                ass = Assembly.LoadFrom("Plugins\\" + args + ".dll");
                if (ass != null)
                {
                    types[i] = ass.GetType(args + ".PlugIn");
                }

                try
                {
                    // OK Lets create the object as we have the Report Type
                    if (types[i] != null)
                    {
                        ITlPlugin pl = (ITlPlugin)Activator.CreateInstance(types[i]);
                        ret.Add(pl.Name, pl);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            return ret;
        }
    }
}
