using System;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ModMagnetized : IModification
{
	public const int MAX_CARRIED_WEIGHT = 25;

	public ModMagnetized()
	{
	}

	public ModMagnetized(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "MagLev";
		IsEMPSensitive = true;
		IsTechScannable = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<Metal>())
		{
			return false;
		}
		if (Object.WeightEach > 100)
		{
			return false;
		}
		if (Object.HasPart<ModGesticulating>())
		{
			return false;
		}
		if (Object.HasPart<IntegratedPowerSystems>())
		{
			return false;
		}
		if (!Object.CanBeUnequipped())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (Object.Equipped != null)
		{
			EquipmentAPI.UnequipObject(ParentObject);
		}
		Armor armor = Object.GetPart<Armor>();
		Shield part = Object.GetPart<Shield>();
		MissileWeapon part2 = Object.GetPart<MissileWeapon>();
		if (armor == null && part == null && part2 == null)
		{
			armor = Object.AddPart<Armor>();
			MeleeWeapon part3 = Object.GetPart<MeleeWeapon>();
			if (part3 != null)
			{
				part3.Slot = "Floating Nearby";
			}
		}
		if (armor != null)
		{
			armor.WornOn = "Floating Nearby";
		}
		if (part != null)
		{
			part.WornOn = "Floating Nearby";
		}
		if (part2 != null)
		{
			part2.SlotType = "Floating Nearby";
		}
		Object.UsesTwoSlots = false;
		if (!string.IsNullOrEmpty(Object.UsesSlots))
		{
			Object.UsesSlots = "";
		}
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanBeMagneticallyManipulatedEvent>.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeMagneticallyManipulatedEvent E)
	{
		E.Allow = true;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("magnetized", -5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckFloating();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckFloating();
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		CheckFloating();
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginBeingEquipped");
		Registrar.Register("EncumbranceChanged");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginBeingEquipped")
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				string text = ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("are") + " not able to float right now.";
				if (E.GetIntParameter("AutoEquipTry") > 0)
				{
					E.SetParameter("FailureMessage", text);
				}
				else if (E.GetGameObjectParameter("Equipper").IsPlayer())
				{
					Popup.Show(text);
				}
				return false;
			}
		}
		else if (E.ID == "EncumbranceChanged")
		{
			CheckFloating();
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Magnetized: This item floats around you.";
	}

	public void CheckFloating()
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped == null || (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ParentObject.GetCarriedWeight() <= 25))
		{
			return;
		}
		if (equipped.OnWorldMap())
		{
			ParentObject.ForceUnequip(Silent: true);
			if (ParentObject.Equipped == null && equipped.IsPlayer())
			{
				Popup.Show(ParentObject.Does("fall") + " to the ground; you pick " + ParentObject.them + " up.");
			}
		}
		else
		{
			if (equipped.CurrentCell == null)
			{
				return;
			}
			ParentObject.ForceUnequip(Silent: true);
			if (ParentObject.Equipped == null)
			{
				if (equipped.IsPlayer())
				{
					Popup.Show(ParentObject.Does("fall") + " to the ground.");
				}
				InventoryActionEvent.Check(equipped, equipped, ParentObject, "CommandDropObject", Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, Forced: true, Silent: true);
			}
		}
	}
}
