using UnityEngine;

namespace Genkit;

public static class ColorExtensionMethods
{
	public static bool Equals(this Color32 x, Color32 y)
	{
		if (x.a == y.a && x.b == y.b && x.g == y.g)
		{
			return x.r == y.r;
		}
		return false;
	}
}
