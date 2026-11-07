using System;
using System.Collections;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

internal static class Util
{
	[Serializable]
	private class WrapEnumerable<T> : IEnumerable<T>, IEnumerable
	{
		private IEnumerable<T> wrapped;

		public WrapEnumerable(IEnumerable<T> wrapped)
		{
			this.wrapped = wrapped;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return wrapped.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)wrapped).GetEnumerator();
		}
	}

	public static bool IsCloneableType(Type type, out bool isValue)
	{
		isValue = false;
		if (typeof(ICloneable).IsAssignableFrom(type))
		{
			return true;
		}
		if (type.IsValueType)
		{
			isValue = true;
			return true;
		}
		return false;
	}

	public static string SimpleClassName(Type type)
	{
		string text = type.Name;
		int num = text.IndexOfAny(new char[3] { '<', '{', '`' });
		if (num >= 0)
		{
			text = text.Substring(0, num);
		}
		return text;
	}

	public static IEnumerable<T> CreateEnumerableWrapper<T>(IEnumerable<T> wrapped)
	{
		return new WrapEnumerable<T>(wrapped);
	}

	public static int GetHashCode<T>(T item, IEqualityComparer<T> equalityComparer)
	{
		if (item == null)
		{
			return 394715708;
		}
		return equalityComparer.GetHashCode(item);
	}
}
