using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class GetMissileWeaponPerformanceEvent : PooledEvent<GetMissileWeaponPerformanceEvent>
{
	private class Perf
	{
		public int BasePenetration;

		public int PenetrationCap;

		public int PenetrationBonus;

		public string BaseDamage;

		public string Attributes;

		public bool PenetrateCreatures;

		public bool PenetrateWalls;

		public bool Quiet;
	}

	public new static readonly int CascadeLevel = 1;

	public GameObject Subject;

	public GameObject Actor;

	public GameObject Launcher;

	public GameObject Projectile;

	public int BasePenetration;

	public int PenetrationCap;

	public int PenetrationBonus;

	public string BaseDamage;

	public string Attributes;

	public string DamageColor;

	public bool PenetrateCreatures;

	public bool PenetrateWalls;

	public bool Quiet;

	public DieRoll DamageRoll;

	public bool Active;

	private static Dictionary<string, Perf> ProjectilePerformance = new Dictionary<string, Perf>(8);

	public int Penetration => Math.Min(BasePenetration, PenetrationCap) + PenetrationBonus;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Subject = null;
		Actor = null;
		Launcher = null;
		Projectile = null;
		BasePenetration = 0;
		PenetrationCap = 0;
		PenetrationBonus = 0;
		BaseDamage = null;
		Attributes = null;
		DamageColor = null;
		PenetrateCreatures = false;
		PenetrateWalls = false;
		Quiet = false;
		DamageRoll = null;
		Active = false;
	}

	public static GetMissileWeaponPerformanceEvent FromPool(GameObject Actor, GameObject Launcher, GameObject Projectile = null, int? BasePenetration = null, int? PenetrationCap = null, int? PenetrationBonus = null, string BaseDamage = null, string Attributes = null, string DamageColor = null, bool? PenetrateCreatures = null, bool? PenetrateWalls = null, bool? Quiet = null, DieRoll DamageRoll = null, string ProjectileBlueprint = null, bool Active = false)
	{
		GetMissileWeaponPerformanceEvent getMissileWeaponPerformanceEvent = PooledEvent<GetMissileWeaponPerformanceEvent>.FromPool();
		getMissileWeaponPerformanceEvent.Actor = Actor;
		getMissileWeaponPerformanceEvent.Launcher = Launcher;
		if (Projectile == null && ProjectileBlueprint == null && Launcher != null)
		{
			GetMissileWeaponProjectileEvent.GetFor(Launcher, ref Projectile, ref ProjectileBlueprint);
		}
		getMissileWeaponPerformanceEvent.Projectile = Projectile;
		if (BaseDamage == null || !BasePenetration.HasValue || !PenetrationCap.HasValue || !PenetrationBonus.HasValue || Attributes == null || !PenetrateCreatures.HasValue || !PenetrateWalls.HasValue || !Quiet.HasValue)
		{
			if (Projectile != null)
			{
				Projectile part = Projectile.GetPart<Projectile>();
				if (part != null)
				{
					int num = BasePenetration.GetValueOrDefault();
					if (!BasePenetration.HasValue)
					{
						num = part.BasePenetration;
						BasePenetration = num;
						if (Actor != null)
						{
							MissileWeapon missileWeapon = Launcher?.GetPart<MissileWeapon>();
							if (missileWeapon != null && !missileWeapon.ProjectilePenetrationStat.IsNullOrEmpty())
							{
								BasePenetration += Actor.StatMod(missileWeapon.ProjectilePenetrationStat);
							}
						}
					}
					if (!PenetrationCap.HasValue)
					{
						PenetrationCap = num + part.StrengthPenetration;
					}
					if (!PenetrationBonus.HasValue)
					{
						PenetrationBonus = 0;
					}
					if (BaseDamage == null)
					{
						BaseDamage = part.BaseDamage;
					}
					if (Attributes == null)
					{
						Attributes = part.Attributes;
					}
					if (!PenetrateCreatures.HasValue)
					{
						PenetrateCreatures = part.PenetrateCreatures;
					}
					if (!PenetrateWalls.HasValue)
					{
						PenetrateWalls = part.PenetrateWalls;
					}
					if (!Quiet.HasValue)
					{
						Quiet = part.Quiet;
					}
				}
			}
			else if (ProjectileBlueprint != null)
			{
				if (!ProjectilePerformance.TryGetValue(ProjectileBlueprint, out var value))
				{
					value = (ProjectilePerformance[ProjectileBlueprint] = new Perf());
					GameObject gameObject = GameObject.CreateSample(ProjectileBlueprint);
					Projectile part2 = gameObject.GetPart<Projectile>();
					if (part2 != null)
					{
						value.BasePenetration = part2.BasePenetration;
						value.PenetrationCap = part2.BasePenetration + part2.StrengthPenetration;
						value.PenetrationBonus = 0;
						value.BaseDamage = part2.BaseDamage;
						value.Attributes = part2.Attributes;
						value.PenetrateCreatures = part2.PenetrateCreatures;
						value.PenetrateWalls = part2.PenetrateWalls;
						value.Quiet = part2.Quiet;
					}
					gameObject.Obliterate();
				}
				if (!BasePenetration.HasValue)
				{
					BasePenetration = value.BasePenetration;
					if (Actor != null)
					{
						MissileWeapon missileWeapon2 = Launcher?.GetPart<MissileWeapon>();
						if (missileWeapon2 != null && !missileWeapon2.ProjectilePenetrationStat.IsNullOrEmpty())
						{
							BasePenetration += Actor.StatMod(missileWeapon2.ProjectilePenetrationStat);
						}
					}
				}
				if (!PenetrationCap.HasValue)
				{
					PenetrationCap = value.PenetrationCap;
				}
				if (!PenetrationBonus.HasValue)
				{
					PenetrationBonus = value.PenetrationBonus;
				}
				if (BaseDamage == null)
				{
					BaseDamage = value.BaseDamage;
				}
				if (Attributes == null)
				{
					Attributes = value.Attributes;
				}
				if (!PenetrateCreatures.HasValue)
				{
					PenetrateCreatures = value.PenetrateCreatures;
				}
				if (!PenetrateWalls.HasValue)
				{
					PenetrateWalls = value.PenetrateWalls;
				}
				if (!Quiet.HasValue)
				{
					Quiet = value.Quiet;
				}
			}
		}
		if (Attributes != null && Attributes.Contains("Psionic") && getMissileWeaponPerformanceEvent.Actor != null)
		{
			int num2 = getMissileWeaponPerformanceEvent.Actor.StatMod("Ego");
			PenetrationBonus = PenetrationBonus.GetValueOrDefault() + num2;
		}
		getMissileWeaponPerformanceEvent.Subject = null;
		getMissileWeaponPerformanceEvent.BaseDamage = BaseDamage;
		getMissileWeaponPerformanceEvent.BasePenetration = BasePenetration.GetValueOrDefault();
		getMissileWeaponPerformanceEvent.PenetrationCap = PenetrationCap ?? getMissileWeaponPerformanceEvent.BasePenetration;
		getMissileWeaponPerformanceEvent.PenetrationBonus = PenetrationBonus.GetValueOrDefault();
		getMissileWeaponPerformanceEvent.Attributes = Attributes;
		getMissileWeaponPerformanceEvent.DamageColor = DamageColor;
		getMissileWeaponPerformanceEvent.PenetrateCreatures = PenetrateCreatures == true;
		getMissileWeaponPerformanceEvent.PenetrateWalls = PenetrateWalls == true;
		getMissileWeaponPerformanceEvent.Quiet = Quiet == true;
		getMissileWeaponPerformanceEvent.DamageRoll = DamageRoll;
		getMissileWeaponPerformanceEvent.Active = Active;
		return getMissileWeaponPerformanceEvent;
	}

	public DieRoll GetDamageRoll()
	{
		if (DamageRoll == null && BaseDamage != null)
		{
			DamageRoll = new DieRoll(BaseDamage);
		}
		return DamageRoll;
	}

	public DieRoll GetPossiblyCachedDamageRoll()
	{
		return DamageRoll ?? BaseDamage.GetCachedDieRoll();
	}

	public int RollDamagePenetrations(int TargetInclusive)
	{
		return Stat.RollDamagePenetrations(TargetInclusive, BasePenetration + PenetrationBonus, PenetrationCap + PenetrationBonus);
	}

	public string GetDamageColor()
	{
		if (!DamageColor.IsNullOrEmpty())
		{
			return DamageColor;
		}
		return Damage.GetDamageColor(Attributes);
	}

	public static GetMissileWeaponPerformanceEvent GetFor(GameObject Actor, GameObject Launcher, GameObject Projectile = null, int? BasePenetration = null, int? PenetrationCap = null, string BaseDamage = null, string Attributes = null, string DamageColor = null, bool? PenetrateCreatures = null, bool? PenetrateWalls = null, bool? Quiet = null, DieRoll DamageRoll = null, string ProjectileBlueprint = null, bool Active = false)
	{
		GameObject actor = Actor;
		GameObject launcher = Launcher;
		GameObject projectile = Projectile;
		bool active = Active;
		GetMissileWeaponPerformanceEvent getMissileWeaponPerformanceEvent = FromPool(actor, launcher, projectile, BasePenetration, PenetrationCap, null, BaseDamage, Attributes, DamageColor, PenetrateCreatures, PenetrateWalls, Quiet, DamageRoll, ProjectileBlueprint, active);
		Projectile = getMissileWeaponPerformanceEvent.Projectile;
		bool flag = true;
		if (flag && GameObject.Validate(ref Launcher) && Launcher.WantEvent(PooledEvent<GetMissileWeaponPerformanceEvent>.ID, CascadeLevel))
		{
			getMissileWeaponPerformanceEvent.Subject = Launcher;
			flag = Launcher.HandleEvent(getMissileWeaponPerformanceEvent);
		}
		if (flag && GameObject.Validate(ref Projectile) && Projectile.WantEvent(PooledEvent<GetMissileWeaponPerformanceEvent>.ID, CascadeLevel))
		{
			getMissileWeaponPerformanceEvent.Subject = Projectile;
			flag = Projectile.HandleEvent(getMissileWeaponPerformanceEvent);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetMissileWeaponPerformanceEvent>.ID, CascadeLevel))
		{
			getMissileWeaponPerformanceEvent.Subject = Actor;
			flag = Actor.HandleEvent(getMissileWeaponPerformanceEvent);
		}
		return getMissileWeaponPerformanceEvent;
	}
}
