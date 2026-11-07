using System;
using System.Collections.Generic;

namespace HistoryKit;

public static class TupleUtilities<A, B>
{
	public static A[] GetFirstArray(List<Tuple<A, B>> list)
	{
		A[] array = new A[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = list[i].Item1;
		}
		return array;
	}
}
