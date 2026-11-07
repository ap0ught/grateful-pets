using System;

namespace XRL.World.Parts;

[Serializable]
public class ModGearbox : IModification
{
	public ModGearbox()
	{
	}

	public ModGearbox(int Tier)
		: base(Tier)
	{
	}

	public override void ApplyModification(GameObject Object)
	{
		MechanicalPowerTransmission part = Object.GetPart<MechanicalPowerTransmission>();
		if (part == null)
		{
			part = new MechanicalPowerTransmission();
			if (Tier > 1)
			{
				part.ChargeRate = 100 + Tier * 20;
			}
			Object.AddPart(part);
		}
		else if (part.ChargeRate < 100 + Tier * 20)
		{
			part.ChargeRate = 100 + Tier * 20;
		}
		IncreaseDifficulty(1);
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
			E.Rating += 8;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddWithClause("gearbox");
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
		return "Gearbox: Has mechanical power transmission components installed.";
	}
}
