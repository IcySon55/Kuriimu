using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Kontract.Interface
{
    public interface ILayoutInterpreter : IFilePlugin
    {
        // Feature Support
        bool LayoutHasExtendedProperties { get; } // Format provides an extended properties dialog?
        bool CanAddNodes { get; } // Is the plugin able to add nodes?
        bool CanDeleteNodes { get; } // Is the plugin able to delete nodes?
        bool CanSave { get; } // Is saving supported?

        // I/O
        FileInfo FileInfo { get; set; }
        bool Identify(string filename); // Determines if the given file is opened by the plugin.
        void Load(string filename);
        void Save(string filename = ""); // A non-blank filename is provided when using Save As...

        // Nodes
        ILayoutRoot Layout { get; } // The node tree root
        bool AddNode(INode node);
        bool DeleteNode(INode node);

        // Features
        bool ShowProperties(Icon icon);
        bool CreateNode(out INode node);
    }

    public interface ILayoutRoot
    {
        List<INode> Nodes { get; set; }

        void Render(Graphics gfx);
    }

    public interface INode
    {
        INode Parent { get; set; }
        List<INode> Nodes { get; set; }

        NodeProperties NodeProperties { get; set; }

        void Render(Graphics gfx);
    }

    public enum NodeProperties
    {
        NoProperties,
        PropertyGrid,
        PropertyDialog
    }
}
