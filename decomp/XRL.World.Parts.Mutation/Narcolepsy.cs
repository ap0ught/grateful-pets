using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Narcolepsy : BaseMutation
{
	public Narcolepsy()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (!ParentObject.OnWorldMap() && !ParentObject.HasEffect<Asleep>() && 2.in1000() && ((!ParentObject.IsPlayer() && !ParentObject.WasPlayer()) || ParentObject.IsInCombat()))
		{
			ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_physicalDefect_generic_activate");
			if (ParentObject.ForceApplyEffect(new Asleep(Stat.Random(20, 29), forced: true)))
			{
				ParentObject.StopMoving();
				DidX("fall", "asleep", "!", null, null, null, ParentObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You fall asleep involuntarily from time to time.\n\nSmall chance each round you're in combat that you fall asleep for 20-29 rounds.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}
}
