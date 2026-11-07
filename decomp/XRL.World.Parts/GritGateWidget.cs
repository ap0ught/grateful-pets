using System;
using XRL.World.Quests;

namespace XRL.World.Parts;

[Serializable]
public class GritGateWidget : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ZoneActivatedEvent.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		GritGateScripts.CheckGritGateDoors();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		GritGateScripts.CheckGritGateDoors();
		return base.HandleEvent(E);
	}
}
