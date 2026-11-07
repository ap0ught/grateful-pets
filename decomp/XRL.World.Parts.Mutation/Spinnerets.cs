using System;
using System.Text;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Spinnerets : BaseMutation
{
	public const string SAVE_BONUS_VS = "Move";

	public int SpinTimer;

	public bool Phase;

	public bool Active;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetMovementAbilityListEvent.ID && ID != AIGetOffensiveAbilityListEvent.ID && ID != LeftCellEvent.ID && ID != ModifyDefendingSaveEvent.ID)
		{
			return ID == LeavingCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetMovementAbilityListEvent E)
	{
		if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandSpinWeb");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance > 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandSpinWeb");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeavingCellEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID) && !ParentObject.OnWorldMap() && SpinTimer > 0)
		{
			E?.Cell?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_spinnerets_webDrop", 0.5f, 0f, Combat: true);
		}
		return true;
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			if (E.Cell.OnWorldMap())
			{
				SpinTimer = -1;
			}
			else
			{
				SpinTimer--;
			}
			if (SpinTimer < 0)
			{
				ToggleMyActivatedAbility(ActivatedAbilityID);
			}
			else
			{
				GameObject gameObject;
				if (!Phase && ParentObject.GetPhase() != 2)
				{
					gameObject = GameObject.Create("Web");
					Sticky part = gameObject.GetPart<Sticky>();
					if (part != null)
					{
						part.SaveTarget = 13 + 3 * Level;
					}
				}
				else
				{
					gameObject = GameObject.Create("PhaseWeb");
					gameObject.ApplyEffect(new Phased());
					PhaseSticky part2 = gameObject.GetPart<PhaseSticky>();
					if (part2 != null)
					{
						part2.SaveTarget = 18 + 2 * Level;
					}
				}
				E.Cell.AddObject(gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Move", E))
		{
			E.Roll += GetMoveSaveModifier();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyStuck");
		Registrar.Register("CommandSpinWeb");
		Registrar.Register("VillageInit");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You can spin sticky silk webs.";
	}

	public int GetMoveSaveModifier()
	{
		return 5 + Level;
	}

	public override string GetLevelText(int Level)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Compound("While spinning, you leave webs in your wake as you move.", '\n');
		if (Level != base.Level)
		{
			stringBuilder.Compound("{{rules|Increased web strength}}", '\n');
		}
		stringBuilder.Compound("Duration: {{rules|", '\n').Append(GetDuration(Level)).Append("}} move actions");
		SavingThrows.AppendSaveBonusDescription(stringBuilder, GetMoveSaveModifier(), "Move", HighlightNumber: true);
		stringBuilder.Compound("Cooldown: 80 rounds", '\n');
		stringBuilder.Compound("You are immune to getting stuck.", '\n');
		stringBuilder.Compound("+300 reputation with {{w|arachnids}}", '\n');
		return stringBuilder.ToString();
	}

	public int GetDuration(int Level)
	{
		return 5 + Level;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("SpinDuration", GetDuration(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 80);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyStuck")
		{
			return false;
		}
		if (E.ID == "CommandSpinWeb")
		{
			ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_physical_generic_activate");
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot do that on the world map.");
				}
				return false;
			}
			ToggleMyActivatedAbility(ActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				CooldownMyActivatedAbility(ActivatedAbilityID, 80);
				SpinTimer = 5 + Level;
			}
		}
		else if (E.ID == "VillageInit")
		{
			Active = false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Spin Webs", "CommandSpinWeb", "Physical Mutations", null, "#", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: true);
		if (Phase)
		{
			GO.ApplyEffect(new Phased(9999));
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
