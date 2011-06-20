using System;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace TlPlugin
{
	/// <summary>
	/// Translator Group plugin interface
	/// </summary>
	public interface ITlPlugin
	{
		string Name{get;}
        ITlPluginHost Host { get; set; }
        int[] GetTranslatedAnimeListIds();
        int LatestEpisode(int id);
        Type type{get; set;}
        float GetStatus(int id); //(sgkk)
        bool StatusSupported();
        string GetLink();
	}
	/// <summary>
	/// The host
	/// </summary>
    public interface ITlPluginHost
	{
		bool Register(ITlPlugin ipi);
	}
    public class Libs
    {
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
