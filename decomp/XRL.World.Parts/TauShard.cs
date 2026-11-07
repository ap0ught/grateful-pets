using System;

namespace XRL.World.Parts;

[Serializable]
public class TauShard : IPart
{
	public bool TauKilled => (The.Game?.GetStringGameState("TauElse"))?.EndsWith("KilledByPlayer") ?? false;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectCreatedEvent.ID && ID != ZoneActivatedEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (TauKilled)
		{
			E.ReplacementObject = GameObjectFactory.Factory.CreateObject("TauDagger");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (TauKilled)
		{
			ParentObject.ReplaceWith("TauDagger");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		if (TauKilled)
		{
			ParentObject.ReplaceWith("TauDagger");
		}
		return base.HandleEvent(E);
	}
}
