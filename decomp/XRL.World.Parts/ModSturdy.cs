using System;

namespace XRL.World.Parts;

[Serializable]
public class ModSturdy : IModification
{
	public ModSturdy()
	{
	}

	public ModSturdy(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "StructuralSupport";
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RemovePart<BreakableInMelee>();
		StatShifter.SetStatShift(Object, "Hitpoints", Object.hitpoints / 4, baseValue: true);
	}

	public override void Remove()
	{
		StatShifter.RemoveStatShifts(ParentObject);
		base.Remove();
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
		if (E.Understood() && !E.Object.HasProperName && !E.Object.IsNatural())
		{
			E.AddAdjective("sturdy");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier, ParentObject));
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyBroken");
		Registrar.Register("ApplyShatterArmor");
		Registrar.Register("ApplyShatteredArmor");
		Registrar.Register("CanApplyBroken");
		Registrar.Register("CanApplyShatterArmor");
		Registrar.Register("CanApplyShatteredArmor");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyBroken" || E.ID == "CanApplyShatteredArmor" || E.ID == "CanApplyShatterArmor")
		{
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
		}
		else if ((E.ID == "ApplyBroken" || E.ID == "ApplyShatteredArmor" || E.ID == "ApplyShatterArmor") && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Sturdy: This item cannot break or crack, though it can still be destroyed.";
	}

	public static string GetDescription(int Tier, GameObject obj)
	{
		if (obj.HasPart<NoDamage>())
		{
			return "Sturdy: This item cannot break or crack.";
		}
		return "Sturdy: This item cannot break or crack, though it can still be destroyed.";
	}
}
