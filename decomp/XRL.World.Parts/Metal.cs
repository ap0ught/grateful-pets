using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World.Parts;

[Serializable]
[GeneratePoolingPartial(Capacity = 64)]
public class Metal : IPart
{
	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	private static readonly IPartPool MetalPool = new IPartPool(64);

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override IPartPool Pool => MetalPool;

	public override void AddedAfterCreation()
	{
		base.AddedAfterCreation();
		ParentObject.MakeNonflammable();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != PooledEvent<CanBeMagneticallyManipulatedEvent>.ID && ID != PooledEvent<GetElectricalConductivityEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == PooledEvent<TransparentToEMPEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeMagneticallyManipulatedEvent E)
	{
		E.Allow = true;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 2 && E.Object == ParentObject)
		{
			E.MinValue(90);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 25;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject && !ParentObject.HasTag("Creature") && E.Damage.IsAcidDamage())
		{
			E.Damage.Amount /= 4;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TransparentToEMPEvent E)
	{
		return false;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ParentObject.MakeNonflammable();
		return base.HandleEvent(E);
	}
}
