using System;

namespace XRL.World.Parts;

[Serializable]
public class ModMetered : IModification
{
	public ModMetered()
	{
	}

	public ModMetered(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "ChargeMeter";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<EnergyCell>();
	}

	public override void ApplyModification(GameObject Object)
	{
		EnergyCell part = Object.GetPart<EnergyCell>();
		part.ChargeDisplayStyle = "percentage";
		part.AltChargeDisplayStyle = "percentage";
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
			E.AddAdjective("{{c|metered}}");
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
		return "Metered: This item has a readout displaying its charge percentage.";
	}
}
