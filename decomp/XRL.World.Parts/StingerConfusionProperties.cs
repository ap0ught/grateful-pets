using System;
using System.Text;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class StingerConfusionProperties : IStingerProperties
{
	public override string SaveVs => "Stinger Injected Poison Confusion";

	public override Effect CreateEffect(GameObject Attacker, GameObject Defender, int Level)
	{
		return new Confused(GetDuration(Level).RollCached(), Level, Level + 2);
	}

	public override void AppendLevelText(StringBuilder SB, int Level)
	{
		SB.Append("Venom confuses opponents for {{rules|").Append(GetDuration(Level)).Append("}} rounds\n");
	}

	public override void VisualEffect(GameObject Attacker, GameObject Defender)
	{
		Defender.Splatter("&B.");
	}

	public override string GetAdjective()
	{
		return "confusing";
	}

	public override string GetDuration(int Level)
	{
		if (Level >= 3)
		{
			return "2d3+" + Math.Min(14, Level * 2 / 3 + 2);
		}
		return "2d3+2";
	}
}
