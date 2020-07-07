using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Knit
{
    public static class Common
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);

        public static string ProcessVariableTokens(string input, Dictionary<string, object> variableCache)
        {
            // Replace standard tokens
            var result = variableCache.Aggregate(input, (str, pair) => str.Replace("{" + pair.Key + "}", pair.Value.ToString()));

            // Handle variable modifiers
            foreach (var key in variableCache.Keys)
            {
                // DIRECTORY_NAME - Perform Path.GetDirectoryName on the value
                var pathKey = "{" + key + ":DIRECTORY_NAME}";
                if (result.Contains(pathKey))
                    result = Regex.Replace(result, pathKey + @"\\?", Path.GetDirectoryName(variableCache[key].ToString()) + "\\");

                // FILE_NAME - Perform Path.GetFileName on the value
                pathKey = "{" + key + ":FILE_NAME}";
                if (result.Contains(pathKey))
                    result = Regex.Replace(result, pathKey, Path.GetFileName(variableCache[key].ToString()));

                // FILE_NAME_NO_EXT - Perform Path.GetFileNameWithoutExtension on the value
                pathKey = "{" + key + ":FILE_NAME_NO_EXT}";
                if (result.Contains(pathKey))
                    result = Regex.Replace(result, pathKey, Path.GetFileNameWithoutExtension(variableCache[key].ToString()));
            }

            return result;
        }

        public static void SetState(this ProgressBar pbr, ProgressBarStyle state)
        {
            try
            {
                SendMessage(pbr.Handle, 1040, (IntPtr)state, IntPtr.Zero);
            }
            catch (Exception) { }
        }
    }

    public enum ProgressBarStyle
    {
        Normal = 1,
        Error = 2,
        Pause = 3
    }
}
