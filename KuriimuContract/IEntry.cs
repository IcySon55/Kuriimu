using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KuriimuContract
{
	public interface IEntry : IComparable<IEntry>
	{
		Encoding Encoding { get; set; }
		string Name { get; set; }
		byte[] OriginalText { get; set; }
		byte[] EditedText { get; set; }

		string ToString();
		string GetOriginalString();
		string GetEditedString();
	}
}