using System.Drawing;
using System.Windows.Forms;
using ext_director.Properties;
using Kontract.Interface;

namespace ext_director
{
    public sealed class Director : IExtension
    {
        #region Properties

        public string Name => Settings.Default.PluginName;

        public Image Icon => Resources.icon;

        #endregion

        public Form CreateInstance()
        {
            Extension configure = new Extension();
            configure.StartPosition = FormStartPosition.CenterParent;
            return configure;
        }

        public Form CreateInstance(string[] args)
        {
            Extension configure = new Extension(args);
            configure.StartPosition = FormStartPosition.CenterParent;
            return configure;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}