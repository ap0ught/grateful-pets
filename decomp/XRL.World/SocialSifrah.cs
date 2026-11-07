using System.Collections.Generic;
using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
public abstract class SocialSifrah : SifrahGame
{
	public static readonly string CATEGORY = "Social";

	public static bool AnyEnabled
	{
		get
		{
			if (!Options.SifrahHaggling)
			{
				return Options.SifrahRecruitment;
			}
			return true;
		}
	}

	public SocialSifrah()
	{
		CorrectTokenSound = "Sounds/Interact/sfx_artifact_examination_success_total";
		CorrectTokenSoundDelay = 300;
		IncorrectTokenSound = "Sounds/Creatures/VO/sfx_creature_animal_grazingAnimal_vo_die";
		IncorrectTokenSoundDelay = 300;
	}

	public override string GetSifrahCategory()
	{
		return CATEGORY;
	}

	public static void AwardInsight()
	{
		if (AnyEnabled)
		{
			SifrahGame.AwardInsight(CATEGORY, "social interaction");
		}
	}

	public static void AddTokenTokens(List<SifrahPrioritizableToken> List, GameObject ContextObject)
	{
		if (ContextObject.IsMemberOfFaction("Farmers") || ContextObject.IsMemberOfFaction("Wardens"))
		{
			List.Add(new SocialSifrahTokenDisplayAFarmersToken());
		}
		if (ContextObject.IsMemberOfFaction("Merchants") || ContextObject.IsMemberOfFaction("Dromad") || ContextObject.IsMemberOfFaction("Consortium"))
		{
			List.Add(new SocialSifrahTokenDisplayAMerchantsToken());
		}
		if (ContextObject.IsMemberOfFaction("Birds") || ContextObject.IsMemberOfFaction("Prey") || ContextObject.IsMemberOfFaction("Antelopes") || ContextObject.IsMemberOfFaction("Equines") || ContextObject.IsMemberOfFaction("Water"))
		{
			List.Add(new SocialSifrahTokenDisplayAMinstrelsToken());
		}
		if (ContextObject.IsMemberOfFaction("Barathrumites"))
		{
			List.Add(new SocialSifrahTokenDisplayABarathrumiteToken());
		}
	}
}
