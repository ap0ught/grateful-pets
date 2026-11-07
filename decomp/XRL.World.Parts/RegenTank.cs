using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class RegenTank : IPoweredPart
{
	public const int STATUS_READY = 1;

	public const int STATUS_TANK_NOT_FULL = 2;

	public const int STATUS_WRONG_MIXTURE = 3;

	public const int STATUS_MISSING_REGEN_LIQUID = 4;

	public const int STATUS_WRONG_SUBJECT = 5;

	public string RulesDescription = "Rejuvenates health and regenerates lost body parts.\nMust be filled with at least 100 drams of a liquid mixture that's at least 2/3rds convalessence to be fully functional.\nIf 1+ dram of cloning draught is present in the mixture, all lost body parts are regenerated over time and 1 dram of cloning draught is consumed. Excess cloning draught is not consumed.\nMust be entered and waited inside for regeneration to take effect.";

	public string RejuvenationStatusLine = "{{C|Rejuvenation status: }}";

	public string LimbRegenerationStatusLine = "{{C|Limb regeneration status: }}";

	public string RejuvenationTriggerNotify = "RegenTankRejuvenation";

	public string LimbRegenerationTriggerNotify = "RegenTankLimbRegeneration";

	public string BaseLiquid = "convalessence";

	public string RegenLiquid = "cloning";

	public string RejuvenationHealing = "1d3";

	public string RejuvenationEvent = "Recuperating";

	public string RejuvenationQuery;

	public string RejuvenationNotify;

	public string LimbRegenerationHealing = "5d8";

	public string LimbRegenerationEvent;

	public string LimbRegenerationQuery;

	public string LimbRegenerationNotify;

	public int MinTotalDrams = 100;

	public int BaseLiquidPermillageNeeded = 666;

	public int RegenLiquidMilliDramsNeeded = 850;

	public int RegenLiquidDramsUsed = 1;

	public int RejuvenationRegeneraLevel = 4;

	public int LimbRegenerationRegeneraLevel = 4;

	public bool RequiresAlive = true;

	public bool LimbRegenerationMinEvents = true;

	public bool LimbRegenerationMinEventsAll = true;

	public bool LimbRegenerationMinEventsWhole;

	public string LimbRegenerationMinEventsCategories;

	public string LimbRegenerationMinEventsExceptCategories;

	public RegenTank()
	{
		WorksOnSelf = true;
	}

	public override void Attach()
	{
		base.Attach();
		if (LimbRegenerationQuery == "AnyRegenerableLimbs" && LimbRegenerationNotify == "RegenerateAllLimbs")
		{
			LimbRegenerationQuery = null;
			LimbRegenerationNotify = null;
		}
	}

	public override bool SameAs(IPart p)
	{
		RegenTank regenTank = p as RegenTank;
		if (regenTank.RulesDescription != RulesDescription)
		{
			return false;
		}
		if (regenTank.RejuvenationStatusLine != RejuvenationStatusLine)
		{
			return false;
		}
		if (regenTank.LimbRegenerationStatusLine != LimbRegenerationStatusLine)
		{
			return false;
		}
		if (regenTank.RejuvenationTriggerNotify != RejuvenationTriggerNotify)
		{
			return false;
		}
		if (regenTank.LimbRegenerationTriggerNotify != LimbRegenerationTriggerNotify)
		{
			return false;
		}
		if (regenTank.BaseLiquid != BaseLiquid)
		{
			return false;
		}
		if (regenTank.RegenLiquid != RegenLiquid)
		{
			return false;
		}
		if (regenTank.RejuvenationHealing != RejuvenationHealing)
		{
			return false;
		}
		if (regenTank.RejuvenationEvent != RejuvenationEvent)
		{
			return false;
		}
		if (regenTank.RejuvenationQuery != RejuvenationQuery)
		{
			return false;
		}
		if (regenTank.RejuvenationNotify != RejuvenationNotify)
		{
			return false;
		}
		if (regenTank.LimbRegenerationHealing != LimbRegenerationHealing)
		{
			return false;
		}
		if (regenTank.LimbRegenerationEvent != LimbRegenerationEvent)
		{
			return false;
		}
		if (regenTank.LimbRegenerationQuery != LimbRegenerationQuery)
		{
			return false;
		}
		if (regenTank.LimbRegenerationNotify != LimbRegenerationNotify)
		{
			return false;
		}
		if (regenTank.MinTotalDrams != MinTotalDrams)
		{
			return false;
		}
		if (regenTank.BaseLiquidPermillageNeeded != BaseLiquidPermillageNeeded)
		{
			return false;
		}
		if (regenTank.RegenLiquidMilliDramsNeeded != RegenLiquidMilliDramsNeeded)
		{
			return false;
		}
		if (regenTank.RegenLiquidDramsUsed != RegenLiquidDramsUsed)
		{
			return false;
		}
		if (regenTank.RejuvenationRegeneraLevel != RejuvenationRegeneraLevel)
		{
			return false;
		}
		if (regenTank.LimbRegenerationRegeneraLevel != LimbRegenerationRegeneraLevel)
		{
			return false;
		}
		if (regenTank.RequiresAlive != RequiresAlive)
		{
			return false;
		}
		if (regenTank.LimbRegenerationMinEvents != LimbRegenerationMinEvents)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GenericNotifyEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GenericNotifyEvent E)
	{
		if (E.Notify == RejuvenationTriggerNotify)
		{
			if (E.Subject != null && GetRejuvenationStatus(E.Subject) == 1 && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && (RejuvenationQuery.IsNullOrEmpty() || GenericQueryEvent.Check(E.Subject, RejuvenationQuery, null, ParentObject, RejuvenationRegeneraLevel)))
			{
				if (!RejuvenationHealing.IsNullOrEmpty())
				{
					int num = RejuvenationHealing.RollCached();
					if (num > 0)
					{
						E.Subject.Heal(num, Message: false, FloatText: true, RandomMinimum: true);
					}
				}
				if (!RejuvenationEvent.IsNullOrEmpty())
				{
					E.Subject.FireEvent(RejuvenationEvent);
				}
				if (RejuvenationRegeneraLevel != 0)
				{
					E.Subject.FireEvent(Event.New("Regenera", "Source", ParentObject, "Level", RejuvenationRegeneraLevel));
				}
				if (!RejuvenationNotify.IsNullOrEmpty())
				{
					GenericNotifyEvent.Send(E.Subject, RejuvenationNotify, null, ParentObject, RejuvenationRegeneraLevel);
				}
			}
		}
		else if (E.Notify == LimbRegenerationTriggerNotify && E.Subject != null && GetLimbRegenerationStatus(E.Subject) == 1 && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && (LimbRegenerationQuery.IsNullOrEmpty() || GenericQueryEvent.Check(E.Subject, LimbRegenerationQuery, null, ParentObject, LimbRegenerationRegeneraLevel)))
		{
			if (!LimbRegenerationHealing.IsNullOrEmpty())
			{
				int num2 = LimbRegenerationHealing.RollCached();
				if (num2 > 0)
				{
					E.Subject.Heal(num2, Message: false, FloatText: true, RandomMinimum: true);
				}
			}
			if (LimbRegenerationMinEvents)
			{
				BodyPartCategory.ProcessList(LimbRegenerationMinEventsCategories, out var Category, out var Categories);
				BodyPartCategory.ProcessList(LimbRegenerationMinEventsExceptCategories, out var Category2, out var Categories2);
				GameObject subject = E.Subject;
				GameObject parentObject = ParentObject;
				bool limbRegenerationMinEventsWhole = LimbRegenerationMinEventsWhole;
				bool limbRegenerationMinEventsWhole2 = LimbRegenerationMinEventsWhole;
				bool all = limbRegenerationMinEventsWhole;
				int? category = Category;
				int[] categories = Categories;
				int? exceptCategory = Category2;
				int[] exceptCategories = Categories2;
				RegenerateLimbEvent.Send(subject, null, parentObject, limbRegenerationMinEventsWhole2, all, IncludeMinor: true, Voluntary: true, null, category, categories, exceptCategory, exceptCategories);
			}
			if (!LimbRegenerationEvent.IsNullOrEmpty())
			{
				E.Subject.FireEvent(LimbRegenerationEvent);
			}
			if (LimbRegenerationRegeneraLevel != 0)
			{
				E.Subject.FireEvent(Event.New("Regenera", "Source", ParentObject, "Level", LimbRegenerationRegeneraLevel));
			}
			if (!LimbRegenerationNotify.IsNullOrEmpty())
			{
				GenericNotifyEvent.Send(E.Subject, LimbRegenerationNotify, null, ParentObject, LimbRegenerationRegeneraLevel);
			}
			if (RegenLiquidDramsUsed > 0)
			{
				ParentObject.LiquidVolume?.UseDrams(RegenLiquid, RegenLiquidDramsUsed);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!RejuvenationStatusLine.IsNullOrEmpty() || !LimbRegenerationStatusLine.IsNullOrEmpty())
		{
			E.Base.Append('\n');
			if (!RejuvenationStatusLine.IsNullOrEmpty())
			{
				E.Base.Append('\n').Append(RejuvenationStatusLine).Append(GetRejuvenationStatusString());
			}
			if (!LimbRegenerationStatusLine.IsNullOrEmpty())
			{
				E.Base.Append('\n').Append(LimbRegenerationStatusLine).Append(GetLimbRegenerationStatusString());
			}
		}
		if (!RulesDescription.IsNullOrEmpty())
		{
			E.Postfix.AppendRules(RulesDescription);
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return GetRejuvenationStatus() != 1;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return GetRejuvenationStatus() switch
		{
			2 => "TankNotFull", 
			3 => "ImproperMixture", 
			5 => "ImproperSubject", 
			_ => null, 
		};
	}

	public int GetRejuvenationStatus(GameObject who = null)
	{
		if (RequiresAlive && who != null && !who.IsAlive)
		{
			return 5;
		}
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume == null || liquidVolume.Volume < MinTotalDrams)
		{
			return 2;
		}
		if (liquidVolume.Proportion(BaseLiquid) < BaseLiquidPermillageNeeded)
		{
			return 3;
		}
		return 1;
	}

	public int GetLimbRegenerationStatus(GameObject who = null)
	{
		if (RequiresAlive && who != null && !who.IsAlive)
		{
			return 5;
		}
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume == null || liquidVolume.Volume < MinTotalDrams)
		{
			return 2;
		}
		if (liquidVolume.Proportion(BaseLiquid) < BaseLiquidPermillageNeeded)
		{
			return 3;
		}
		liquidVolume.MilliAmount(RegenLiquid);
		if (liquidVolume.MilliAmount(RegenLiquid) < RegenLiquidMilliDramsNeeded)
		{
			return 4;
		}
		return 1;
	}

	public string GetRejuvenationStatusString()
	{
		int rejuvenationStatus = GetRejuvenationStatus();
		if (rejuvenationStatus == 1)
		{
			string statusSummary = GetStatusSummary();
			if (statusSummary != null)
			{
				return statusSummary;
			}
		}
		return GetStatusString(rejuvenationStatus);
	}

	public string GetLimbRegenerationStatusString()
	{
		int limbRegenerationStatus = GetLimbRegenerationStatus();
		if (limbRegenerationStatus == 1)
		{
			string statusSummary = GetStatusSummary();
			if (statusSummary != null)
			{
				return statusSummary;
			}
		}
		return GetStatusString(limbRegenerationStatus);
	}

	public string GetStatusString(int status)
	{
		return status switch
		{
			1 => "{{G|ready}}", 
			2 => "{{K|tank not full}}", 
			3 => "{{R|improper mixture}}", 
			4 => "{{K|insufficient " + LiquidVolume.GetLiquid(RegenLiquid).GetName().Strip() + "}}", 
			5 => "{{R|improper subject}}", 
			_ => "{{K|unknown failure}}", 
		};
	}
}
