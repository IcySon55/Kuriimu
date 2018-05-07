using System.Drawing;
using System.Windows.Forms;
using ext_ridge_racer.Properties;
using Kontract.Interface;

namespace ext_ridge_racer
{
    public sealed class RidgeRacer : IExtension
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

        public Form CreateInstance(string[] args)
        {
            frmExtension configure = new frmExtension(args);
            configure.StartPosition = FormStartPosition.CenterParent;
            return configure;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
