using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SpacetimeVortex : BaseMutation
{
	public SpacetimeVortex()
	{
		Type = "Mental";
	}

	public override string GetDescription()
	{
		return "You sunder spacetime, sending things nearby careening through a tear in the cosmic fabric.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Summons a vortex that swallows everything in its path.\n";
		if (Level > 10)
		{
			text = text + "Bonus duration: {{rules|" + (Level - 10) + "}} rounds\n";
		}
		text = text + "Cooldown: {{rules|" + GetCooldown(Level) + "}} rounds\n";
		text += "You may enter the vortex to teleport to a random location in Qud.\n";
		return text + "+200 reputation with {{w|highly entropic beings}}";
	}

	public int GetCooldown(int Level)
	{
		int num = 550 - 50 * Level;
		if (num < 5)
		{
			num = 5;
		}
		return num;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		bool flag = stats.mode.Contains("ability");
		if (Level > 10)
		{
			stats.Set("BonusDuration", "\n\nBonus duration: " + (Level - 10) + " rounds", !flag);
		}
		else
		{
			stats.Set("BonusDuration", "", !flag);
		}
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
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
		if (E.Distance <= 5 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandSpaceTimeVortex");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("time", BaseElementWeight);
			E.Add("chance", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandSpaceTimeVortex");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandSpaceTimeVortex")
		{
			Zone currentZone = ParentObject.CurrentZone;
			if (currentZone == null)
			{
				return false;
			}
			if (currentZone.IsWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You may not use this mutation on the world map.");
				}
				return false;
			}
			Cell cell = PickDestinationCell(5, AllowVis.OnlyVisible, Locked: false, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Space-Time Vortex");
			if (cell == null)
			{
				return false;
			}
			if (cell.PathDistanceTo(ParentObject.CurrentCell) > 5)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("That target is out of range! (5 squares)");
				}
				return false;
			}
			Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", cell);
			if (!ParentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
			{
				return false;
			}
			UseEnergy(1000, "Mental Mutation SpaceTimeVortex");
			int turns = Math.Max(550 - 50 * Level, 5);
			CooldownMyActivatedAbility(ActivatedAbilityID, turns);
			Vortex(cell);
		}
		return base.FireEvent(E);
	}

	public void Vortex(Cell C)
	{
		if (C != null)
		{
			List<Cell> adjacentCells = C.GetAdjacentCells();
			if (ParentObject.IsPlayer())
			{
				adjacentCells.Add(C);
			}
			Cell randomElement = adjacentCells.GetRandomElement();
			GameObject gameObject = GameObject.Create("Space-Time Vortex");
			Temporary part = gameObject.GetPart<Temporary>();
			part.Duration = Stat.Random(15, 18);
			if (Level > 10)
			{
				part.Duration += Level - 10;
			}
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("{{G|You sunder spacetime.}}");
			}
			IComponent<GameObject>.XDidY(gameObject, "appear", null, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: true, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
			randomElement.AddObject(gameObject);
		}
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Spacetime Vortex", "CommandSpaceTimeVortex", "Mental Mutations", null, "\u0015", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
