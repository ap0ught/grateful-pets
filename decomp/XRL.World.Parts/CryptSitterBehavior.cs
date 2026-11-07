using System;
using System.Linq;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class CryptSitterBehavior : IPart
{
	public bool Alerted;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		Brain brain = ParentObject.Brain;
		if (brain.PartyLeader != null)
		{
			ParentObject.RemovePart(this);
			return base.HandleEvent(E);
		}
		if (ParentObject.IsPlayer())
		{
			return base.HandleEvent(E);
		}
		Cell cell = ParentObject.CurrentCell;
		Brain brain2 = brain;
		GlobalLocation globalLocation = brain2.StartingCell ?? (brain2.StartingCell = cell.GetGlobalLocation());
		if (!Alerted)
		{
			if (!globalLocation.Equals(cell))
			{
				if (!brain.Goals.Items.Any((GoalHandler g) => g is IMovementGoal))
				{
					brain.Goals.Clear();
					brain.PushGoal(new MoveTo(globalLocation));
				}
				return true;
			}
			ParentObject.UseEnergy(1000);
			return false;
		}
		if (globalLocation.PathDistanceTo(cell) > 10)
		{
			Unalert();
			brain.Goals.Clear();
			brain.PushGoal(new MoveTo(globalLocation));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetFeelingEvent E)
	{
		if (!Alerted)
		{
			E.Feeling = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		Alert();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AICryptHelpBroadcast");
		Registrar.Register(PooledEvent<GetFeelingEvent>.ID);
	}

	public void Alert()
	{
		Alerted = true;
		ParentObject.Brain.Allegiance.Hostile = true;
		ParentObject.Render.DetailColor = "R";
		DidX("stir", null, null, null, null, null, IComponent<GameObject>.ThePlayer);
	}

	public void Unalert()
	{
		Alerted = false;
		ParentObject.Brain.Allegiance.Hostile = false;
		ParentObject.Render.DetailColor = "Y";
		DidX("sleep", null, null, null, null, IComponent<GameObject>.ThePlayer);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AICryptHelpBroadcast")
		{
			Alert();
		}
		return base.FireEvent(E);
	}
}
