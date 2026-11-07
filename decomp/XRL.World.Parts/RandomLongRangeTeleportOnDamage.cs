using System;

namespace XRL.World.Parts;

[Serializable]
public class RandomLongRangeTeleportOnDamage : IPoweredPart
{
	public int Chance;

	public string Message;

	public bool AffectsUnavoidable;

	public bool ShowInShortDescription = true;

	public RandomLongRangeTeleportOnDamage()
	{
		ChargeUse = 0;
		IsRealityDistortionBased = true;
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		RandomLongRangeTeleportOnDamage randomLongRangeTeleportOnDamage = p as RandomLongRangeTeleportOnDamage;
		if (randomLongRangeTeleportOnDamage.Chance != Chance)
		{
			return false;
		}
		if (randomLongRangeTeleportOnDamage.Message != Message)
		{
			return false;
		}
		if (randomLongRangeTeleportOnDamage.AffectsUnavoidable != AffectsUnavoidable)
		{
			return false;
		}
		if (randomLongRangeTeleportOnDamage.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != TookDamageEvent.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return ShowInShortDescription;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (IsObjectActivePartSubject(E.Object) && (AffectsUnavoidable || !E.Damage.HasAttribute("Unavoidable")) && Chance.in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			PerformTeleport(E.Object, UsePopups: true, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			string text = ((IsPowerSwitchSensitive && ParentObject.HasPart<PowerSwitch>()) ? "When activated, " : "");
			if (Chance > 0)
			{
				E.Postfix.AppendRules(text + Chance + "% chance of being randomly long-range teleported when taking " + (AffectsUnavoidable ? "" : "avoidable ") + "damage.", GetEventSensitiveAddStatusSummary(E));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineFailureEvent E)
	{
		if (ExamineFailure(E, 25))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (ExamineFailure(E, 50))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && PerformTeleport(E.Actor, E.Actor.IsPlayer(), E))
		{
			E.Identify = true;
			E.IdentifyIfDestroyed = true;
			return true;
		}
		return false;
	}

	private bool PerformTeleport(GameObject Subject, bool UsePopups = true, IEvent FromEvent = null)
	{
		string randomDestinationZoneID = SpaceTimeVortex.GetRandomDestinationZoneID(GetAnyBasisCell()?.ParentZone?.GetZoneWorld());
		if (randomDestinationZoneID == null)
		{
			return false;
		}
		if (!Message.IsNullOrEmpty())
		{
			string text = GameText.VariableReplace(Message, Subject, ParentObject, UsePopups);
			if (!text.IsNullOrEmpty())
			{
				EmitMessage(text, ' ', FromDialog: false, UsePopups);
			}
		}
		GameObject parentObject = ParentObject;
		return Subject.ZoneTeleport(randomDestinationZoneID, -1, -1, FromEvent, parentObject, null, null, null);
	}
}
