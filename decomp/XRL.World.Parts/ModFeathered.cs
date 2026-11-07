using System;

namespace XRL.World.Parts;

[Serializable]
public class ModFeathered : IModification
{
	public int Amount = 250;

	public ModFeathered()
	{
	}

	public ModFeathered(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override void ApplyModification(GameObject Object)
	{
		AddsRep.AddModifier(Object, "Birds:" + Amount + ":hidden");
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModFeathered).Amount != Amount)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageIncrease += 50;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Understood() || !E.Object.HasProperName)
		{
			E.AddAdjective("{{feathered|feathered}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Feathered: This item grants the wearer " + Amount.Signed() + " reputation with birds.");
		return base.HandleEvent(E);
	}
}
