using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class MushroomCider : CookingRecipe
{
	public MushroomCider()
	{
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Pickled Mushrooms"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Starapple Preserves"));
		Components.Add(new PreparedCookingRecipieComponentLiquid("cider"));
		Effects.Add(new CookingRecipeResultProceduralEffect(ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainQuickness_UnitQuickness", "CookingDomainRegenLowtier_BleedResistUnit", "CookingDomainFungus_FungusResistUnit" })));
	}

	public override string GetDescription()
	{
		return "+4-5 Quickness\n+8-12 to saves vs. bleeding\n75% chance that itchy skin doesn't develop into a fungal infection.";
	}

	public override string GetApplyMessage()
	{
		return "";
	}

	public override string GetDisplayName()
	{
		return "{{W|Mulled Mushroom Cider}}";
	}
}
