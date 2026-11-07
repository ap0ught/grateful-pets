using System;
using System.Collections.Generic;
using XRL.Collections;
using XRL.Rules;
using XRL.World.AI.Pathfinding;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Kill : GoalHandler
{
	public GameObject _Target;

	public int ExtinguishSelfTries;

	public bool Directed;

	private int LastSeen;

	private int LastAttacked;

	private int MoveTries;

	[NonSerialized]
	public static List<string> tryMeleeOrder = new List<string> { "items", "missile", "abilities", "defensiveItems", "defensiveAbilities" };

	[NonSerialized]
	public static List<string> tryMissileOrder = new List<string> { "items", "missile", "abilities", "thrown", "defensiveItems", "defensiveAbilities" };

	public GameObject Target
	{
		get
		{
			GameObject.Validate(ref _Target);
			return _Target;
		}
		set
		{
			_Target = value;
		}
	}

	public Kill(GameObject Target, bool Directed = false)
	{
		this.Target = Target;
		this.Directed = Directed;
	}

	public override void Create()
	{
		if (Target == null)
		{
			return;
		}
		Think("I'm trying to kill someone!");
		if (ParentObject.HasRegisteredEvent("AICreateKill"))
		{
			Event e = Event.New("AICreateKill", "Actor", ParentObject, "Target", Target);
			if (!ParentObject.FireEvent(e))
			{
				return;
			}
		}
		if (Target.HasRegisteredEvent("AITargetCreateKill"))
		{
			Event e2 = Event.New("AITargetCreateKill", "Actor", ParentObject, "Target", Target);
			Target.FireEvent(e2);
		}
	}

	public override string GetDetails()
	{
		if (Target != null)
		{
			if (!_Target.IsPlayer())
			{
				return _Target.DebugName;
			}
			return "Player";
		}
		return null;
	}

	public override bool Finished()
	{
		return false;
	}

	public bool TryThrownWeapon()
	{
		if (ParentObject.Body == null)
		{
			return false;
		}
		GameObject target = Target;
		Cell currentCell = ParentObject.CurrentCell;
		Cell cell = target?.CurrentCell;
		if (currentCell == null || cell == null || currentCell.ParentZone != cell.ParentZone)
		{
			return false;
		}
		if (!ParentObject.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		using ScopeDisposedList<GameObject> scopeDisposedList = ScopeDisposedList<GameObject>.GetFromPool();
		ParentObject.GetThrownWeapons(scopeDisposedList);
		if (scopeDisposedList.IsNullOrEmpty())
		{
			return false;
		}
		GameObject gameObject = scopeDisposedList[0];
		if (gameObject.HasPart<StickyOnHit>() && target.HasEffect<Stuck>())
		{
			return false;
		}
		GetThrowProfileEvent.Process(out var Range, out var _, out var _, out var _, ParentObject, gameObject, target, cell, ParentObject.DistanceTo(target));
		int num = ParentObject.GetThrowRangeRandomVariance();
		int i = 2;
		for (int num2 = Math.Min(ParentObject.Stat("Intelligence") / 5, 5); i < num2; i++)
		{
			num = Math.Min(num, ParentObject.GetThrowRangeRandomVariance());
		}
		List<Point> list = Zone.Line(currentCell.X, currentCell.Y, cell.X, cell.Y);
		if (list.Count - 1 > Range + num)
		{
			Think("Too far to target for a throw, I'll try a different weapon...");
			return false;
		}
		int j = 1;
		for (int num3 = list.Count - 1; j < num3; j++)
		{
			Cell cell2 = cell.ParentZone.GetCell(list[j].X, list[j].Y);
			int k = 0;
			for (int count = cell2.Objects.Count; k < count; k++)
			{
				GameObject gameObject2 = cell2.Objects[k];
				if (gameObject2.IsCombatObject() && !ParentBrain.IsHostileTowards(gameObject2))
				{
					Think("Friendly or neutral in the way, I'll try a different weapon...");
					return false;
				}
				if (gameObject2.ConsiderSolidFor(ParentObject) && !ParentObject.ShouldAttackToReachTarget(gameObject2, target))
				{
					Think("Solid object in the way, I'll try a different weapon...");
					return false;
				}
			}
			if (cell2.IsOccluding())
			{
				int num4 = Stat.Random(1, ParentObject.Stat("Intelligence"));
				if (num4 < 7)
				{
					Think("I can't figure out where my target might be.");
					return false;
				}
				if (num4 < 14 && 75.in100())
				{
					cell = cell.GetRandomLocalAdjacentCell() ?? cell;
				}
			}
		}
		Think("I'm going to throw!");
		if (!Combat.ThrowWeapon(ParentObject, target, cell, null, scopeDisposedList))
		{
			Think("It didn't work, I'll try a different weapon...");
			return false;
		}
		foreach (GameObject item in scopeDisposedList)
		{
			if (GameObject.Validate(item))
			{
				AIAfterThrowEvent.Send(item, ParentObject, target);
			}
		}
		return true;
	}

	public bool TryMissileReload()
	{
		bool flag = false;
		try
		{
			List<GameObject> missileWeapons = ParentObject.GetMissileWeapons();
			if (missileWeapons != null)
			{
				int i = 0;
				for (int count = missileWeapons.Count; i < count; i++)
				{
					GameObject gameObject = missileWeapons[i];
					if (gameObject.GetPart<EnergyCellSocket>() != null)
					{
						EnergyAmmoLoader part = gameObject.GetPart<EnergyAmmoLoader>();
						if (part != null && part.GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.Unpowered)
						{
							flag = CommandReloadEvent.Execute(ParentObject, gameObject, null, FreeAction: false, FromDialog: false, part.GetActiveChargeUse());
							break;
						}
						int num = QueryDrawEvent.GetFor(gameObject);
						if (num > 0 && !gameObject.TestCharge(num, LiveOnly: false, 0L) && !gameObject.HasEffect<ElectromagneticPulsed>())
						{
							flag = CommandReloadEvent.Execute(ParentObject, gameObject, null, FreeAction: false, FromDialog: false, num);
							break;
						}
					}
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("TryMissileReload", x);
		}
		if (flag)
		{
			return ParentObject.Stat("Energy") < 1000;
		}
		return false;
	}

	public bool TryMissileWeapon()
	{
		GameObject target = Target;
		if (target == null)
		{
			return false;
		}
		if (!target.IsCombatObject())
		{
			Think("Target can't be targeted with missile weapons, I'll try a different weapon...");
			return false;
		}
		Cell currentCell = ParentObject.CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		Zone parentZone = currentCell.ParentZone;
		if (parentZone == null)
		{
			return false;
		}
		Cell cell = target.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (parentZone != cell.ParentZone)
		{
			Think("Target is in another zone, I'll try a different weapon...");
			return false;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			Think("I'm not a creature that can equip missile weapons, I'll try a different weapon...");
			return false;
		}
		if (ParentObject.GetIntProperty("TurretWarmup") > 0 && (target.IsPlayerControlled() || Visible()))
		{
			if (ParentObject.GetIntProperty("TurretWarmup") == 1)
			{
				if (parentZone.IsActive())
				{
					ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_turret_chirp");
					if (Visible())
					{
						ParentObject.Physics.DidX("chirp");
						if (AutoAct.IsActive() && The.Player.IsRelevantHostile(ParentObject))
						{
							AutoAct.Interrupt(null, null, ParentObject, IsThreat: true);
						}
					}
					else if (ParentObject.IsAudible(The.Player))
					{
						GoalHandler.AddPlayerMessage("Something chirps " + The.Player.DescribeDirectionToward(ParentObject) + ".");
						if (AutoAct.IsActive())
						{
							AutoAct.Interrupt(null, null, ParentObject, IsThreat: true);
						}
					}
				}
				ParentObject.ApplyEffect(new WarmingUp());
				ParentObject.UseEnergy(1000, "Warmup");
				ParentObject.SetIntProperty("TurretWarmup", 2);
				return true;
			}
			if (ParentObject.GetIntProperty("TurretWarmup") == 2)
			{
				ParentObject.RemoveEffect<WarmingUp>();
				ParentObject.SetIntProperty("TurretWarmup", 3);
			}
		}
		if (ParentBrain.NeedToReload)
		{
			ParentBrain.NeedToReload = false;
			if (!CommandReloadEvent.Execute(ParentObject))
			{
				Think("Need to reload and can't, I'll try a different weapon...");
				return false;
			}
			if (ParentObject.Stat("Energy") < 1000)
			{
				return true;
			}
		}
		List<GameObject> missileWeapons = body.GetMissileWeapons();
		if (missileWeapons == null || missileWeapons.Count <= 0)
		{
			Think("No missile weapons equipped, I'll try a different weapon...");
			return false;
		}
		int num = cell.PathDistanceTo(currentCell);
		if (num > ParentBrain.MaxMissileRange)
		{
			Think("Too far to target for missile weapons, I'll try a different weapon...");
			return false;
		}
		List<Point> list = null;
		int i = 0;
		for (int count = missileWeapons.Count; i < count; i++)
		{
			GameObject gameObject = missileWeapons[i];
			if (!AIWantUseWeaponEvent.Check(gameObject, ParentObject, target))
			{
				continue;
			}
			MissileWeapon part = gameObject.GetPart<MissileWeapon>();
			if (part != null && num > part.MaxRange)
			{
				continue;
			}
			if (list == null)
			{
				list = Zone.Line(currentCell.X, currentCell.Y, cell.X, cell.Y, ReadOnly: true);
			}
			int j = 1;
			for (int num2 = list.Count - 1; j < num2; j++)
			{
				Cell cell2 = parentZone.GetCell(list[j].X, list[j].Y);
				int k = 0;
				for (int count2 = cell2.Objects.Count; k < count2; k++)
				{
					GameObject gameObject2 = cell2.Objects[k];
					if (gameObject2.IsCombatObject() && !ParentBrain.IsHostileTowards(gameObject2))
					{
						Think("Friendly or neutral in the way, I'll try a different weapon...");
						return false;
					}
					if (gameObject2.ConsiderSolidFor(ParentObject) && !ParentObject.ShouldAttackToReachTarget(gameObject2, target))
					{
						Think("Solid object in the way, I'll try a different weapon...");
						return false;
					}
				}
				if (cell2.IsOccluding())
				{
					int num3 = Stat.Random(1, ParentObject.Stat("Intelligence"));
					if (num3 < 7)
					{
						Think("I can't figure out where my target might be.");
						return false;
					}
					if (num3 < 14 && 75.in100())
					{
						cell = cell.GetRandomLocalAdjacentCell() ?? cell;
					}
				}
			}
			Think("I'm going to fire one or more missile weapons, probably my " + gameObject.Blueprint + "!");
			if (Combat.FireMissileWeapon(ParentObject, target, cell, FireType.Normal, null, 0, 0, 90, null, SkipPastMaxRange: true))
			{
				return true;
			}
			Think("It didn't work, I'll try a different weapon...");
		}
		Think("No missile weapons I can use, I'll try a different weapon...");
		return false;
	}

	public override void Push(Brain pBrain)
	{
		if (pBrain.ParentObject != Target && pBrain.CanFight())
		{
			base.Push(pBrain);
		}
	}

	public bool TryDefensiveItems()
	{
		if ((!ParentObject.HasStat("Intelligence") || ParentObject.Stat("Intelligence") >= 7) && Target.InSameZone(ParentObject) && AICommandList.HandleCommandList(AIGetDefensiveItemListEvent.GetFor(ParentObject), ParentObject, Target))
		{
			return true;
		}
		return false;
	}

	public bool TryDefensiveAbilities()
	{
		if (Target != null && Target.InSameZone(ParentObject) && AICommandList.HandleCommandList(AIGetDefensiveAbilityListEvent.GetFor(ParentObject), ParentObject, Target))
		{
			return true;
		}
		return false;
	}

	public bool TryItems()
	{
		if (Target != null && Target.InSameZone(ParentObject) && AICommandList.HandleCommandList(AIGetOffensiveItemListEvent.GetFor(ParentObject), ParentObject, Target))
		{
			return true;
		}
		return false;
	}

	public bool TryAbilities()
	{
		if (Target.InSameZone(ParentObject) && AICommandList.HandleCommandList(AIGetOffensiveAbilityListEvent.GetFor(ParentObject), ParentObject, Target))
		{
			return true;
		}
		return false;
	}

	public GameObject FindTargetOfOpportunity(Cell MyCell, Cell TargetCell = null)
	{
		if (TargetCell != null)
		{
			Cell cellFromDirectionOfCell = MyCell.GetCellFromDirectionOfCell(TargetCell);
			if (cellFromDirectionOfCell != null)
			{
				GameObject combatTarget = cellFromDirectionOfCell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: false);
				if (combatTarget != null)
				{
					if (ParentObject.IsHostileTowards(combatTarget))
					{
						return combatTarget;
					}
				}
				else
				{
					combatTarget = cellFromDirectionOfCell.GetCombatTarget(ParentObject);
					if (combatTarget != null && ParentObject.ShouldAttackToReachTarget(combatTarget, Target))
					{
						return combatTarget;
					}
				}
			}
		}
		foreach (Cell localAdjacentCell in MyCell.GetLocalAdjacentCells())
		{
			GameObject combatTarget2 = localAdjacentCell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: false);
			if (combatTarget2 != null && ParentObject.IsHostileTowards(combatTarget2))
			{
				return combatTarget2;
			}
		}
		return null;
	}

	public GameObject FindTargetOfOpportunity(GameObject Target = null)
	{
		return FindTargetOfOpportunity(ParentObject.CurrentCell, Target?.CurrentCell);
	}

	public override void TakeAction()
	{
		if (!ParentObject.IsCombatObject())
		{
			FailToParent();
			return;
		}
		ParentObject.FireEvent("AICombatStart");
		if (Target == null)
		{
			Think("I don't have a target any more!");
			FailToParent();
			return;
		}
		if (Target.Render == null || !Target.Render.Visible)
		{
			Think("I can't see my target any more!");
			FailToParent();
			return;
		}
		if (Target.IsNowhere())
		{
			Think("My target has been destroyed!");
			FailToParent();
			return;
		}
		if (Target == ParentBrain.PartyLeader)
		{
			Think("I shouldn't kill my party leader.");
			FailToParent();
			return;
		}
		if (ParentBrain.IsPlayerLed() && Target.IsPlayerLed())
		{
			Think("I shouldn't kill other members of the player's party.");
			FailToParent();
			return;
		}
		if (!Directed && !Target.IsRegardedWithHostilityBy(ParentObject))
		{
			Think("I'm not hostile to my target anymore.");
			FailToParent();
			return;
		}
		Cell currentCell = ParentObject.CurrentCell;
		Cell currentCell2 = Target.GetCurrentCell();
		if (currentCell2 == null || currentCell == null)
		{
			return;
		}
		if (currentCell2 == currentCell)
		{
			Think("I should get some distance from my target.");
			PushChildGoal(new Flee(Target, 1));
			return;
		}
		GameObject gameObject = currentCell2.GetCombatTarget(ParentObject, AllowInanimate: !Target.IsCombat, IgnoreFlight: ParentObject.IsFlying);
		if (gameObject != null && gameObject != Target && !ParentBrain.IsHostileTowards(gameObject))
		{
			Think("I shouldn't try to kill something if by doing so I would hit somebody else I don't want to.");
			FailToParent();
			return;
		}
		int num = currentCell.DistanceToRespectStairs(currentCell2);
		if (currentCell2.ParentZone == null || currentCell2.ParentZone.ZoneID == null || num > 80)
		{
			LastSeen++;
		}
		if (LastSeen > 5)
		{
			Think("I can't find my target...");
			Target = null;
			FailToParent();
		}
		else if (Target.IsInvalid())
		{
			Think("My target is dead!");
			Target = null;
			FailToParent();
		}
		else if (ParentObject.IsAflame() && (ParentObject.isDamaged(50, inclusive: true) || ParentObject.Physics.Temperature >= ParentObject.Physics.FlameTemperature * 2) && ++ExtinguishSelfTries < 5)
		{
			Think("I'm on fire!");
			PushChildGoal(new ExtinguishSelf());
		}
		else
		{
			if (!ParentObject.FireEvent("AIBeginKill"))
			{
				return;
			}
			if (currentCell2 != null && num == 1)
			{
				if (!ParentObject.FireEvent("AIAttackMelee"))
				{
					if (!ParentObject.FireEvent("AICanAttackMelee"))
					{
						Target = null;
						FailToParent();
					}
					return;
				}
				bool isFlying = Target.IsFlying;
				bool isFlying2 = ParentObject.IsFlying;
				if ((isFlying && !isFlying2) || !ParentObject.PhaseMatches(Target))
				{
					GameObject gameObject2 = FindTargetOfOpportunity(ParentObject.CurrentCell);
					if (gameObject2 != null && ParentObject.FireEvent(Event.New("AISwitchToTargetOfOpportunity", "Target", Target, "altTarget", gameObject2)))
					{
						PushChildGoal(new Kill(gameObject2));
					}
					else
					{
						PushChildGoal(new Flee(Target, 2));
					}
					return;
				}
				List<string> list = tryMeleeOrder;
				string tagOrStringProperty = ParentObject.GetTagOrStringProperty("customMeleeOrder");
				if (tagOrStringProperty != null)
				{
					list = tagOrStringProperty.CachedCommaExpansion();
				}
				else
				{
					list.ShuffleInPlace();
				}
				Think("I'm going to melee my target in melee!");
				foreach (string item in list)
				{
					if (Target == null)
					{
						Think("I lost my target.");
						FailToParent();
						return;
					}
					switch (item)
					{
					case "defensiveItems":
					{
						Think("I'm going to try my defensive items.");
						int num2 = ParentObject.Stat("Energy");
						if (TryDefensiveItems())
						{
							if (ParentObject.Stat("Energy") == num2)
							{
								ParentObject.UseEnergy(1000);
							}
							return;
						}
						break;
					}
					case "missile":
						if ((ParentBrain.PointBlankRange || !Target.FlightMatches(ParentObject)) && TryMissileWeapon())
						{
							return;
						}
						break;
					case "abilities":
						if (TryAbilities())
						{
							return;
						}
						break;
					case "defensiveAbilities":
						if (TryDefensiveAbilities())
						{
							return;
						}
						break;
					case "items":
						if (TryItems())
						{
							return;
						}
						break;
					}
				}
				if (isFlying && !isFlying2)
				{
					if (ParentBrain.PartyLeader == null)
					{
						Target = null;
					}
					FailToParent();
					return;
				}
				Cell cell = currentCell2;
				if (ParentObject.IsFlying && !Target.IsFlying)
				{
					if (ParentObject.IsActivatedAbilityAIUsable(Flight.SWOOP_ATTACK_COMMAND_NAME))
					{
						Combat.SwoopAttack(ParentObject, currentCell.GetDirectionFromCell(cell));
					}
				}
				else
				{
					Combat.AttackCell(ParentObject, cell);
				}
				return;
			}
			if (!ParentObject.FireEvent("AIAttackRange"))
			{
				if (!ParentObject.FireEvent("AICanAttackRange"))
				{
					Target = null;
					FailToParent();
				}
				return;
			}
			Think("I'm going to try attacking my target at range!");
			List<string> list2 = tryMissileOrder;
			string tagOrStringProperty2 = ParentObject.GetTagOrStringProperty("customMissileOrder");
			if (tagOrStringProperty2 != null)
			{
				list2 = tagOrStringProperty2.CachedCommaExpansion();
			}
			else
			{
				list2.ShuffleInPlace();
			}
			foreach (string item2 in list2)
			{
				switch (item2)
				{
				case "defensiveItems":
				{
					int num3 = ParentObject.Stat("Energy");
					if (TryDefensiveItems())
					{
						if (ParentObject.Stat("Energy") == num3)
						{
							ParentObject.UseEnergy(1000);
						}
						return;
					}
					break;
				}
				case "defensiveAbilities":
					if (TryDefensiveAbilities())
					{
						return;
					}
					break;
				case "abilities":
					if (TryAbilities())
					{
						return;
					}
					break;
				case "missile":
					if (TryMissileReload() || TryMissileWeapon())
					{
						return;
					}
					break;
				case "thrown":
					if (TryThrownWeapon())
					{
						return;
					}
					break;
				case "items":
					if (TryItems())
					{
						return;
					}
					break;
				}
			}
			if (!ParentObject.FireEvent("AICantAttackRange"))
			{
				return;
			}
			if (ParentObject.IsMobile())
			{
				LastAttacked++;
				if (LastAttacked > 8 && Math.Max(1, 6 - ParentObject.StatMod("Intelligence") * 3 / 2).in100())
				{
					Think("I'm going to stop pursuing my target.");
					ParentBrain.Goals.Clear();
					if (!ParentObject.IsPlayerControlled())
					{
						ParentBrain.Hibernating = true;
						ParentBrain.PushGoal(new Wait(2));
					}
				}
				else if (num <= 1)
				{
					ParentObject.UseEnergy(1000);
					Think("I'm close enough to my target.");
				}
				else if (ParentObject.FireEvent(Event.New("AIMovingTowardsTarget", "Target", Target)))
				{
					Think("I'm going to move towards my target.");
					bool pathGlobal = Target.IsPlayer();
					Cell currentCell3 = ParentObject.CurrentCell;
					Cell currentCell4 = Target.CurrentCell;
					Zone parentZone = currentCell3.ParentZone;
					Zone parentZone2 = currentCell4.ParentZone;
					if (parentZone == null || parentZone2 == null)
					{
						Think("Incoherent zone situation trying to reach target.");
						Target = null;
						FailToParent();
						return;
					}
					if (parentZone2.IsWorldMap())
					{
						Think("Target's on the world map, can't follow!");
						Target = null;
						FailToParent();
						return;
					}
					if (parentZone2 != currentCell3.ParentZone && ParentObject.HasTagOrProperty("StaysOnZLevel") && parentZone2 != null && currentCell3.ParentZone != null && parentZone2.Z != currentCell3.ParentZone.Z)
					{
						Think("Target's on another Z level, can't follow!");
						Target = null;
						FailToParent();
						return;
					}
					if (parentZone.ZoneID == null || parentZone2.ZoneID == null)
					{
						Target = null;
						FailToParent();
						return;
					}
					FindPath findPath = new FindPath(currentCell3, currentCell4, pathGlobal, PathUnlimited: true, ParentObject, 95);
					if (findPath.Usable)
					{
						Think("I found a step to take toward my target, I'm going to try it.");
						MoveTries = 0;
						if (Target != null && Target.IsPlayerControlled())
						{
							parentZone.MarkActive(parentZone2);
						}
						PushChildGoal(new Step(findPath.Directions[0], careful: false, overridesCombat: false, wandering: false, juggernaut: false, Target));
						return;
					}
					GameObject gameObject3 = FindTargetOfOpportunity(currentCell3, currentCell4);
					if (gameObject3 != null && ParentObject.FireEvent(Event.New("AISwitchToTargetOfOpportunity", "Target", Target, "altTarget", gameObject3)))
					{
						if (gameObject3.IsPlayerControlled())
						{
							parentZone.MarkActive(parentZone2);
						}
						PushChildGoal(new Kill(gameObject3));
						return;
					}
					if (ParentBrain.LimitToAquatic())
					{
						PushChildGoal(new Flee(Target, 10));
						return;
					}
					Think("I can't find a path.");
					if (!ParentObject.FireEvent(Event.New("AIFailCombatPathfind", "Target", Target)))
					{
						FailToParent();
						return;
					}
					switch (MoveTries++)
					{
					case 1:
						ParentBrain.FlushLocalNavigationCaches();
						break;
					case 2:
						ParentBrain.FlushZoneNavigationCaches();
						break;
					case 3:
						ParentBrain.FlushZoneNavigationCaches();
						PushChildGoal(new Wait(Stat.Random(1, 3)));
						break;
					default:
						FailToParent();
						break;
					case 0:
						break;
					}
				}
				else
				{
					ParentObject.UseEnergy(1000);
					Think("I'm not allowed to move.");
				}
			}
			else
			{
				ParentObject.UseEnergy(1000);
				Think("My target is too far and I'm immobile.");
				FailToParent();
			}
		}
	}
}
