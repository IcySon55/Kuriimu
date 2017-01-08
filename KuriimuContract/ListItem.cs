namespace KuriimuContract
{
	public class ListItem
	{
		public string Text { get; }
		public object Value { get; }

		public ListItem(string text, object value)
		{
			Text = text;
			Value = value;
		}

		public override string ToString()
		{
			return Text;
		}
	}
}