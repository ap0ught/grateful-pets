using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Disintegration : BaseMutation
{
	public static readonly string COMMAND_NAME = "CommandDisintegration";

	public Disintegration()
	{
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 3 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			Cell cell = E.Actor.CurrentCell;
			if (cell != null)
			{
				bool flag = false;
				bool flag2 = true;
				if (ParentObject.Brain != null)
				{
					foreach (GameObject item in cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 4, "Combat", E.Actor))
					{
						if (item != E.Actor)
						{
							if (E.Actor.Brain.GetFeeling(item) >= 0)
							{
								flag2 = false;
								break;
							}
							flag = true;
						}
					}
				}
				if (flag & flag2)
				{
					E.Add(COMMAND_NAME);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell == null)
			{
				return false;
			}
			Disintegrate(cell, 3, Level, ParentObject);
			IComponent<GameObject>.XDidYToZ("the air", "vibrates", "destructively around", ParentObject, null, "!", ParentObject, null, ParentObject);
			UseEnergy(1000, "Mental Mutation Disintegration");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
			ParentObject.ApplyEffect(new Exhausted(3));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("chance", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You disintegrate nearby matter.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat("" + "Area: 7x7 around self\n", "Damage to non-structural objects: {{rules|", GetNonStructuralDamage(Level), "}}\n"), "Damage to structural objects: {{rules|", GetStructuralDamage(Level), "}}\n"), "You are exhausted for 3 rounds after using this power.\n"), "Cooldown: ", GetCooldown(Level).ToString(), " rounds");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Area", "7x7 around self");
		stats.Set("DamageNonStructural", GetNonStructuralDamage(Level), !stats.mode.Contains("ability"));
		stats.Set("DamageStructural", GetStructuralDamage(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public static string GetNonStructuralDamage(int Level)
	{
		return Level + "d10+" + 2 * Level;
	}

	public static string GetStructuralDamage(int Level)
	{
		return Level + "d100+20";
	}

	public static void Disintegrate(Cell C, int Radius, int Level, GameObject immunity, GameObject owner = null, GameObject source = null, bool lowPrecision = false, bool indirect = false)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		if (owner == null)
		{
			owner = immunity;
		}
		int phase = Phase.getPhase(source ?? owner);
		List<Cell> list = new List<Cell>();
		C.GetAdjacentCells(Radius, list, LocalOnly: false);
		bool flag = false;
		if (C.ParentZone != null && C.ParentZone.IsActive())
		{
			for (int i = 0; i < Radius; i++)
			{
				foreach (Cell item in list)
				{
					if (item.ParentZone == C.ParentZone && item.IsVisible())
					{
						flag = true;
						if (Radius < 3 || (item.PathDistanceTo(C) <= i - Stat.Random(0, 1) && item.PathDistanceTo(C) > i - Stat.Random(2, 3)))
						{
							scrapBuffer.Goto(item.X, item.Y);
							scrapBuffer.Write("&" + Phase.getRandomDisintegrationColor(phase) + (char)Stat.Random(191, 198));
						}
					}
				}
				if (flag)
				{
					textConsole.DrawBuffer(scrapBuffer);
					Thread.Sleep(75);
				}
			}
		}
		if (list.Count > 0 && owner != null && owner.Physics != null)
		{
			owner.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_disintegration_disintegrate", 0.5f, 0f, Combat: true);
		}
		string nonStructuralDamage = GetNonStructuralDamage(Level);
		string structuralDamage = GetStructuralDamage(Level);
		bool flag2 = lowPrecision;
		if (!flag2 && owner != null)
		{
			foreach (Cell item2 in list)
			{
				if (item2.HasObject(owner.IsRegardedWithHostilityBy))
				{
					flag2 = true;
					break;
				}
			}
		}
		foreach (Cell item3 in list)
		{
			foreach (GameObject item4 in item3.GetObjectsInCell())
			{
				if (item4 != immunity && item4.PhaseMatches(phase) && item4.GetMatterPhase() <= 3 && item4.GetIntProperty("Electromagnetic") <= 0)
				{
					string dice = (item4.HasPart<Inorganic>() ? structuralDamage : nonStructuralDamage);
					int amount = dice.RollCached();
					bool accidental = flag2 && owner != null && !item4.IsHostileTowards(owner);
					item4.TakeDamage(amount, "from %t disintegration!", "Disintegration", null, null, owner, null, source, null, null, accidental, Environmental: false, indirect);
				}
			}
		}
	}

	public int GetCooldown(int Level)
	{
		return 75;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Disintegration", COMMAND_NAME, "Mental Mutations", null, "ê");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
