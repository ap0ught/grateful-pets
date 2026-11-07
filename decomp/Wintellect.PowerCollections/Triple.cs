using System;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public struct Triple<TFirst, TSecond, TThird>(TFirst first, TSecond second, TThird third) : IComparable, IComparable<Triple<TFirst, TSecond, TThird>>
{
	private static IComparer<TFirst> firstComparer = Comparer<TFirst>.Default;

	private static IComparer<TSecond> secondComparer = Comparer<TSecond>.Default;

	private static IComparer<TThird> thirdComparer = Comparer<TThird>.Default;

	private static IEqualityComparer<TFirst> firstEqualityComparer = EqualityComparer<TFirst>.Default;

	private static IEqualityComparer<TSecond> secondEqualityComparer = EqualityComparer<TSecond>.Default;

	private static IEqualityComparer<TThird> thirdEqualityComparer = EqualityComparer<TThird>.Default;

	public TFirst First = first;

	public TSecond Second = second;

	public TThird Third = third;

	public override bool Equals(object obj)
	{
		if (obj != null && obj is Triple<TFirst, TSecond, TThird> other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(Triple<TFirst, TSecond, TThird> other)
	{
		if (firstEqualityComparer.Equals(First, other.First) && secondEqualityComparer.Equals(Second, other.Second))
		{
			return thirdEqualityComparer.Equals(Third, other.Third);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = ((First == null) ? 1642088727 : First.GetHashCode());
		int num2 = ((Second == null) ? 428791459 : Second.GetHashCode());
		int num3 = ((Third == null) ? 1090263159 : Third.GetHashCode());
		return num ^ num2 ^ num3;
	}

	public int CompareTo(Triple<TFirst, TSecond, TThird> other)
	{
		try
		{
			int num = firstComparer.Compare(First, other.First);
			if (num != 0)
			{
				return num;
			}
			int num2 = secondComparer.Compare(Second, other.Second);
			if (num2 != 0)
			{
				return num2;
			}
			return thirdComparer.Compare(Third, other.Third);
		}
		catch (ArgumentException)
		{
			if (!typeof(IComparable<TFirst>).IsAssignableFrom(typeof(TFirst)) && !typeof(IComparable).IsAssignableFrom(typeof(TFirst)))
			{
				throw new NotSupportedException(string.Format(Strings.UncomparableType, typeof(TFirst).FullName));
			}
			if (!typeof(IComparable<TSecond>).IsAssignableFrom(typeof(TSecond)) && !typeof(IComparable).IsAssignableFrom(typeof(TSecond)))
			{
				throw new NotSupportedException(string.Format(Strings.UncomparableType, typeof(TSecond).FullName));
			}
			if (!typeof(IComparable<TThird>).IsAssignableFrom(typeof(TThird)) && !typeof(IComparable).IsAssignableFrom(typeof(TThird)))
			{
				throw new NotSupportedException(string.Format(Strings.UncomparableType, typeof(TThird).FullName));
			}
			throw;
		}
	}

	int IComparable.CompareTo(object obj)
	{
		if (obj is Triple<TFirst, TSecond, TThird>)
		{
			return CompareTo((Triple<TFirst, TSecond, TThird>)obj);
		}
		throw new ArgumentException(Strings.BadComparandType, "obj");
	}

	public override string ToString()
	{
		return string.Format("First: {0}, Second: {1}, Third: {2}", (First == null) ? "null" : First.ToString(), (Second == null) ? "null" : Second.ToString(), (Third == null) ? "null" : Third.ToString());
	}

	public static bool operator ==(Triple<TFirst, TSecond, TThird> pair1, Triple<TFirst, TSecond, TThird> pair2)
	{
		return pair1.Equals(pair2);
	}

	public static bool operator !=(Triple<TFirst, TSecond, TThird> pair1, Triple<TFirst, TSecond, TThird> pair2)
	{
		return !pair1.Equals(pair2);
	}
}
