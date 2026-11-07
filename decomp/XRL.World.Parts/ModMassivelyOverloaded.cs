using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModMassivelyOverloaded : IModification
{
	public ModMassivelyOverloaded()
	{
	}

	public ModMassivelyOverloaded(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		IsEMPSensitive = true;
		IsTechScannable = true;
		NameForStatus = "AuxiliaryPowerRegulator";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<ModOverloaded>();
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetPowerLoadLevelEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPowerLoadLevelEvent E)
	{
		E.Level += 800;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{overloaded|massively overloaded}}", -20);
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
		return "Massively overloaded: This item has significantly increased performance but consumes a great deal of extra charge, generates considerable heat when used, and has a chance to break relative to its charge draw.";
	}
}
