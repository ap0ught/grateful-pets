using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, save bonus and activation chance are
/// increased by a percentage equal to ((power load - 100) / 10),
/// i.e. 30% for the standard overload power load of 400.
/// </remarks>
public class SlipRing : IPoweredPart
{
	public const string SAVE_BONUS_VS = "Grab";

	public int SaveBonus = 15;

	public int ActivationChance = 5;

	public SlipRing()
	{
		IsEMPSensitive = false;
		IsPowerLoadSensitive = true;
		WorksOnWearer = true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (ChargeUse > 0 && !OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "DefenderBeforeHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "DefenderBeforeHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Grab", E))
		{
			int num = MyPowerLoadLevel();
			int? powerLoadLevel = num;
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
			{
				E.Roll += GetSaveBonus(num);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int powerLoad = MyPowerLoadLevel();
		SavingThrows.AppendSaveBonusDescription(E.Postfix, GetSaveBonus(powerLoad), "Grab", HighlightNumber: false, Highlight: true);
		E.Postfix.AppendRules(GetActivationChance(powerLoad).Signed() + "% chance to slip away from natural melee attacks");
		return base.HandleEvent(E);
	}

	public int GetSaveBonus(int PowerLoad = int.MinValue)
	{
		int num = MyPowerLoadBonus(PowerLoad, 100, 10);
		if (num == 0)
		{
			return SaveBonus;
		}
		return SaveBonus * (100 + num) / 100;
	}

	public int GetActivationChance(int PowerLoad = int.MinValue)
	{
		int num = MyPowerLoadBonus(PowerLoad, 100, 10);
		if (num == 0)
		{
			return ActivationChance;
		}
		return ActivationChance * (100 + num) / 100;
	}

	public bool IsActiveFor(GameObject Weapon)
	{
		if (Weapon == null || !Weapon.IsNatural())
		{
			return false;
		}
		int num = MyPowerLoadLevel();
		if (GetActivationChance(num).in100())
		{
			int? powerLoadLevel = num;
			return IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel);
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DefenderBeforeHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
			if (IsActiveFor(gameObjectParameter))
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
				GameObject gameObjectParameter3 = E.GetGameObjectParameter("Defender");
				if (gameObjectParameter2 != null && gameObjectParameter3 != null)
				{
					IComponent<GameObject>.XDidYToZ(gameObjectParameter3, "slip", "away from", gameObjectParameter2, gameObjectParameter.ShortDisplayName, "!", null, null, gameObjectParameter3, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
					E.SetParameter("NoMissMessage", 1);
				}
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
