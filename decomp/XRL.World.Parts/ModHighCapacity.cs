using System;

namespace XRL.World.Parts;

[Serializable]
public class ModHighCapacity : IModification
{
	public ModHighCapacity()
	{
	}

	public ModHighCapacity(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "HighCapacityStorage";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<EnergyCell>();
	}

	public override void ApplyModification(GameObject Object)
	{
		EnergyCell part = Object.GetPart<EnergyCell>();
		part.MaxCharge = part.MaxCharge * (14 + Tier) / 10;
		if (Object.GetCurrentCell() == null)
		{
			part.Charge = part.Charge * (14 + Tier) / 10;
		}
		IncreaseComplexity(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{c|high-capacity}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "High-capacity: This item has increased charge capacity.";
	}
}
