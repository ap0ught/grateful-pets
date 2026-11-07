using System;

namespace XRL.World.Parts;

[Serializable]
public class ModInduction : IModification
{
	public ModInduction()
	{
	}

	public ModInduction(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<EnergyCell>();
	}

	public override void ApplyModification(GameObject Object)
	{
		InductionChargeReceiver inductionChargeReceiver = Object.GetPart<InductionChargeReceiver>();
		if (inductionChargeReceiver == null)
		{
			inductionChargeReceiver = new InductionChargeReceiver();
			if (Tier > 1)
			{
				inductionChargeReceiver.ChargeRate *= Tier;
			}
			Object.AddPart(inductionChargeReceiver);
		}
		else if (Tier > 0 && inductionChargeReceiver.ChargeRate < Tier * 10)
		{
			inductionChargeReceiver.ChargeRate = Tier * 10;
		}
		IntegralRecharger part = Object.GetPart<IntegralRecharger>();
		if (part == null)
		{
			part = new IntegralRecharger();
			part.ChargeRate = inductionChargeReceiver.ChargeRate;
			Object.AddPart(part);
		}
		else if (part.ChargeRate != 0 && part.ChargeRate < inductionChargeReceiver.ChargeRate)
		{
			part.ChargeRate = inductionChargeReceiver.ChargeRate;
		}
		IncreaseDifficultyAndComplexity(4, 1);
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
			E.AddAdjective("{{Y|induction}}");
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
		return "Induction: This item may be charged at induction charging stations.";
	}
}
