using System;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class LongBladesCore : IPart
{
	public const string SUPPORT_TYPE = "LongBladesCore";

	public const string STR_DEFENSIVE = "defensive";

	public const string STR_AGGRESSIVE = "aggressive";

	public const string STR_DUELIST = "dueling";

	public int Ultmode;

	public string currentStance = "";

	public Guid AggressiveStanceID = Guid.Empty;

	public Guid DefensiveStanceID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<GetToHitModifierEvent>.ID && ID != PooledEvent<NeedPartSupportEvent>.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public void CollectStatsAgressive(Templates.StatCollector stats)
	{
		if (ParentObject.HasPart<LongBladesImprovedAggressiveStance>())
		{
			stats.Set("HitPenalty", 3, changes: true, -1);
			stats.Set("PenBonus", 2, changes: true, 1);
			stats.AddChangePostfix("Penetration bonus", 1, "Improved Agressive Stance");
			stats.AddChangePostfix("To-hit penalty", 1, "Improved Agressive Stance");
		}
		else
		{
			stats.Set("HitPenalty", 2);
			stats.Set("PenBonus", 1);
		}
	}

	public void CollectStatsDefensive(Templates.StatCollector stats)
	{
		if (ParentObject.HasPart<LongBladesImprovedDefensiveStance>())
		{
			stats.Set("DVBonus", 3, changes: true, 1);
			stats.AddChangePostfix("DV bonus", 1, "Improved Defensive Stance");
		}
		else
		{
			stats.Set("DVBonus", 2);
		}
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(AggressiveStanceID, CollectStatsAgressive);
		DescribeMyActivatedAbility(DefensiveStanceID, CollectStatsDefensive);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetToHitModifierEvent E)
	{
		if (E.Actor == ParentObject && E.Checking == "Actor" && E.Melee && (E.Skill == "LongBlades" || E.Skill == "ShortBlades") && IsPrimaryBladeEquipped())
		{
			if (currentStance == "aggressive")
			{
				E.Modifier -= (ParentObject.HasPart<LongBladesImprovedAggressiveStance>() ? 3 : 2);
			}
			else if (currentStance == "dueling")
			{
				E.Modifier += (ParentObject.HasPart<LongBladesImprovedDuelistStance>() ? 3 : 2);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (IsPrimaryBladeEquipped() && GameObject.Validate(E.Target))
		{
			string text = "aggressive";
			if (E.Actor.isDamaged(0.6))
			{
				text = "defensive";
			}
			else if (E.Actor.HasPart<LongBladesDuelingStance>() && (double)ConTarget(E.Target) >= 0.8)
			{
				text = "dueling";
			}
			if (currentStance != text)
			{
				switch (text)
				{
				case "aggressive":
					if (IsMyActivatedAbilityAIUsable(AggressiveStanceID))
					{
						E.Add("CommandAggressiveStance", 10);
					}
					break;
				case "defensive":
					if (IsMyActivatedAbilityAIUsable(DefensiveStanceID))
					{
						E.Add("CommandDefensiveStance", 10);
					}
					break;
				case "dueling":
				{
					LongBladesDuelingStance part = E.Actor.GetPart<LongBladesDuelingStance>();
					if (part != null && part.IsMyActivatedAbilityAIUsable(part.ActivatedAbilityID))
					{
						E.Add("CommandDuelingStance", 10);
					}
					break;
				}
				}
			}
			if (E.Distance == 1)
			{
				if ((double)ConTarget(E.Target) >= 0.8)
				{
					LongBladesDeathblow part2 = E.Actor.GetPart<LongBladesDeathblow>();
					if (part2 != null && part2.IsMyActivatedAbilityAIUsable(part2.ActivatedAbilityID))
					{
						E.Add("CommandDeathblow");
						return true;
					}
				}
				if (currentStance != "aggressive")
				{
					LongBladesLunge part3 = E.Actor.GetPart<LongBladesLunge>();
					if (part3 != null && part3.IsMyActivatedAbilityAIUsable(part3.ActivatedAbilityID))
					{
						E.Add("CommandLunge");
					}
				}
				LongBladesSwipe part4 = E.Actor.GetPart<LongBladesSwipe>();
				if (part4 != null && part4.IsMyActivatedAbilityAIUsable(part4.ActivatedAbilityID))
				{
					E.Add("CommandSwipe");
				}
			}
			else if (E.Distance == 2 && currentStance == "aggressive")
			{
				LongBladesLunge part5 = E.Actor.GetPart<LongBladesLunge>();
				if (part5 != null && part5.IsMyActivatedAbilityAIUsable(part5.ActivatedAbilityID))
				{
					E.Add("CommandLunge");
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedPartSupportEvent E)
	{
		if (E.Type == "LongBladesCore" && !PartSupportEvent.Check(E, this))
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("CommandAggressiveStance");
		Registrar.Register("CommandDeathblow");
		Registrar.Register("CommandDefensiveStance");
		Registrar.Register("CommandDuelingStance");
		Registrar.Register("CommandLunge");
		Registrar.Register("CommandSwipe");
		Registrar.Register("EquipperEquipped");
		Registrar.Register("EquipperUnequipped");
		Registrar.Register("GetAttackerHitDice");
		Registrar.Register("GetDefenderDV");
		Registrar.Register("LongBlades.UpdateStance");
		Registrar.Register("PrimaryLimbRecalculated");
		base.Register(Object, Registrar);
	}

	public override void Initialize()
	{
		base.Initialize();
		if (AggressiveStanceID == Guid.Empty)
		{
			AggressiveStanceID = AddMyActivatedAbility("Aggressive Stance", "CommandAggressiveStance", "Stances", "+1/2 to penetration rolls, -2/-3 to hit while wielding a long blade in your primary hand", "\u009f");
		}
		if (DefensiveStanceID == Guid.Empty)
		{
			DefensiveStanceID = AddMyActivatedAbility("Defensive Stance", "CommandDefensiveStance", "Stances", "+2/3 DV while wielding a long blade in your primary hand", "\u009f");
		}
		if (currentStance.IsNullOrEmpty())
		{
			ChangeStance(ParentObject.GetPropertyOrTag("InitialStance") ?? "defensive");
		}
	}

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref AggressiveStanceID);
		RemoveMyActivatedAbility(ref DefensiveStanceID);
		ParentObject.RemoveEffect(typeof(LongbladeStance_Aggressive));
		ParentObject.RemoveEffect(typeof(LongbladeStance_Defensive));
		ParentObject.RemoveEffect(typeof(LongbladeStance_Dueling));
		ParentObject.RemoveEffect(typeof(LongbladeEffect_EnGarde));
		base.Remove();
	}

	public GameObject GetPrimaryBlade()
	{
		return ParentObject.GetPrimaryWeaponOfType("LongBlades");
	}

	public bool IsPrimaryBladeEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("LongBlades");
	}

	public string GetCommandForStance(string stance)
	{
		return stance switch
		{
			"defensive" => "CommandDefensiveStance", 
			"aggressive" => "CommandAggressiveStance", 
			"dueling" => "CommandDuelingStance", 
			_ => null, 
		};
	}

	public ActivatedAbilityEntry GetAbilityForStance(string stance)
	{
		return ParentObject.ActivatedAbilities.GetAbilityByCommand(GetCommandForStance(stance));
	}

	public void ChangeStance(string newStance)
	{
		if (currentStance == "defensive")
		{
			StatShifter.RemoveStatShifts();
		}
		ActivatedAbilityEntry abilityForStance = GetAbilityForStance(currentStance);
		if (abilityForStance != null)
		{
			abilityForStance.ToggleState = false;
		}
		if (newStance != currentStance)
		{
			currentStance = newStance;
			if (Visible())
			{
				if (!ParentObject.IsPlayer() || MyActivatedAbilities == null || !MyActivatedAbilities.Silent)
				{
					ParentObject.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_physicalStanceChange");
					DidX("switch", "to " + newStance + " stance", null, null, null, ParentObject);
				}
				Cell cell = ParentObject.GetCurrentCell();
				if (cell != null)
				{
					int x = cell.X;
					int y = cell.Y;
					string text = "&W";
					if (currentStance == "dueling")
					{
						text = "&W";
					}
					else if (currentStance == "aggressive")
					{
						text = "&R";
					}
					else if (currentStance == "defensive")
					{
						text = "&G";
					}
					for (int i = 0; i < 8; i++)
					{
						The.ParticleManager.AddRadial(text + ".", x, y, (float)(i * 45) / 360f * 6.14f, Stat.Random(2, 4), -0.035f * (float)Stat.Random(8, 12), -0.3f + -0.15f * (float)Stat.Random(1, 3), 40, 0L);
					}
				}
			}
		}
		ActivatedAbilityEntry abilityForStance2 = GetAbilityForStance(currentStance);
		if (abilityForStance2 != null)
		{
			abilityForStance2.ToggleState = true;
		}
		if (currentStance == "defensive" && IsPrimaryBladeEquipped())
		{
			int num = 2;
			if (ParentObject.HasPart<LongBladesImprovedDefensiveStance>())
			{
				num++;
			}
			StatShifter.SetStatShift("DV", num);
		}
		if (currentStance == "aggressive")
		{
			ParentObject.ApplyEffect(new LongbladeStance_Aggressive());
		}
		else if (currentStance == "dueling")
		{
			ParentObject.ApplyEffect(new LongbladeStance_Dueling());
		}
		else if (currentStance == "defensive")
		{
			ParentObject.ApplyEffect(new LongbladeStance_Defensive());
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (Ultmode > 0)
			{
				Ultmode--;
				if (Ultmode <= 0)
				{
					ParentObject.RemoveEffect<LongbladeEffect_EnGarde>();
				}
				else if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(Ultmode.Things("turn remains", "turns remain") + " until your guard is down.");
				}
			}
		}
		else if (E.ID == "GetAttackerHitDice")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
			if (gameObjectParameter != null)
			{
				MeleeWeapon part = gameObjectParameter.GetPart<MeleeWeapon>();
				if (part != null && (part.Skill == "LongBlades" || part.Skill == "ShortBlades") && IsPrimaryBladeEquipped() && currentStance == "aggressive")
				{
					int num = ((!ParentObject.HasPart<LongBladesImprovedAggressiveStance>()) ? 1 : 2);
					E.SetParameter("PenetrationBonus", E.GetIntParameter("PenetrationBonus") + num);
				}
			}
		}
		else if (E.ID == "CommandAggressiveStance")
		{
			if (!IsPrimaryBladeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a long blade equipped to switch stances.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			ChangeStance("aggressive");
		}
		else if (E.ID == "CommandDefensiveStance")
		{
			if (!IsPrimaryBladeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a long blade equipped to switch stances.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			ChangeStance("defensive");
		}
		else if (E.ID == "CommandDuelingStance")
		{
			if (!IsPrimaryBladeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a long blade equipped to switch stances.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			ChangeStance("dueling");
		}
		else if (E.ID == "LongBlades.UpdateStance" || E.ID == "EquipperEquipped" || E.ID == "EquipperUnequipped" || E.ID == "PrimaryLimbRecalculated")
		{
			ChangeStance(currentStance);
		}
		else if (E.ID == "CommandLunge")
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell == null)
			{
				return false;
			}
			if (cell.OnWorldMap())
			{
				return ParentObject.ShowFailure("You cannot do that on the world map.");
			}
			if (!IsPrimaryBladeEquipped())
			{
				return ParentObject.ShowFailure("You must have a long blade equipped in your primary hand to lunge.");
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			if (currentStance == "aggressive")
			{
				string direction = PickDirectionS("Lunge");
				Cell cellFromDirection = ParentObject.GetCurrentCell().GetCellFromDirection(direction);
				Cell cellFromDirection2 = cellFromDirection.GetCellFromDirection(direction);
				if (cellFromDirection == null || cellFromDirection2 == null)
				{
					return false;
				}
				GameObject combatTarget = cellFromDirection.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true);
				if (combatTarget != null)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You can't aggressively lunge through " + combatTarget.the + combatTarget.ShortDisplayName + ".");
					}
					return false;
				}
				GameObject combatTarget2 = cellFromDirection2.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 5);
				if (combatTarget2 == null)
				{
					if (ParentObject.IsPlayer())
					{
						combatTarget2 = cellFromDirection2.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
						if (combatTarget2 == null)
						{
							Popup.ShowFail("There's nothing there to lunge at.");
						}
						else
						{
							Popup.ShowFail("There's nothing there you can lunge at.");
						}
					}
					return false;
				}
				ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_longBlade_lunge");
				DidXToY("lunge", "at", combatTarget2, null, null, null, null, ParentObject);
				E.RequestInterfaceExit();
				if (!ParentObject.DirectMoveTo(cellFromDirection))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.Show("Your lunge is interrupted.");
					}
					else if (Visible())
					{
						IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " lunge is interrupted.");
					}
					return false;
				}
				if (ParentObject.PhaseMatches(combatTarget2))
				{
					GameObject primaryBlade = GetPrimaryBlade();
					Combat.MeleeAttackWithWeapon(ParentObject, combatTarget2, primaryBlade, ParentObject.Body.FindDefaultOrEquippedItem(primaryBlade), "Lunging", 0, 2, 2, 0, 0, Primary: true);
					combatTarget2.Bloodsplatter();
					ParentObject.FireEvent(Event.New("LungedTarget", "Defender", combatTarget2));
				}
				else if (ParentObject.IsPlayer())
				{
					Popup.Show("Your lunge passes through " + combatTarget2.the + combatTarget2.ShortDisplayName + ".");
				}
				else if (Visible())
				{
					IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " lunge passes through " + combatTarget2.the + combatTarget2.ShortDisplayName + ".");
				}
				ParentObject.UseEnergy(1000, "Skill Lunge");
				if (Ultmode <= 0)
				{
					LongBladesLunge part2 = ParentObject.GetPart<LongBladesLunge>();
					part2?.CooldownMyActivatedAbility(part2.ActivatedAbilityID, LongBladesLunge.COOLDOWN, null, "Agility");
				}
			}
			else if (currentStance == "defensive")
			{
				Cell cell2 = PickDirection("Lunge");
				if (cell2 == null)
				{
					return false;
				}
				GameObject combatTarget3 = cell2.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, null, null, null, null, null, AllowInanimate: false);
				if (combatTarget3 == null)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("There's nothing there to lunge away from.");
					}
					return false;
				}
				ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_longBlade_lunge");
				string directionFromCell = cell2.GetDirectionFromCell(cell);
				DidXToY("lunge", "away from", combatTarget3, null, null, null, null, ParentObject);
				E.RequestInterfaceExit();
				if (ParentObject.PhaseAndFlightMatches(combatTarget3))
				{
					GameObject primaryBlade2 = GetPrimaryBlade();
					Combat.MeleeAttackWithWeapon(ParentObject, combatTarget3, primaryBlade2, ParentObject.Body.FindDefaultOrEquippedItem(primaryBlade2), "Lunging", 0, 0, 0, 0, 0, Primary: true);
					ParentObject.FireEvent(Event.New("LungedTarget", "Defender", combatTarget3));
					combatTarget3.DustPuff();
				}
				int force = ParentObject.GetKineticResistance() * 3 / 2;
				for (int i = 0; i < 2; i++)
				{
					if (!ParentObject.Physics.Push(directionFromCell, force, 1, IgnoreGravity: true, Involuntary: false, ParentObject))
					{
						break;
					}
				}
				ParentObject.UseEnergy(1000, "Skill Lunge");
				ParentObject.Gravitate();
				if (Ultmode <= 0)
				{
					LongBladesLunge part3 = ParentObject.GetPart<LongBladesLunge>();
					part3?.CooldownMyActivatedAbility(part3.ActivatedAbilityID, 15, null, "Agility");
				}
			}
			else
			{
				if (!(currentStance == "dueling"))
				{
					if (ParentObject.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You must be in a long blade stance to use that ability.");
					}
					return false;
				}
				Cell cell3 = PickDirection("Lunge");
				if (cell3 == null)
				{
					return false;
				}
				GameObject combatTarget4 = cell3.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 5);
				if (combatTarget4 == null)
				{
					if (ParentObject.IsPlayer())
					{
						combatTarget4 = cell3.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
						if (combatTarget4 == null)
						{
							Popup.ShowFail("There's nothing there to lunge at.");
						}
						else
						{
							Popup.ShowFail("There's nothing there you can lunge at.");
						}
					}
					return false;
				}
				DidXToY("lunge", "at", combatTarget4, null, null, null, null, ParentObject);
				E.RequestInterfaceExit();
				if (ParentObject.PhaseMatches(combatTarget4))
				{
					combatTarget4.Bloodsplatter();
					GameObject primaryBlade3 = GetPrimaryBlade();
					Combat.MeleeAttackWithWeapon(ParentObject, combatTarget4, primaryBlade3, ParentObject.Body.FindDefaultOrEquippedItem(primaryBlade3), "Autohit,Autopen,Lunging", 0, 1, 1, 0, 0, Primary: true);
					ParentObject.FireEvent(Event.New("LungedTarget", "Defender", combatTarget4));
				}
				else if (ParentObject.IsPlayer())
				{
					Popup.Show("Your lunge passes through " + combatTarget4.the + combatTarget4.ShortDisplayName + ".");
				}
				else if (Visible())
				{
					IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " lunge passes through " + combatTarget4.the + combatTarget4.ShortDisplayName + ".");
				}
				ParentObject.UseEnergy(1000, "Skill Lunge");
				if (Ultmode <= 0)
				{
					LongBladesLunge part4 = ParentObject.GetPart<LongBladesLunge>();
					part4?.CooldownMyActivatedAbility(part4.ActivatedAbilityID, 15, null, "Agility");
				}
			}
		}
		else if (E.ID == "CommandSwipe")
		{
			Cell cell4 = ParentObject.GetCurrentCell();
			if (cell4 == null)
			{
				return false;
			}
			if (cell4.OnWorldMap())
			{
				return ParentObject.ShowFailure("You cannot do that on the world map.");
			}
			GameObject primaryBlade4 = GetPrimaryBlade();
			if (primaryBlade4 == null)
			{
				return ParentObject.ShowFailure("You must have a long blade equipped in your primary hand to swipe.");
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_longBlade_swipe");
			if (currentStance == "aggressive")
			{
				for (int j = 0; j < 1; j++)
				{
					if (ParentObject.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You aggressively swipe your blade in the air.", 'G');
					}
					else if (Visible())
					{
						IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + " aggressively" + ParentObject.GetVerb("swipe") + " " + ParentObject.its + " blade in the air.", 'R');
					}
					string[] directionList = Directions.DirectionList;
					foreach (string direction2 in directionList)
					{
						GameObject gameObject = cell4.GetCellFromDirection(direction2)?.GetCombatTarget(ParentObject);
						if (gameObject != null && (gameObject.Brain == null || gameObject.Brain.IsHostileTowards(ParentObject)))
						{
							gameObject.Bloodsplatter();
							Combat.MeleeAttackWithWeapon(ParentObject, gameObject, primaryBlade4, ParentObject.Body.FindDefaultOrEquippedItem(primaryBlade4), null, 0, 0, 0, 0, 0, Primary: true);
						}
					}
				}
				ParentObject.UseEnergy(1000, "Skill Aggressive Swipe");
				if (Ultmode <= 0)
				{
					LongBladesSwipe part5 = ParentObject.GetPart<LongBladesSwipe>();
					part5?.CooldownMyActivatedAbility(part5.ActivatedAbilityID, 15);
				}
			}
			else if (currentStance == "defensive")
			{
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You swipe your blade in the air, pushing your enemies backward.", 'G');
				}
				else if (Visible())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("swipe") + " " + ParentObject.its + " blade in the air, pushing " + ParentObject.its + " foes backward.", 'R');
				}
				string[] directionList = Directions.DirectionList;
				foreach (string direction3 in directionList)
				{
					GameObject gameObject2 = ParentObject.GetCurrentCell().GetCellFromDirection(direction3)?.GetCombatTarget(ParentObject);
					if (gameObject2 != null && gameObject2.GetMatterPhase() == 1)
					{
						gameObject2.DustPuff();
						gameObject2.Physics.Push(direction3, 1000, 4, IgnoreGravity: false, Involuntary: true, ParentObject);
						if (!gameObject2.MakeSave("Agility,Strength", 30, null, null, "LongBlades Blade Swipe Knockdown", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, primaryBlade4))
						{
							gameObject2.ApplyEffect(new Prone());
						}
					}
				}
				ParentObject.UseEnergy(1000, "Skill Defensive Swipe");
				if (Ultmode <= 0)
				{
					LongBladesSwipe part6 = ParentObject.GetPart<LongBladesSwipe>();
					part6?.CooldownMyActivatedAbility(part6.ActivatedAbilityID, LongBladesSwipe.COOLDOWN);
				}
			}
			else
			{
				if (!(currentStance == "dueling"))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You must be in a long blade stance to use that ability.");
					}
					return false;
				}
				Cell cell5 = PickDirection("Swipe");
				if (cell5 == null)
				{
					return false;
				}
				GameObject combatTarget5 = cell5.GetCombatTarget(ParentObject);
				if (combatTarget5 == null)
				{
					if (ParentObject.IsPlayer())
					{
						combatTarget5 = cell5.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
						if (combatTarget5 == null)
						{
							Popup.ShowFail("There's nothing there to swipe at.");
						}
						else
						{
							Popup.ShowFail("There's nothing there you can swipe at.");
						}
					}
					return false;
				}
				DidXToY("swipe", ParentObject.its + " blade at", combatTarget5, null, null, null, null, ParentObject);
				Disarming.Disarm(combatTarget5, ParentObject, 25, "Strength", "Agility", null, primaryBlade4);
				Combat.MeleeAttackWithWeapon(ParentObject, combatTarget5, primaryBlade4, ParentObject.Body.FindDefaultOrEquippedItem(primaryBlade4), "Autohit,Autopen", 0, 0, 0, 0, 0, Primary: true);
				ParentObject.UseEnergy(1000, "Skill Duelist Swipe");
				if (Ultmode <= 0)
				{
					LongBladesSwipe part7 = ParentObject.GetPart<LongBladesSwipe>();
					part7?.CooldownMyActivatedAbility(part7.ActivatedAbilityID, 15);
				}
			}
		}
		else if (E.ID == "CommandDeathblow")
		{
			if (!IsPrimaryBladeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a long blade equipped to effectively yell out 'En garde!'");
				}
				return false;
			}
			ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_longBlade_enGarde");
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("{{G|En garde!}}");
				ParentObject.ParticleText("En garde!", 'W');
			}
			else if (ParentObject.IsVisible())
			{
				ParentObject.ParticleText("En garde!", 'W');
			}
			ParentObject.ApplyEffect(new LongbladeEffect_EnGarde());
			Ultmode = 10;
			LongBladesDeathblow part8 = ParentObject.GetPart<LongBladesDeathblow>();
			part8?.CooldownMyActivatedAbility(part8.ActivatedAbilityID, 100, null, "Agility");
			LongBladesLunge part9 = ParentObject.GetPart<LongBladesLunge>();
			if (part9 != null && part9.IsMyActivatedAbilityCoolingDown(part9.ActivatedAbilityID))
			{
				part9.MyActivatedAbility(part9.ActivatedAbilityID).Cooldown = 0;
			}
			LongBladesSwipe part10 = ParentObject.GetPart<LongBladesSwipe>();
			if (part10 != null && part10.IsMyActivatedAbilityCoolingDown(part10.ActivatedAbilityID))
			{
				part10.MyActivatedAbility(part10.ActivatedAbilityID).Cooldown = 0;
			}
		}
		return base.FireEvent(E);
	}
}
