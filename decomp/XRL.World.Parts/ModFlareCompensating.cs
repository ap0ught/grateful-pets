using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModFlareCompensating : IModification
{
	public ModFlareCompensating()
	{
	}

	public ModFlareCompensating(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return IModification.CheckWornSlot(Object, "Head", "Face", null, null, AllowGeneric: true, AllowMagnetized: true, AllowPartial: false);
	}

	public override void ApplyModification(GameObject Object)
	{
		FlareCompensation part = Object.GetPart<FlareCompensation>();
		int modificationLevel = GetModificationLevel(Tier);
		if (part == null)
		{
			part = new FlareCompensation();
			part.Level = modificationLevel;
			part.ChargeUse = 1;
			part.IsEMPSensitive = true;
			part.ShowInShortDescription = false;
			part.ComputePowerFactor = 1.5f;
			Object.AddPart(part);
		}
		else if (part.IsEMPSensitive)
		{
			part.Level += modificationLevel;
			if (part.ChargeUse < 1)
			{
				part.ChargeUse = 1;
			}
			if ((double)part.ComputePowerFactor < 1.5)
			{
				part.ComputePowerFactor = 1.5f;
			}
		}
		else
		{
			part.LevelHardened += part.Level;
			part.Level = modificationLevel;
			if (part.ChargeUse < 1)
			{
				part.ChargeUse = 1;
			}
			if ((double)part.ComputePowerFactor < 1.5)
			{
				part.ComputePowerFactor = 1.5f;
			}
			part.IsEMPSensitive = true;
		}
		int num = Math.Max((Tier >= 5) ? (5 - Tier / 2) : Tier, 2);
		BootSequence part2 = Object.GetPart<BootSequence>();
		if (part2 == null)
		{
			part2 = new BootSequence();
			part2.BootTime = num;
			part2.ReadoutInName = true;
			part2.ReadoutInDescription = true;
			Object.AddPart(part2);
		}
		else if (part2.BootTime < num)
		{
			part2.BootTime = num;
		}
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexity(2, 1);
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
			E.AddAdjective("{{K|flare-compensating}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier), GetEventSensitiveAddStatusSummary(E));
		return base.HandleEvent(E);
	}

	public static int GetModificationLevel(int Tier)
	{
		return Tier * 2;
	}

	public static string GetDescription(int Tier)
	{
		return "Flare-compensating: When powered and started up, the item offers protection against visual flash effects.";
	}
}
