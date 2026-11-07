using System;

namespace XRL.World.Parts;

[Serializable]
public class ModPolarized : IModification
{
	public ModPolarized()
	{
	}

	public ModPolarized(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return IModification.CheckWornSlot(Object, "Head", "Face");
	}

	public override void ApplyModification(GameObject Object)
	{
		FlareCompensation part = Object.GetPart<FlareCompensation>();
		int modificationLevel = GetModificationLevel(Tier);
		if (part == null)
		{
			part = new FlareCompensation();
			part.Level = modificationLevel;
			part.IsBootSensitive = false;
			part.IsEMPSensitive = false;
			part.ShowInShortDescription = false;
			part.DescribeStatusForProperty = null;
			Object.AddPart(part);
		}
		else if (part.IsEMPSensitive)
		{
			part.LevelHardened += modificationLevel;
			part.ShowInShortDescription = false;
		}
		else
		{
			part.Level += modificationLevel;
			part.ShowInShortDescription = false;
		}
		IncreaseComplexityIfComplex(1);
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
			E.AddAdjective("{{polarized|polarized}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static int GetModificationLevel(int Tier)
	{
		return Math.Max(1, Tier / 3);
	}

	public static string GetDescription(int Tier)
	{
		return "Polarized: The item offers protection against visual flash effects.";
	}
}
