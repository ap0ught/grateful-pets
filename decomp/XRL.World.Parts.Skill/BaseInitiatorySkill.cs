using System;
using System.Linq;
using XRL.Language;
using XRL.UI;
using XRL.World.Skills;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public abstract class BaseInitiatorySkill : BaseSkill
{
	public override bool ShowWaterRitualTagName(IBaseSkillEntry Entry)
	{
		return false;
	}

	public override string GetWaterRitualText(IBaseSkillEntry Entry)
	{
		return "I seek =skill.name=.";
	}

	public override int GetWaterRitualSkillPointCost(IBaseSkillEntry Entry)
	{
		return Entry.Cost;
	}

	/// <remarks>Invoked on stateless instance.</remarks>
	public virtual string GetCompletedText(GameObject Actor, GameObject Object, SkillEntry Skill, string Context)
	{
		return Actor.Does("have", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Actor.IsPlayer()) + " completed " + Skill.Name + ".";
	}

	/// <remarks>Invoked on stateless instance.</remarks>
	public virtual string GetExpendedText(GameObject Actor, GameObject Object, SkillEntry Skill, string Context)
	{
		if (Context.HasDelimitedSubstring(',', "TrainingBook"))
		{
			return Actor.Does("have", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Actor.IsPlayer()) + " already gleaned as many insights into " + Skill.Name + " from " + Object.t() + " as " + Actor.does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " going to.";
		}
		return null;
	}

	public override void ShowAddPopup(BeforeAddSkillEvent E)
	{
		if (E.Source == null || (!E.IsWaterRitual && !E.IsBook))
		{
			return;
		}
		string verb = (E.IsWaterRitual ? "lead" : "guide");
		IBaseSkillEntry entry = E.Entry;
		PowerEntry power = entry as PowerEntry;
		if (power != null && power.ParentSkill != null)
		{
			bool flag = power.ParentSkill.PowerList.Any((PowerEntry p) => p != power && !E.Actor.HasSkill(p.Class));
			E.Actor.ShowSuccess(E.Source.Does(verb, int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you through a mysterious rite. Your journey upon " + power.ParentSkill.Name + (flag ? " continues" : " has reached completion") + (power.Name.IsNullOrEmpty() ? "" : (" with initiation into " + power.Name)) + ".");
		}
		else if (E.Entry is SkillEntry)
		{
			E.Actor.ShowSuccess(E.Source.Does(verb, int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you through a rite of ancient mystery, one not for profane eyes or ears. You have begun your journey upon " + DisplayName + (E.Include.IsNullOrEmpty() ? "" : (" with initiation into " + Grammar.MakeAndList(E.Include.Select((IBaseSkillEntry x) => x.Name).ToList()))) + ".");
		}
	}

	public static IBaseSkillEntry GetSkillFor(GameObject Object, SkillEntry Skill)
	{
		if (!Object.HasPart(Skill.Class))
		{
			return Skill;
		}
		foreach (PowerEntry power in Skill.PowerList)
		{
			if (!Object.HasSkill(power.Class) && power.MeetsRequirements(Object))
			{
				return power;
			}
		}
		return null;
	}

	public static bool ShowCompletedPopup(GameObject Actor, GameObject Object, SkillEntry Skill, string Context)
	{
		if (Skill.Generic is BaseInitiatorySkill baseInitiatorySkill)
		{
			string completedText = baseInitiatorySkill.GetCompletedText(Actor, Object, Skill, Context);
			if (!completedText.IsNullOrEmpty())
			{
				Popup.Show(completedText);
				return true;
			}
		}
		return false;
	}

	public static bool ShowExpendedPopup(GameObject Actor, GameObject Object, SkillEntry Skill, string Context)
	{
		if (Skill.Generic is BaseInitiatorySkill baseInitiatorySkill)
		{
			string expendedText = baseInitiatorySkill.GetExpendedText(Actor, Object, Skill, Context);
			if (!expendedText.IsNullOrEmpty())
			{
				Popup.Show(expendedText);
				return true;
			}
		}
		return false;
	}

	public static string GetInitiatoryKey(GameObject Actor, IBaseSkillEntry Entry)
	{
		return Entry.Class + "_Initiated_By_" + Actor.ID;
	}
}
