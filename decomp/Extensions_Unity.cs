using UnityEngine;

public static class Extensions_Unity
{
	public static bool FromHex(this Color c1, string hex)
	{
		Color color;
		return ColorUtility.TryParseHtmlString(hex, out color);
	}
}
