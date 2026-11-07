using System;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_Intimidate : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandIntimidate";

	public static readonly string COMMAND_ATTACK = "Terrify Intimidate";

	public static readonly int COOLDOWN = 50;

	public static readonly string DURATION = "6d4";

	public Guid ActivatedAbilityID = Guid.Empty;

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Duration", DURATION);
		int num = COOLDOWN;
		if (ParentObject.GetIntProperty("Horrifying") > 0)
		{
			num -= 10;
			stats.AddChangePostfix("Cooldown", -10, "your " + PropertyDescription.GetPropertyDisplayName("Horrifying"));
		}
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), num, num - COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("jewels", 3);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (ParentObject.OnWorldMap())
			{
				return ParentObject.Fail("You cannot do that on the world map.");
			}
			int num = COOLDOWN;
			if (ParentObject.GetIntProperty("Horrifying") > 0)
			{
				num -= 10;
			}
			ApplyIntimidate(ParentObject.GetCurrentCell(), ParentObject);
			CooldownMyActivatedAbility(ActivatedAbilityID, num);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 1 && GameObject.Validate(E.Target) && E.Target.Brain != null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && 10.in100())
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static void ApplyIntimidate(Cell FromCell, GameObject Actor, bool FreeAction = false)
	{
		if (!GameObject.Validate(ref Actor) || Actor.OnWorldMap())
		{
			return;
		}
		Actor?.PlayWorldSound("Sounds/Abilities/sfx_ability_intimidate");
		IComponent<GameObject>.XDidY(Actor, "assume", "an intimidating posture");
		foreach (Cell localAdjacentCell in FromCell.GetLocalAdjacentCells())
		{
			TextConsole textConsole = Look._TextConsole;
			ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
			if (localAdjacentCell != null)
			{
				foreach (GameObject item in localAdjacentCell.GetObjectsInCell())
				{
					if (item.IsHostileTowards(Actor) || Actor.IsHostileTowards(item))
					{
						if (!FreeAction)
						{
							Actor.UseEnergy(1000, "Persuasion Skill Intimidate");
							FreeAction = true;
						}
						int attackModifier = Actor.StatMod("Ego") + Actor.GetIntProperty("Persuasion_Intimidate") * 2;
						Mental.PerformAttack(Terrified.OfAttacker, Actor, item, null, COMMAND_ATTACK, "1d8", 8388610, DURATION.RollCached(), int.MinValue, attackModifier);
					}
				}
			}
			if (localAdjacentCell.IsVisible())
			{
				scrapBuffer.WriteAt(localAdjacentCell, "&Y#");
				textConsole.DrawBuffer(scrapBuffer);
				Thread.Sleep(50);
			}
		}
	}

	public static bool Terrify(MentalAttackEvent E)
	{
		if (E.Penetrations > 0)
		{
			Terrified e = new Terrified(DURATION.RollCached(), E.Attacker);
			if (E.Defender.ApplyEffect(e))
			{
				return true;
			}
		}
		IComponent<GameObject>.XDidY(E.Defender, "resist", "becoming afraid", null, null, null, E.Defender);
		return false;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Intimidate", COMMAND_NAME, "Skills", null, "\u0005");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
