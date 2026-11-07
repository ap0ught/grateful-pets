using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Charge : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandMeleeCharge";

	public static readonly int COOLDOWN = 15;

	public Guid ActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private string ForcedMoveDirection;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != EnteredCellEvent.ID && ID != GetAttackerMeleePenetrationEvent.ID)
		{
			return ID == GetMovementCapabilitiesEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		E.Add("Charge", COMMAND_NAME, 8000, MyActivatedAbility(ActivatedAbilityID), IsAttack: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && !PerformCharge())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		GetPropertyModDescription.GetFor(ParentObject, "ChargeRangeModifier", "Range", stats);
		int num = stats.CollectBonusModifiers("Range", 0);
		stats.Set("Range", GetMinimumRange() + "-" + GetMaximumRange(), num != 0, num);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		int minimumRange = GetMinimumRange();
		int maximumRange = GetMaximumRange();
		if (E.Distance >= minimumRange && E.Distance <= maximumRange + 1 && GameObject.Validate(E.Target) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !E.Actor.IsFlying && E.Actor.CanChangeBodyPosition() && E.Actor.CanChangeMovementMode() && !E.Actor.HasEffect<Terrified>() && !E.Actor.IsOverburdened() && !E.Actor.AreViableHostilesAdjacent())
		{
			List<Cell> list = PickLine(maximumRange + 1, AllowVis.OnlyVisible, ValidChargeTarget);
			if (list != null)
			{
				int num = list.Count - 1;
				if (num >= minimumRange && num <= maximumRange)
				{
					int Nav = 268435456;
					int i = 0;
					for (int count = list.Count; i < count; i++)
					{
						Cell cell = list[i];
						GameObject combatTarget = cell.GetCombatTarget(E.Actor, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 5, null, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true);
						if (combatTarget == E.Target)
						{
							E.Add(COMMAND_NAME);
							break;
						}
						if (combatTarget != null && !E.Actor.IsHostileTowards(combatTarget))
						{
							break;
						}
						int num2 = cell.NavigationWeight(E.Actor, ref Nav);
						if (num2 > 40 && (i == count - 1 || num2 > 80))
						{
							break;
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (E.Forced && E.Type != "Teleporting")
		{
			ForcedMoveDirection = E.Direction ?? "?";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAttackerMeleePenetrationEvent E)
	{
		if (!E.Properties.IsNullOrEmpty() && E.Hand == "Primary" && E.Properties.HasDelimitedSubstring(',', "Charging") && !E.Properties.HasDelimitedSubstring(',', "DeathFromAbove"))
		{
			E.PenetrationBonus++;
			E.MaxPenetrationBonus++;
		}
		return base.HandleEvent(E);
	}

	private bool ValidChargeTarget(GameObject obj)
	{
		if (obj != null && obj.IsCombat)
		{
			return obj.FlightMatches(ParentObject);
		}
		return false;
	}

	public int GetMinimumRange()
	{
		return 2;
	}

	public int GetMaximumRange()
	{
		return 3 + ParentObject.GetIntProperty("ChargeRangeModifier");
	}

	public bool PerformCharge()
	{
		if (ParentObject.OnWorldMap())
		{
			ParentObject.Fail("You cannot charge on the world map.");
			return false;
		}
		if (ParentObject.IsFlying)
		{
			ParentObject.Fail("You cannot charge while flying.");
			return false;
		}
		if (ParentObject.IsOverburdened())
		{
			ParentObject.Fail("You cannot charge while overburdened.");
			return false;
		}
		if (!ParentObject.CanChangeBodyPosition("Charging", ShowMessage: true))
		{
			return false;
		}
		if (!ParentObject.CanChangeMovementMode("Charging", ShowMessage: true))
		{
			return false;
		}
		int minimumRange = GetMinimumRange();
		int maximumRange = GetMaximumRange();
		List<Cell> list = PickLine(maximumRange + 1, AllowVis.OnlyVisible, ValidChargeTarget, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, BlackoutStops: false, null, null, "Charge", Snap: true);
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		if (ParentObject.IsPlayer())
		{
			list.RemoveAt(0);
		}
		int num = list.Count - 1;
		if (num < minimumRange)
		{
			ParentObject.Fail("You must charge at least " + Grammar.Cardinal(minimumRange) + " " + ((minimumRange == 1) ? "space" : "spaces") + ".");
			return false;
		}
		if (num > maximumRange)
		{
			ParentObject.Fail("You can't charge more than " + Grammar.Cardinal(maximumRange) + " " + ((maximumRange == 1) ? "space" : "spaces") + ".");
			return false;
		}
		if (ParentObject.AreViableHostilesAdjacent())
		{
			ParentObject.Fail("You cannot charge while in melee combat.");
			return false;
		}
		GameObject gameObject = null;
		Cell cell = list[list.Count - 1];
		gameObject = ((!ParentObject.IsPlayer()) ? ParentObject.Target : cell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 5));
		if (gameObject == null)
		{
			if (IsPlayer())
			{
				gameObject = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
				if (gameObject != null)
				{
					ParentObject.Fail("You cannot charge a flying target.");
				}
				else
				{
					ParentObject.Fail("You must charge at a target!");
				}
			}
			return false;
		}
		string text = null;
		string text2 = null;
		string colorString = null;
		string detailColor = null;
		int num2 = 10;
		Disguised effect = ParentObject.GetEffect<Disguised>();
		if (effect != null)
		{
			if (!effect.Tile.IsNullOrEmpty() && Options.UseTiles)
			{
				text2 = effect.Tile;
				colorString = (effect.TileColor.IsNullOrEmpty() ? effect.ColorString : effect.TileColor);
				detailColor = effect.DetailColor;
			}
			else
			{
				text = effect.ColorString + effect.RenderString;
			}
		}
		else if (!ParentObject.Render.Tile.IsNullOrEmpty() && Options.UseTiles)
		{
			text2 = ParentObject.Render.Tile;
			colorString = (ParentObject.Render.TileColor.IsNullOrEmpty() ? ParentObject.Render.ColorString : ParentObject.Render.TileColor);
			detailColor = ParentObject.Render.DetailColor;
		}
		else
		{
			text = ParentObject.Render.ColorString + ParentObject.Render.RenderString;
		}
		if (Visible())
		{
			if (text2 != null)
			{
				ParentObject.TileParticleBlip(text2, colorString, detailColor, num2, IgnoreVisibility: false, ParentObject.Render.getHFlip(), ParentObject.Render.getVFlip(), 0L);
			}
			else
			{
				ParentObject.ParticleBlip(text, num2, 0L);
			}
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		Cell cell2 = ParentObject.CurrentCell;
		string item = null;
		List<string> list2 = new List<string>(maximumRange + 2);
		int i = 0;
		for (int num3 = maximumRange + 2; i < num3; i++)
		{
			if (i >= list.Count)
			{
				list2.Add(item);
				continue;
			}
			Cell cell3 = list[i];
			string directionFromCell = cell2.GetDirectionFromCell(cell3);
			list2.Add(directionFromCell);
			item = directionFromCell;
			cell2 = cell3;
		}
		bool flag6 = ParentObject.HasPart<Robot>();
		GameObject gameObject2 = null;
		int j = 0;
		int count = list2.Count;
		while (true)
		{
			if (j < count)
			{
				string text3 = list2[j];
				Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection(text3, BuiltOnly: false);
				if (cellFromDirection != null)
				{
					if (flag6)
					{
						gameObject2 = cellFromDirection.GetFirstObjectWithPropertyOrTag("RobotStop");
						if (gameObject2 != null)
						{
							goto IL_06f4;
						}
					}
					bool flag7 = cellFromDirection.Objects.Contains(gameObject);
					GameObject combatTarget = cellFromDirection.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true);
					if (combatTarget != null)
					{
						DidXToY("charge", combatTarget, null, "!", null, null, null, combatTarget.IsPlayer() ? combatTarget : null);
						if (ParentObject.DistanceTo(cellFromDirection) <= 1)
						{
							Combat.AttackCell(ParentObject, cellFromDirection, "Charging");
						}
						else
						{
							ParentObject.UseEnergy(1000, "Charging");
						}
						ParentObject.FireEvent(Event.New("ChargedTarget", "Defender", combatTarget));
						combatTarget.FireEvent(Event.New("WasCharged", "Attacker", ParentObject));
						break;
					}
					if (flag7)
					{
						flag3 = true;
					}
					else if (flag3)
					{
						flag4 = true;
						flag3 = false;
					}
					if (ParentObject.DistanceTo(gameObject) == 1)
					{
						flag = true;
					}
					else if (flag)
					{
						flag2 = true;
					}
					if (j >= maximumRange)
					{
						flag5 = true;
					}
					ForcedMoveDirection = null;
					if (ParentObject.Move(text3, Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: true, DoConfirmations: false, null, null, NearestAvailable: false, 0, "Charge"))
					{
						if (ForcedMoveDirection != null)
						{
							if (ForcedMoveDirection == "U" || ForcedMoveDirection == "D" || ForcedMoveDirection == "?")
							{
								goto IL_06f4;
							}
							if (ForcedMoveDirection != text3)
							{
								int index = j + 1;
								for (; j < count; j++)
								{
									list2[index] = ForcedMoveDirection;
								}
							}
						}
						num2 += 5;
						if (Visible())
						{
							if (text2 != null)
							{
								ParentObject.TileParticleBlip(text2, colorString, detailColor, num2, IgnoreVisibility: false, ParentObject.Render.getHFlip(), ParentObject.Render.getVFlip(), 0L);
							}
							else
							{
								ParentObject.ParticleBlip(text, num2, 0L);
							}
						}
						j++;
						continue;
					}
				}
			}
			goto IL_06f4;
			IL_06f4:
			ForcedMoveDirection = null;
			if (gameObject2 != null)
			{
				DidXToY("are", "stopped in " + ParentObject.its + " tracks by", gameObject2, null, "!", null, null, null, ParentObject);
			}
			else if (flag4)
			{
				DidXToY("charge", "right through", gameObject, null, "!", null, null, null, ParentObject);
			}
			else if (flag2)
			{
				DidXToY("charge", "right past", gameObject, null, "!", null, null, null, ParentObject);
			}
			else if (flag3 | flag | flag5)
			{
				DidXToY("charge", gameObject, ", but" + ParentObject.GetVerb("fail") + " to make contact", "!", null, null, null, ParentObject);
			}
			else
			{
				DidX("charge", ", but" + ParentObject.Is + " brought up short", "!", null, null, null, ParentObject);
			}
			if (flag5)
			{
				ParentObject.ApplyEffect(new Dazed(1));
			}
			ParentObject.UseEnergy(1000, "Charging");
			break;
		}
		ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_charge");
		CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN);
		return true;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Charge", COMMAND_NAME, "Skills", null, "\u0010", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
