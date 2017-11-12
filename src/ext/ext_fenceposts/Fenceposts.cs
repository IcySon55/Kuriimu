using System.Drawing;
using System.Windows.Forms;
using ext_fenceposts.Properties;
using Kontract.Interface;

namespace ext_fenceposts
{
    public sealed class Fenceposts : IExtension
    {
        #region Properties

        public string Name => Settings.Default.PluginName;

        public Image Icon => Resources.icon;

        #endregion

        public Form CreateInstance()
        {
            frmExtension configure = new frmExtension();
            configure.StartPosition = FormStartPosition.CenterParent;
            return configure;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}