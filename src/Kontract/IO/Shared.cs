using System.IO;
using System.Windows.Forms;

namespace Kuriimu.IO
{
    public enum ByteOrder : ushort
    {
        LittleEndian = 0xFEFF,
        BigEndian = 0xFFFE
    }

    public static class Shared
    {
        public static bool PrepareFiles(string openCaption, string saveCaption, string saveExtension, out FileStream openFile, out FileStream saveFile, bool isOut = false)
        {
            openFile = null;
            saveFile = null;

            var ofd = new OpenFileDialog
            {
                Title = openCaption,
                Filter = "All Files (*.*)|*.*"
            };

            if (ofd.ShowDialog() != DialogResult.OK) return false;
            openFile = File.OpenRead(ofd.FileName);

            var sfd = new SaveFileDialog()
            {
                Title = saveCaption,
                FileName = !isOut ? Path.GetFileNameWithoutExtension(ofd.FileName) + saveExtension + Path.GetExtension(ofd.FileName) : Path.GetFileName(ofd.FileName.Replace(saveExtension, string.Empty)),
                Filter = "All Files (*.*)|*.*"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
            {
                openFile.Dispose();
                return false;
            }
            saveFile = File.Create(sfd.FileName);

            return true;
        }

        /// <summary>
        /// Loops through a byte[] and reverse every 'count' bytes
        /// </summary>
        /// <param name="input">byte[] to reverse</param>
        /// <param name="count">Count of bytes to reverse</param>
        /// <returns>Reversed byte[]</returns>
        public static byte[] ReverseByteValues(byte[] input, int count = 4)
        {
            if (input.Length % count != 0) throw new System.Exception($"byte[] has to be dividable by {count}!");

            var result = new byte[input.Length];
            for (int i = 0; i < input.Length; i += count)
                for (int j = count - 1; j >= 0; j--)
                    result[i + (count - j)] = input[i + j];

            return result;
        }
    }
}