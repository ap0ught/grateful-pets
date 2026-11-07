using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, the item's tier for purposes of calculating
/// damage behavior is increased by the standard power load bonus, i.e.
/// 2 for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class ModElectrified : IMeleeModification
{
	public ModElectrified()
	{
	}

	public ModElectrified(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		ChargeUse = 10;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsPowerLoadSensitive = true;
		IsTechScannable = true;
		NameForStatus = "ArcDischarger";
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeMeleeAttackEvent>.ID && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeMeleeAttackEvent E)
	{
		if (E.Weapon == ParentObject && WasReady())
		{
			PlayWorldSound("Sounds/Enhancements/sfx_enhancement_electric_attack", 0.5f, 0f, Combat: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{electrical|electrified}}");
		}
		return base.HandleEvent(E);
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
		if (ExamineFailure(E, 50))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("circuitry", 5);
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
				int lowDamage = GetLowDamage(num);
				int highDamage = GetHighDamage(num);
				int voltage = Stat.Random(lowDamage, highDamage);
				string damageRange = GetDamageRange(num);
				GameObject target = gameObjectParameter2;
				gameObjectParameter.Discharge(null, voltage, 0, damageRange, null, gameObjectParameter, ParentObject, target);
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
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
			{
				int lowDamage = GetLowDamage(num);
				int highDamage = GetHighDamage(num);
				int voltage = Stat.Random(lowDamage, highDamage);
				string damageRange = GetDamageRange(num);
				if (ParentObject.Discharge(null, Target: E.Actor, Voltage: voltage, Damage: 0, DamageRange: damageRange, DamageRoll: null, Owner: E.Actor, Source: ParentObject, DescribeAsFrom: ParentObject, Skip: null, SkipList: null, StartCell: null, Phase: 0, Accidental: false, Environmental: false, UsePopups: E.Actor.IsPlayer()) > 0)
				{
					if (ParentObject.IsBlueprintUnderstood())
					{
						E.Identify = true;
					}
					return true;
				}
			}
		}
		return false;
	}

	public static int GetLowDamage(int Tier, int PowerLoad = 100)
	{
		return Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad);
	}

	public int GetLowDamage(int PowerLoad = 100)
	{
		return GetLowDamage(Tier, PowerLoad);
	}

	public static int GetHighDamage(int Tier, int PowerLoad = 100)
	{
		return (Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) * 3 / 2;
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

	public static string GetDescription(int Tier)
	{
		return "Electrified: When powered, this weapon deals " + ((Tier > 0) ? ("an additional " + GetDamageRange(Tier, 100)) : "additional") + " electrical damage on hit.";
	}

	public string GetInstanceDescription()
	{
		int powerLoad = MyPowerLoadLevel();
		return "Electrified: When powered, this weapon deals an additional " + GetDamageRange(Tier, powerLoad) + " electrical damage on hit.";
	}
}
