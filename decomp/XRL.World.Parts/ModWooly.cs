using System;

namespace XRL.World.Parts;

[Serializable]
public class ModWooly : IModification
{
	public ModWooly()
	{
	}

	public ModWooly(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<Armor>();
	}

	public override void ApplyModification(GameObject Object)
	{
		Armor part = Object.GetPart<Armor>();
		if (part.WornOn == "Body")
		{
			part.Heat += 10;
			part.Cold += 10;
		}
		else
		{
			part.Heat += 5;
			part.Cold += 5;
		}
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
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{Y|wooly}}");
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
		return "Wooly: This item grants resistance to heat and cold.";
	}
}
