using System.Drawing;
using System.Windows.Forms;

namespace Kuriimu.Contract
{
    public interface IExtension
    {
        string Name { get; }
        Image Icon { get; }
        Form CreateInstance();
    }
}