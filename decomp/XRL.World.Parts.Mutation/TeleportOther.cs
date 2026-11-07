using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class TeleportOther : BaseMutation
{
	public TeleportOther()
	{
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Actor, E.Target.CurrentCell, E.Actor, null, this))
		{
			E.Add("CommandTeleportOther");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("travel", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandTeleportOther");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You teleport an adjacent creature to a random nearby location.";
	}

	public override string GetLevelText(int Level)
	{
		return "Cooldown: {{rules|" + GetCooldownTurns(Level) + "}} rounds";
	}

	public int GetCooldownTurns(int Level)
	{
		return Math.Max(115 - 10 * Level, 5);
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldownTurns(Level));
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandTeleportOther")
		{
			if (!ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
				return false;
			}
			Cell cell = PickDirection("Teleport Other");
			if (cell == null)
			{
				return false;
			}
			if (cell == ParentObject.CurrentCell)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You may not teleport " + ParentObject.itself + " with Teleport Other!");
				}
				return false;
			}
			GameObject gameObject = null;
			foreach (GameObject item in cell.GetObjectsInCell())
			{
				if (item.IsCombat)
				{
					MentalMirror part = item.GetPart<MentalMirror>();
					if (part != null && part.CheckActive())
					{
						part.Activate();
						part.ReflectMessage(item);
						gameObject = ParentObject;
					}
					else
					{
						gameObject = item;
					}
					break;
				}
			}
			if (gameObject == null)
			{
				return false;
			}
			if (!gameObject.RandomTeleport(Swirl: true, this, null, null, E, 0, 0, InterruptMovement: true, null, Forced: false, IgnoreCombat: true, Voluntary: false))
			{
				return false;
			}
			if (ParentObject.IsPlayer())
			{
				DidX("teleport", gameObject.the + gameObject.ShortDisplayNameWithoutTitles + " away", null, null, null, ParentObject);
			}
			UseEnergy(1000, "Mental Mutation TeleportOther");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldownTurns(Level));
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Teleport Other", "CommandTeleportOther", "Mental Mutations", null, "\u001b");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
