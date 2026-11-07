using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ForcefieldMaterial : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Object.ModIntProperty("Electromagnetic", 1);
		base.Register(Object, Registrar);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID)
		{
			return ID == PooledEvent<RealityStabilizeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check(CanDestroy: true))
		{
			Cell cell = ParentObject.GetCurrentCell()?.GetRandomLocalAdjacentCell();
			if (cell != null)
			{
				ParentObject.Discharge(cell, Stat.Random(1, 4), 0, "1d8", null, E.Effect.Owner, ParentObject);
			}
			ParentObject.TileParticleBlip("items/sw_quills.bmp", "&B", "K", 10, IgnoreVisibility: false, HFlip: false, VFlip: false, 0L);
			DidX("collapse", "under the pressure of normality", null, null, null, null, ParentObject);
			ParentObject.Destroy();
			return false;
		}
		return base.HandleEvent(E);
	}
}
