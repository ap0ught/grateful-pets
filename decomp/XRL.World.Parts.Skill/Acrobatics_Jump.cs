using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Acrobatics_Jump : BaseSkill
{
	public const int DEFAULT_MAX_DISTANCE = 2;

	public static readonly string COMMAND_NAME = "CommandAcrobaticsJump";

	public int MaxDistance = 2;

	public Guid ActivatedAbilityID = Guid.Empty;

	public string ActiveAbilityName;

	public string ActiveAbilityDescription;

	public int ActiveRange;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterAddSkillEvent.ID && ID != AIGetMovementAbilityListEvent.ID && ID != AIGetOffensiveAbilityListEvent.ID && ID != AIGetRetreatAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != GetMovementCapabilitiesEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		GetJumpingBehaviorEvent.Retrieve(ParentObject, out var _, out var MinimumRange, out var _, out var _, out var _, out var CanJumpOverCreatures, stats);
		stats.Set("CanJumpOverCreatures", CanJumpOverCreatures ? "true" : "false");
		stats.Set("MinimumRange", MinimumRange, MinimumRange != 2, MinimumRange - 2);
		int num = stats.CollectBonusModifiers("Range", 2, "Range");
		stats.Set("IsRangeIncreased", (num > 2) ? "true" : "false");
		stats.Set("Range", num, num != 2, num - 2);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetBaseCooldown());
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		E.Add("Jump to Square", COMMAND_NAME, 7000, MyActivatedAbility(ActivatedAbilityID));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetMovementAbilityListEvent E)
	{
		if (GameObject.Validate(E.Actor) && E.TargetCell != null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Actor.HasBodyPart("Feet"))
		{
			GetJumpingBehaviorEvent.Retrieve(E.Actor, out var RangeModifier, out var minimumRange, out var _, out var Verb, out var _, out var canJumpOverCreatures);
			int range = GetBaseRange() + RangeModifier;
			if (E.StandoffDistance > 0)
			{
				Cell cell = null;
				int num = 1000;
				int num2 = 1000;
				foreach (Cell item in E.TargetCell.GetLocalAdjacentCellsAtRadius(E.StandoffDistance))
				{
					if (!ValidJump(item, E.Actor, range, minimumRange) || !CheckPath(E.Actor, item, canJumpOverCreatures, CanLandOnCreature: false, Silent: true, Verb))
					{
						continue;
					}
					int navigationWeightFor = item.GetNavigationWeightFor(E.Actor);
					if (navigationWeightFor < 10)
					{
						int num3 = E.Actor.DistanceTo(item);
						if (num3 + navigationWeightFor < num + num2)
						{
							cell = item;
							num = num3;
							num2 = navigationWeightFor;
						}
					}
				}
				if (cell != null && Stat.Random(1, 20) < num)
				{
					E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, cell);
				}
			}
			else if (E.Distance < minimumRange || !CheckPath(E.Actor, E.TargetCell, canJumpOverCreatures, CanLandOnCreature: false, Silent: true) || E.TargetCell.GetNavigationWeightFor(E.Actor) >= 10)
			{
				Cell randomLocalAdjacentCell = E.TargetCell.GetRandomLocalAdjacentCell((Cell c) => ValidJump(c, E.Actor, range, minimumRange) && CheckPath(E.Actor, c, canJumpOverCreatures, CanLandOnCreature: false, Silent: true) && c.GetNavigationWeightFor(E.Actor) < 10);
				if (randomLocalAdjacentCell != null && Stat.Random(1, 20) < E.Actor.DistanceTo(randomLocalAdjacentCell))
				{
					E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, randomLocalAdjacentCell);
				}
			}
			else if (ValidJump(E.Distance, E.Actor, range, minimumRange) && Stat.Random(1, 20) < E.Actor.DistanceTo(E.TargetCell))
			{
				E.Add(COMMAND_NAME);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetRetreatAbilityListEvent E)
	{
		if (GameObject.Validate(E.Actor) && E.Actor.HasBodyPart("Feet") && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			GetJumpingBehaviorEvent.Retrieve(E.Actor, out var RangeModifier, out var minimumRange, out var _, out var _, out var _, out var canJumpOverCreatures);
			int range = GetBaseRange() + RangeModifier;
			if (E.TargetCell == null)
			{
				Cell cell = E.AvoidCell ?? E.Target?.CurrentCell;
				if (cell != null)
				{
					Cell cell2 = null;
					int num = 0;
					int num2 = 0;
					foreach (Cell localAdjacentCell in E.Actor.CurrentCell.GetLocalAdjacentCells(range))
					{
						if (!ValidJump(localAdjacentCell, E.Actor, range, minimumRange) || !CheckPath(E.Actor, localAdjacentCell, canJumpOverCreatures, CanLandOnCreature: false, Silent: true))
						{
							continue;
						}
						int navigationWeightFor = localAdjacentCell.GetNavigationWeightFor(E.Actor);
						if (navigationWeightFor < 10)
						{
							int num3 = cell.DistanceTo(localAdjacentCell);
							if (num3 - navigationWeightFor > num - num2)
							{
								cell2 = localAdjacentCell;
								num = num3;
								num2 = navigationWeightFor;
							}
						}
					}
					if (cell2 != null)
					{
						E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, cell2);
					}
				}
				else
				{
					Zone currentZone = E.Actor.CurrentZone;
					Cell cell3 = ((currentZone != null) ? currentZone.GetEmptyReachableCells().GetRandomElement() : null);
					if (cell3 != null && ValidJump(cell3, E.Actor, range, minimumRange) && CheckPath(E.Actor, cell3, canJumpOverCreatures, CanLandOnCreature: false, Silent: true) && cell3.GetNavigationWeightFor(E.Actor) < 10)
					{
						E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, cell3);
					}
				}
			}
			else if (E.StandoffDistance > 0)
			{
				Cell randomLocalAdjacentCellAtRadius = E.TargetCell.GetRandomLocalAdjacentCellAtRadius(E.StandoffDistance, (Cell c) => ValidJump(c, E.Actor, range, minimumRange) && CheckPath(E.Actor, c, canJumpOverCreatures, CanLandOnCreature: false, Silent: true) && c.GetNavigationWeightFor(E.Actor) < 10);
				if (randomLocalAdjacentCellAtRadius != null)
				{
					E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, randomLocalAdjacentCellAtRadius);
				}
			}
			else if (E.Distance < minimumRange || !CheckPath(E.Actor, E.TargetCell, canJumpOverCreatures, CanLandOnCreature: false, Silent: true) || E.TargetCell.GetNavigationWeightFor(E.Actor) > 10)
			{
				Cell randomLocalAdjacentCell = E.TargetCell.GetRandomLocalAdjacentCell((Cell c) => ValidJump(c, E.Actor, range, minimumRange) && CheckPath(E.Actor, c, canJumpOverCreatures, CanLandOnCreature: false, Silent: true) && c.GetNavigationWeightFor(E.Actor) < 10);
				if (randomLocalAdjacentCell != null)
				{
					E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, randomLocalAdjacentCell);
				}
			}
			else if (ValidJump(E.Distance, E.Actor, range, minimumRange))
			{
				E.Add(COMMAND_NAME);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Actor.HasBodyPart("Feet") && Stat.Random(1, 10) < E.Distance)
		{
			GetJumpingBehaviorEvent.Retrieve(E.Actor, out var RangeModifier, out var MinimumRange, out var _, out var _, out var _, out var _);
			if (E.Distance > MinimumRange + 1)
			{
				int num = GetBaseRange() + RangeModifier;
				if (E.Distance <= num + 1)
				{
					Cell cell = FindCellToApproachTarget(E.Actor, E.Target, MinimumRange, num);
					if (cell != null)
					{
						E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, null, cell);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && Jump(ParentObject, 0, E.TargetCell))
		{
			ParentObject.UseEnergy(1000, "Movement Jump");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetBaseCooldown());
		}
		return base.HandleEvent(E);
	}

	public override bool BeforeAddSkill(BeforeAddSkillEvent E)
	{
		if (E.Actor.IsPlayer() && E.Source != null && E.Source.GetClass() == "frog")
		{
			Achievement.LEARN_JUMP.Unlock();
		}
		return base.BeforeAddSkill(E);
	}

	public override bool HandleEvent(AfterAddSkillEvent E)
	{
		if (E.Skill == this)
		{
			SyncAbility();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}

	public static bool CheckPath(GameObject Actor, Cell TargetCell, out GameObject Over, out List<Point> Path, bool Silent = false, bool CanJumpOverCreatures = false, bool CanLandOnCreature = false, string Verb = "jump")
	{
		Over = null;
		Path = null;
		Cell cell = Actor.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		Zone parentZone = cell.ParentZone;
		Path = Zone.Line(cell.X, cell.Y, TargetCell.X, TargetCell.Y);
		int num = 0;
		foreach (Point item in Path)
		{
			Cell cell2 = parentZone.GetCell(item.X, item.Y);
			if (cell2 != cell)
			{
				int i = 0;
				for (int count = cell2.Objects.Count; i < count; i++)
				{
					GameObject gameObject = cell2.Objects[i];
					if (num == Path.Count - 1)
					{
						if (gameObject.ConsiderSolidFor(Actor) || (!CanLandOnCreature && gameObject.IsCombatObject()))
						{
							if (!Silent)
							{
								Actor.Fail("You can only " + Verb + " into empty spaces.");
							}
							return false;
						}
					}
					else if (((gameObject.ConsiderSolidFor(Actor) && !gameObject.HasPropertyOrTag("Flyover")) || (!CanJumpOverCreatures && gameObject.IsCombatObject())) && gameObject.PhaseAndFlightMatches(Actor))
					{
						if (!Silent)
						{
							Actor.Fail("You can't " + Verb + " over " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
						}
						return false;
					}
					if (gameObject.IsReal && (Over == null || (Over.IsScenery && !gameObject.IsScenery) || (CanJumpOverCreatures && !Over.IsCreature && gameObject.IsCreature)))
					{
						Over = gameObject;
					}
				}
			}
			num++;
		}
		return true;
	}

	public static bool CheckPath(GameObject Actor, Cell TargetCell, out GameObject Over, bool Silent = false, bool CanJumpOverCreatures = false, bool CanLandOnCreature = false, string Verb = "jump")
	{
		List<Point> Path;
		return CheckPath(Actor, TargetCell, out Over, out Path, Silent, CanJumpOverCreatures, CanLandOnCreature, Verb);
	}

	public static bool CheckPath(GameObject Actor, Cell TargetCell, out List<Point> Path, bool Silent = false, bool CanJumpOverCreatures = false, bool CanLandOnCreature = false, string Verb = "jump")
	{
		GameObject Over;
		return CheckPath(Actor, TargetCell, out Over, out Path, Silent, CanJumpOverCreatures, CanLandOnCreature, Verb);
	}

	public static bool CheckPath(GameObject Actor, Cell TargetCell, bool CanJumpOverCreatures = false, bool CanLandOnCreature = false, bool Silent = false, string Verb = "jump")
	{
		GameObject Over;
		List<Point> Path;
		return CheckPath(Actor, TargetCell, out Over, out Path, Silent, CanJumpOverCreatures, CanLandOnCreature, Verb);
	}

	public static Cell FindCellToApproachTarget(GameObject Actor, GameObject Target, int MinimumRange, int MaximumRange, bool CanJumpOverCreatures = false, bool CanLandOnCreature = false, string Verb = "jump")
	{
		if (Target == null || Target.IsInvalid())
		{
			return null;
		}
		Cell result = null;
		int num = 1000;
		int num2 = 1000;
		foreach (Cell localAdjacentCell in Target.CurrentCell.GetLocalAdjacentCells())
		{
			int navigationWeightFor = localAdjacentCell.GetNavigationWeightFor(Actor);
			if (navigationWeightFor <= 20)
			{
				int num3 = Actor.DistanceTo(localAdjacentCell);
				if (num3 >= MinimumRange && num3 <= MaximumRange && num3 + navigationWeightFor < num + num2 && localAdjacentCell.IsEmptyOfSolidFor(Actor) && !localAdjacentCell.HasCombatObject(NoBrainOnly: true) && CheckPath(Actor, localAdjacentCell, CanJumpOverCreatures, CanLandOnCreature, Silent: true, Verb))
				{
					result = localAdjacentCell;
					num = num3;
					num2 = navigationWeightFor;
				}
			}
		}
		return result;
	}

	public static int GetBaseRange(GameObject Actor)
	{
		return Actor.GetPart<Acrobatics_Jump>()?.GetBaseRange() ?? (2 + Actor.GetIntProperty("JumpRangeModifier"));
	}

	public int GetBaseRange()
	{
		return MaxDistance + ParentObject.GetIntProperty("JumpRangeModifier");
	}

	public static int GetActiveRange(GameObject Actor, out int MinimumRange, int RangeModifier = 0)
	{
		GetJumpingBehaviorEvent.Retrieve(Actor, out var RangeModifier2, out MinimumRange, out var _, out var _, out var _, out var _);
		return GetBaseRange(Actor) + RangeModifier + RangeModifier2;
	}

	public static int GetActiveRange(GameObject Actor, int RangeModifier = 0)
	{
		GetJumpingBehaviorEvent.Retrieve(Actor, out var RangeModifier2, out var _, out var _, out var _, out var _, out var _);
		return GetBaseRange(Actor) + RangeModifier + RangeModifier2;
	}

	public int GetActiveRange(int RangeModifier = 0)
	{
		GetJumpingBehaviorEvent.Retrieve(ParentObject, out var RangeModifier2, out var _, out var _, out var _, out var _, out var _);
		return GetBaseRange() + RangeModifier + RangeModifier2;
	}

	public static bool Jump(GameObject Actor, int RangeModifier = 0, Cell TargetCell = null, string SourceKey = null)
	{
		bool flag = TargetCell != null;
		GetJumpingBehaviorEvent.Retrieve(Actor, out var RangeModifier2, out var MinimumRange, out var AbilityName, out var Verb, out var ProviderKey, out var CanJumpOverCreatures);
		int num = GetBaseRange(Actor) + RangeModifier + RangeModifier2;
		if (Actor.OnWorldMap())
		{
			Actor.Fail("You cannot " + Verb + " on the world map.");
			return false;
		}
		if (Actor.IsFlying)
		{
			Actor.Fail("You cannot " + Verb + " while flying.");
			return false;
		}
		if (!Actor.CanChangeMovementMode("Jumping", ShowMessage: true))
		{
			return false;
		}
		if (!Actor.CanChangeBodyPosition("Jumping", ShowMessage: true))
		{
			return false;
		}
		if (!Actor.HasBodyPart("Feet"))
		{
			Actor.Fail("You cannot " + Verb + " without feet.");
			return false;
		}
		SoundManager.PreloadClipSet("Sounds/Abilities/sfx_ability_jump");
		Cell cell = Actor.CurrentCell;
		GameObject Over;
		List<Point> Path;
		while (true)
		{
			if (TargetCell == null)
			{
				TargetCell = ((!Actor.IsPlayer()) ? FindCellToApproachTarget(Actor, Actor.Target, MinimumRange, num, CanJumpOverCreatures, CanLandOnCreature: false, Verb) : PickTarget.ShowPicker(PickTarget.PickStyle.Line, num, num, cell.X, cell.Y, Locked: false, AllowVis.OnlyVisible, null, null, Actor, null, "Jump where?", EnforceRange: false, UseTarget: false));
				if (TargetCell == null)
				{
					return false;
				}
			}
			int num2 = Actor.DistanceTo(TargetCell);
			if (num2 > num)
			{
				if (!flag)
				{
					Actor.Fail("You may not " + Verb + " more than " + Grammar.Cardinal(num) + " " + ((num == 1) ? "square" : "squares") + "!");
					if (Actor.IsPlayer())
					{
						TargetCell = null;
						continue;
					}
				}
				return false;
			}
			if (num2 < MinimumRange)
			{
				if (!flag)
				{
					Actor.Fail("You must " + Verb + " at least " + Grammar.Cardinal(MinimumRange) + " " + ((MinimumRange == 1) ? "square" : "squares") + "!");
					if (Actor.IsPlayer())
					{
						TargetCell = null;
						continue;
					}
				}
				return false;
			}
			Cell targetCell = TargetCell;
			bool canJumpOverCreatures = CanJumpOverCreatures;
			string verb = Verb;
			if (CheckPath(Actor, targetCell, out Over, out Path, flag, canJumpOverCreatures, CanLandOnCreature: false, verb))
			{
				break;
			}
			if (!flag && Actor.IsPlayer())
			{
				TargetCell = null;
				continue;
			}
			return false;
		}
		Actor?.PlayWorldSound("Sounds/Abilities/sfx_ability_jump");
		Actor.MovementModeChanged("Jumping");
		Actor.BodyPositionChanged("Jumping");
		PlayAnimation(Actor, TargetCell);
		if (Over != null)
		{
			IComponent<GameObject>.XDidYToZ(Actor, Verb, "over", Over, null, ".");
		}
		else
		{
			IComponent<GameObject>.XDidY(Actor, Verb, null, ".");
		}
		if (Actor.DirectMoveTo(TargetCell, 0, Forced: false, IgnoreCombat: true, IgnoreGravity: true))
		{
			JumpedEvent.Send(Actor, cell, TargetCell, Path, num, AbilityName, ProviderKey, SourceKey);
		}
		Actor.Gravitate();
		Land(cell, TargetCell);
		return true;
	}

	public static void PlayAnimation(GameObject Actor, Cell TargetCell)
	{
		if (Options.UseOverlayCombatEffects && TargetCell.IsVisible())
		{
			CombatJuice.BlockUntilFinished(CombatJuice.Jump(Actor, Actor.CurrentCell.Location, TargetCell.Location, Stat.Random(0.3f, 0.4f), Stat.Random(0.4f, 0.5f), 0.75f, Actor.IsPlayer()), new GameObject[1] { Actor });
		}
	}

	public static void Leap(Cell From, Cell To, int Count = 3, int Life = 12)
	{
		if (From.IsVisible())
		{
			float angle = (float)Math.Atan2(From.X - To.X, From.Y - To.Y);
			Leap(From.X, From.Y, angle, Count, Life);
		}
	}

	public static void Leap(int X, int Y, float Angle, int Count = 3, int Life = 12)
	{
		for (int i = 0; i < Count; i++)
		{
			float f = (float)Stat.RandomCosmetic(-90, 90) * (MathF.PI / 180f) + Angle;
			float xDel = Mathf.Sin(f) / (float)Life;
			float yDel = Mathf.Cos(f) / (float)Life;
			XRLCore.ParticleManager.Add("&y±", X, Y, xDel, yDel, Life, 0f, 0f, 0L);
		}
	}

	public static void Land(Cell From, Cell To, int Count = 4, int Life = 8)
	{
		if (To.IsVisible())
		{
			float angle = (float)Math.Atan2(To.X - From.X, To.Y - From.Y);
			Land(To.X, To.Y, angle, Count, Life);
		}
	}

	public static void Land(int X, int Y, float Angle, int Count = 4, int Life = 8)
	{
		for (int i = 0; i < Count; i++)
		{
			float f = (float)Stat.RandomCosmetic(-75, 75) * (MathF.PI / 180f) + Angle;
			float xDel = Mathf.Sin(f) / ((float)Life / 2f);
			float yDel = Mathf.Cos(f) / ((float)Life / 2f);
			string text = ((Stat.RandomCosmetic(1, 4) <= 3) ? "&y." : "&y±");
			XRLCore.ParticleManager.Add(text, X, Y, xDel, yDel, Life, 0f, 0f, 0L);
		}
	}

	private bool ValidJump(Cell Cell, GameObject Actor = null, int? Range = null, int? MinimumRange = null)
	{
		return ValidJump((Actor ?? ParentObject).DistanceTo(Cell), Actor, Range, MinimumRange);
	}

	private bool ValidJump(int Distance, GameObject Actor = null, int? Range = null, int? MinimumRange = null)
	{
		if (Distance >= (MinimumRange ?? 2))
		{
			return Distance <= (Range ?? GetBaseRange());
		}
		return false;
	}

	public void SyncAbility(bool Silent = false)
	{
		GetJumpingBehaviorEvent.Retrieve(ParentObject, out var RangeModifier, out var _, out var AbilityName, out var _, out var _, out var _);
		int num = GetBaseRange() + RangeModifier;
		if (ActivatedAbilityID == Guid.Empty || AbilityName != ActiveAbilityName || num != ActiveRange)
		{
			bool flag = ActiveAbilityName == AbilityName;
			RemoveMyActivatedAbility(ref ActivatedAbilityID);
			ActiveAbilityName = AbilityName;
			ActiveRange = num;
			if (!AbilityName.IsNullOrEmpty())
			{
				ActivatedAbilityID = AddMyActivatedAbility(AbilityName, COMMAND_NAME, "Skills", null, "\u0017", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent | flag);
			}
		}
	}

	public static void SyncAbility(GameObject Actor, bool Silent = false)
	{
		Actor.GetPart<Acrobatics_Jump>()?.SyncAbility(Silent);
	}

	public int GetBaseCooldown()
	{
		return 100;
	}
}
