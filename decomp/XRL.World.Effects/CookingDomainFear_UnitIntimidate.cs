using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFear_UnitIntimidate : ProceduralCookingEffectUnitSkill<Persuasion_Intimidate>
{
	public override string GetDescription()
	{
		if (skillWasAdded)
		{
			return "Can use " + SkillDisplayName + ".";
		}
		return (Tier * 2).Signed() + " bonus on Ego roll when using Intimidate.";
	}

	public override string GetTemplatedDescription()
	{
		string text = "Can use " + SkillDisplayName + ".";
		if (BonusTier != "0")
		{
			text = text + " If @they already have " + SkillDisplayName + ", gain a " + (Convert.ToInt32(BonusTier) * 2).Signed() + " bonus on the Ego roll when using " + SkillDisplayName + ".";
		}
		return text;
	}
}
