using System;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, the chance to activate will, if less than 100,
/// be increased by a relative percentage of ((power load - 100) / 10),
/// i.e. 30% for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class CancelRangedAttacks : IPoweredPart
{
	public int Chance;

	public string Message;

	public string Sound;

	public bool UseChargeOnAttempt = true;

	public bool UseChargeOnSuccess;

	public bool DestroyProjectiles = true;

	public bool ShowInShortDescription = true;

	public float ComputePowerFactor;

	public CancelRangedAttacks()
	{
		IsPowerLoadSensitive = true;
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		CancelRangedAttacks cancelRangedAttacks = p as CancelRangedAttacks;
		if (cancelRangedAttacks.Chance != Chance)
		{
			return false;
		}
		if (cancelRangedAttacks.Message != Message)
		{
			return false;
		}
		if (cancelRangedAttacks.Sound != Sound)
		{
			return false;
		}
		if (cancelRangedAttacks.UseChargeOnAttempt != UseChargeOnAttempt)
		{
			return false;
		}
		if (cancelRangedAttacks.UseChargeOnSuccess != UseChargeOnSuccess)
		{
			return false;
		}
		if (cancelRangedAttacks.DestroyProjectiles != DestroyProjectiles)
		{
			return false;
		}
		if (cancelRangedAttacks.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		if (cancelRangedAttacks.ComputePowerFactor != ComputePowerFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<DefenderMissileHitEvent>.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return ShowInShortDescription;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(DefenderMissileHitEvent E)
	{
		if (IsReady(UseChargeOnAttempt, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && GetEffectiveChance().in100())
		{
			if (UseChargeOnSuccess)
			{
				ConsumeCharge();
			}
			if (!Message.IsNullOrEmpty())
			{
				string text = GameText.VariableReplace(Message, ParentObject, E.Projectile);
				if (!text.IsNullOrEmpty())
				{
					EmitMessage(text);
				}
			}
			PlayWorldSound(Sound, 1f);
			E.Done = true;
			if (DestroyProjectiles)
			{
				E.Projectile?.Obliterate();
			}
			return false;
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
				E.Postfix.AppendRules(text + GetEffectiveChance() + "% chance of stopping ranged attacks.", GetEventSensitiveAddStatusSummary(E));
			}
			if (ComputePowerFactor > 0f)
			{
				E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
			}
			else if (ComputePowerFactor < 0f)
			{
				E.Postfix.AppendRules("Compute power on the local lattice decreases this item's effectiveness.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public int GetEffectiveChance()
	{
		int num = Chance;
		if (num < 100 && IsPowerLoadSensitive)
		{
			num = num * (100 + MyPowerLoadBonus(int.MinValue, 100, 10)) / 100;
		}
		return GetAvailableComputePowerEvent.AdjustUp(this, num, ComputePowerFactor);
	}
}
