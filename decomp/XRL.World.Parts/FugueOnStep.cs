using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class FugueOnStep : IActivePart
{
	public int Chance = 100;

	public string SaveStat;

	public string SaveDifficultyStat;

	public int SaveTarget = 15;

	public string SaveVs;

	public bool TriggerOnSaveSuccess = true;

	public string Duration = "2d6+20";

	public string Copies = "1d4+1";

	public string Cooldown = "100-200";

	public int HostileCopyChance;

	public string HostileCopyColorString;

	public string HostileCopyPrefix;

	public string FriendlyCopyColorString;

	public string FriendlyCopyPrefix;

	public new string ReadyColorString = "&G";

	public string ReadyTileColor = "&G";

	public new string ReadyDetailColor = "M";

	public string CooldownColorString = "&g";

	public string CooldownTileColor = "&g";

	public string CooldownDetailColor = "m";

	public bool TreatAsPsychic = true;

	public int CooldownLeft;

	public FugueOnStep()
	{
		WorksOnCellContents = true;
	}

	public override bool SameAs(IPart p)
	{
		FugueOnStep fugueOnStep = p as FugueOnStep;
		if (fugueOnStep.Chance != Chance)
		{
			return false;
		}
		if (fugueOnStep.SaveStat != SaveStat)
		{
			return false;
		}
		if (fugueOnStep.SaveDifficultyStat != SaveDifficultyStat)
		{
			return false;
		}
		if (fugueOnStep.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (fugueOnStep.SaveVs != SaveVs)
		{
			return false;
		}
		if (fugueOnStep.TriggerOnSaveSuccess != TriggerOnSaveSuccess)
		{
			return false;
		}
		if (fugueOnStep.Duration != Duration)
		{
			return false;
		}
		if (fugueOnStep.Copies != Copies)
		{
			return false;
		}
		if (fugueOnStep.Cooldown != Cooldown)
		{
			return false;
		}
		if (fugueOnStep.HostileCopyChance != HostileCopyChance)
		{
			return false;
		}
		if (fugueOnStep.HostileCopyColorString != HostileCopyColorString)
		{
			return false;
		}
		if (fugueOnStep.HostileCopyPrefix != HostileCopyPrefix)
		{
			return false;
		}
		if (fugueOnStep.FriendlyCopyColorString != FriendlyCopyColorString)
		{
			return false;
		}
		if (fugueOnStep.FriendlyCopyPrefix != FriendlyCopyPrefix)
		{
			return false;
		}
		if (fugueOnStep.ReadyColorString != ReadyColorString)
		{
			return false;
		}
		if (fugueOnStep.ReadyTileColor != ReadyTileColor)
		{
			return false;
		}
		if (fugueOnStep.ReadyDetailColor != ReadyDetailColor)
		{
			return false;
		}
		if (fugueOnStep.CooldownColorString != CooldownColorString)
		{
			return false;
		}
		if (fugueOnStep.CooldownTileColor != CooldownTileColor)
		{
			return false;
		}
		if (fugueOnStep.CooldownDetailColor != CooldownDetailColor)
		{
			return false;
		}
		if (fugueOnStep.CooldownLeft != CooldownLeft)
		{
			return false;
		}
		if (fugueOnStep.TreatAsPsychic != TreatAsPsychic)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (CooldownLeft > 0)
		{
			CooldownLeft--;
			if (CooldownLeft <= 0)
			{
				SyncColor();
			}
		}
		CheckActivate();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<InterruptAutowalkEvent>.ID && (ID != PooledEvent<IsSensableAsPsychicEvent>.ID || !TreatAsPsychic))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		E.IndicateObject = ParentObject;
		return false;
	}

	public override bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		if (TreatAsPsychic)
		{
			E.Sensable = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		SyncColor();
		return base.HandleEvent(E);
	}

	public void CheckActivate()
	{
		if (CooldownLeft > 0 || !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) || !Chance.in100())
		{
			return;
		}
		List<GameObject> activePartSubjects = GetActivePartSubjects();
		bool flag = false;
		for (int i = 0; i < activePartSubjects.Count; i++)
		{
			GameObject gameObject = activePartSubjects[i];
			if (ValidStepTarget(gameObject) && (string.IsNullOrEmpty(SaveStat) || gameObject.MakeSave(SaveStat, SaveTarget, ParentObject, SaveDifficultyStat) == TriggerOnSaveSuccess) && Activate(gameObject))
			{
				flag = true;
			}
		}
		if (flag && !string.IsNullOrEmpty(Cooldown))
		{
			ConsumeChargeIfOperational();
			CooldownLeft = Stat.Roll(Cooldown);
			SyncColor();
		}
	}

	public bool Activate(GameObject Subject)
	{
		if (!TemporalFugue.PerformTemporalFugue(ParentObject, Subject, null, null, null, Involuntary: true, IsRealityDistortionBased: true, Stat.Roll(Duration), Stat.Roll(Copies), HostileCopyChance, "Temporal Fugue", HostileCopyColorString: HostileCopyColorString, HostileCopyPrefix: HostileCopyPrefix, FriendlyCopyColorString: FriendlyCopyColorString, FriendlyCopyPrefix: FriendlyCopyPrefix))
		{
			return false;
		}
		if (Visible())
		{
			if (Subject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You step on " + ParentObject.t() + " and vibrate through spacetime.");
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage(Subject.Does("step") + " on " + ParentObject.t() + " and" + Subject.GetVerb("vibrate") + " through spacetime.");
			}
		}
		return true;
	}

	public void SyncColor()
	{
		if (CooldownLeft > 0)
		{
			if (!string.IsNullOrEmpty(CooldownColorString))
			{
				ParentObject.Render.ColorString = CooldownColorString;
			}
			if (!string.IsNullOrEmpty(CooldownTileColor))
			{
				ParentObject.Render.TileColor = CooldownTileColor;
			}
			if (!string.IsNullOrEmpty(CooldownDetailColor))
			{
				ParentObject.Render.DetailColor = CooldownDetailColor;
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(ReadyColorString))
			{
				ParentObject.Render.ColorString = ReadyColorString;
			}
			if (!string.IsNullOrEmpty(ReadyTileColor))
			{
				ParentObject.Render.TileColor = ReadyTileColor;
			}
			if (!string.IsNullOrEmpty(ReadyDetailColor))
			{
				ParentObject.Render.DetailColor = ReadyDetailColor;
			}
		}
	}

	public bool ValidStepTarget(GameObject obj)
	{
		if (obj.IsCombatObject() && obj.FlightMatches(ParentObject))
		{
			return !obj.HasPart(typeof(FugueOnStep));
		}
		return false;
	}
}
