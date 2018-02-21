using System.Windows.Forms;

namespace Kontract.Interface
{
    public interface IExtension
    {
        string Name { get; }
        System.Drawing.Image Icon { get; }
        Form CreateInstance();
        Form CreateInstance(string[] args);
    }
}
