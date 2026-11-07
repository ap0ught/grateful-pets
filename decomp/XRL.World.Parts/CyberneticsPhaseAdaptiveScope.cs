using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsPhaseAdaptiveScope : IPoweredPart, IThrownWeaponFlexPhaseProvider
{
	public static readonly string COMMAND_NAME = "CommandTogglePhaseAdaptiveScope";

	public Guid ActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private long SafetyIncidentTurn;

	public CyberneticsPhaseAdaptiveScope()
	{
		WorksOnImplantee = true;
		ChargeUse = 0;
		NameForStatus = "PhaseAdaptiveScope";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != PooledEvent<GetCyberneticsBehaviorDescriptionEvent>.ID && ID != PooledEvent<GetThrownWeaponFlexPhaseProviderEvent>.ID && ID != PooledEvent<BeforeProjectileHitEvent>.ID && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID)
		{
			return ID == PooledEvent<ProjectileMovingEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Phase-Adaptive Projectiles", COMMAND_NAME, "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && E.Actor == ParentObject.Implantee)
		{
			ParentObject.Implantee.ToggleActivatedAbility(ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetThrownWeaponFlexPhaseProviderEvent E)
	{
		if (SafetyIncidentTurn < The.Game.TimeTicks && E.Priority < 100 && IsObjectActivePartSubject(E.Actor) && E.Actor.IsActivatedAbilityUsable(ActivatedAbilityID) && E.Actor.IsActivatedAbilityToggledOn(ActivatedAbilityID) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Provider = this;
			E.Priority = 100;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeProjectileHitEvent E)
	{
		if (!E.Prospective && SafetyIncidentTurn < The.Game.TimeTicks && GameObject.Validate(E.Projectile) && GameObject.Validate(E.Attacker) && GameObject.Validate(E.ApparentTarget) && GameObject.Validate(E.Object) && IsObjectActivePartSubject(E.Attacker) && E.Object != E.ApparentTarget && E.Attacker.DistanceTo(E.Object) <= E.Attacker.DistanceTo(E.ApparentTarget) && E.Attacker.IsActivatedAbilityUsable(ActivatedAbilityID) && E.Attacker.IsActivatedAbilityToggledOn(ActivatedAbilityID) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Cell cell = E.Cell ?? E.Projectile.CurrentCell;
			Cell cell2 = E.ApparentTarget.CurrentCell;
			if (cell != null && cell != cell2)
			{
				TryDesyncPhase(E.Projectile, E.Object, cell, E.PenetrateCreatures, E.PenetrateWalls, out var RecheckHit, out var RecheckPhase, out var SafetyIncident);
				if (RecheckHit)
				{
					E.Recheck = true;
				}
				if (RecheckPhase)
				{
					E.RecheckPhase = true;
				}
				if (SafetyIncident)
				{
					SafetyIncidentTurn = The.Game.TimeTicks;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ProjectileMovingEvent E)
	{
		if (SafetyIncidentTurn < The.Game.TimeTicks && GameObject.Validate(E.Projectile) && GameObject.Validate(E.Attacker) && GameObject.Validate(E.ApparentTarget) && E.PathIndex < E.Path.Count - 1 && IsObjectActivePartSubject(E.Attacker) && E.Attacker.IsActivatedAbilityUsable(ActivatedAbilityID) && E.Attacker.IsActivatedAbilityToggledOn(ActivatedAbilityID) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Cell cell = E.Cell ?? E.Projectile.CurrentCell;
			Cell cell2 = E.ApparentTarget.CurrentCell;
			if (cell != null && cell2 != null)
			{
				Point point = E.Path[E.PathIndex + 1];
				if (point.X == cell2.X && point.Y == cell2.Y)
				{
					TrySyncPhase(E.Projectile, E.ApparentTarget, cell, out var _, out var RecheckPhase, out var SafetyIncident);
					if (RecheckPhase)
					{
						E.RecheckPhase = true;
					}
					if (SafetyIncident)
					{
						SafetyIncidentTurn = The.Game.TimeTicks;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void ThrownWeaponFlexPhaseStart(GameObject Weapon)
	{
	}

	public bool ThrownWeaponFlexPhaseIsActive(GameObject Weapon)
	{
		return SafetyIncidentTurn < The.Game.TimeTicks;
	}

	public bool ThrownWeaponFlexPhaseTraversal(GameObject Actor, GameObject WillHit, GameObject Target, GameObject Weapon, int Phase, Cell FromCell, Cell ToCell, out bool RecheckHit, out bool RecheckPhase, bool HasDynamicTargets = false)
	{
		bool SafetyIncident = false;
		RecheckHit = false;
		RecheckPhase = false;
		if (Target != null && Actor.IsActivatedAbilityUsable(ActivatedAbilityID) && Actor.IsActivatedAbilityToggledOn(ActivatedAbilityID) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (Target == WillHit)
			{
				TrySyncPhase(Weapon, Target, FromCell, out RecheckHit, out RecheckPhase, out SafetyIncident);
			}
			else if (WillHit != null && (HasDynamicTargets || Actor.DistanceTo(WillHit) < Actor.DistanceTo(Target)))
			{
				TryDesyncPhase(Weapon, WillHit, FromCell, PenetrateCreatures: false, PenetrateWalls: false, out RecheckHit, out RecheckPhase, out SafetyIncident);
			}
			else
			{
				GameObject combatTarget = ToCell.GetCombatTarget(Actor, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, Phase, Weapon, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true, (GameObject o) => !o.IsCreature);
				if (combatTarget != null && (HasDynamicTargets || Actor.DistanceTo(combatTarget) < Actor.DistanceTo(Target)))
				{
					TryDesyncPhase(Weapon, combatTarget, FromCell, PenetrateCreatures: false, PenetrateWalls: false, out RecheckHit, out RecheckPhase, out SafetyIncident);
				}
			}
			if (SafetyIncident)
			{
				SafetyIncidentTurn = The.Game.TimeTicks;
			}
		}
		return true;
	}

	public void ThrownWeaponFlexPhaseEnd(GameObject Weapon)
	{
	}

	private void TryDesyncPhase(GameObject Projectile, GameObject Subject, Cell Cell, bool PenetrateCreatures, bool PenetrateWalls, out bool RecheckHit, out bool RecheckPhase, out bool SafetyIncident)
	{
		RecheckHit = false;
		RecheckPhase = false;
		SafetyIncident = false;
		if (Cell == null || (PenetrateCreatures && Subject.IsCreature) || (PenetrateWalls && Subject.IsWall()))
		{
			return;
		}
		int phase = Subject.GetPhase();
		if (phase == 3 || phase == 4 || phase == 0 || phase == 5 || SafetyIncidentTurn == The.Game.TimeTicks)
		{
			return;
		}
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (Projectile.HasEffect<Omniphase>())
		{
			ConsumeCharge();
			Event obj = Event.New("InitiateRealityDistortionTransit");
			obj.SetParameter("Object", activePartFirstSubject);
			obj.SetParameter("Device", ParentObject);
			obj.SetParameter("Operator", activePartFirstSubject);
			obj.SetParameter("Cell", Cell);
			obj.SetParameter("Purpose", "alter the phase of " + Projectile.t());
			if (!activePartFirstSubject.FireEvent(obj) || !Cell.FireEvent(obj))
			{
				SafetyIncident = true;
				return;
			}
			Projectile.RemoveAllEffects<Omniphase>();
			RecheckHit = true;
			RecheckPhase = true;
			PhaseChangeVisual(Cell);
		}
		switch (phase)
		{
		case 1:
			if (!Projectile.HasEffect<Phased>())
			{
				ConsumeCharge();
				Event obj3 = Event.New("InitiateRealityDistortionTransit");
				obj3.SetParameter("Object", activePartFirstSubject);
				obj3.SetParameter("Device", ParentObject);
				obj3.SetParameter("Operator", activePartFirstSubject);
				obj3.SetParameter("Cell", Cell);
				obj3.SetParameter("Purpose", "alter the phase of " + Projectile.t());
				if (!activePartFirstSubject.FireEvent(obj3) || !Cell.FireEvent(obj3))
				{
					SafetyIncident = true;
				}
				else if (Projectile.ForceApplyEffect(new Phased(1)))
				{
					RecheckHit = true;
					RecheckPhase = true;
					PhaseChangeVisual(Cell);
				}
			}
			break;
		case 2:
			if (Projectile.HasEffect<Phased>())
			{
				ConsumeCharge();
				Event obj2 = Event.New("InitiateRealityDistortionTransit");
				obj2.SetParameter("Object", activePartFirstSubject);
				obj2.SetParameter("Device", ParentObject);
				obj2.SetParameter("Operator", activePartFirstSubject);
				obj2.SetParameter("Cell", Cell);
				obj2.SetParameter("Purpose", "alter the phase of " + Projectile.t());
				if (!activePartFirstSubject.FireEvent(obj2) || !Cell.FireEvent(obj2))
				{
					SafetyIncident = true;
					break;
				}
				Projectile.RemoveAllEffects<Phased>();
				RecheckHit = true;
				RecheckPhase = true;
				PhaseChangeVisual(Cell);
			}
			break;
		}
	}

	private void TrySyncPhase(GameObject Projectile, GameObject Subject, Cell Cell, out bool RecheckHit, out bool RecheckPhase, out bool SafetyIncident)
	{
		RecheckHit = false;
		RecheckPhase = false;
		SafetyIncident = false;
		if (Cell == null)
		{
			return;
		}
		int phase = Subject.GetPhase();
		if (phase == 3 || phase == 4 || phase == 0 || phase == 5 || Projectile.HasEffect<Omniphase>() || SafetyIncidentTurn == The.Game.TimeTicks)
		{
			return;
		}
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		switch (phase)
		{
		case 1:
			if (Projectile.HasEffect<Phased>())
			{
				ConsumeCharge();
				Event obj2 = Event.New("InitiateRealityDistortionTransit");
				obj2.SetParameter("Object", activePartFirstSubject);
				obj2.SetParameter("Device", ParentObject);
				obj2.SetParameter("Operator", activePartFirstSubject);
				obj2.SetParameter("Cell", Cell);
				obj2.SetParameter("Purpose", "alter the phase of " + Projectile.t());
				if (!activePartFirstSubject.FireEvent(obj2) || !Cell.FireEvent(obj2))
				{
					SafetyIncident = true;
					break;
				}
				Projectile.RemoveAllEffects<Phased>();
				RecheckHit = true;
				RecheckPhase = true;
				PhaseChangeVisual(Cell);
			}
			break;
		case 2:
			if (!Projectile.HasEffect<Phased>())
			{
				ConsumeCharge();
				Event obj = Event.New("InitiateRealityDistortionTransit");
				obj.SetParameter("Object", activePartFirstSubject);
				obj.SetParameter("Device", ParentObject);
				obj.SetParameter("Operator", activePartFirstSubject);
				obj.SetParameter("Cell", Cell);
				obj.SetParameter("Purpose", "alter the phase of " + Projectile.t());
				if (!activePartFirstSubject.FireEvent(obj) || !Cell.FireEvent(obj))
				{
					SafetyIncident = true;
				}
				else if (Projectile.ForceApplyEffect(new Phased(1)))
				{
					RecheckHit = true;
					RecheckPhase = true;
					PhaseChangeVisual(Cell);
				}
			}
			break;
		}
	}

	public void PhaseChangeVisual(Cell Cell)
	{
		if (Options.UseTiles)
		{
			Cell.TileParticleBlip("Tiles2/status_phase_change.bmp", "&Y", "y", 10, IgnoreVisibility: false, HFlip: false, VFlip: false, 0L);
		}
		else
		{
			Cell.ParticleBlip("&Y÷", 10, 0L);
		}
	}
}
