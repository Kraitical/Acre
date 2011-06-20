using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace NewPlugin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            Directory.CreateDirectory(@".\" + name + @"\");
            string guid = Guid.NewGuid().ToString("B");
            #region files
            string csproj = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
"<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">" + Environment.NewLine +
"  <PropertyGroup>" + Environment.NewLine +
"    <Platform Condition=\" '$(Platform)' == '' \">Default</Platform>" + Environment.NewLine +
"    <ProductVersion>9.0.30729</ProductVersion>" + Environment.NewLine +
"    <SchemaVersion>2.0</SchemaVersion>" + Environment.NewLine +
"    <ProjectGuid>" + guid + "</ProjectGuid>" + Environment.NewLine +
"    <OutputType>Library</OutputType>" + Environment.NewLine +
"    <RootNamespace>" + name + ".Plugin</RootNamespace>" + Environment.NewLine +
"    <AssemblyName>" + name + ".Plugin</AssemblyName>" + Environment.NewLine +
"    <TargetFrameworkVersion>v2.0</TargetFrameworkVersion>" + Environment.NewLine +
"    <FileAlignment>512</FileAlignment>" + Environment.NewLine +
"    <PublishUrl>publish\\</PublishUrl>" + Environment.NewLine +
"    <Install>true</Install>" + Environment.NewLine +
"    <InstallFrom>Disk</InstallFrom>" + Environment.NewLine +
"    <UpdateEnabled>false</UpdateEnabled>" + Environment.NewLine +
"    <UpdateMode>Foreground</UpdateMode>" + Environment.NewLine +
"    <UpdateInterval>7</UpdateInterval>" + Environment.NewLine +
"    <UpdateIntervalUnits>Days</UpdateIntervalUnits>" + Environment.NewLine +
"    <UpdatePeriodically>false</UpdatePeriodically>" + Environment.NewLine +
"    <UpdateRequired>false</UpdateRequired>" + Environment.NewLine +
"    <MapFileExtensions>true</MapFileExtensions>" + Environment.NewLine +
"    <ApplicationRevision>0</ApplicationRevision>" + Environment.NewLine +
"    <ApplicationVersion>1.0.0.%2a</ApplicationVersion>" + Environment.NewLine +
"    <IsWebBootstrapper>false</IsWebBootstrapper>" + Environment.NewLine +
"    <UseApplicationTrust>false</UseApplicationTrust>" + Environment.NewLine +
"    <BootstrapperEnabled>true</BootstrapperEnabled>" + Environment.NewLine +
"  </PropertyGroup>" + Environment.NewLine +
"  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'RLS|x86' \">" + Environment.NewLine +
"    <PlatformTarget>x86</PlatformTarget>" + Environment.NewLine +
"    <OutputPath>bin\x86\\RLS\\</OutputPath>" + Environment.NewLine +
"    <Optimize>true</Optimize>" + Environment.NewLine +
"  </PropertyGroup>" + Environment.NewLine +
"  <ItemGroup>" + Environment.NewLine +
"    <Compile Include=\"" + name + ".cs\" />" + Environment.NewLine +
"  </ItemGroup>" + Environment.NewLine +
"  <ItemGroup>" + Environment.NewLine +
"    <Reference Include=\"System\" />" + Environment.NewLine +
"  </ItemGroup>" + Environment.NewLine +
"  <ItemGroup>" + Environment.NewLine +
"    <ProjectReference Include=\"..\\PlugIn\\_PlugIn.csproj\">" + Environment.NewLine +
"      <Project>{0D3ECFE3-92D6-46DE-9D67-16759384458B}</Project>" + Environment.NewLine +
"      <Name>_PlugIn</Name>" + Environment.NewLine +
"    </ProjectReference>" + Environment.NewLine +
"  </ItemGroup>" + Environment.NewLine +
"  <Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />" + Environment.NewLine +
"  <Target Name=\"AfterBuild\">" + Environment.NewLine +
"    <Copy SourceFiles=\"$(TargetPath)\" DestinationFolder=\"$(SolutionDir)bin\\Plugins\" ContinueOnError=\"true\" />" + Environment.NewLine +
"  </Target>" + Environment.NewLine +
"</Project>";


            string cs = "using System;" + Environment.NewLine +
"using PlugIn;" + Environment.NewLine +
"namespace " + name + ".Plugin" + Environment.NewLine +
"{" + Environment.NewLine +
"    class PlugIn : IPlugin" + Environment.NewLine +
"    {" + Environment.NewLine +
"        private IPluginHost m_Host;" + Environment.NewLine +
"        internal Type m_type = null;" + Environment.NewLine +
"        public PlugIn()" + Environment.NewLine +
"        {" + Environment.NewLine +
"            System.Net.ServicePointManager.Expect100Continue = false;" + Environment.NewLine +
"        }" + Environment.NewLine +
"        public bool StatusSupported() { return false; }" + Environment.NewLine +
"        public string Name" + Environment.NewLine +
"        { get { return \"" + name + "\"; } }" + Environment.NewLine +
"        private string[] listname = { \"\" };" + Environment.NewLine +
"        private int[] ids = {  };" + Environment.NewLine +
"        public int[] GetTranslatedAnimeListIds()" + Environment.NewLine +
"        {" + Environment.NewLine +
"            return ids;" + Environment.NewLine +
"        }" + Environment.NewLine +
"        public string GetStatus(int id)" + Environment.NewLine +
"        {" + Environment.NewLine +
"            return \"\";" + Environment.NewLine +
"        }" + Environment.NewLine +
"        public Type type { get { return m_type; } set { m_type = value; } }" + Environment.NewLine +
"        public string GetLink() { return \"\"; }" + Environment.NewLine +
"        public int LatestEpisode(int id)" + Environment.NewLine +
"        {" + Environment.NewLine +
"            " + Environment.NewLine +
"        }" + Environment.NewLine +
"        public IPluginHost Host" + Environment.NewLine +
"        {" + Environment.NewLine +
"            get { return m_Host; }" + Environment.NewLine +
"            set" + Environment.NewLine +
"            {" + Environment.NewLine +
"                m_Host = value;" + Environment.NewLine +
"                m_Host.Register(this);" + Environment.NewLine +
"            }" + Environment.NewLine +
"        }" + Environment.NewLine +
"    }" + Environment.NewLine +
"}";
            #endregion
            FileStream fs = new FileStream(@".\" + name + @"\" + name + ".csproj", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            sw.Write(csproj);
            sw.Close();
            fs = new FileStream(@".\" + name + @"\" + name + ".cs", FileMode.Create);
            sw = new StreamWriter(fs);
            sw.Write(cs);
            sw.Close();

            //TODO: Insert to sln
        }
    }
}
