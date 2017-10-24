namespace Kontract.Interface
{
    public interface IPlugin
    {
        // Information
        string Name { get; }
        string Description { get; } // i.e. return "Kuriimu Archive";
        string Extension { get; } // i.e. return "*.ext";
        string About { get; }
    }
}
