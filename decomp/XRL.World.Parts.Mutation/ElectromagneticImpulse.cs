using System;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ElectromagneticImpulse : BaseMutation
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("circuitry", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You involuntarily release electromagnetic pulses, deactivating robots and artifacts around yourself.\n\nSmall chance each round you're in combat that you release an electromagnetic pulse with radius 3, deactivating robots and artifacts (including those you carry) for 11-20 rounds.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && !ParentObject.OnWorldMap() && 2.in1000())
		{
			if (ParentObject.IsPlayer() && !ParentObject.AreHostilesNearby())
			{
				return true;
			}
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("{{r|You surge with energy!}}");
			}
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				ElectromagneticPulse.EMP(cell, (int)Math.Ceiling(2.5), Stat.Roll("1d10") + 10, IncludeBaseCell: true, ParentObject.GetPhase());
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}
}
