using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsPrecisionForceLathe : IPoweredPart
{
	public string Blueprint = "ForceKnife";

	public string CommandID;

	public Guid ActivatedAbilityID = Guid.Empty;

	public CyberneticsPrecisionForceLathe()
	{
		ChargeUse = 0;
		WorksOnImplantee = true;
		NameForStatus = "PrecisionForceLathe";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CheckExistenceSupportEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != ImplantedEvent.ID && ID != PooledEvent<ReplaceThrownWeaponEvent>.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (!CommandID.IsNullOrEmpty() && GameObject.Validate(E.Actor) && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && IsObjectActivePartSubject(E.Actor) && GetTargetBodyPart(E.Actor) != null && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, null, E.Actor, ParentObject))
		{
			E.Add(CommandID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (E.Object.Blueprint == Blueprint && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && IsObjectActivePartSubject(E.Object.Equipped))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		if (CommandID == null)
		{
			CommandID = Guid.NewGuid().ToString();
		}
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Fabricate Force Knife", CommandID, "Cybernetics", null, "û", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, "CommandFabricateForceKnife");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == CommandID && !ActivatePrecisionForceLathe(E.Actor, E.Target, E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceThrownWeaponEvent E)
	{
		if (E.PreviouslyEquipped?.Blueprint == Blueprint && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && IsObjectActivePartSubject(E.Actor) && E.Actor.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			ConsumeCharge();
			if (CheckRealityDistortion(E.Actor, E))
			{
				GameObject gameObject = GenerateObject();
				if (E.EquipOrDestroy(gameObject, Silent: true))
				{
					IComponent<GameObject>.XDidYToZ(gameObject, "shimmer", "into existence in", E.Actor, "grasp", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: true, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool ActivatePrecisionForceLathe(GameObject Actor, GameObject Target = null, IEvent FromEvent = null)
	{
		if (!GameObject.Validate(ref Actor))
		{
			return false;
		}
		if (!IsObjectActivePartSubject(Actor))
		{
			return false;
		}
		if (!Actor.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			return false;
		}
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return Actor.Fail(ParentObject.Does("are") + " " + GetStatusPhrase() + ".");
		}
		BodyPart targetBodyPart = GetTargetBodyPart(Actor);
		GameObject gameObject = GenerateObject();
		if (targetBodyPart == null)
		{
			string msg = "You have no place available to hold " + (gameObject?.an() ?? "the result") + ".";
			gameObject?.Obliterate();
			return Actor.Fail(msg);
		}
		ConsumeCharge();
		if (!CheckRealityDistortion(Actor, FromEvent))
		{
			gameObject?.Obliterate();
			return false;
		}
		Actor.ReceiveObject(gameObject);
		if (!GameObject.Validate(gameObject) || gameObject.InInventory != Actor)
		{
			gameObject?.Obliterate();
			return Actor.Fail("Something went wrong.");
		}
		Event obj = Event.New("CommandEquipObject");
		obj.SetParameter("Object", gameObject);
		obj.SetParameter("BodyPart", targetBodyPart);
		obj.SetSilent(Silent: true);
		if (!Actor.FireEvent(obj))
		{
			gameObject?.Obliterate();
			return Actor.Fail("Something went wrong.");
		}
		IComponent<GameObject>.XDidYToZ(gameObject, "shimmer", "into existence in", Actor, (targetBodyPart.Type == "Thrown Weapon") ? "grasp" : targetBodyPart.GetOrdinalName(), null, null, null, null, null, UseFullNames: false, IndefiniteSubject: true, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
		FromEvent?.RequestInterfaceExit();
		return true;
	}

	private bool CheckRealityDistortion(GameObject Actor, IEvent FromEvent = null)
	{
		Event obj = Event.New("InitiateRealityDistortionLocal");
		obj.SetParameter("Object", Actor);
		obj.SetParameter("Device", this);
		return Actor.FireEvent(obj, FromEvent);
	}

	private GameObject GenerateObject()
	{
		GameObject gameObject = GameObject.Create(Blueprint);
		gameObject.SetIntProperty("NeverStack", 1);
		gameObject.RemovePart<ExistenceSupport>();
		ExistenceSupport existenceSupport = gameObject.RequirePart<ExistenceSupport>();
		existenceSupport.SupportedBy = ParentObject;
		existenceSupport.ValidateEveryTurn = true;
		existenceSupport.SilentRemoval = true;
		return gameObject;
	}

	public static BodyPart GetTargetBodyPart(GameObject Subject)
	{
		return Subject?.GetUnequippedPreferredBodyPartOrAlternate("Thrown Weapon", "Hand");
	}

	public BodyPart GetTargetBodyPart()
	{
		return GetTargetBodyPart(ParentObject.Implantee);
	}
}
