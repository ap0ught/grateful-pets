using XRL.Core;

namespace XRL.World.Capabilities;

public static class WeaponUsageTracking
{
	public static void TrackMeleeWeaponHit(GameObject Actor, GameObject Weapon, bool DefenderIsCreature, string DefenderBlueprint)
	{
		if (!GameObject.Validate(ref Weapon) || Weapon.Equipped == null)
		{
			return;
		}
		if (DefenderIsCreature)
		{
			Weapon.ModIntProperty("HitsAsMeleeWeapon", 1);
			Weapon.SetLongProperty("LastHitAsMeleeWeaponTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastHitAsMeleeWeaponBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Weapon.ModIntProperty("HitsAsMeleeWeaponByPlayer", 1);
				Weapon.SetLongProperty("LastHitAsMeleeWeaponByPlayerTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastHitAsMeleeWeaponByPlayerBlueprint", DefenderBlueprint);
			}
		}
		else
		{
			Weapon.ModIntProperty("ImpactsAsMeleeWeapon", 1);
			Weapon.SetLongProperty("LastImpactAsMeleeWeaponTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastImpactAsMeleeWeaponBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Weapon.ModIntProperty("ImpactsAsMeleeWeaponByPlayer", 1);
				Weapon.SetLongProperty("LastImpactAsMeleeWeaponByPlayerTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastImpactAsMeleeWeaponByPlayerBlueprint", DefenderBlueprint);
			}
		}
	}

	public static void TrackMeleeWeaponDamage(GameObject Actor, GameObject Weapon, bool DefenderIsCreature, string DefenderBlueprint, Damage Dmg)
	{
		if (!GameObject.Validate(ref Weapon) || Weapon.Equipped == null || Dmg == null || Dmg.Amount <= 0)
		{
			return;
		}
		if (DefenderIsCreature)
		{
			Weapon.ModIntProperty("InjuryAsMeleeWeapon", Dmg.Amount);
			Weapon.SetIntProperty("LastInjuryAsMeleeWeapon", Dmg.Amount);
			Weapon.SetLongProperty("LastInjuryAsMeleeWeaponTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastInjuryAsMeleeWeaponBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Weapon.ModIntProperty("InjuryAsMeleeWeaponByPlayer", Dmg.Amount);
				Weapon.SetIntProperty("LastInjuryAsMeleeWeaponByPlayer", Dmg.Amount);
				Weapon.SetLongProperty("LastInjuryAsMeleeWeaponByPlayerTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastInjuryAsMeleeWeaponByPlayerBlueprint", DefenderBlueprint);
			}
		}
		else
		{
			Weapon.ModIntProperty("DamageAsMeleeWeapon", Dmg.Amount);
			Weapon.SetIntProperty("LastDamageAsMeleeWeapon", Dmg.Amount);
			Weapon.SetLongProperty("LastDamageAsMeleeWeaponTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastDamageAsMeleeWeaponBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Weapon.ModIntProperty("DamageAsMeleeWeaponByPlayer", Dmg.Amount);
				Weapon.SetIntProperty("LastDamageAsMeleeWeaponByPlayer", Dmg.Amount);
				Weapon.SetLongProperty("LastDamageAsMeleeWeaponByPlayerTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastDamageAsMeleeWeaponByPlayerBlueprint", DefenderBlueprint);
			}
		}
	}

