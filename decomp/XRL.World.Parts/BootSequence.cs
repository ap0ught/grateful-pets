using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class BootSequence : IPoweredPart
{
	public int BootTime = 100;

	public string VariableBootTime;

	public int BootTimeLeft;

	public bool ReadoutInName;

	public bool ReadoutInDescription;

	public bool AlwaysObvious;

	public bool ObviousIfUnderstood;

	public string TextInDescription;

	public string VerbOnBootInitialized = "beep";

	public string VerbOnBootDone = "click";

	public string VerbOnBootAborted = "bloop";

	public string SoundOnBootInitialized = "Sounds/Interact/sfx_interact_artifact_windup";

	public string SoundOnBootDone = "Sounds/Interact/sfx_interact_artifact_ready";

	public string SoundOnBootAborted = "Sounds/Interact/sfx_interact_artifact_abort_bloop";

	public float SoundVolumeOnBootInitialized = 0.6f;

	public float SoundVolumeOnBootDone = 0.5f;

	public float SoundVolumeOnBootAborted = 1f;

	public float ComputePowerFactor = 2.5f;

	public bool Sensitive;

	public bool PartWasReady;

	public BootSequence()
	{
		IsBootSensitive = false;
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		BootSequence bootSequence = p as BootSequence;
		if (bootSequence.BootTime != BootTime)
		{
			return false;
		}
		if (bootSequence.VariableBootTime != VariableBootTime)
		{
			return false;
		}
		if (bootSequence.ReadoutInName != ReadoutInName)
		{
			return false;
		}
		if (bootSequence.ReadoutInDescription != ReadoutInDescription)
		{
			return false;
		}
		if (bootSequence.AlwaysObvious != AlwaysObvious)
		{
			return false;
		}
		if (bootSequence.ObviousIfUnderstood != ObviousIfUnderstood)
		{
			return false;
		}
		if (bootSequence.TextInDescription != TextInDescription)
		{
			return false;
		}
		if (bootSequence.VerbOnBootInitialized != VerbOnBootInitialized)
		{
			return false;
		}
		if (bootSequence.VerbOnBootDone != VerbOnBootDone)
		{
			return false;
		}
		if (bootSequence.VerbOnBootAborted != VerbOnBootAborted)
		{
			return false;
		}
		if (bootSequence.SoundOnBootInitialized != SoundOnBootInitialized)
		{
			return false;
		}
		if (bootSequence.SoundOnBootDone != SoundOnBootDone)
		{
			return false;
		}
		if (bootSequence.SoundOnBootAborted != SoundOnBootAborted)
		{
			return false;
		}
		if (bootSequence.ComputePowerFactor != ComputePowerFactor)
		{
			return false;
		}
		if (bootSequence.Sensitive != Sensitive)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public bool IsObvious()
	{
		if (AlwaysObvious)
		{
			return true;
		}
		if ((ObviousIfUnderstood || ReadoutInName || ReadoutInDescription) && ParentObject.Understood())
		{
			return true;
		}
		return false;
	}

	private void BootUI(GameObject Context, string Verb, string Sound)
	{
		if (!string.IsNullOrEmpty(Verb))
		{
			DidX(Verb, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: false, Context);
		}
		if (!string.IsNullOrEmpty(Sound))
		{
			PlayWorldSound(Sound, 0.5f, 0f, Combat: false, Context?.GetCurrentCell());
		}
	}

	private void ResetBootTime()
	{
		BootTimeLeft = GetAvailableComputePowerEvent.AdjustDown(this, (VariableBootTime != null) ? Math.Max(VariableBootTime.RollCached(), BootTime) : BootTime, ComputePowerFactor);
	}

	private void InitBoot(GameObject Context = null)
	{
		ResetBootTime();
		BootSequenceInitializedEvent.Send(ParentObject);
		SyncRenderEvent.Send(ParentObject);
		BootUI(Context, VerbOnBootInitialized, SoundOnBootInitialized);
		ConsumeCharge();
	}

	private void AbortBoot(GameObject Context = null)
	{
		BootSequenceAbortedEvent.Send(ParentObject);
		SyncRenderEvent.Send(ParentObject);
		BootUI(Context, VerbOnBootAborted, SoundOnBootAborted);
	}

	private void SyncBoot(GameObject Context = null)
	{
		if (PartWasReady && Sensitive)
		{
			AbortBoot(Context);
		}
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (PartWasReady)
			{
				if (!Sensitive)
				{
					AbortBoot(Context);
				}
				PartWasReady = false;
			}
		}
		else
		{
			if (!PartWasReady || Sensitive)
			{
				InitBoot(Context);
			}
			PartWasReady = true;
		}
	}

	public void Reboot()
	{
		if (PartWasReady)
		{
			AbortBoot();
		}
		PartWasReady = IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (PartWasReady)
		{
			InitBoot();
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EnteredCellEvent.ID && ID != EquippedEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != LeftCellEvent.ID && ID != PooledEvent<QueryDrawEvent>.ID && ID != TakenEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(QueryDrawEvent E)
	{
		if (BootTimeLeft <= 0 || !WasReadyIfKnown())
		{
			return true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "BootTime", BootTime);
		E.AddEntry(this, "VariableBootTime", VariableBootTime);
		E.AddEntry(this, "BootTimeLeft", BootTimeLeft);
		E.AddEntry(this, "Sensitive", Sensitive);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		Reboot();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (PartWasReady && !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, (BootTimeLeft > 0) ? ChargeUse : 0, UseChargeIfUnpowered: false, 0L))
		{
			AbortBoot();
			PartWasReady = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		SyncBoot(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		SyncBoot(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		SyncBoot(E.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		SyncBoot(E.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		SyncBoot();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		SyncBoot();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (BootTimeLeft > 0 && ReadoutInName && PartWasReady && E.Understood())
		{
			E.AddTag("[{{K|" + BootTimeLeft + " sec}}]", 40);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (BootTimeLeft > 0 && PartWasReady)
		{
			if (!string.IsNullOrEmpty(TextInDescription))
			{
				E.Postfix.Append('\n').Append(TextInDescription);
			}
			if (ReadoutInDescription)
			{
				E.Postfix.Append('\n').Append(ParentObject.Its).Append(" readout indicates that ")
					.Append(ParentObject.its)
					.Append(" startup sequence will take an estimated ")
					.Append(Grammar.Cardinal(BootTimeLeft))
					.Append(" more ")
					.Append((BootTimeLeft == 1) ? "round" : "rounds")
					.Append(".");
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("GameStart");
		Registrar.Register("LiquidFueledPowerPlantFueled");
		Registrar.Register("ObjectEntered");
		Registrar.Register("ObjectExited");
		Registrar.Register("Reboot");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Reboot")
		{
			Reboot();
		}
		else if (E.ID == "GameStart")
		{
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, 0, UseChargeIfUnpowered: false, 0L))
			{
				BootTimeLeft = 0;
				BootSequenceDoneEvent.Send(ParentObject);
				PartWasReady = true;
			}
			else
			{
				PartWasReady = false;
			}
		}
		else if (E.ID == "ObjectEntered" || E.ID == "ObjectExited")
		{
			SyncBoot();
		}
		else if (E.ID == "LiquidFueledPowerPlantFueled" && !PartWasReady && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			InitBoot();
			PartWasReady = true;
		}
		return base.FireEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (!PartWasReady)
			{
				InitBoot();
			}
			else if (BootTimeLeft > 0)
			{
				BootTimeLeft = Math.Max(BootTimeLeft - Amount, 0);
				if (BootTimeLeft <= 0)
				{
					BootSequenceDoneEvent.Send(ParentObject);
					BootUI(null, VerbOnBootDone, SoundOnBootDone);
				}
				ConsumeCharge();
			}
			PartWasReady = true;
		}
		else if (PartWasReady && !IsWorldMapActive)
		{
			AbortBoot();
			PartWasReady = false;
		}
	}

	public void ForceBoot()
	{
		BootTimeLeft = 0;
		PartWasReady = true;
	}
}
