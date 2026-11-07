using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class GoatAndSweetLeaf : CookingRecipe
{
	public GoatAndSweetLeaf()
	{
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Goat Jerky"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Sun-Dried Banana"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Fermented Yondercane"));
		Effects.Add(new CookingRecipeResultProceduralEffect(ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainArtifact_UnitPsychometry" }, new List<string> { "CookingDomainHP_OnDamaged" }, new List<string> { "CookingDomainTeleport_MassTeleportOther_ProceduralCookingTriggeredAction" })));
	}

	public override string GetDescription()
	{
		return "Can use Psychometry at level 4-5.\nWhenever you take damage, there's a 12-15% chance you teleport all creatures surrounding you.";
	}

	public override string GetApplyMessage()
	{
		return "";
	}

	public override string GetDisplayName()
	{
		return "{{W|Goat in Sweet Leaf}}";
	}
}
