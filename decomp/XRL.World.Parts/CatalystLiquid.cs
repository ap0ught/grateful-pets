using System;

namespace XRL.World.Parts;

[Serializable]
[HasModSensitiveStaticCache]
public class CatalystLiquid : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectCreatedEvent.ID)
		{
			return ID == PooledEvent<RepairedEvent>.ID;
		}
		return true;
	}

	public void SetComponentsFromBloodOf(GameObject Object)
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume != null && Object != null)
		{
			liquidVolume.InitialLiquid = Object.GetBleedLiquid("proteangunk-1000");
		}
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (ZoneManager.ZoneGenerationContext is InteriorZone interiorZone)
		{
			SetComponentsFromBloodOf(interiorZone.ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RepairedEvent E)
	{
		if (ParentObject.CurrentZone is InteriorZone interiorZone)
		{
			SetComponentsFromBloodOf(interiorZone.ParentObject);
		}
		return base.HandleEvent(E);
	}
}
