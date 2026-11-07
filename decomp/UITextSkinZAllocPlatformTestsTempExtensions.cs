using System;

public static class UITextSkinZAllocPlatformTestsTempExtensions
{
	public static bool Contains(this ReadOnlySpan<char> Haystack, ReadOnlySpan<char> Needle)
	{
		return Haystack.IndexOf(Needle, StringComparison.Ordinal) >= 0;
	}

	public static int IndexOfAnyStartFrom(this ReadOnlySpan<char> builder, int Start, ReadOnlySpan<char> C)
	{
		for (int i = Start; i < builder.Length; i++)
		{
			for (int j = 0; j < C.Length; j++)
			{
				if (builder[i] == C[j])
				{
					return i;
				}
			}
		}
		return -1;
	}
}
