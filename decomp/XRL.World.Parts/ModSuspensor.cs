using System;

namespace XRL.World.Parts;

[Serializable]
public class ModSuspensor : IModification
{
	public ModSuspensor()
	{
	}

	public ModSuspensor(int Tier)
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
		if (suspensor == null)
		{
			suspensor = new Suspensor();
			suspensor.PercentageForce = 100;
			Object.AddPart(suspensor);
		}
		else
		{
			suspensor.PercentageForce = 100;
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
			E.AddWithClause("{{watery|suspensors}}");
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
		return "Fitted with suspensors: When powered, this item is weightless.";
	}
}
