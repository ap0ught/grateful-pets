using System;
using System.Text;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class StingerParalysisProperties : IStingerProperties
{
	public override string SaveVs => "Stinger Injected Poison Paralysis";

	public override Effect CreateEffect(GameObject Attacker, GameObject Defender, int Level)
	{
		return new Paralyzed(GetDuration(Level).RollCached(), -1);
	}

	public override void AppendLevelText(StringBuilder SB, int Level)
	{
		SB.Append("Venom paralyzes opponents for {{rules|").Append(GetDuration(Level)).Append("}} rounds\n");
	}

	public override void VisualEffect(GameObject Attacker, GameObject Defender)
	{
		Defender.Splatter("&m.");
	}

	public override string GetAdjective()
	{
		return "paralyzing";
	}

	public override string GetDuration(int Level)
	{
		if (Level >= 3)
		{
			return "1d3+" + Math.Min(7, Level / 3 + 1);
		}
		return "1d3+1";
	}
}
