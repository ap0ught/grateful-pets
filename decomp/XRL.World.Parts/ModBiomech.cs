using System;

namespace XRL.World.Parts;

[Serializable]
public class ModBiomech : IModification
{
	public ModBiomech()
	{
	}

	public ModBiomech(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "Biomachinery";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.IsWall();
	}

	public override void ApplyModification(GameObject Object)
	{
		int num = Tier * Tier * 50;
		BiomechanicalPowerTransmission biomechanicalPowerTransmission = Object.RequirePart<BiomechanicalPowerTransmission>();
		if (biomechanicalPowerTransmission.ChargeRate < num)
		{
			biomechanicalPowerTransmission.ChargeRate = num;
		}
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
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
			E.Rating += 6;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{biomech|biomech}}");
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
		return "Biomech: Has biomechanical power transmission systems.";
	}
}
