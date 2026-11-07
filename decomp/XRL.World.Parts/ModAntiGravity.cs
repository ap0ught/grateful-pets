using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModAntiGravity : IModification
{
	public ModAntiGravity()
	{
	}

	public ModAntiGravity(int Tier)
		: base(Tier)
	{
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.GetWeight() <= 0.0)
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Suspensor suspensor = Object.GetPart<Suspensor>();
		int forceLevel = GetForceLevel(Tier);
		int percentageForceLevel = GetPercentageForceLevel(Tier);
		if (suspensor == null)
		{
			suspensor = new Suspensor();
			suspensor.Force = forceLevel;
			suspensor.PercentageForce = percentageForceLevel;
			Object.AddPart(suspensor);
		}
		else
		{
			suspensor.Force += forceLevel;
			suspensor.PercentageForce = percentageForceLevel;
			if (suspensor.ChargeUse < 1)
			{
				suspensor.ChargeUse = 1;
			}
			suspensor.IsRealityDistortionBased = true;
			suspensor.IsEMPSensitive = true;
			suspensor.IsBootSensitive = true;
		}
		int num = Math.Max((Tier >= 5) ? (5 - Tier / 2) : Tier, 2);
		BootSequence part = Object.GetPart<BootSequence>();
		if (part == null)
		{
			part = new BootSequence();
			part.BootTime = num;
			part.WorksOn(AdjacentCellContents: false, Carrier: false, CellContents: false, Enclosed: false, Equipper: false, Holder: false, Implantee: false, Inventory: false, Self: true);
			Object.AddPart(part);
		}
		else if (part.WorksOnSelf)
		{
			if (part.BootTime < num)
			{
				part.BootTime = num;
			}
		}
		else
		{
			suspensor.IsBootSensitive = false;
		}
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexity(1, 2);
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
			E.AddAdjective("{{B|anti-gravity}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(GetPhysicalDescription());
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		int forceLevel = GetForceLevel(Tier);
		int percentageForceLevel = GetPercentageForceLevel(Tier);
		return "Anti-gravity: When powered, this item's weight is reduced by " + percentageForceLevel + "% plus " + forceLevel + " " + ((forceLevel == 1) ? "lb" : "lbs") + ".";
	}

	public string GetPhysicalDescription()
	{
		return "";
	}

	public static int GetForceLevel(int Tier)
	{
		if (Tier <= 0)
		{
			return 2;
		}
		return Tier + 1 + Tier / 3;
	}

	public static int GetPercentageForceLevel(int Tier)
	{
		if (Tier <= 0)
		{
			return 20;
		}
		return (Tier + 3) * 5;
	}
}
