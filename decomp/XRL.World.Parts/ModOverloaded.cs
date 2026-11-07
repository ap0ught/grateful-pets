using System;

namespace XRL.World.Parts;

[Serializable]
public class ModOverloaded : IModification
{
	public ModOverloaded()
	{
	}

	public ModOverloaded(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		IsEMPSensitive = true;
		IsTechScannable = true;
		NameForStatus = "PowerRegulator";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return IsOverloadableEvent.Check(Object);
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexity(2, 1);
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
		E.Level += 300;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName && !E.Object.HasPart<ModMassivelyOverloaded>())
		{
			E.AddAdjective("{{overloaded|overloaded}}", -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!E.Object.HasPart<ModMassivelyOverloaded>())
		{
			E.Postfix.AppendRules(GetDescription(Tier));
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Basis, IEventRegistrar Registrar)
	{
		Registrar.Register("QueryMissileFireSound");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "QueryMissileFireSound")
		{
			PlayWorldSound("sfx_missile_addLayer_overloaded_fire");
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Overloaded: This item has increased performance but consumes extra charge, generates heat when used, and has a chance to break relative to its charge draw.";
	}
}
