using System;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SpontaneousCombustion : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override string GetDescription()
	{
		return "You spontaneously erupt into flames.\n\nSmall chance each round you're in combat that you spontaneously erupt into flames.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!ParentObject.OnWorldMap() && 2.in1000() && ParentObject.AreHostilesNearby())
		{
			ParentObject.TemperatureChange(400 + Stat.Random(1, 300), ParentObject);
			if (ParentObject.IsAflame())
			{
				ParentObject.StopMoving();
				DidX("erupt", "into flames", "!", null, null, null, ParentObject);
			}
		}
		base.TurnTick(TimeTick, Amount);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}
}
