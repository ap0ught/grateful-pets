using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class TongueAndCheek : CookingRecipe
{
	public TongueAndCheek()
	{
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Fermented Tongue"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Sliced Bop Cheek"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Congealed Love"));
		Effects.Add(new CookingRecipeResultProceduralEffect(ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainTongue_UnitStickyTongue" }, new List<string> { "CookingDomainRubber_OnJump" }, new List<string> { "CookingDomainLove_BeguilingCharge_ProceduralCookingTriggeredAction" })));
	}

	public override string GetDescription()
	{
		return "Can use Sticky Tongue at rank 4-5.\nWhenever you jump, you beguile a creature as per Beguiling at rank 7-8 for the duration of this effect.";
	}

	public override string GetApplyMessage()
	{
		return "";
	}

	public override string GetDisplayName()
	{
		return "{{W|Tongue and Cheek}}";
	}
}
