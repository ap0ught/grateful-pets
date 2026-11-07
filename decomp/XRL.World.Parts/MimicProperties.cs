using System;

namespace XRL.World.Parts;

[Serializable]
public class MimicProperties : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != EffectAppliedEvent.ID)
		{
			return ID == EffectRemovedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone != null && ParentObject.HasID)
		{
			currentZone.FireEvent(Event.New("CheckStuck", "Invalidate", ParentObject.ID));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		ZoneCheckStuck();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		ZoneCheckStuck();
		return base.HandleEvent(E);
	}

	public void ZoneCheckStuck()
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone != null && ParentObject.HasID)
		{
			currentZone.FireEvent(Event.New("CheckStuck"));
		}
	}
}
