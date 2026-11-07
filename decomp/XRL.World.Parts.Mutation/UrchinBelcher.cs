using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class UrchinBelcher : Belcher
{
	public UrchinBelcher()
	{
		EventKey = "CommandBelchUrchins";
		Description = "You belch forth various urchins.";
		BelchTable = "UrchinsToBelch";
		CommandName = "Belch Urchins";
		CommandDescription = "You belch forth various urchins.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("You belch urchins in a nearby area.\n" + "Number of urchins: 1d2+" + Level / 4 + " \n", "Range: ", GetRange(Level).ToString(), "\n"), "Radius: ", GetRadius().ToString(), "\n"), "Cooldown: ", GetCooldown(Level).ToString(), " rounds\n");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		bool flag = stats.mode.Contains("ability");
		if (Level / 4 == 0)
		{
			stats.Set("Amount", "1d2", !flag);
		}
		else
		{
			stats.Set("Amount", "1d2+" + Level / 4, !flag);
		}
		stats.Set("Range", GetRange(Level), !flag);
		stats.Set("Radius", GetRadius());
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}
}
