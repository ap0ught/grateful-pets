using System;
using System.Collections.Generic;
using System.Linq;

namespace FuzzySharp.Utils;

public class Permutor<T> where T : IComparable<T>
{
	private readonly List<T> _set;

	public Permutor(IEnumerable<T> set)
	{
		_set = set.ToList();
	}

	public List<T> PermutationAt(long i)
	{
		List<T> list = new List<T>(_set.OrderBy((T e) => e).ToList());
		for (long num = 0L; num < i - 1; num++)
		{
			NextPermutation(list);
		}
		return list;
	}

	public List<T> NextPermutation()
	{
		NextPermutation(_set);
		return _set;
	}

	public bool NextPermutation(List<T> set)
	{
		int num = set.Count - 1;
		while (num > 0 && set[num - 1].CompareTo(set[num]) >= 0)
		{
			num--;
		}
		if (num <= 0)
		{
			return false;
		}
		int num2 = set.Count - 1;
		while (set[num2].CompareTo(set[num - 1]) <= 0)
		{
			num2--;
		}
		T value = set[num - 1];
		set[num - 1] = set[num2];
		set[num2] = value;
		num2 = set.Count - 1;
		while (num < num2)
		{
			value = set[num];
			set[num] = set[num2];
			set[num2] = value;
			num++;
			num2--;
		}
		return true;
	}
}
