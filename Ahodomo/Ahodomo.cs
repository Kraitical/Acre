using System;
using TlPlugin;
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
        private string[] listname = { "" };
        private int[] ids = {  };
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
            return 0;
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