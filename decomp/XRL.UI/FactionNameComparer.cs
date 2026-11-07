using System.Collections.Generic;
using XRL.World;

namespace XRL.UI;

public class FactionNameComparer : IComparer<string>
{
	public int Compare(string F1, string F2)
	{
		if (F1 == F2)
		{
			return 0;
		}
		int num = Factions.Get(F1).DisplayName.CompareTo(Factions.Get(F2).DisplayName);
		if (num != 0)
		{
			return num;
		}
		return F1.CompareTo(F2);
	}
}
