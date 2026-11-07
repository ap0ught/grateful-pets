using System.Collections.Generic;

namespace XRL.Core;

public class SortScoreEntry2 : Comparer<ScoreEntry2>
{
	public override int Compare(ScoreEntry2 x, ScoreEntry2 y)
	{
		if (x.Score < y.Score)
		{
			return 1;
		}
		if (x.Score > y.Score)
		{
			return -1;
		}
		return x.Details.CompareTo(y.Details);
	}
}
