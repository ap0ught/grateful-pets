using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsTonicDurationModifier : IActivePart
{
	public int Chance = 100;

	public string Types;

	public int Percentage = 100;

	public int Pass = 2;

	public bool HealingOnly;

	public bool AsSubject = true;

	public CyberneticsTonicDurationModifier()
	{
		WorksOnImplantee = true;
	}

	public override bool SameAs(IPart p)
	{
		CyberneticsTonicDurationModifier cyberneticsTonicDurationModifier = p as CyberneticsTonicDurationModifier;
		if (cyberneticsTonicDurationModifier.Chance != Chance)
		{
			return false;
		}
		if (cyberneticsTonicDurationModifier.Types != Types)
		{
			return false;
		}
		if (cyberneticsTonicDurationModifier.Percentage != Percentage)
		{
			return false;
		}
		if (cyberneticsTonicDurationModifier.Pass != Pass)
		{
			return false;
		}
		if (cyberneticsTonicDurationModifier.HealingOnly != HealingOnly)
		{
			return false;
		}
		if (cyberneticsTonicDurationModifier.AsSubject != AsSubject)
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
