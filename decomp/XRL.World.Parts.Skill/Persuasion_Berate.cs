using System;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_Berate : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandPersuasionBerate";

	public static readonly string COMMAND_ATTACK = "Shame Berate";

	public static readonly int VOCAL_RANGE = 8;

	public static readonly string DURATION = "6d6";

	public static readonly int COOLDOWN = 50;

	public Guid ActivatedAbilityID = Guid.Empty;

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Range", VOCAL_RANGE);
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
		if (E.Command == COMMAND_NAME && ParentObject == E.Actor)
		{
			bool flag = E.Actor.IsMissingTongue();
			if (flag && !E.Actor.HasPart<Telepathy>())
			{
				E.Actor.Fail("You cannot berate without a tongue.");
				return false;
			}
			if (!E.Actor.CheckFrozen(Telepathic: true))
			{
				return false;
			}
			Cell cell = PickDestinationCell(E.Actor.HasPart<Telepathy>() ? 80 : VOCAL_RANGE, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Berate whom?", Snap: true);
			if (cell == null)
			{
				return false;
			}
			if (E.Actor.DistanceTo(cell) > VOCAL_RANGE && !E.Actor.HasPart<Telepathy>())
			{
				GameObject actor = E.Actor;
				int vOCAL_RANGE = VOCAL_RANGE;
				actor.Fail("That is out of range! (" + vOCAL_RANGE + " squares)");
				return false;
			}
			int intProperty = E.Actor.GetIntProperty("Horrifying");
			int turns = Math.Max(COOLDOWN - intProperty * 10, 10);
			UseEnergy(1000, "Skill Persuasion Berate");
			CooldownMyActivatedAbility(ActivatedAbilityID, turns);
			ApplyBerate(E.Actor, cell, flag, this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if ((E.Distance <= VOCAL_RANGE || (E.Actor.HasPart<Telepathy>() && E.Actor.CanMakeTelepathicContactWith(E.Target))) && GameObject.Validate(E.Target) && E.Target.Brain != null && !E.Target.HasEffect<Shamed>() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && Stat.Random(-80, 80) >= E.Distance && (!E.Actor.IsMissingTongue() || (E.Actor.HasPart<Telepathy>() && E.Actor.CanMakeTelepathicContactWith(E.Target))) && E.Actor.CheckFrozen(Telepathic: true, Telekinetic: false, Silent: true, E.Target))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public static int ApplyBerate(GameObject Actor, Cell TargetCell, bool? IsMissingTongue = null, object Source = null)
	{
		if (!GameObject.Validate(ref Actor))
		{
			return 0;
		}
		if (TargetCell == null)
		{
			return 0;
		}
		bool flag = IsMissingTongue ?? Actor.IsMissingTongue();
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		string text = "1d8";
		int num = Actor.StatMod("Ego");
		if (num != 0)
		{
			text += num.Signed();
		}
		int num2 = 0;
		int i = 0;
		for (int count = TargetCell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = TargetCell.Objects[i];
			if (!gameObject.HasPart<Brain>())
			{
				continue;
			}
			if (flag && !Actor.CanMakeTelepathicContactWith(gameObject))
			{
				Actor.Fail("{{r|You cannot berate " + gameObject.t() + " without a tongue.}}");
			}
			else if (!Actor.CheckFrozen(Telepathic: true, Telekinetic: false, Silent: true, gameObject))
			{
				Actor.Fail("{{r|You cannot berate " + gameObject.t() + " while frozen.}}");
			}
			else if (Actor.DistanceTo(gameObject) > VOCAL_RANGE && !Actor.CanMakeTelepathicContactWith(gameObject))
			{
				Actor.Fail("{{r|You cannot make telepathic contact with " + gameObject.t() + ".}}");
			}
			else if (gameObject.HasEffect<Shamed>())
			{
				IComponent<GameObject>.XDidYToZ(Actor, "attempt", "to shame", gameObject, "with " + Actor.its + " words, but " + gameObject.does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " already shamed", null, null, null, gameObject);
			}
			else
			{
				int duration = DURATION.RollCached();
				if (CanApplyEffectEvent.Check<Shamed>(gameObject, duration) && gameObject.ApplyEffect(new Shamed(duration)))
				{
					GenericCommandEvent.Send(Actor, COMMAND_ATTACK, null, gameObject, Source);
					IComponent<GameObject>.XDidYToZ(Actor, "shame", gameObject, "with " + Actor.its + " words", null, null, null, null, gameObject);
					num2++;
				}
				else
				{
					IComponent<GameObject>.XDidYToZ(Actor, "attempt", "to shame", gameObject, "with " + Actor.its + " words, but they have no effect", null, null, null, gameObject);
				}
			}
		}
		if (TargetCell.IsVisible())
		{
			scrapBuffer.WriteAt(TargetCell, (num2 > 0) ? "&Y#" : "&w*");
			textConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(50);
		}
		return num2;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Berate", COMMAND_NAME, "Skills", null, "\u0014");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
