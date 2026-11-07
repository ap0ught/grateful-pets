using System.Collections.Generic;

namespace XRL.UI;

public class FactionRepComparer : IComparer<string>
{
	public int Compare(string F1, string F2)
	{
		if (F1 == F2)
		{
			return 0;
		}
		int num = The.Game.PlayerReputation.Get(F2).CompareTo(The.Game.PlayerReputation.Get(F1));
		if (num != 0)
		{
			return num;
		}
		return F1.CompareTo(F2);
	}
}