	public static void TrackThrownWeaponHit(GameObject Actor, GameObject Weapon, bool DefenderIsCreature, string DefenderBlueprint, bool Accidental)
	{
		if (!GameObject.Validate(ref Weapon))
		{
			return;
		}
		if (DefenderIsCreature)
		{
			Weapon.ModIntProperty("HitsAsThrownWeapon", 1);
			Weapon.SetLongProperty("LastHitAsThrownWeaponTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastHitAsThrownWeaponBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Weapon.ModIntProperty("HitsAsThrownWeaponByPlayer", 1);
				Weapon.SetLongProperty("LastHitAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastHitAsThrownWeaponByPlayerBlueprint", DefenderBlueprint);
			}
			if (Accidental)
			{
				Weapon.ModIntProperty("AccidentalHitsAsThrownWeapon", 1);
				Weapon.SetLongProperty("LastAccidentalHitAsThrownWeaponTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastAccidentalHitAsThrownWeaponBlueprint", DefenderBlueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Weapon.ModIntProperty("AccidentalHitsAsThrownWeaponOnByPlayer", 1);
					Weapon.SetLongProperty("LastAccidentalHitAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
					Weapon.SetStringProperty("LastAccidentalHitAsThrownWeaponByPlayerBlueprint", DefenderBlueprint);
				}
			}
			return;
		}
		Weapon.ModIntProperty("ImpactsAsThrownWeapon", 1);
		Weapon.SetLongProperty("LastImpactAsThrownWeaponTurn", XRLCore.CurrentTurn);
		Weapon.SetStringProperty("LastImpactAsThrownWeaponBlueprint", DefenderBlueprint);
		if (Actor != null && Actor.IsPlayer())
		{
			Weapon.ModIntProperty("ImpactsAsThrownWeaponByPlayer", 1);
			Weapon.SetLongProperty("LastImpactAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastImpactAsThrownWeaponByPlayerBlueprint", DefenderBlueprint);
		}
		if (Accidental)
		{
			Weapon.ModIntProperty("AccidentalImpactsAsThrownWeapon", 1);
			Weapon.SetLongProperty("LastAccidentalImpactAsThrownWeaponTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastAccidentalImpactAsThrownWeaponBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Weapon.ModIntProperty("AccidentalImpactsAsThrownWeaponByPlayer", 1);
				Weapon.SetLongProperty("LastAccidentalImpactAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastAccidentalImpactAsThrownWeaponByPlayerBlueprint", DefenderBlueprint);
			}
		}
	}

	public static void TrackThrownWeaponDamage(GameObject Actor, GameObject Weapon, bool DefenderIsCreature, string DefenderBlueprint, bool Accidental, int Amount)
	{
		if (!GameObject.Validate(ref Weapon) || Amount <= 0)
		{
			return;
		}
		if (DefenderIsCreature)
		{
			Weapon.ModIntProperty("InjuryAsThrownWeapon", Amount);
			Weapon.SetIntProperty("LastInjuryAsThrownWeapon", Amount);
			Weapon.SetLongProperty("LastInjuryAsThrownWeaponTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastInjuryAsThrownWeaponBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Weapon.ModIntProperty("InjuryAsThrownWeaponByPlayer", Amount);
				Weapon.SetIntProperty("LastInjuryAsThrownWeaponByPlayer", Amount);
				Weapon.SetLongProperty("LastInjuryAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastInjuryAsThrownWeaponByPlayerBlueprint", DefenderBlueprint);
			}
			if (Accidental)
			{
				Weapon.ModIntProperty("AccidentalInjuryAsThrownWeapon", Amount);
				Weapon.SetIntProperty("LastAccidentalInjuryAsThrownWeapon", Amount);
				Weapon.SetLongProperty("LastAccidentalInjuryAsThrownWeaponTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastAccidentalInjuryAsThrownWeaponBlueprint", DefenderBlueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Weapon.ModIntProperty("AccidentalInjuryAsThrownWeaponByPlayer", Amount);
					Weapon.SetIntProperty("LastAccidentalInjuryAsThrownWeaponByPlayer", Amount);
					Weapon.SetLongProperty("LastAccidentalInjuryAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
					Weapon.SetStringProperty("LastAccidentalInjuryAsThrownWeaponByPlayerBlueprint", DefenderBlueprint);
				}
			}
			return;
		}
		Weapon.ModIntProperty("DamageAsThrownWeapon", Amount);
		Weapon.SetIntProperty("LastDamageAsThrownWeapon", Amount);
		Weapon.SetLongProperty("LastDamageAsThrownWeaponTurn", XRLCore.CurrentTurn);
		Weapon.SetStringProperty("LastDamageAsThrownWeaponBlueprint", DefenderBlueprint);
		if (Actor != null && Actor.IsPlayer())
		{
			Weapon.ModIntProperty("DamageAsThrownWeaponByPlayer", Amount);
			Weapon.SetIntProperty("LastDamageAsThrownWeaponByPlayer", Amount);
			Weapon.SetLongProperty("LastDamageAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastDamageAsThrownWeaponByPlayerBlueprint", DefenderBlueprint);
		}
		if (Accidental)
		{
			Weapon.ModIntProperty("AccidentalDamageAsThrownWeapon", Amount);
			Weapon.SetIntProperty("LastAccidentalDamageAsThrownWeapon", Amount);
			Weapon.SetLongProperty("LastAccidentalDamageAsThrownWeaponTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastAccidentalDamageAsThrownWeaponBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Weapon.ModIntProperty("AccidentalDamageAsThrownWeaponByPlayer", Amount);
				Weapon.SetIntProperty("LastAccidentalDamageAsThrownWeaponByPlayer", Amount);
				Weapon.SetLongProperty("LastAccidentalDamageAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastAccidentalDamageAsThrownWeaponByPlayerBlueprint", DefenderBlueprint);
			}
		}
	}

	public static void TrackThrownWeaponDamage(GameObject Actor, GameObject Weapon, bool DefenderIsCreature, string DefenderBlueprint, bool Accidental, Damage Dmg)
	{
		TrackThrownWeaponDamage(Actor, Weapon, DefenderIsCreature, DefenderBlueprint, Accidental, Dmg?.Amount ?? 0);
	}

	public static void TrackMissileWeaponHit(GameObject Actor, GameObject Launcher, GameObject Projectile, bool DefenderIsCreature, string DefenderBlueprint, bool Accidental)
	{
		if (DefenderIsCreature)
		{
			if (GameObject.Validate(ref Launcher))
			{
				Launcher.ModIntProperty("HitsAsLauncher", 1);
				Launcher.SetLongProperty("LastHitAsLauncherTurn", XRLCore.CurrentTurn);
				Launcher.SetStringProperty("LastHitAsLauncherBlueprint", DefenderBlueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Launcher.ModIntProperty("HitsAsLauncherByPlayer", 1);
					Launcher.SetLongProperty("LastHitAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
					Launcher.SetStringProperty("LastHitAsLauncherByPlayerBlueprint", DefenderBlueprint);
				}
				if (Accidental)
				{
					Launcher.ModIntProperty("AccidentalHitsAsLauncher", 1);
					Launcher.SetLongProperty("LastAccidentalHitAsLauncherTurn", XRLCore.CurrentTurn);
					Launcher.SetStringProperty("LastAccidentalHitAsLauncherBlueprint", DefenderBlueprint);
					if (Actor != null && Actor.IsPlayer())
					{
						Launcher.ModIntProperty("AccidentalHitsAsLauncherByPlayer", 1);
						Launcher.SetLongProperty("LastAccidentalHitAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
						Launcher.SetStringProperty("LastAccidentalHitAsLauncherByPlayerBlueprint", DefenderBlueprint);
					}
				}
			}
			if (!GameObject.Validate(ref Projectile))
			{
				return;
			}
			Projectile.ModIntProperty("HitsAsProjectile", 1);
			Projectile.SetLongProperty("LastHitAsProjectileTurn", XRLCore.CurrentTurn);
			Projectile.SetStringProperty("LastHitAsProjectileBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Projectile.ModIntProperty("HitsAsProjectileByPlayer", 1);
				Projectile.SetLongProperty("LastHitAsProjectileByPlayerTurn", XRLCore.CurrentTurn);
				Projectile.SetStringProperty("LastHitAsProjectileByPlayerBlueprint", DefenderBlueprint);
			}
			if (Accidental)
			{
				Projectile.ModIntProperty("AccidentalHitsAsProjectile", 1);
				Projectile.SetLongProperty("LastAccidentalHitAsProjectileTurn", XRLCore.CurrentTurn);
				Projectile.SetStringProperty("LastAccidentalHitAsProjectileBlueprint", DefenderBlueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Projectile.ModIntProperty("AccidentalHitsAsProjectileByPlayer", 1);
					Projectile.SetLongProperty("LastAccidentalHitAsProjectileByPlayerTurn", XRLCore.CurrentTurn);
					Projectile.SetStringProperty("LastAccidentalHitAsProjectileByPlayerBlueprint", DefenderBlueprint);
				}
			}
			return;
		}
		if (GameObject.Validate(ref Launcher))
		{
			Launcher.ModIntProperty("ImpactsAsLauncher", 1);
			Launcher.SetLongProperty("LastImpactAsLauncherTurn", XRLCore.CurrentTurn);
			Launcher.SetStringProperty("LastImpactAsLauncherBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Launcher.ModIntProperty("ImpactsAsLauncherByPlayer", 1);
				Launcher.SetLongProperty("LastImpactAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
				Launcher.SetStringProperty("LastImpactAsLauncherByPlayerBlueprint", DefenderBlueprint);
			}
			if (Accidental)
			{
				Launcher.ModIntProperty("AccidentalImpactsAsLauncher", 1);
				Launcher.SetLongProperty("LastAccidentalImpactAsLauncherTurn", XRLCore.CurrentTurn);
				Launcher.SetStringProperty("LastAccidentalImpactAsLauncherBlueprint", DefenderBlueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Launcher.ModIntProperty("AccidentalImpactsAsLauncherByPlayer", 1);
					Launcher.SetLongProperty("LastAccidentalImpactAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
					Launcher.SetStringProperty("LastAccidentalImpactAsLauncherByPlayerBlueprint", DefenderBlueprint);
				}
			}
		}
		if (!GameObject.Validate(ref Projectile))
		{
			return;
		}
		Projectile.ModIntProperty("ImpactsAsProjectile", 1);
		Projectile.SetLongProperty("LastImpactAsProjectileTurn", XRLCore.CurrentTurn);
		Projectile.SetStringProperty("LastImpactAsProjectileBlueprint", DefenderBlueprint);
		if (Actor != null && Actor.IsPlayer())
		{
			Projectile.ModIntProperty("ImpactsAsProjectileByPlayer", 1);
			Projectile.SetLongProperty("LastImpactAsProjectileByPlayerTurn", XRLCore.CurrentTurn);
			Projectile.SetStringProperty("LastImpactAsProjectileByPlayerBlueprint", DefenderBlueprint);
		}
		if (Accidental)
		{
			Projectile.ModIntProperty("AccidentalImpactsAsProjectile", 1);
			Projectile.SetLongProperty("LastAccidentalImpactAsProjectileTurn", XRLCore.CurrentTurn);
			Projectile.SetStringProperty("LastAccidentalImpactAsProjectileBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Projectile.ModIntProperty("AccidentalImpactsAsProjectileByPlayer", 1);
				Projectile.SetLongProperty("LastAccidentalImpactAsProjectileByPlayerTurn", XRLCore.CurrentTurn);
				Projectile.SetStringProperty("LastAccidentalImpactAsProjectileByPlayerBlueprint", DefenderBlueprint);
			}
		}
	}

	public static void TrackMissileWeaponDamage(GameObject Actor, GameObject Launcher, GameObject Projectile, bool DefenderIsCreature, string DefenderBlueprint, bool Accidental, Damage Dmg)
	{
		if (Dmg == null || Dmg.Amount <= 0)
		{
			return;
		}
		if (DefenderIsCreature)
		{
			if (GameObject.Validate(ref Launcher))
			{
				Launcher.ModIntProperty("InjuryAsLauncher", Dmg.Amount);
				Launcher.SetIntProperty("LastInjuryAsLauncher", Dmg.Amount);
				Launcher.SetLongProperty("LastInjuryAsLauncherTurn", XRLCore.CurrentTurn);
				Launcher.SetStringProperty("LastInjuryAsLauncherBlueprint", DefenderBlueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Launcher.ModIntProperty("InjuryAsLauncherByPlayer", Dmg.Amount);
					Launcher.SetIntProperty("LastInjuryAsLauncherByPlayer", Dmg.Amount);
					Launcher.SetLongProperty("LastInjuryAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
					Launcher.SetStringProperty("LastInjuryAsLauncherByPlayerBlueprint", DefenderBlueprint);
				}
				if (Accidental)
				{
					Launcher.ModIntProperty("AccidentalInjuryAsLauncher", Dmg.Amount);
					Launcher.SetIntProperty("LastAccidentalInjuryAsLauncher", Dmg.Amount);
					Launcher.SetLongProperty("LastAccidentalInjuryAsLauncherTurn", XRLCore.CurrentTurn);
					Launcher.SetStringProperty("LastAccidentalInjuryAsLauncherBlueprint", DefenderBlueprint);
					if (Actor != null && Actor.IsPlayer())
					{
						Launcher.ModIntProperty("AccidentalInjuryAsLauncherByPlayer", Dmg.Amount);
						Launcher.SetIntProperty("LastAccidentalInjuryAsLauncherByPlayer", Dmg.Amount);
						Launcher.SetLongProperty("LastAccidentalInjuryAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
						Launcher.SetStringProperty("LastAccidentalInjuryAsLauncherByPlayerBlueprint", DefenderBlueprint);
					}
				}
			}
			if (Projectile == null)
			{
				return;
			}
			Projectile.ModIntProperty("InjuryAsProjectile", Dmg.Amount);
			Projectile.SetIntProperty("LastInjuryAsLauncher", Dmg.Amount);
			Projectile.SetLongProperty("LastInjuryAsLauncherTurn", XRLCore.CurrentTurn);
			Projectile.SetStringProperty("LastInjuryAsLauncherBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Projectile.ModIntProperty("InjuryAsProjectileByPlayer", Dmg.Amount);
				Projectile.SetIntProperty("LastInjuryAsLauncherByPlayer", Dmg.Amount);
				Projectile.SetLongProperty("LastInjuryAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
				Projectile.SetStringProperty("LastInjuryAsLauncherByPlayerBlueprint", DefenderBlueprint);
			}
			if (Accidental)
			{
				Projectile.ModIntProperty("AccidentalInjuryAsProjectile", Dmg.Amount);
				Projectile.SetIntProperty("LastAccidentalInjuryAsLauncher", Dmg.Amount);
				Projectile.SetLongProperty("LastAccidentalInjuryAsLauncherTurn", XRLCore.CurrentTurn);
				Projectile.SetStringProperty("LastAccidentalInjuryAsLauncherBlueprint", DefenderBlueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Projectile.ModIntProperty("AccidentalInjuryAsProjectileByPlayer", Dmg.Amount);
					Projectile.SetIntProperty("LastAccidentalInjuryAsLauncherByPlayer", Dmg.Amount);
					Projectile.SetLongProperty("LastAccidentalInjuryAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
					Projectile.SetStringProperty("LastAccidentalInjuryAsLauncherByPlayerBlueprint", DefenderBlueprint);
				}
			}
			return;
		}
		if (GameObject.Validate(ref Launcher))
		{
			Launcher.ModIntProperty("DamageAsLauncher", Dmg.Amount);
			Launcher.SetIntProperty("LastDamageAsLauncher", Dmg.Amount);
			Launcher.SetLongProperty("LastDamageAsLauncherTurn", XRLCore.CurrentTurn);
			Launcher.SetStringProperty("LastDamageAsLauncherBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Launcher.ModIntProperty("DamageAsLauncherByPlayer", Dmg.Amount);
				Launcher.SetIntProperty("LastDamageAsLauncherByPlayer", Dmg.Amount);
				Launcher.SetLongProperty("LastDamageAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
				Launcher.SetStringProperty("LastDamageAsLauncherByPlayerBlueprint", DefenderBlueprint);
			}
			if (Accidental)
			{
				Launcher.ModIntProperty("AccidentalDamageAsLauncher", Dmg.Amount);
				Launcher.SetIntProperty("LastAccidentalDamageAsLauncher", Dmg.Amount);
				Launcher.SetLongProperty("LastAccidentalDamageAsLauncherTurn", XRLCore.CurrentTurn);
				Launcher.SetStringProperty("LastAccidentalDamageAsLauncherBlueprint", DefenderBlueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Launcher.ModIntProperty("AccidentalDamageAsLauncherByPlayer", Dmg.Amount);
					Launcher.SetIntProperty("LastAccidentalDamageAsLauncherByPlayer", Dmg.Amount);
					Launcher.SetLongProperty("LastAccidentalDamageAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
					Launcher.SetStringProperty("LastAccidentalDamageAsLauncherByPlayerBlueprint", DefenderBlueprint);
				}
			}
		}
		if (!GameObject.Validate(ref Projectile))
		{
			return;
		}
		Projectile.ModIntProperty("DamageAsProjectile", Dmg.Amount);
		Projectile.SetIntProperty("LastDamageAsLauncher", Dmg.Amount);
		Projectile.SetLongProperty("LastDamageAsLauncherTurn", XRLCore.CurrentTurn);
		Projectile.SetStringProperty("LastDamageAsLauncherBlueprint", DefenderBlueprint);
		if (Actor != null && Actor.IsPlayer())
		{
			Projectile.ModIntProperty("DamageAsProjectileByPlayer", Dmg.Amount);
			Projectile.SetIntProperty("LastDamageAsLauncherByPlayer", Dmg.Amount);
			Projectile.SetLongProperty("LastDamageAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
			Projectile.SetStringProperty("LastDamageAsLauncherByPlayerBlueprint", DefenderBlueprint);
		}
		if (Accidental)
		{
			Projectile.ModIntProperty("AccidentalDamageAsProjectile", Dmg.Amount);
			Projectile.SetIntProperty("LastAccidentalDamageAsLauncher", Dmg.Amount);
			Projectile.SetLongProperty("LastAccidentalDamageAsLauncherTurn", XRLCore.CurrentTurn);
			Projectile.SetStringProperty("LastAccidentalDamageAsLauncherBlueprint", DefenderBlueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Projectile.ModIntProperty("AccidentalDamageAsProjectileByPlayer", Dmg.Amount);
				Projectile.SetIntProperty("LastAccidentalDamageAsLauncherByPlayer", Dmg.Amount);
				Projectile.SetLongProperty("LastAccidentalDamageAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
				Projectile.SetStringProperty("LastAccidentalDamageAsLauncherByPlayerBlueprint", DefenderBlueprint);
			}
		}
	}

	public static void TrackKill(GameObject Actor, GameObject Defender, GameObject Weapon, GameObject Projectile, bool Accidental)
	{
		if (!GameObject.Validate(ref Weapon) && !GameObject.Validate(ref Projectile))
		{
			return;
		}
		if (Defender.IsCreature)
		{
			if (GameObject.Validate(ref Projectile))
			{
				if (Weapon == Projectile)
				{
					Weapon.ModIntProperty("KillsAsThrownWeapon", 1);
					Weapon.SetLongProperty("LastKillAsThrownWeaponTurn", XRLCore.CurrentTurn);
					Weapon.SetStringProperty("LastKillAsThrownWeaponBlueprint", Defender.Blueprint);
					if (Actor != null && Actor.IsPlayer())
					{
						Weapon.ModIntProperty("KillsAsThrownWeaponByPlayer", 1);
						Weapon.SetLongProperty("LastKillAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
						Weapon.SetStringProperty("LastKillAsThrownWeaponByPlayerBlueprint", Defender.Blueprint);
					}
					if (Accidental)
					{
						Weapon.ModIntProperty("AccidentalKillsAsThrownWeapon", 1);
						Weapon.SetLongProperty("LastAccidentalKillAsThrownWeaponTurn", XRLCore.CurrentTurn);
						Weapon.SetStringProperty("LastAccidentalKillAsThrownWeaponBlueprint", Defender.Blueprint);
						if (Actor != null && Actor.IsPlayer())
						{
							Weapon.ModIntProperty("AccidentalKillsAsThrownWeaponByPlayer", 1);
							Weapon.SetLongProperty("LastAccidentalKillAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
							Weapon.SetStringProperty("LastAccidentalKillAsThrownWeaponByPlayerBlueprint", Defender.Blueprint);
						}
					}
					return;
				}
				Projectile.ModIntProperty("KillsAsProjectile", 1);
				Projectile.SetLongProperty("LastKillAsProjectileTurn", XRLCore.CurrentTurn);
				Projectile.SetStringProperty("LastKillAsProjectileBlueprint", Defender.Blueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Projectile.ModIntProperty("KillsAsProjectileByPlayer", 1);
					Projectile.SetLongProperty("LastKillAsProjectileByPlayerTurn", XRLCore.CurrentTurn);
					Projectile.SetStringProperty("LastKillAsProjectileByPlayerBlueprint", Defender.Blueprint);
				}
				if (GameObject.Validate(ref Weapon))
				{
					Weapon.ModIntProperty("KillsAsLauncher", 1);
					Weapon.SetLongProperty("LastKillAsLauncherTurn", XRLCore.CurrentTurn);
					Weapon.SetStringProperty("LastKillAsLauncherBlueprint", Defender.Blueprint);
					if (Actor != null && Actor.IsPlayer())
					{
						Weapon.ModIntProperty("KillsAsLauncherByPlayer", 1);
						Weapon.SetLongProperty("LastKillAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
						Weapon.SetStringProperty("LastKillAsLauncherByPlayerBlueprint", Defender.Blueprint);
					}
				}
				if (!Accidental)
				{
					return;
				}
				Projectile.ModIntProperty("AccidentalKillsAsProjectile", 1);
				Projectile.SetLongProperty("LastAccidentalKillAsProjectileTurn", XRLCore.CurrentTurn);
				Projectile.SetStringProperty("LastAccidentalKillAsProjectileBlueprint", Defender.Blueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Projectile.ModIntProperty("AccidentalKillsAsProjectileByPlayer", 1);
					Projectile.SetLongProperty("LastAccidentalKillAsProjectileByPlayerTurn", XRLCore.CurrentTurn);
					Projectile.SetStringProperty("LastAccidentalKillAsProjectileByPlayerBlueprint", Defender.Blueprint);
				}
				if (GameObject.Validate(ref Weapon))
				{
					Weapon.ModIntProperty("AccidentalKillsAsLauncher", 1);
					Weapon.SetLongProperty("LastAccidentalKillAsLauncherTurn", XRLCore.CurrentTurn);
					Weapon.SetStringProperty("LastAccidentalKillAsLauncherBlueprint", Defender.Blueprint);
					if (Actor != null && Actor.IsPlayer())
					{
						Weapon.ModIntProperty("AccidentalKillsAsLauncherByPlayer", 1);
						Weapon.SetLongProperty("LastAccidentalKillAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
						Weapon.SetStringProperty("LastAccidentalKillAsLauncherByPlayerBlueprint", Defender.Blueprint);
					}
				}
			}
			else
			{
				if (!GameObject.Validate(ref Weapon) || Weapon.Equipped == null)
				{
					return;
				}
				Weapon.ModIntProperty("KillsAsMeleeWeapon", 1);
				Weapon.SetLongProperty("LastKillAsMeleeWeaponTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastKillAsMeleeWeaponBlueprint", Defender.Blueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Weapon.ModIntProperty("KillsAsMeleeWeaponByPlayer", 1);
					Weapon.SetLongProperty("LastKillAsMeleeWeaponByPlayerTurn", XRLCore.CurrentTurn);
					Weapon.SetStringProperty("LastKillAsMeleeWeaponByPlayerBlueprint", Defender.Blueprint);
				}
				if (Accidental)
				{
					Weapon.ModIntProperty("AccidentalKillsAsMeleeWeapon", 1);
					Weapon.SetLongProperty("LastAccidentalKillAsMeleeWeaponTurn", XRLCore.CurrentTurn);
					Weapon.SetStringProperty("LastAccidentalKillAsMeleeWeaponBlueprint", Defender.Blueprint);
					if (Actor != null && Actor.IsPlayer())
					{
						Weapon.ModIntProperty("AccidentalKillsAsMeleeWeaponByPlayer", 1);
						Weapon.SetLongProperty("LastAccidentalKillAsMeleeWeaponByPlayerTurn", XRLCore.CurrentTurn);
						Weapon.SetStringProperty("LastAccidentalKillAsMeleeWeaponByPlayerBlueprint", Defender.Blueprint);
					}
				}
			}
		}
		else if (GameObject.Validate(ref Projectile))
		{
			if (Weapon == Projectile)
			{
				Weapon.ModIntProperty("DestroysAsThrownWeapon", 1);
				Weapon.SetLongProperty("LastDestroyAsThrownWeaponTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastDestroyAsThrownWeaponBlueprint", Defender.Blueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Weapon.ModIntProperty("DestroysAsThrownWeaponByPlayer", 1);
					Weapon.SetLongProperty("LastDestroyAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
					Weapon.SetStringProperty("LastDestroyAsThrownWeaponByPlayerBlueprint", Defender.Blueprint);
				}
				if (Accidental)
				{
					Weapon.ModIntProperty("AccidentalDestroysAsThrownWeapon", 1);
					Weapon.SetLongProperty("LastAccidentalDestroyAsThrownWeaponTurn", XRLCore.CurrentTurn);
					Weapon.SetStringProperty("LastAccidentalDestroyAsThrownWeaponBlueprint", Defender.Blueprint);
					if (Actor != null && Actor.IsPlayer())
					{
						Weapon.ModIntProperty("AccidentalDestroysAsThrownWeaponByPlayer", 1);
						Weapon.SetLongProperty("LastAccidentalDestroyAsThrownWeaponByPlayerTurn", XRLCore.CurrentTurn);
						Weapon.SetStringProperty("LastAccidentalDestroyAsThrownWeaponByPlayerBlueprint", Defender.Blueprint);
					}
				}
				return;
			}
			Projectile.ModIntProperty("AccidentalDestroysAsProjectile", 1);
			Projectile.SetLongProperty("LastAccidentalDestroyAsProjectileTurn", XRLCore.CurrentTurn);
			Projectile.SetStringProperty("LastAccidentalDestroyAsProjectileBlueprint", Defender.Blueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Projectile.ModIntProperty("AccidentalDestroysAsProjectileByPlayer", 1);
				Projectile.SetLongProperty("LastAccidentalDestroyAsProjectileByPlayerTurn", XRLCore.CurrentTurn);
				Projectile.SetStringProperty("LastAccidentalDestroyAsProjectileByPlayerBlueprint", Defender.Blueprint);
			}
			if (GameObject.Validate(ref Weapon))
			{
				Weapon.ModIntProperty("AccidentalDestroysAsLauncher", 1);
				Weapon.SetLongProperty("LastAccidentalDestroyAsLauncherTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastAccidentalDestroyAsLauncherBlueprint", Defender.Blueprint);
				if (Actor != null && Actor.IsPlayer())
				{
					Weapon.ModIntProperty("AccidentalDestroysAsLauncherByPlayer", 1);
					Weapon.SetLongProperty("LastAccidentalDestroyAsLauncherByPlayerTurn", XRLCore.CurrentTurn);
					Weapon.SetStringProperty("LastAccidentalDestroyAsLauncherByPlayerBlueprint", Defender.Blueprint);
				}
			}
		}
		else if (GameObject.Validate(ref Weapon) && Weapon.Equipped != null)
		{
			Weapon.ModIntProperty("DestroysAsMeleeWeapon", 1);
			Weapon.SetLongProperty("LastDestroyAsMeleeWeaponTurn", XRLCore.CurrentTurn);
			Weapon.SetStringProperty("LastDestroyAsMeleeWeaponBlueprint", Defender.Blueprint);
			if (Actor != null && Actor.IsPlayer())
			{
				Weapon.ModIntProperty("DestroysAsMeleeWeaponByPlayer", 1);
				Weapon.SetLongProperty("LastDestroyAsMeleeWeaponByPlayerTurn", XRLCore.CurrentTurn);
				Weapon.SetStringProperty("LastDestroyAsMeleeWeaponByPlayerBlueprint", Defender.Blueprint);
			}
		}
	}
}
