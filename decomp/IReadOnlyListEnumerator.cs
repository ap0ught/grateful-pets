using System.Collections.Generic;

public ref struct IReadOnlyListEnumerator<T>
{
	private IReadOnlyList<T> List;

	private T Value;

	private int Index;

	private int Length;

	public T Current => Value;

	public IReadOnlyListEnumerator(IReadOnlyList<T> List, int Start, int Length)
	{
		this.List = List;
		Index = Start;
		this.Length = Start + Length;
		Value = default;
	}

	public IReadOnlyListEnumerator<T> GetEnumerator()
	{
		return this;
	}

	public bool MoveNext()
	{
		int index = Index;
		if (index >= Length)
		{
			return false;
		}
		Value = List[index];
		Index = index + 1;
		return true;
	}
}
