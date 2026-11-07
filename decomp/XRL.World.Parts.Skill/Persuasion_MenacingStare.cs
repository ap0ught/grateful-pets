using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Language;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_MenacingStare : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandPersuasionMenacingStare";

	public static readonly string COMMAND_ATTACK = "Terrify MenacingStare";

	public static readonly int COOLDOWN = 50;

	public string RatingBase = "1d8";

	public int RatingOffset = 2;

	public int MaxRange = 5;

	public Guid ActivatedAbilityID = Guid.Empty;

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Range", MaxRange);
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
			PickStare(ParentObject, RatingBase, RatingOffset, MaxRange, ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= MaxRange && GameObject.Validate(E.Target) && E.Target.Brain != null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && 50.in100())
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool PickStare(GameObject Actor, string RatingBase, int RatingOffset, int MaxRange, Guid? AbilityID = null)
	{
		Cell cell = Actor.Physics.PickDestinationCell(80, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Menacing Stare", Snap: true);
		if (cell == null)
		{
			return false;
		}
		if (cell.DistanceTo(Actor) > MaxRange)
		{
			Actor.Fail("That is out of range! (" + MaxRange + " " + ((MaxRange == 1) ? "square" : "squares") + ")");
			return false;
		}
		int num = COOLDOWN;
		if (Actor.GetIntProperty("Horrifying") > 0)
		{
			num -= 10;
		}
		if (AbilityID.HasValue)
		{
			Actor.CooldownActivatedAbility(AbilityID.Value, num);
		}
		ApplyStare(Actor, cell, RatingBase, RatingOffset);
		return true;
	}

	public static int ApplyStare(GameObject Actor, Cell Cell, string RatingBase, int RatingOffset)
	{
		if (Cell == null)
		{
			return 0;
		}
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		int num = 0;
		List<GameObject> list = Event.NewGameObjectList();
		foreach (GameObject item in Cell.GetObjectsInCell())
		{
			if ((item != Actor || item.GetBodyPartCount("Head") >= 2) && item.Brain != null)
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			if (list.Count > 1)
			{
				Actor.Physics.DidX("stare", "at " + Grammar.MakeAndList(list, DefiniteArticles: true, Serial: true, Reflexive: true) + " menacingly");
			}
			else
			{
				Actor.Physics.DidXToY("stare", "at", list[0], "menacingly");
			}
		}
		foreach (GameObject item2 in list)
		{
			if (Actor.Physics.PerformMentalAttack(Terrified.OfAttacker, Actor, item2, null, COMMAND_ATTACK, RatingBase, 8388612, "6d4".RollCached(), int.MinValue, Actor.StatMod("Ego") + RatingOffset))
			{
				num++;
			}
		}
		if (Cell.IsVisible())
		{
			scrapBuffer.WriteAt(Cell, (num > 0) ? "&Y#" : "&w*");
			textConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(50);
		}
		return num;
	}

	public static Guid AddAbility(GameObject Object)
	{
		return Object.AddActivatedAbility("Menacing Stare", COMMAND_NAME, "Skills", null, "ì", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, null, Renderable.UITile("Abilities/abil_menacing_stare.bmp", 'w', 'M'));
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddAbility(GO);
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
