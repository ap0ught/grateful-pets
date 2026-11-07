using System;
using System.Linq;
using XRL.Wish;
using XRL.World.Skills;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
[HasWishCommand]
public class TenfoldPath : BaseInitiatorySkill
{
	[WishCommand(null, null, Command = "tenfold")]
	public static bool HandleTenfoldPathInitiationWish()
	{
		if (The.Player.HasSkill("TenfoldPath"))
		{
			PowerEntry powerEntry = SkillFactory.Factory.SkillByClass["TenfoldPath"].Powers.Values.FirstOrDefault((PowerEntry p) => !The.Player.HasSkill(p.Class) && p.MeetsRequirements(The.Player));
			if (powerEntry != null)
			{
				The.Player.AddSkill(powerEntry.Class);
			}
		}
		else
		{
			The.Player.AddSkill("TenfoldPath");
		}
		return true;
	}
}
