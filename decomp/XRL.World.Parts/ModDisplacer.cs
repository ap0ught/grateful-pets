using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, the maximum teleport distance is increased
/// by the standard power load bonus, i.e. 2 for the standard overload
/// power load of 400.
/// </remarks>
[Serializable]
public class ModDisplacer : IModification
{
	public const int MIN_DISTANCE = 1;

	public const int MAX_DISTANCE = 4;

	public ModDisplacer()
	{
	}

	public ModDisplacer(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		ChargeUse = 250;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsPowerLoadSensitive = true;
		IsTechScannable = true;
		NameForStatus = "SpatialTransposer";
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexity(2, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{displacer|displacer}}", 5);
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
			E.Add("travel", 2);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("LauncherProjectileHit");
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponPseudoThrowHit");
		Registrar.Register("WeaponThrowHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "LauncherProjectileHit" || E.ID == "WeaponThrowHit" || E.ID == "WeaponPseudoThrowHit")
		{
			PerformTeleport(E.GetGameObjectParameter("Defender"), E.GetGameObjectParameter("Attacker"), null, null, IgnoreSubject: false, UsePopups: false, E);
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Displacer: When powered, this weapon randomly teleports its target " + 1 + "-" + 4 + " tiles away on a successful hit.";
	}

	public string GetInstanceDescription()
	{
		int load = MyPowerLoadLevel();
		return "Displacer: When powered, this weapon randomly teleports its target " + 1 + "-" + (4 + IComponent<GameObject>.PowerLoadBonus(load)) + " tiles away on a successful hit.";
	}

	private bool PerformTeleport(GameObject Subject, GameObject Actor = null, int? UseMaxDistance = null, int? PowerLoad = null, bool IgnoreSubject = false, bool UsePopups = false, IEvent FromEvent = null)
	{
		if (!GameObject.Validate(ref Subject))
		{
			return false;
		}
		int load = PowerLoad ?? MyPowerLoadLevel();
		int high = UseMaxDistance ?? (4 + IComponent<GameObject>.PowerLoadBonus(load));
		int num = Stat.Random(1, high);
		if (num <= 0)
		{
			return false;
		}
		int? powerLoadLevel = PowerLoad;
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			return false;
		}
		GameObject gameObject = Subject;
		int maxDistance = num;
		bool interruptMovement = !Subject.IsPlayer();
		GameObject parentObject = ParentObject;
		GameObject deviceOperator = Actor ?? Subject;
		bool swirl = IComponent<GameObject>.Visible(Subject);
		bool usePopups = UsePopups;
		return gameObject.RandomTeleport(swirl, null, parentObject, deviceOperator, FromEvent, 0, maxDistance, interruptMovement, null, Forced: false, IgnoreCombat: true, Voluntary: false, usePopups);
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100() && !ParentObject.HasPart<MissileWeapon>())
		{
			IComponent<GameObject>.XDidYToZ(E.Actor, "bump", "into", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, E.Actor.IsPlayer());
			if (PerformTeleport(E.Actor, E.Actor, null, null, IgnoreSubject: true, UsePopups: true, E))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("You are suddenly elsewhere!");
				}
				E.Identify = true;
				E.IdentifyIfDestroyed = true;
				return true;
			}
		}
		return false;
	}
}
