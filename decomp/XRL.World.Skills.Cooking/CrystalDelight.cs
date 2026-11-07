using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class CrystalDelight : CookingRecipe
{
	public CrystalDelight()
	{
		Components.Add(new PreparedCookingRecipeUnusualComponentBlueprint("Crystal of Eve"));
		Components.Add(new PreparedCookingRecipieComponentLiquid("warmstatic"));
		Components.Add(new PreparedCookingRecipeUnusualComponentBlueprint("GlitterGrenade3"));
		Components.Add(new PreparedCookingRecipeUnusualComponentBlueprint("Gentling Collar"));
		Effects.Add(new CookingRecipeResultProceduralEffect(ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainSpecial_UnitCrystalTransform" })));
	}

	public override string GetDescription()
	{
		return "?????";
	}

	public override string GetApplyMessage()
	{
		return "";
	}

	public override string GetDisplayName()
	{
		return "{{W|Crystal Delight}}";
	}

	public override bool ApplyEffectsTo(GameObject target, bool showMessage = true)
	{
		string text = "";
		if (showMessage)
		{
			text = GetApplyMessage();
		}
		foreach (ICookingRecipeResult effect in Effects)
		{
			text += effect.apply(target);
			text += "\n";
		}
		return true;
	}
}
