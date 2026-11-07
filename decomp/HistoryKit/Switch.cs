using System.Collections.Generic;
using System.Linq;
using XRL.Rules;

namespace HistoryKit;

public class Switch
{
	public static int RandomWhere(params bool[] predicates)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < predicates.Length; i++)
		{
			if (predicates[i])
			{
				list.Add(i);
			}
		}
		if (list.Count > 0)
		{
			return list.GetRandomElement();
		}
		return -1;
	}

	public static int SwitchByWeights(params int[] args)
	{
		int high = args.Sum();
		int num = Stat.Random(1, high);
		int num2 = 0;
		if (args.Length == 0)
		{
			return 0;
		}
		for (int i = 0; i < args.Length; i++)
		{
			if (num <= args[i] + num2)
			{
				return i;
			}
			num2 += args[i];
		}
		return 0;
	}
}
