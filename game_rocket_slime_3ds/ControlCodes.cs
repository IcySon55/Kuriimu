using System.Collections.Generic;
using System.Drawing;
using System.Text;
using game_rocket_slime_3ds.Properties;
using KuriimuContract;

namespace game_rocket_slime_3ds
{
	public class ControlCodes : IGameHandler
	{
		Dictionary<string, string> _pairs = null;
		Encoding _encoding = Encoding.Unicode;

		#region Properties

		public string Name
		{
			get { return "Rocket Slime 3DS"; }
		}

		public Image Icon
		{
			get { return Resources.icon; }
		}

		#endregion

		private void Initialize(Encoding encoding)
		{
			if (_encoding != encoding || _pairs == null)
			{
				_pairs = new Dictionary<string, string>();
				_pairs.Add("<prompt>", encoding.GetString(new byte[] { 0x1F, 0x00, 0x20, 0x00 }));
				_pairs.Add("<player>", encoding.GetString(new byte[] { 0x1F, 0x00, 0x05, 0x00 }));
				_pairs.Add("<name>", encoding.GetString(new byte[] { 0x1F, 0x00, 0x02, 0x00 }));
				_pairs.Add("</name>", encoding.GetString(new byte[] { 0x02, 0x00 }));
				_pairs.Add("<u1>", encoding.GetString(new byte[] { 0x1F, 0x00, 0x00, 0x00 }));
				_pairs.Add("<top?>", encoding.GetString(new byte[] { 0x1F, 0x00, 0x00, 0x01 }));
				_pairs.Add("<middle?>", encoding.GetString(new byte[] { 0x1F, 0x00, 0x03, 0x01 }));
				_pairs.Add("<bottom?>", encoding.GetString(new byte[] { 0x1F, 0x00, 0x00, 0x02 }));
				_pairs.Add("<u2>", encoding.GetString(new byte[] { 0x15, 0x00 }));
				_pairs.Add("<u3>", encoding.GetString(new byte[] { 0x17, 0x00 }));
				_pairs.Add("<end>", encoding.GetString(new byte[] { 0x1F, 0x00, 0x15, 0x01 }));

				// Color
				_pairs.Add("</color>", encoding.GetString(new byte[] { 0x13, 0x00, 0x00, 0x00 }));
				_pairs.Add("<color>", encoding.GetString(new byte[] { 0x13, 0x00 }));
				_pairs.Add("<red>", encoding.GetString(new byte[] { 0x01, 0x00 }));
				_pairs.Add("<blue>", encoding.GetString(new byte[] { 0x03, 0x00 }));

				_encoding = encoding;
			}
		}
		
		public string GetString(byte[] text, Encoding encoding)
		{
			Initialize(encoding);

			string result = encoding.GetString(text);

			if (_pairs != null)
				foreach (string key in _pairs.Keys)
				{
					result = result.Replace(_pairs[key], key);
				}

			return result;
		}

		public byte[] GetBytes(string text, Encoding encoding)
		{
			Initialize(encoding);

			if (_pairs != null)
				foreach (string key in _pairs.Keys)
				{
					text = text.Replace(key, _pairs[key]);
				}

			return encoding.GetBytes(text);
		}
	}
}