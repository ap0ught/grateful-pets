using System;

public ref struct RingSpanEnumerator<T>
{
	private int Start;

	private int Delta;

	private int Length;

	private ReadOnlySpan<T> Span;

	private T Element;

	public T Current => Element;

	public RingSpanEnumerator(ReadOnlySpan<T> Span, int Start, bool IncludeSelf = false, bool Invert = false)
	{
		int length = Span.Length;
		if (Invert)
		{
			Start -= length / 2;
			if (Start < 0)
			{
				Start += length;
			}
		}
		this.Start = Start;
		this.Span = Span;
		Delta = ((!IncludeSelf) ? (-1) : 0);
		Length = length;
		Element = default;
	}

	public RingSpanEnumerator<T> GetEnumerator()
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
			Element = Span[(num < 0) ? (num + length) : num];
			Delta = -delta;
		}
		else
		{
			if (delta + delta >= length)
			{
				return false;
			}
			Element = Span[(num >= length) ? (num - length) : num];
			Delta = -(delta + 1);
		}
		return true;
	}
}
