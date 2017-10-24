using System.Windows.Forms;

namespace Kuriimu.Kontract
{
    public interface IExtension
    {
        string Name { get; }
        System.Drawing.Image Icon { get; }
        Form CreateInstance();
    }
}
