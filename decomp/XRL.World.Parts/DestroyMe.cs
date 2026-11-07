using System;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class DestroyMe : IActivePart
{
	public string who;

	public DestroyMe()
	{
	}

	public DestroyMe(string who)
		: this()
	{
		this.who = who;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<IdleQueryEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (TryDestroyMe(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool TryDestroyMe(GameObject actor)
	{
		if (ParentObject.GetIntProperty("DroppedByPlayer") != 0)
		{
			return false;
		}
		if (!actor.BelongsToFaction(who))
		{
			return false;
		}
		if (ParentObject.IsAflame())
		{
			return false;
		}
		if (actor.Brain == null)
		{
			return false;
		}
		actor.Brain.PushGoal(new Kill(ParentObject));
		return true;
	}
}
