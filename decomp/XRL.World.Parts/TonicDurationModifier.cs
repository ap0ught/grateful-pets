using System;

namespace XRL.World.Parts;

[Serializable]
public class TonicDurationModifier : IPoweredPart
{
	public int Chance = 100;

	public string Types;

	public int Percentage = 100;

	public int Pass = 2;

	public bool HealingOnly;

	public bool AsSubject = true;

	public TonicDurationModifier()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		TonicDurationModifier tonicDurationModifier = p as TonicDurationModifier;
		if (tonicDurationModifier.Chance != Chance)
		{
			return false;
		}
		if (tonicDurationModifier.Types != Types)
		{
			return false;
		}
		if (tonicDurationModifier.Percentage != Percentage)
		{
			return false;
		}
		if (tonicDurationModifier.Pass != Pass)
		{
			return false;
		}
		if (tonicDurationModifier.HealingOnly != HealingOnly)
		{
			return false;
		}
		if (tonicDurationModifier.AsSubject != AsSubject)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetTonicDurationEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTonicDurationEvent E)
	{
		if (E.Pass == Pass && (Types.IsNullOrEmpty() || Types.CachedCommaExpansion().Contains(E.Type)) && (!HealingOnly || E.Healing) && E.Checking == (AsSubject ? "Subject" : "Actor") && IsObjectActivePartSubject(AsSubject ? E.Subject : E.Actor) && Chance.in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Duration += E.BaseDuration * Percentage / 100;
		}
		return base.HandleEvent(E);
	}
}
