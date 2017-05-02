using System;
using System.Reflection;

namespace Kuriimu
{
    static class Common
    {
        public static string GetAppMessage()
        {
            //Version version = Assembly.GetExecutingAssembly().GetName().Version;

            return string.Format("Kuriimu v{0} built on {1}\r\nCopyright {2} IcySon55\r\n\r\n",
                Version.gitLastTag + "." + Version.gitCount,
                RetrieveLinkerTimestamp().ToString("MMM dd yyyy hh:mm:ss"),
                GetCopyrightYears(new DateTime(2016, 1, 1)));
        }

        public static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];

            using (var s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                s.Read(b, 0, 2048);
            }

            int i = BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }

        private static string GetCopyrightYears(DateTime startYear)
        {
            if (startYear.Year == DateTime.Now.Year)
                return DateTime.Now.Year.ToString();
            else
                return startYear.Year + "-" + DateTime.Now.Year;
        }
    }
}