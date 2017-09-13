using System.IO;

namespace archive_dpk.DPK4
{
    public sealed class DPK4
    {
        // I/O Members
        public FileStream fs;
        public BinaryReader br;
        public string Filename;
        public bool isOpen = false;

        // DPK4 Header
        public string Header;
        public uint Size;
        public uint Directory;
        public uint numFiles;

        // Info gathering
        public uint decompTotal = 0, compTotal = 0;

        // Entries
        public DPK_Entry[] Entries;

        // Methods
        public bool openDPK(string str)
        {
            // If the filename passed is blank, just fail
            if (Filename == "")
            {
                isOpen = false;
                return false;
            }

            Filename = str;

            // Set the I/O readers
            fs = new FileStream(Filename, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(fs);

            // The file is now openened by the program
            isOpen = true;

            // Declare variables
            string s;
            int i;
            //double convert;

            if (File.Exists(Filename))
            {
                s = new string(br.ReadChars(4));
                Header = s;

                if (Header == "DPK4")
                {
                    Size = br.ReadUInt32();

                    if (Size == fs.Length)
                    {
                        Directory = br.ReadUInt32();
                        numFiles = br.ReadUInt32();

                        Entries = new DPK_Entry[numFiles];

                        // Array.Resize(ref DPK4.Entries, (int)++DPK4.numFiles);

                        for (i = 0; i < numFiles; i++)
                        {
                            Entries[i] = new DPK_Entry();
                            Entries[i].entryLen = br.ReadUInt32();
                            Entries[i].decompSize = br.ReadUInt32();
                            Entries[i].compSize = br.ReadUInt32();
                            Entries[i].Offset = br.ReadUInt32();
                            Entries[i].index = i;

                            decompTotal += Entries[i].decompSize;
                            compTotal += Entries[i].compSize;

                            s = new string(br.ReadChars((int)Entries[i].entryLen - 16));
                            Entries[i].Filename = s.Trim('\0');
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public void closeDPK()
        {
            if (isOpen)
            {
                br.Close();
                fs.Close();
                isOpen = false;
            }
        }
    }

    public class DPK_Entry
    {
        public uint entryLen = 0;   // Size of this entry
        public uint decompSize = 0; // If this equals the compressed size, the file was not compressed
        public uint compSize = 0;
        public uint Offset = 0;     // From the beginning of the archive
        public string Filename = "";

        public int index;
    }
}
