using System;

namespace XRL.World.Parts;

[Serializable]
public class ModMercurial : IModification
{
	public int Chance = 100;

	public ModMercurial()
	{
		WorksOnEquipper = true;
		ChargeUse = 100;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsTechScannable = true;
		NameForStatus = "ReactiveTeleporter";
	}

	public ModMercurial(int Tier)
		: base(Tier)
	{
	}

	public ModMercurial(int Tier, int Chance)
		: this(Tier)
	{
		this.Chance = Chance;
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexity(3, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterEvent(this, BeforeApplyDamageEvent.ID, 0, Serialize: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterEvent(this, BeforeApplyDamageEvent.ID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{Y|mercurial}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription(), GetEventSensitiveAddStatusSummary(E));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				if (!activePartSubject.OnWorldMap() && IComponent<GameObject>.CheckRealityDistortionUsability(activePartSubject, null, activePartSubject, ParentObject) && Chance.in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					activePartSubject.RandomTeleport(Swirl: true);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public string GetInstanceDescription()
	{
		return "Mercurial: Teleports the user to safety upon taking damage" + ((Chance == 100) ? "" : (" (" + Chance + "% chance)")) + ".";
	}
}
