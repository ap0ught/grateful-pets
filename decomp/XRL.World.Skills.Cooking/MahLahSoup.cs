using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class MahLahSoup : CookingRecipe
{
	public MahLahSoup()
	{
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Dried Lah Petals"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Vinewafer Sheaf"));
		Effects.Add(new CookingRecipeResultProceduralEffect(ProceduralCookingEffect.CreateSpecific(null, new List<string> { "OnDrinkWaterProceduralCookingTrigger" }, new List<string> { "CookingDomainFear_FearImmunity_ProceduralCookingTriggeredAction" })));
	}

	public override string GetDescription()
	{
		return "Whenever you drink freshwater, there's a 25% chance you become immune to fear for 6 hours.";
	}

	public override string GetApplyMessage()
	{
		return "";
	}

	public override string GetDisplayName()
	{
		return "{{W|Mah Lah Soup}}";
	}
}
