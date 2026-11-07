using System;
using XRL.World.Parts;

namespace XRL.World.Units;

[Serializable]
public class GameObjectExperienceUnit : GameObjectUnit
{
	public int Experience;

	public int Levels;

	public override void Apply(GameObject Object)
	{
		int level = Object.Level;
		int num = Experience;
		if (Levels > 0)
		{
			num += Leveler.GetXPForLevel(level + Levels);
			num -= Leveler.GetXPForLevel(level);
		}
		Object.AwardXP(num, -1, 0, int.MaxValue, null, null, null, Object);
	}

	public override void Reset()
	{
		base.Reset();
		Experience = 0;
		Levels = 0;
	}

	public override string GetDescription(bool Inscription = false)
	{
		if (Levels > 0)
		{
			return "+" + Levels + " levels";
		}
		return "+" + Experience + " experience";
	}
}
