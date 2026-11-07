using System;

namespace XRL.World.Parts;

[Serializable]
public class ModWired : IModification
{
	public ModWired()
	{
	}

	public ModWired(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "Wiring";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.IsWall();
	}

	public override void ApplyModification(GameObject Object)
	{
		int num = Tier * Tier * 500;
		ElectricalPowerTransmission electricalPowerTransmission = Object.RequirePart<ElectricalPowerTransmission>();
		if (electricalPowerTransmission.ChargeRate < num)
		{
			electricalPowerTransmission.ChargeRate = num;
		}
		IncreaseDifficultyIfComplex(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GenericRatingEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GenericRatingEvent E)
	{
		if (E.Type == "WallDigNavigationWeight" && E.Object == ParentObject)
		{
			E.Rating += 12;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{c|wired}}");
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
		return "Wired: Has electrical wiring.";
	}
}
