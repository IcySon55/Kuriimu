using System;
using System.Runtime.InteropServices;

namespace Kuriimu.Contract.UI
{
    public class NativeTreeView : System.Windows.Forms.TreeView
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            Win32.SetWindowTheme(Handle, "explorer", null);
        }
    }
}
