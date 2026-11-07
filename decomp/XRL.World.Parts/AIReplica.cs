using System;

namespace XRL.World.Parts;

[Serializable]
public class AIReplica : AIBehaviorPart
{
	public string OriginalID;

	public bool ForceAllied = true;

	public AIReplica()
	{
	}

	public AIReplica(GameObject Original)
	{
		OriginalID = Original.ID;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (ForceAllied || Registrar.IsUnregister)
		{
			Registrar.Register(PooledEvent<GetFeelingEvent>.ID);
			Registrar.Register(PooledEvent<BeforeSetFeelingEvent>.ID);
		}
	}

	public override bool HandleEvent(GetFeelingEvent E)
	{
		if (ForceAllied && E.Feeling < 100 && IsAlly(E.Target))
		{
			E.Feeling = 100;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeSetFeelingEvent E)
	{
		if (ForceAllied && E.Feeling < 0 && IsAlly(E.Target))
		{
			if (!E.Target.IsPlayer())
			{
				E.Target.StopFighting(ParentObject);
			}
			E.Feeling = 100;
		}
		return base.HandleEvent(E);
	}

	public bool IsAlly(GameObject Object)
	{
		if (Object.IDMatch(OriginalID))
		{
			return true;
		}
		AIReplica part = Object.GetPart<AIReplica>();
		if (part != null)
		{
			return part.OriginalID == OriginalID;
		}
		return false;
	}
}
