using System;

public ref struct DelimitedEnumeratorString
{
	private ReadOnlySpan<char> Value;

	private string Separator;

	private int Start;

	private int End;

	private int Length;

	private int SeparatorLength;

	public ReadOnlySpan<char> Current => Value.Slice(Start, End - Start);

	public DelimitedEnumeratorString(ReadOnlySpan<char> Value, string Separator)
	{
		this.Value = Value;
		this.Separator = Separator;
		Start = 0;
		Length = Value.Length;
		SeparatorLength = Separator.Length;
		End = -SeparatorLength;
	}

	public DelimitedEnumeratorString GetEnumerator()
	{
		return this;
	}

	public bool MoveNext()
	{
		Start = End + SeparatorLength;
		if (Start >= Length)
		{
			return false;
		}
		End = -1;
		ReadOnlySpan<char> value = Value;
		char c = Separator[0];
		for (int i = Start; i < Length; i++)
		{
			if (value[i] != c)
			{
				continue;
			}
			int num = i + SeparatorLength - 1;
			if (i == num)
			{
				End = i;
				break;
			}
			if (num >= Length)
			{
				continue;
			}
			bool flag = true;
			for (int j = i + 1; j <= num; j++)
			{
				if (value[j] != Separator[j - i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				End = i;
				break;
			}
		}
		if (End == -1)
		{
			End = Length;
		}
		return true;
	}
}
