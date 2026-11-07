using Qud.UI;
using UnityEngine;

namespace XRL.UI;

public static class Media
{
	public enum SizeClass
	{
		Unset = int.MinValue,
		Small = 0,
		Medium = 100,
		Large = 200
	}

	public static string WEB_COLOR_YELLOW = "858951";

	public static SizeClass sizeClass
	{
		get
		{
			if ((float)Screen.width / UIManager.scale < 1536f)
			{
				return SizeClass.Small;
			}
			return SizeClass.Medium;
		}
	}
}
