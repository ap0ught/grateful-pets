using System.Collections.Generic;

public ref struct RingEnumerator<T>
{
	private int Start;

	private int Delta;

	private int Length;

	private IReadOnlyList<T> List;

	private T Element;

	public T Current => Element;

	public RingEnumerator(IReadOnlyList<T> List, int Start, bool IncludeSelf = false, bool Invert = false)
	{
		int count = List.Count;
		if (Invert)
		{
			Start -= count / 2;
			if (Start < 0)
			{
				Start += count;
			}
		}
		this.Start = Start;
		this.List = List;
		Delta = ((!IncludeSelf) ? (-1) : 0);
		Length = count;
		Element = default;
	}

	public RingEnumerator<T> GetEnumerator()
	{
		return this;
	}

	public bool MoveNext()
	{
		int delta = Delta;
		int num = Start + delta;
		int length = Length;
		if (delta < 0)
		{
			if (delta + delta < -length)
			{
				return false;
			}
			Element = List[(num < 0) ? (num + length) : num];
			Delta = -delta;
		}
		else
		{
			if (delta + delta >= length)
			{
				return false;
			}
			Element = List[(num >= length) ? (num - length) : num];
			Delta = -(delta + 1);
		}
		return true;
	}
}
