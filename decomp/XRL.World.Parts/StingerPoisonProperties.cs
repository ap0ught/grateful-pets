using System;
using System.Text;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class StingerPoisonProperties : IStingerProperties
{
	public override string SaveVs => "Stinger Injected Poison Damaging";

	public override Effect CreateEffect(GameObject Attacker, GameObject Defender, int Level)
	{
		return new StingerPoisoned(GetDuration(Level).RollCached(), GetIncrement(Level), Level, Attacker);
	}

	public override void AppendLevelText(StringBuilder SB, int Level)
	{
		SB.Append("Venom poisons opponents for {{rules|").Append(GetDuration(Level)).Append("}} rounds (damage increment {{rules|")
			.Append(GetIncrement(Level))
			.Append("}})\n");
	}

	public override string GetDuration(int Level)
	{
		return "8-12";
	}

	public virtual string GetIncrement(int Level)
	{
		return Level + "d2";
	}
}
