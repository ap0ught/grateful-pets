using System;
using System.Collections.Generic;
using System.Linq;

namespace AiUnity.Common.Utilities;

public static class EnumUtility
{
	public static IEnumerable<T> GetValues<T>(bool allowZero = true, bool allowCombinators = false)
	{
		return ((T[])Enum.GetValues(typeof(T))).Where((T t) => (allowZero || Convert.ToInt32(t) > 0) && (allowCombinators || !typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false) || (Convert.ToInt32(t) & (Convert.ToInt32(t) - 1)) == 0));
	}
}
