using System;

public ref struct Int32Enumerator
{
	private ReadOnlySpan<char> Text;

	private int Value;

	private int Index;

	private int Length;

	public int Current => Value;

	public Int32Enumerator(ReadOnlySpan<char> Text)
	{
		this.Text = Text;
		Value = 0;
		Index = 0;
		Length = Text.Length;
	}

	public Int32Enumerator GetEnumerator()
	{
		return this;
	}

	public bool MoveNext()
	{
		int i = Index;
		int length = Length;
		if (i >= length)
		{
			return false;
		}
		ReadOnlySpan<char> text = Text;
		int num = -1;
		for (; i < length; i++)
		{
			if (char.IsDigit(text[i]))
			{
				if (num == -1)
				{
					num = i;
				}
			}
			else if (num != -1)
			{
				break;
			}
		}
		if (num == -1)
		{
			return false;
		}
		Value = int.Parse(text.Slice(num, i - num));
		Index = i;
		return true;
	}
}
