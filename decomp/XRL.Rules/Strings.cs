using System.Text;
using XRL.World;
using XRL.World.Capabilities;

namespace XRL.Rules;

public class Strings
{
	public static StringBuilder SB = new StringBuilder(2048);

	public static string HealthStatusColor(GameObject gameObject)
	{
		if (gameObject.baseHitpoints == 0)
		{
			return "Y";
		}
		int num = gameObject.hitpoints * 100 / gameObject.baseHitpoints;
		if (num >= 100)
		{
			return "Y";
		}
		if (num >= 66)
		{
			return "G";
		}
		if (num >= 33)
		{
			return "W";
		}
		if (num >= 15)
		{
			return "R";
		}
		return "r";
	}

	public static int WoundLevelN(GameObject GO)
	{
		if (GO == null)
		{
			return -2;
		}
		if (GO.HasTagOrProperty("NoDamageReadout"))
		{
			return -2;
		}
		if (GO.HasStat("Hitpoints"))
		{
			if (The.Player == null)
			{
				return -2;
			}
			int num = 100;
			if (GO.baseHitpoints != 0)
			{
				num = GO.hitpoints * 100 / GO.baseHitpoints;
			}
			if (Scanning.HasScanningFor(The.Player, GO))
			{
				return 0;
			}
			if (num < 15)
			{
				return 1;
			}
			if (num < 33)
			{
				return 2;
			}
			if (num < 66)
			{
				return 3;
			}
			if (num < 100)
			{
				return 4;
			}
			return 5;
		}
		return -1;
	}

	public static string WoundLevel(GameObject GO)
	{
		if (GO == null)
		{
			return "";
		}
		if (GO.HasTagOrProperty("NoDamageReadout"))
		{
			return "";
		}
		if (GO.HasStat("Hitpoints"))
		{
			if (The.Player == null)
			{
				return "";
			}
			int num = 100;
			if (GO.baseHitpoints != 0)
			{
				num = GO.hitpoints * 100 / GO.baseHitpoints;
			}
			if (Scanning.HasScanningFor(The.Player, GO))
			{
				SB.Length = 0;
				SB.Append("{{");
				if (num < 15)
				{
					SB.Append('r');
				}
				else if (num < 33)
				{
					SB.Append('R');
				}
				else if (num < 66)
				{
					SB.Append('W');
				}
				else if (num < 100)
				{
					SB.Append('G');
				}
				else
				{
					SB.Append('Y');
				}
				SB.Append('|').Append(GO.hitpoints).Append('/')
					.Append(GO.baseHitpoints)
					.Append("}} {{b|")
					.Append('\u0004')
					.Append("}}")
					.Append(Stats.GetCombatAV(GO))
					.Append(" {{K|")
					.Append('\t')
					.Append("}}")
					.Append(Stats.GetCombatDV(GO));
				return SB.ToString();
			}
			if (GO.HasTag("Creature") && GO.IsOrganic)
			{
				if (num < 15)
				{
					return "{{r|Badly Wounded}}";
				}
				if (num < 33)
				{
					return "{{R|Wounded}}";
				}
				if (num < 66)
				{
					return "{{W|Injured}}";
				}
				if (num < 100)
				{
					return "{{G|Fine}}";
				}
				return "{{Y|Perfect}}";
			}
			if (num < 15)
			{
				return "{{r|Badly Damaged}}";
			}
			if (num < 33)
			{
				return "{{R|Damaged}}";
			}
			if (num < 66)
			{
				return "{{W|Lightly Damaged}}";
			}
			if (num < 100)
			{
				return "{{G|Fine}}";
			}
			return "{{Y|Perfect}}";
		}
		return "Undamaged";
	}
}
