using System;
using System.Collections.Generic;
using XRL.Collections;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, a chance to target a given projectile that
/// is over 0 will be increased by ((power load - 100) / 30), i.e.
/// 10 for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class PointDefense : IPoweredPart
{
	public int MinRange = 2;

	public int MaxRange = 10;

	public int TargetExplosives = 100;

	public int TargetThrownWeapons = 100;

	public int TargetArrows = 100;

	public int TargetSlugs;

	public int TargetEnergy;

	public bool UsesSelfEquipment;

	public bool UsesEquipperEquipment;

	public bool ShowComputeMessage = true;

	public string EquipmentEvent = "UseForPointDefense";

	public float ComputePowerFactor = 1f;

	[NonSerialized]
	private Cell TargetCell;

	[NonSerialized]
	private bool TargetingPlayerProjectile;

	[NonSerialized]
	private GameObject TargetProjectile;

	[NonSerialized]
	private GameObject WeaponSystem;

	[NonSerialized]
	private GameObject ActiveActor;

	[NonSerialized]
	private int ActivePhase;

	public PointDefense()
	{
		WorksOnEquipper = true;
		IsPowerLoadSensitive = true;
		NameForStatus = "PointDefenseTracking";
	}

	public override bool SameAs(IPart p)
	{
		PointDefense pointDefense = p as PointDefense;
		if (pointDefense.MinRange != MinRange)
		{
			return false;
		}
		if (pointDefense.MaxRange != MaxRange)
		{
			return false;
		}
		if (pointDefense.TargetExplosives != TargetExplosives)
		{
			return false;
		}
		if (pointDefense.TargetThrownWeapons != TargetThrownWeapons)
		{
			return false;
		}
		if (pointDefense.TargetArrows != TargetArrows)
		{
			return false;
		}
		if (pointDefense.TargetSlugs != TargetSlugs)
		{
			return false;
		}
		if (pointDefense.TargetEnergy != TargetEnergy)
		{
			return false;
		}
		if (pointDefense.UsesSelfEquipment != UsesSelfEquipment)
		{
			return false;
		}
		if (pointDefense.UsesEquipperEquipment != UsesEquipperEquipment)
		{
			return false;
		}
		if (pointDefense.ShowComputeMessage != ShowComputeMessage)
		{
			return false;
		}
		if (pointDefense.EquipmentEvent != EquipmentEvent)
		{
			return false;
		}
		if (pointDefense.ComputePowerFactor != ComputePowerFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == PooledEvent<ProjectileMovingEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowComputeMessage)
		{
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

	public override bool HandleEvent(ProjectileMovingEvent E)
	{
		if (E.Launcher == WeaponSystem && WeaponSystem != null)
		{
			if (E.Cell == TargetCell)
			{
				try
				{
					if (!TargetProjectile.PhaseMatches(ActivePhase))
					{
						IComponent<GameObject>.XDidYToZ(WeaponSystem, "intercept", TargetProjectile, ", but " + E.Projectile.does("pass") + " through " + TargetProjectile.them, "!", null, null, ActiveActor, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, IComponent<GameObject>.Visible(ActiveActor) || IComponent<GameObject>.Visible(WeaponSystem));
					}
					else if (!PointDefenseInterceptEvent.Check(TargetProjectile, E.Projectile, WeaponSystem))
					{
						IComponent<GameObject>.XDidYToZ(WeaponSystem, "intercept", TargetProjectile, ", but " + E.Projectile.does("fail") + " to affect " + TargetProjectile.them, "!", null, null, ActiveActor, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, IComponent<GameObject>.Visible(ActiveActor) || IComponent<GameObject>.Visible(WeaponSystem));
					}
					else
					{
						IComponent<GameObject>.XDidYToZ(WeaponSystem, "intercept", TargetProjectile, null, "!", null, null, ActiveActor, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, IComponent<GameObject>.Visible(ActiveActor) || IComponent<GameObject>.Visible(WeaponSystem));
						if (!TargetProjectile.IsReal || !GameObject.Validate(TargetProjectile))
						{
							return false;
						}
						E.HitOverride = TargetProjectile;
						if (TargetingPlayerProjectile)
						{
							E.ActivateShowUninvolved = true;
						}
					}
				}
				finally
				{
					TargetCell = null;
					TargetProjectile = null;
					ActiveActor = null;
				}
			}
		}
		else if (E.Defender == null)
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null)
			{
				int num = cell.PathDistanceTo(E.Cell);
				if (num >= MinRange && num <= MaxRange)
				{
					GameObject gameObject = GetActivePartFirstSubject() ?? ParentObject;
					if (gameObject != E.Attacker)
					{
						int? PowerLoad = null;
						if (ChanceToTargetProjectile(E.Projectile, ref PowerLoad, E.Throw).in100() && E.Attacker != gameObject && !gameObject.IsLedBy(E.Attacker) && !E.Attacker.IsLedBy(gameObject) && (E.Attacker == null || E.Attacker.IsHostileTowards(gameObject) || IsCellOnPath(cell, E.Path)))
						{
							int? powerLoadLevel = PowerLoad;
							if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel) && PathClear(cell, E.Cell, gameObject))
							{
								if (UsesSelfEquipment || UsesEquipperEquipment)
								{
									WeaponSystem = null;
									if (UsesSelfEquipment)
									{
										WeaponSystem = ParentObject.Body?.FindEquipmentByEvent(EquipmentEvent);
									}
									if (UsesEquipperEquipment && WeaponSystem == null && ParentObject.Equipped == null)
									{
										WeaponSystem = ParentObject.Equipped.Body?.FindEquipmentByEvent(EquipmentEvent);
									}
									if (WeaponSystem == null)
									{
										goto IL_04f6;
									}
								}
								else
								{
									WeaponSystem = ParentObject;
								}
								ActiveActor = gameObject;
								ActivePhase = Phase.getWeaponPhase(gameObject, GetActivationPhaseEvent.GetFor(WeaponSystem));
								TargetProjectile = E.Projectile;
								TargetingPlayerProjectile = E.Attacker != null && E.Attacker.IsPlayer();
								TargetCell = E.Cell;
								try
								{
									Event obj = Event.New("CommandFireMissile");
									obj.SetParameter("Owner", gameObject);
									obj.SetParameter("TargetCell", TargetCell);
									obj.SetParameter("ScreenBuffer", E.ScreenBuffer);
									obj.SetParameter("MessageAsFrom", WeaponSystem);
									obj.SetParameter("EnergyMultiplier", 0f);
									WeaponSystem.FireEvent(obj);
								}
								finally
								{
									WeaponSystem = null;
									TargetCell = null;
									TargetProjectile = null;
									ActiveActor = null;
								}
							}
						}
					}
				}
			}
		}
		goto IL_04f6;
		IL_04f6:
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
	}

	public bool PathClear(Cell OurCell, Cell TargetCell, GameObject Actor)
	{
		Zone parentZone = OurCell.ParentZone;
		List<Point> list = Zone.Line(OurCell.X, OurCell.Y, TargetCell.X, TargetCell.Y, ReadOnly: true);
		int i = 1;
		for (int num = list.Count - 1; i < num; i++)
		{
			Cell cell = parentZone.GetCell(list[i].X, list[i].Y);
			if (cell.IsOccludingFor(Actor))
			{
				return false;
			}
			using ScopeDisposedList<GameObject> scopeDisposedList = cell.Objects.GetScopeDisposedCopy();
			int j = 0;
			for (int count = scopeDisposedList.Count; j < count; j++)
			{
				GameObject gameObject = scopeDisposedList[j];
				if (gameObject.IsCombatObject() && !Actor.IsHostileTowards(gameObject))
				{
					return false;
				}
				if ((gameObject.ConsiderSolidFor(Actor) || GetMissileCoverPercentageEvent.GetFor(gameObject, Actor) > 0) && !OkayToDamageEvent.Check(gameObject, Actor))
				{
					return false;
				}
			}
		}
		return true;
	}

	public int ChanceToTargetProjectile(GameObject Projectile, ref int? PowerLoad, bool? Thrown = null)
	{
		int num = 0;
		if (TargetThrownWeapons > 0 && num < TargetThrownWeapons)
		{
			if (Thrown == true)
			{
				num = TargetThrownWeapons;
			}
			else if (!Thrown.HasValue && Projectile.HasPart<ThrownWeapon>())
			{
				num = TargetThrownWeapons;
			}
		}
		if (TargetExplosives > 0 && num < TargetExplosives && IsExplosiveEvent.Check(Projectile))
		{
			num = TargetExplosives;
		}
		if (TargetArrows > 0 && num < TargetArrows && (Projectile.HasPart<AmmoArrow>() || Projectile.HasPart<AmmoDart>() || Projectile.HasTag("Arrow")))
		{
			num = TargetArrows;
		}
		if (TargetSlugs > 0 && num < TargetSlugs && (Projectile.HasPart<AmmoSlug>() || Projectile.HasPart<AmmoShotgunShell>() || Projectile.HasTag("Slug")))
		{
			num = TargetSlugs;
		}
		if (TargetEnergy > 0 && num < TargetEnergy && ProjectileIsEnergy(Projectile))
		{
			num = TargetEnergy;
		}
		if (num > 0)
		{
			num = GetAvailableComputePowerEvent.AdjustUp(this, num, ComputePowerFactor);
			if (IsPowerLoadSensitive)
			{
				int valueOrDefault = PowerLoad.GetValueOrDefault();
				if (!PowerLoad.HasValue)
				{
					valueOrDefault = MyPowerLoadLevel();
					PowerLoad = valueOrDefault;
				}
				num += IComponent<GameObject>.PowerLoadBonus(PowerLoad.Value, 100, 30);
			}
		}
		return num;
	}

	public int ChanceToTargetProjectile(GameObject Projectile, bool? Thrown = null)
	{
		int? PowerLoad = 100;
		return ChanceToTargetProjectile(Projectile, ref PowerLoad, Thrown);
	}

	private bool ProjectileIsEnergy(GameObject Projectile)
	{
		if (Projectile.TryGetPart<Projectile>(out var Part) && !Part.Attributes.IsNullOrEmpty())
		{
			if (Damage.ContainsLightDamage(Part.Attributes))
			{
				return true;
			}
			if (Damage.ContainsHeatDamage(Part.Attributes))
			{
				return true;
			}
			if (Damage.ContainsColdDamage(Part.Attributes))
			{
				return true;
			}
			if (Damage.ContainsDisintegrationDamage(Part.Attributes))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCellOnPath(Cell C, List<Point> Path)
	{
		int i = 0;
		for (int count = Path.Count; i < count; i++)
		{
			if (Path[i].X == C.X && Path[i].Y == C.Y)
			{
				return true;
			}
		}
		return false;
	}
}
