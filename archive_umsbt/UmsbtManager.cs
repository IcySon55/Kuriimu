using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;

namespace archive_umsbt
{
    public class UmsbtManager : IArchiveManager
    {
        private UMSBT _umsbt = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "UMSBT Archive";
        public string Extension => "*.umsbt";
        public string About => "This is the UMSBT archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            // TODO: Make this way more robust
            return filename.EndsWith(".umsbt");
        }

        public LoadResult Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            if (!FileInfo.Exists) return LoadResult.FileNotFound;

            _umsbt = new UMSBT(FileInfo.OpenRead());
            return LoadResult.Success;
        }

        public SaveResult Save(string filename = "")
        {
            SaveResult result = SaveResult.Success;

            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            try
            {
                // Save As...
                if (!string.IsNullOrEmpty(filename))
                {
                    _umsbt.Save(FileInfo.Create());
                    _umsbt.Close();
                }
                else
                {
                    // Create the temp file
                    _umsbt.Save(File.Create(FileInfo.FullName + ".tmp"));
                    _umsbt.Close();
                    // Delete the original
                    FileInfo.Delete();
                    // Rename the temporary file
                    File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
                }

                // Reload the new file to make sure everything is in order
                Load(FileInfo.FullName);
            }
            catch (Exception)
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        public void Unload()
        {
            _umsbt?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _umsbt.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            return false;
        }

        public bool DeleteFile(ArchiveFileInfo afi)
        {
            return false;
        }

        // Features
        public bool ShowProperties(Icon icon)
        {
            return false;
        }
    }
}