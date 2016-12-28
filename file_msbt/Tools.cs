namespace file_msbt
{
	public static class Tools
	{
		public static bool SequenceEqual(this byte[] a1, byte[] b1)
		{
			int i = 0;

			if (a1.Length == b1.Length)
			{
				while (i < a1.Length && (a1[i] == b1[i]))
					i++;

				if (i == a1.Length)
					return true;
			}

			return false;
		}
	}
}