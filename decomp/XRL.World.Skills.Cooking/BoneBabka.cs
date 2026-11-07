using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class BoneBabka : CookingRecipe
{
	public BoneBabka()
	{
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Compacted Bone Meal"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Sun-Dried Banana"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Congealed Skulk"));
		Effects.Add(new CookingRecipeResultProceduralEffect(ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainBurrowing_UnitBurrowingClaws" }, new List<string> { "CookingDomainArtifact_OnIdentify" }, new List<string> { "CookingDomainArmor_LargeAVBuff_ProceduralCookingTriggeredAction" })));
	}

	public override string GetDescription()
	{
		return "Can use Burrowing Claws at level 5-6. If you already have Burrowing Claws, it's enhanced by 3-4 levels.\nWhenever you identify an artifact, you gain +6 AV for 50 turns.";
	}

	public override string GetApplyMessage()
	{
		return "";
	}

	public override string GetDisplayName()
	{
		return "{{W|Bone Babka}}";
	}
}
