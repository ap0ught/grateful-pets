using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class BlinkingTic : BaseMutation
{
	public BlinkingTic()
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
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("travel", BaseElementWeight);
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
		return "You teleport about uncontrollably.\n\nSmall chance each round you're in combat that you randomly teleport to a nearby location.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell == null || cell.ParentZone.IsWorldMap())
			{
				return true;
			}
			if (!CheckMyRealityDistortionUsability())
			{
				return true;
			}
			if (1.in1000())
			{
				if (ParentObject.IsPlayer() && !ParentObject.AreHostilesNearby())
				{
					return true;
				}
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You lurch suddenly!", 'r');
				}
				ParentObject.RandomTeleport(Swirl: true);
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}
}
