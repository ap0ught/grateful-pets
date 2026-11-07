using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class AppleMatz : CookingRecipe
{
	public AppleMatz()
	{
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Vinewafer Sheaf"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Starapple Preserves"));
		Effects.Add(new CookingRecipeResultProceduralEffect(ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainRegenLowtier_RegenerationUnit", "ProceduralCookingEffectUnit_LessThirst" })));
	}

	public override string GetDescription()
	{
		return "+10-15% to natural healing rate\nYou thirst at half rate.";
	}

	public override string GetApplyMessage()
	{
		return "";
	}

	public override string GetDisplayName()
	{
		return "{{W|Apple Matz}}";
	}
}
