using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;

namespace file_ttbin
{
	#region Entry_Definition
	public sealed class Entry : IEntry
	{
		// Interface
		public string Name
		{
			get { return EditedLabel.Name; }
			set { }
		}

		public string OriginalText => OriginalLabel.Text;

		public string EditedText
		{
			get { return EditedLabel.Text; }
			set { EditedLabel.Text = value; }
		}

		public int MaxLength { get; set; }

		public IEntry ParentEntry { get; set; }

		public bool IsSubEntry => ParentEntry != null;

		public bool HasText { get; }

		public List<IEntry> SubEntries { get; set; }

		// Adapter
		public Label OriginalLabel { get; }
		public Label EditedLabel { get; set; }

		public Entry()
		{
			OriginalLabel = new Label();
			EditedLabel = new Label();

			Name = string.Empty;
			MaxLength = 0;
			ParentEntry = null;
			HasText = true;
			SubEntries = new List<IEntry>();
		}

		public Entry(Label editedLabel) : this()
		{
			if (editedLabel != null)
				EditedLabel = editedLabel;
		}

		public Entry(Label editedLabel, Label originalLabel) : this(editedLabel)
		{
			if (originalLabel != null)
				OriginalLabel = originalLabel;
		}

		public override string ToString()
		{
			return Name == string.Empty ? EditedLabel.TextOffset.ToString("X2") : Name;
		}

		public int CompareTo(IEntry rhs)
		{
			int result = Name.CompareTo(rhs.Name);
			if (result == 0)
				result = EditedLabel.TextID.CompareTo(((Entry)rhs).EditedLabel.TextID);
			return result;
		}
	}
	#endregion

	#region Label_Definition
	public sealed class Label
	{
		public uint NameOffset { get; set; }
		public string Name { get; set; }

		public uint ExtraID { get; set; }
		public uint ExtraOffset { get; set; }
		public string Extra { get; set; }

		public uint TextID { get; set; }
		public uint TextOffset { get; set; }
		public string Text { get; set; }

		public Label()
		{
			NameOffset = 0;
			Name = string.Empty;

			ExtraID = 0;
			ExtraOffset = 0;
			Extra = string.Empty;

			TextID = 0;
			TextOffset = 0;
			Text = string.Empty;
		}
	}
	#endregion
}
