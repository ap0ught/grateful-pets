using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL;

/// This class is not used in the base game.
[Serializable]
public class SifrahSlot : SifrahSlotConfiguration
{
	public int Token;

	public List<int> Moves = new List<int>();

	public int CurrentMove = -1;

	public bool Solved
	{
		get
		{
			if (Moves.Count > 0)
			{
				return Moves[Moves.Count - 1] == Token;
			}
			return false;
		}
	}

	public int SolvedOnTurn
	{
		get
		{
			if (!Solved)
			{
				return -1;
			}
			return Moves.Count;
		}
	}

	public SifrahSlot()
	{
	}

	public SifrahSlot(SifrahSlotConfiguration From)
		: base(From)
	{
	}

	public SifrahSlot(SifrahSlotConfiguration From, int NumberOfTokens)
		: this(From)
	{
		Token = Stat.Random(0, NumberOfTokens - 1);
	}

	public static List<SifrahSlot> GenerateListFromConfigurations(List<SifrahSlotConfiguration> Configurations, int NumberOfSlots, int NumberOfTokens)
	{
		if (NumberOfSlots > Configurations.Count)
		{
			NumberOfSlots = Configurations.Count;
		}
		bool flag = false;
		int i = 0;
		for (int count = Configurations.Count; i < count; i++)
		{
			if (Configurations[i].UptakeOrder != 0)
			{
				flag = true;
				break;
			}
		}
		List<int> list = null;
		Dictionary<SifrahSlot, int> sortOrder = null;
		if (flag)
		{
			list = new List<int>(Configurations.Count);
			int j = 0;
			for (int count2 = Configurations.Count; j < count2; j++)
			{
				list.Add(0);
			}
			int k = 0;
			for (int count3 = Configurations.Count; k < count3; k++)
			{
				list[Configurations[k].UptakeOrder] = k;
			}
			sortOrder = new Dictionary<SifrahSlot, int>(NumberOfSlots);
		}
		List<SifrahSlot> list2 = new List<SifrahSlot>(NumberOfSlots);
		for (int l = 0; l < NumberOfSlots; l++)
		{
			int num = (flag ? list[l] : l);
			SifrahSlot sifrahSlot = new SifrahSlot(Configurations[num], NumberOfTokens);
			list2.Add(sifrahSlot);
			if (flag)
			{
				sortOrder[sifrahSlot] = num;
			}
		}
		if (flag)
		{
			list2.Sort((SifrahSlot a, SifrahSlot b) => sortOrder[a].CompareTo(sortOrder[b]));
		}
		return list2;
	}
}
