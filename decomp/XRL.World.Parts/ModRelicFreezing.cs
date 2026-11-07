using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, the item's tier for purposes of calculating
/// damage is increased by the standard power load bonus, i.e. 2 for the
/// standard overload power load of 400.
/// </remarks>
[Serializable]
public class ModRelicFreezing : IModification
{
	public ModRelicFreezing()
	{
	}

	public ModRelicFreezing(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		ChargeUse = 8;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsPowerLoadSensitive = true;
		IsTechScannable = true;
		NameForStatus = "HeatPump";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.HasPart<ModFreezing>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription(), GetEventSensitiveAddStatusSummary(E));
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
		if (ExamineFailure(E, 25))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			int num = MyPowerLoadLevel();
			int? powerLoadLevel = num;
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
				gameObjectParameter2.TemperatureChange(GetTemperatureChangeRange(num).RollCached(), gameObjectParameter);
				gameObjectParameter2.TakeDamage(Stat.Random(GetLowDamage(num), GetHighDamage(num)), "from %t freezing weapon!", "Cold", null, null, null, gameObjectParameter);
			}
		}
		return base.FireEvent(E);
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100())
		{
			int num = MyPowerLoadLevel();
			int? powerLoadLevel = num;
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel) && E.Actor.TakeDamage(Stat.Random(GetLowDamage(num), GetHighDamage(num)), Attacker: E.Actor, Message: "from " + ParentObject.t() + "!", Attributes: "Cold", DeathReason: null, ThirdPersonDeathReason: null, Owner: null, Source: null, Perspective: null, DescribeAsFrom: null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: false, SilentIfNoDamage: true, NoSetTarget: false, UsePopups: E.Actor.IsPlayer(), Phase: 5, ShowDamageType: "{{icy|cold damage}}"))
			{
				E.Actor.TemperatureChange(GetTemperatureChangeRange(num).RollCached(), E.Actor);
				if (ParentObject.IsBlueprintUnderstood())
				{
					E.Identify = true;
				}
				return true;
			}
		}
		return false;
	}

	public static int GetLowDamage(int Tier, int PowerLoad = 100)
	{
		return (int)Math.Round((double)(Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) * 1.6) + 3;
	}

	public int GetLowDamage(int PowerLoad = 100)
	{
		return GetLowDamage(Tier, PowerLoad);
	}

	public static int GetHighDamage(int Tier, int PowerLoad = 100)
	{
		return (int)Math.Round((double)(Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) * 2.4) + 3;
	}

	public int GetHighDamage(int PowerLoad = 100)
	{
		return GetHighDamage(Tier, PowerLoad);
	}

	public static string GetDamageRange(int Tier, int PowerLoad = 100)
	{
		int lowDamage = GetLowDamage(Tier, PowerLoad);
		int highDamage = GetHighDamage(Tier, PowerLoad);
		if (lowDamage == highDamage)
		{
			return lowDamage.ToString();
		}
		return lowDamage + "-" + highDamage;
	}

	public string GetDamageRange(int PowerLoad = 100)
	{
		return GetDamageRange(Tier, PowerLoad);
	}

	public string GetTemperatureChangeRange(int PowerLoad = 100)
	{
		return "-" + (Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) + "d4";
	}

	public static string GetDescription(int Tier)
	{
		return "Freezing: When powered, this weapon deals " + ((Tier > 0) ? ("an additional " + GetDamageRange(Tier, 100)) : "") + " cold damage on hit.";
	}

	public string GetInstanceDescription()
	{
		int powerLoad = MyPowerLoadLevel();
		return "Freezing: When powered, this weapon deals an additional " + GetDamageRange(Tier, powerLoad) + " cold damage on hit.";
	}
}
