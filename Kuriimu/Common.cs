using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace Kuriimu
{
	static class Common
	{
		public static string GetAppMessage()
		{
			System.Version version = Assembly.GetExecutingAssembly().GetName().Version;

			return string.Format("Kuriimu v{0}.{1}.{2}.{3} built on {4}\r\nCopyright {6} IcySon55\r\n\r\n",
				version.Major,
				version.Minor,
				version.Build,
				version.Revision,
				RetrieveLinkerTimestamp().ToString("MMM dd yyyy hh:mm:ss"),
				Environment.NewLine,
				GetCopyrightYears(new DateTime(2016, 1, 1)));
		}

		public static DateTime RetrieveLinkerTimestamp()
		{
			string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;
			byte[] b = new byte[2048];

			using (var s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				s.Read(b, 0, 2048);
			}

			int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
			int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
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