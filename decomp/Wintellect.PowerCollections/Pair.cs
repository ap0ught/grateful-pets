using System;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public struct Pair<TFirst, TSecond> : IComparable, IComparable<Pair<TFirst, TSecond>>
{
	private static IComparer<TFirst> firstComparer = Comparer<TFirst>.Default;

	private static IComparer<TSecond> secondComparer = Comparer<TSecond>.Default;

	private static IEqualityComparer<TFirst> firstEqualityComparer = EqualityComparer<TFirst>.Default;

	private static IEqualityComparer<TSecond> secondEqualityComparer = EqualityComparer<TSecond>.Default;

	public TFirst First;

	public TSecond Second;

	public Pair(TFirst first, TSecond second)
	{
		First = first;
		Second = second;
	}

	public Pair(KeyValuePair<TFirst, TSecond> keyAndValue)
	{
		First = keyAndValue.Key;
		Second = keyAndValue.Value;
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is Pair<TFirst, TSecond> other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(Pair<TFirst, TSecond> other)
	{
		if (firstEqualityComparer.Equals(First, other.First))
		{
			return secondEqualityComparer.Equals(Second, other.Second);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = ((First == null) ? 1642088727 : First.GetHashCode());
		int num2 = ((Second == null) ? 428791459 : Second.GetHashCode());
		return num ^ num2;
	}

	public int CompareTo(Pair<TFirst, TSecond> other)
	{
		try
		{
			int num = firstComparer.Compare(First, other.First);
			if (num != 0)
			{
				return num;
			}
			return secondComparer.Compare(Second, other.Second);
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
			throw;
		}
	}

	int IComparable.CompareTo(object obj)
	{
		if (obj is Pair<TFirst, TSecond>)
		{
			return CompareTo((Pair<TFirst, TSecond>)obj);
		}
		throw new ArgumentException(Strings.BadComparandType, "obj");
	}

	public override string ToString()
	{
		return string.Format("First: {0}, Second: {1}", (First == null) ? "null" : First.ToString(), (Second == null) ? "null" : Second.ToString());
	}

	public static bool operator ==(Pair<TFirst, TSecond> pair1, Pair<TFirst, TSecond> pair2)
	{
		if (firstEqualityComparer.Equals(pair1.First, pair2.First))
		{
			return secondEqualityComparer.Equals(pair1.Second, pair2.Second);
		}
		return false;
	}

	public static bool operator !=(Pair<TFirst, TSecond> pair1, Pair<TFirst, TSecond> pair2)
	{
		return !(pair1 == pair2);
	}

	public static explicit operator KeyValuePair<TFirst, TSecond>(Pair<TFirst, TSecond> pair)
	{
		return new KeyValuePair<TFirst, TSecond>(pair.First, pair.Second);
	}

	public KeyValuePair<TFirst, TSecond> ToKeyValuePair()
	{
		return new KeyValuePair<TFirst, TSecond>(First, Second);
	}

	public static explicit operator Pair<TFirst, TSecond>(KeyValuePair<TFirst, TSecond> keyAndValue)
	{
		return new Pair<TFirst, TSecond>(keyAndValue);
	}
}
