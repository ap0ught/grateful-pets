using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Slam : BaseSkill
{
	public const int MAX_STUN_DURATION = 4;

	public static readonly int COOLDOWN = 50;

	public Guid ActivatedAbilityID = Guid.Empty;

	public Cudgel_Slam()
	{
	}

	public Cudgel_Slam(GameObject ParentObject)
		: this()
	{
		this.ParentObject = ParentObject;
	}

	public int GetSlamDistance()
	{
		return 3 + ParentObject.GetIntProperty("SlamDistanceBonus");
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Distance", GetSlamDistance());
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (GameObject.Validate(E.Target) && E.Target != E.Actor && E.Distance == 1 && (E.Actor.PhaseAndFlightMatches(E.Target) || (E.Actor.Stat("Intelligence") < 30 && E.Actor.FlightMatches(E.Target) && !E.Actor.HasIntProperty("HasTriedToSlamOutOfPhase" + E.Target.ID))) && IsCudgelEquipped() && E.Actor.CanMoveExtremities() && !E.Actor.HasEffect<Enclosed>() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandCudgelSlam");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandCudgelSlam");
		base.Register(Object, Registrar);
	}

	public GameObject GetEquippedCudgel()
	{
		return ParentObject.Body?.GetWeaponOfType("Cudgel", NeedPrimary: false, PreferPrimary: true);
	}

	public bool IsCudgelEquipped()
	{
		return GetEquippedCudgel() != null;
	}

	public static bool IsSlammable(GameObject Object)
	{
		if (Object.IsWall())
		{
			return true;
		}
		if (Object.IsDoor())
		{
			return true;
		}
		if (Object.IsCombatObject())
		{
			return true;
		}
		return false;
	}

	public static bool IsSlammableAndNotOpenDoor(GameObject Object)
	{
		if (Object.IsWall())
		{
			return true;
		}
		if (Object.IsDoor() && Object.TryGetPart<Door>(out var Part) && !Part.Open)
		{
			return true;
		}
		if (Object.IsCombatObject())
		{
			return true;
		}
		return false;
	}

	public static bool Cast(GameObject attacker, Cudgel_Slam skill = null, string slamDir = null, GameObject target = null, bool requireWeapon = true, int presetSlamPower = int.MinValue, string impactDamageIncrement = null)
	{
		Cell cell = attacker.GetCurrentCell();
		if (cell == null)
		{
			return false;
		}
		if (skill == null)
		{
			skill = new Cudgel_Slam(attacker);
		}
		Cell cellFromDirection;
		string direction;
		if (target != null)
		{
			cellFromDirection = target.GetCurrentCell();
			direction = slamDir ?? cell.GetDirectionFromCell(cellFromDirection, NullIfSame: true) ?? Directions.GetRandomDirection();
		}
		else
		{
			direction = slamDir ?? skill.PickDirectionS(ForAttack: true, null, "Slam what?");
			cellFromDirection = cell.GetCellFromDirection(direction);
			if (cellFromDirection == null)
			{
				return false;
			}
			Engulfed effect = attacker.GetEffect<Engulfed>();
			if (effect == null || effect.EngulfedBy == null)
			{
				target = ((cellFromDirection != attacker.CurrentCell) ? cellFromDirection.GetCombatTarget(attacker, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: true, 0, null, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: false, IsSlammable) : attacker);
			}
			else
			{
				target = effect.EngulfedBy;
				cellFromDirection = target.CurrentCell;
			}
			if (target == null)
			{
				if (attacker.IsPlayer())
				{
					if ((target = cellFromDirection.GetCombatTarget(attacker)) != null)
					{
						attacker.ShowFailure("You cannot slam " + target.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
					}
					else if ((target = cellFromDirection.GetCombatTarget(attacker, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: false, IsSlammable)) != null)
					{
						attacker.ShowFailure("You cannot reach " + target.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " to slam " + target.them + ".");
					}
					else
					{
						attacker.ShowFailure("There's nothing there to slam.");
					}
				}
				return false;
			}
		}
		if (!attacker.IsPlayer() && target == attacker)
		{
			return false;
		}
		GameObject gameObject = skill.GetEquippedCudgel() ?? attacker.GetPrimaryWeapon();
		if ((gameObject == null) & requireWeapon)
		{
			attacker.ShowFailure("You have no weapon!");
			return false;
		}
		if (!attacker.PhaseMatches(target))
		{
			IComponent<GameObject>.XDidYToZ(attacker, "try", "to slam", target, "but " + attacker.poss("attack") + " passes through " + target.them + " harmlessly", null, null, null, null, attacker);
			attacker.UseEnergy(1000, "Skill Cudgel Slam");
			return false;
		}
		int parameter = 1;
		Event obj = new Event("GetslamMultiplier", "Multiplier", parameter, "Weapon", gameObject);
		attacker.FireEvent(obj);
		gameObject?.FireEvent(obj);
		parameter = obj.GetIntParameter("Multiplier");
		int SlamPower = attacker.StatMod("Strength") * 5 * parameter;
		if (presetSlamPower != int.MinValue)
		{
			SlamPower = presetSlamPower;
		}
		int slamWeightLimit = 2000 * parameter;
		if (target == attacker)
		{
			if (attacker.IsPlayer())
			{
				if (Popup.ShowYesNo("Are you sure you want to slam " + attacker.itself + "?") != DialogResult.Yes)
				{
					return false;
				}
			}
			else
			{
				MetricsManager.LogError(attacker.DebugName + " attempted to use Slam on self " + ((target == null) ? "via cell targeting" : "via explicit specification"));
			}
		}
		int num = parameter;
		if (!BeforeSlamEvent.Check(attacker, target, cellFromDirection, ref parameter, ref SlamPower))
		{
			return false;
		}
		if (parameter != num && presetSlamPower != int.MinValue)
		{
			SlamPower = SlamPower / num * parameter;
		}
		bool showJuice;
		string juiceTile;
		string juiceDetail;
		string juiceColor;
		Location2D juiceStart;
		Location2D juiceEnd;
		if (target.IsWall() && !target.IsCombatObject())
		{
			if (Stats.GetCombatAV(target) >= SlamPower)
			{
				attacker.ShowFailure("You aren't strong enough to slam through " + target.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				return false;
			}
			attacker.PlayWorldSound("Sounds/Abilities/sfx_ability_cudgel_slam");
			target.DustPuff();
			for (int i = 0; i < 5; i++)
			{
				target.ParticleText(target.Render.TileColor + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
			}
			Cudgel_Slam cudgel_Slam = skill;
			GameObject gameObject2 = target;
			GameObject colorAsBadFor = target;
			cudgel_Slam.DidXToY("slam", "through", gameObject2, null, null, null, null, attacker, colorAsBadFor);
			IComponent<GameObject>.XDidY(target, "are", "destroyed", null, null, null, null, target);
			target.Destroy();
			attacker.UseEnergy(1000, "Skill Cudgel Slam");
			if (!attacker.HasEffect<Cudgel_SmashingUp>())
			{
				skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, COOLDOWN);
			}
		}
		else if (target.IsDoor() && !target.IsCombatObject())
		{
			if (target.TryGetPart<Door>(out var Part) && Part.Open)
			{
				attacker.ShowFailure(target.Does("are") + " open.");
				return false;
			}
			if (Stats.GetCombatAV(target) >= SlamPower)
			{
				attacker.ShowFailure("You aren't strong enough to slam through " + target.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				return false;
			}
			attacker.PlayWorldSound("Sounds/Abilities/sfx_ability_cudgel_slam");
			target.DustPuff();
			for (int j = 0; j < 5; j++)
			{
				target.ParticleText(target.Render.TileColor + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
			}
			Cudgel_Slam cudgel_Slam2 = skill;
			GameObject gameObject3 = target;
			GameObject colorAsBadFor = target;
			cudgel_Slam2.DidXToY("slam", "through", gameObject3, null, null, null, null, attacker, colorAsBadFor);
			IComponent<GameObject>.XDidY(target, "are", "destroyed", null, null, null, null, target);
			target.Destroy();
			attacker.UseEnergy(1000, "Skill Cudgel Slam");
			if (!attacker.HasEffect<Cudgel_SmashingUp>())
			{
				skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, COOLDOWN);
			}
		}
		else
		{
			attacker.PlayWorldSound("Sounds/Abilities/sfx_ability_cudgel_slam");
			Cudgel_Slam cudgel_Slam3 = skill;
			GameObject gameObject4 = target;
			GameObject colorAsBadFor = target;
			cudgel_Slam3.DidXToY("attempt", "to slam into", gameObject4, null, "!", null, null, attacker, colorAsBadFor);
			Event obj2 = Event.New("BeginAttack");
			obj2.SetParameter("target", target);
			obj2.SetParameter("TargetCell", cellFromDirection);
			if (attacker.FireEvent(obj2) && cellFromDirection != null && attacker.Physics != null)
			{
				MeleeAttackResult meleeAttackResult = Combat.MeleeAttackWithWeapon(attacker, target, gameObject, attacker.Body?.FindDefaultOrEquippedItem(gameObject), "Slamming", 0, 1, 1, 0, 0, gameObject?.IsEquippedOrDefaultOfPrimary(attacker) ?? false);
				attacker.UseEnergy(1000, "Skill Cudgel Slam");
				if (!attacker.HasEffect<Cudgel_SmashingUp>())
				{
					skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, COOLDOWN);
				}
				if (meleeAttackResult.Hits > 0 || !requireWeapon)
				{
					bool flag = false;
					if (!GameObject.Validate(target))
					{
						skill.LogInEditor("Displacing corpse mode!");
						flag = true;
						if (cellFromDirection.Objects.Count == 0)
						{
							cellFromDirection.AddObject("Garbage");
						}
						target = cellFromDirection.GetObjectWithTag("Corpse");
					}
					int num2 = parameter;
					if (!SlamEvent.Check(attacker, target, cellFromDirection, ref parameter, ref SlamPower))
					{
						return true;
					}
					if (parameter != num2 && presetSlamPower != int.MinValue)
					{
						SlamPower = SlamPower / num2 * parameter;
					}
					showJuice = false;
					juiceTile = null;
					juiceDetail = null;
					juiceColor = null;
					juiceStart = Location2D.Invalid;
					juiceEnd = Location2D.Invalid;
					if (target != null)
					{
						juiceTile = target.Render.Tile;
						juiceDetail = target.Render.getDetailColor().ToString();
						juiceColor = target.Render.GetTileForegroundColor();
						juiceStart = target.CurrentCell.Location;
						juiceEnd = target.CurrentCell.Location;
						if (target.IsVisible())
						{
							showJuice = true;
							CombatJuice.cameraShake(0.1f);
						}
					}
					Dictionary<GameObject, string> dictionary = new Dictionary<GameObject, string>(8);
					int num3 = (3 + attacker.GetIntProperty("SlamDistanceBonus")) * parameter;
					Cell cell2 = cellFromDirection;
					for (int k = 0; k < num3 && skill.Slam(target, direction, num3 - k, SlamPower, slamWeightLimit, dictionary); k++)
					{
						if (GameObject.Validate(target))
						{
							cell2 = target.CurrentCell;
						}
					}
					juiceEnd = cell2.Location;
					ExecuteJuice();
					string dice = impactDamageIncrement ?? gameObject.GetPart<MeleeWeapon>().BaseDamage;
					if (dictionary.Count == 0 && target != null)
					{
						dictionary.Add(target, "s");
					}
					if (!Options.UseParticleVFX)
					{
						XRLCore.Core.RenderBaseToBuffer(ScreenBuffer.GetScrapBuffer2());
					}
					foreach (KeyValuePair<GameObject, string> item in dictionary)
					{
						GameObject key = item.Key;
						string value = item.Value;
						if (key.IsPlayer())
						{
							CombatJuice.cameraShake(0.5f);
						}
						if (flag && key == target)
						{
							continue;
						}
						_ = value.Length;
						int num4 = 0;
						int num5 = 0;
						for (int l = 0; l < value.Length; l++)
						{
							if (value[l] == 'w')
							{
								num4++;
							}
							else if (value[l] == 's')
							{
								num5++;
							}
						}
						int num6 = 0;
						for (int m = 0; m < num4; m++)
						{
							num6 += dice.RollCached();
						}
						int duration = Math.Min(4, (num5 == 1) ? 1 : (num5 + 1));
						if (num6 > 0)
						{
							string message = null;
							string deathReason = null;
							string thirdPersonDeathReason = null;
							string text = attacker.an(int.MaxValue, null, null, AsIfKnown: false, Single: true);
							string text2 = ((gameObject != null) ? (" with " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true)) : "");
							if (num4 == 0)
							{
								message = "from %o slam!";
								deathReason = "You were slammed by " + text + text2 + ".";
								thirdPersonDeathReason = key.It + " was @@slammed by " + text + text2 + ".";
							}
							else if (num4 == 1)
							{
								message = "from slamming into a wall!";
								deathReason = "You were slammed into a wall by " + text + text2 + ".";
								thirdPersonDeathReason = key.It + " was @@slammed into a wall by " + text + text2 + ".";
							}
							else if (num4 == 2)
							{
								message = "from slamming into {{W|two}} walls!";
								deathReason = "You were slammed into two walls by " + text + text2 + ".";
								thirdPersonDeathReason = key.It + " was @@slammed into two walls by " + text + text2 + ".";
							}
							else if (num4 >= 3)
							{
								string text3 = Grammar.Cardinal(num4);
								message = "from slamming into {{r|" + text3 + "}} walls!";
								deathReason = "You were slammed into " + text3 + " walls by " + text + text2 + ".";
								thirdPersonDeathReason = key.It + " was @@slammed into " + text3 + " walls by " + text + text2 + ".";
							}
							int amount = num6;
							bool accidental = key != target;
							colorAsBadFor = attacker;
							key.TakeDamage(amount, message, "Crushing", deathReason, thirdPersonDeathReason, null, colorAsBadFor, null, null, null, accidental, Environmental: false, Indirect: false, ShowUninvolved: true);
						}
						key.ApplyEffect(new Stun(duration, -1));
					}
					if (cell2 != null && cell2 != cellFromDirection)
					{
						foreach (GameObject realNonSceneryObject in cellFromDirection.GetRealNonSceneryObjects((GameObject gameObject5) => gameObject5.PhaseAndFlightMatches(attacker) && gameObject5.Weight < slamWeightLimit && gameObject5.CanBeInvoluntarilyMoved()))
						{
							realNonSceneryObject.DirectMoveTo(cell2);
						}
					}
					target?.Gravitate();
				}
			}
		}
		return true;
		void ExecuteJuice()
		{
			if ((Options.UseParticleVFX & showJuice) && juiceEnd != Location2D.Invalid && !string.IsNullOrEmpty(juiceTile) && !string.IsNullOrEmpty(juiceColor) && !string.IsNullOrEmpty(juiceDetail))
			{
				CombatJuice.playPrefabAnimation(juiceEnd, "Abilities/AbilityVFXSlam", null, juiceTile + ";" + juiceColor + ";" + juiceDetail + ";" + juiceStart.ToString() + ";" + juiceEnd.ToString(), null, async: true);
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandCudgelSlam")
		{
			if (!IsCudgelEquipped())
			{
				ParentObject.ShowFailure("You must have a cudgel equipped in order to use slam.");
				return false;
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			Enclosed effect = ParentObject.GetEffect<Enclosed>();
			if (effect != null && effect.EnclosedBy != null)
			{
				return false;
			}
			if (!Cast(ParentObject, this))
			{
				return false;
			}
		}
		else
		{
			_ = E.ID == "AttackerHit";
		}
		return base.FireEvent(E);
	}

	public bool Slam(GameObject Target, string Direction, int MaxDistance, int SlamPower, int SlamWeightLimit, Dictionary<GameObject, string> Effects)
	{
		if (Target == null)
		{
			return false;
		}
		if (MaxDistance < 0)
		{
			return false;
		}
		if (!GameObject.Validate(ref Target))
		{
			return false;
		}
		Cell cell = Target.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		int num = 0;
		GameObject gameObject = null;
		while (true)
		{
			GameObject combatTarget = cell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: false, (GameObject o) => o != Target && IsSlammableAndNotOpenDoor(o));
			if (combatTarget == null || combatTarget == gameObject)
			{
				break;
			}
			if ((combatTarget.IsWall() || combatTarget.IsDoor()) && !combatTarget.IsCombatObject())
			{
				if (gameObject == null)
				{
					if (!Effects.ContainsKey(Target))
					{
						Effects.Add(Target, "");
					}
					Effects[Target] += "w";
				}
				if (Stats.GetCombatAV(combatTarget) >= SlamPower)
				{
					return false;
				}
				combatTarget.DustPuff();
				for (int num2 = 0; num2 < 5; num2++)
				{
					combatTarget.ParticleText(combatTarget.Render.TileColor + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
				}
				IComponent<GameObject>.XDidY(combatTarget, "are", "destroyed", "!");
				combatTarget.Destroy();
			}
			gameObject = combatTarget;
		}
		cell?.Smoke();
		num++;
		cell = Target.CurrentCell.GetLocalCellFromDirection(Direction);
		if (cell == null)
		{
			return false;
		}
		gameObject = null;
		while (true)
		{
			GameObject combatTarget2 = cell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: false, IsSlammableAndNotOpenDoor);
			if (combatTarget2 == null || combatTarget2 == gameObject)
			{
				break;
			}
			if ((combatTarget2.IsWall() || combatTarget2.IsDoor()) && !combatTarget2.IsCombatObject())
			{
				if (gameObject == null)
				{
					if (!Effects.ContainsKey(Target))
					{
						Effects.Add(Target, "");
					}
					Effects[Target] += "w";
				}
				if (Stats.GetCombatAV(combatTarget2) >= SlamPower)
				{
					return false;
				}
				combatTarget2.DustPuff();
				for (int num3 = 0; num3 < 5; num3++)
				{
					combatTarget2.ParticleText(combatTarget2.Render.TileColor + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
				}
				IComponent<GameObject>.XDidY(combatTarget2, "are", "destroyed", "!");
				combatTarget2.Destroy();
			}
			gameObject = combatTarget2;
		}
		if (cell.IsEmptyOfSolidFor(Target) && Target.Weight < SlamWeightLimit && Target.CanBeInvoluntarilyMoved())
		{
			if (Target.Move(Direction, Forced: true, System: false, IgnoreGravity: true))
			{
				if (!Effects.ContainsKey(Target))
				{
					Effects.Add(Target, "");
				}
				Effects[Target] += "s";
				if (Target.CurrentCell != null)
				{
					foreach (Cell adjacentCell in Target.CurrentCell.GetAdjacentCells())
					{
						adjacentCell.FireEvent(Event.New("ObjectEnteredAdjacentCell", "Object", Target));
					}
				}
				return true;
			}
		}
		else
		{
			gameObject = null;
			while (true)
			{
				GameObject Object = cell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: false, IsSlammableAndNotOpenDoor);
				if (Object == null || Object == gameObject)
				{
					break;
				}
				gameObject = Object;
				Slam(Object, Direction, MaxDistance - 1, SlamPower, SlamWeightLimit, Effects);
				if (GameObject.Validate(ref Object))
				{
					Object.Gravitate();
				}
			}
		}
		if (cell.IsEmptyOfSolidFor(Target) && Target.Weight < SlamWeightLimit && Target.CanBeInvoluntarilyMoved() && Target.Move(Direction, Forced: true, System: false, IgnoreGravity: true))
		{
			if (!Effects.ContainsKey(Target))
			{
				Effects.Add(Target, "");
			}
			Effects[Target] += "s";
			if (Target.CurrentCell != null)
			{
				foreach (Cell adjacentCell2 in Target.CurrentCell.GetAdjacentCells())
				{
					adjacentCell2.FireEvent(Event.New("ObjectEnteredAdjacentCell", "Object", Target));
				}
			}
			return true;
		}
		return false;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Slam", "CommandCudgelSlam", "Skills", null, "-", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
