using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KuriimuContract
{
	public class ListItem
	{
		private string _text = string.Empty;
		private object _value = null;

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
			}
		}

		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public ListItem() { }

		public ListItem(string text, object value)
		{
			_text = text;
			_value = value;
		}

		public override string ToString()
		{
			return Text;
		}
	}
}