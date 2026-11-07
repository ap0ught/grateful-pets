public static class CharCache
{
	public static readonly int Length;

	public static string[] Values;

	static CharCache()
	{
		Length = 256;
		Values = new string[Length];
		for (int i = 0; i < Length; i++)
		{
			Values[i] = char.ToString((char)i);
		}
	}
}
