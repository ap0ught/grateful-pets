using System;
using XRL.World;

namespace XRL.Rules;

public class Stats
{
	[Obsolete("Use XRL.Rules.RuleSettings.MAXIMUM_CARRIED_WEIGHT_PER_STRENGTH instead")]
	public const int MAX_WEIGHT_PER_STRENGTH = 15;

	[Obsolete("Use XRL.Rules.RuleSettings.BASE_SECONDARY_ATTACK_CHANCE instead")]
	public const int BASE_SECONDARY_ATTACK_CHANCE = 15;

	[Obsolete("Use GameObject.GetMaxCarriedWeight() instead")]
	public static int GetMaxWeight(GameObject GO)
	{
		return GO.GetMaxCarriedWeight();
	}

	public static int GetCombatDV(GameObject GO)
	{
		if (GO == null)
		{
			return 0;
		}
		Statistic stat = GO.GetStat("DV");
		if (stat == null)
		{
			return 0;
		}
		int result = 6 + stat.Value + GO.StatMod("Agility");
		if (!GO.IsMobile())
		{
			result = -10;
		}
		return result;
	}

	public static int GetCombatAV(GameObject GO)
	{
		return GO.Stat("AV");
	}

	public static int GetCombatMA(GameObject Object)
	{
		return 4 + Object.Stat("MA") + Object.StatMod("Willpower");
	}

	[Obsolete("The activeAttack parameter has been deprecated, use Mental.PerformAttack instead")]
	public static int GetCombatMA(GameObject GO, bool activeAttack)
	{
		return GetCombatMA(GO);
	}

	public static int GetSecondaryAttackChance(GameObject Object)
	{
		return Object.GetIntProperty("SecondaryAttackChance", RuleSettings.BASE_SECONDARY_ATTACK_CHANCE);
	}
}
