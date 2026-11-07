using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class DefensiveChromatophores : BaseMutation
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<CommandTakeActionEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		if (!ParentObject.IsPlayer() && AttemptScintillate(Auto: true))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("jewels", 2);
			E.Add("chance", 1);
		}
		return base.HandleEvent(E);
	}

	public override string GetLevelText(int Level)
	{
		return "You can't act while scintillating.\nConfuses nearby hostile creatures per Confusion rank " + Level + ".\nDuration: 5 rounds\nCooldown: 200 rounds";
	}

	public override string GetDescription()
	{
		return "In stressful situations, you scintillate.";
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("ConfusionRank", GetConfusionRank(Level));
		stats.Set("Duration", 5);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 5);
	}

	public int GetConfusionRank(int Level)
	{
		return Level;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandScintillate");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandScintillate" && !AttemptScintillate())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Scintillate", "CommandScintillate", "Physical Mutations", null, "\u000f");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}

	public bool AttemptScintillate(bool Auto = false)
	{
		if (!ShouldStartScintillating())
		{
			if (!Auto && ParentObject.IsPlayer())
			{
				if (IsMyActivatedAbilityCoolingDown(ActivatedAbilityID))
				{
					Popup.Show("You can't scintillate again so soon.");
				}
				else
				{
					Popup.Show("You're not under enough stress to scintillate.");
				}
			}
			return false;
		}
		if (!ParentObject.ApplyEffect(new Scintillating(5, Level)))
		{
			return false;
		}
		ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_physical_generic_activate");
		CooldownMyActivatedAbility(ActivatedAbilityID, 200);
		return true;
	}

	public bool ShouldStartScintillating()
	{
		if (!IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID))
		{
			return false;
		}
		if (ParentObject.GetHPPercent() > 15 && (ParentObject.PartyLeader == null || ParentObject.PartyLeader.GetHPPercent() > 15 || !ParentObject.HasLOSTo(ParentObject.PartyLeader) || ParentObject.DistanceTo(ParentObject.PartyLeader) > 30))
		{
			return false;
		}
		return true;
	}
}
