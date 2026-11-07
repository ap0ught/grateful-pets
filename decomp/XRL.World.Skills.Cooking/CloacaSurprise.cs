using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class CloacaSurprise : CookingRecipe
{
	public CloacaSurprise()
	{
		Components.Add(new PreparedCookingRecipieComponentLiquid("goo"));
		Components.Add(new PreparedCookingRecipieComponentLiquid("sludge"));
		Components.Add(new PreparedCookingRecipieComponentLiquid("ooze"));
		Effects.Add(new CookingRecipeResultProceduralEffect(ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainSpecial_UnitSlogTransform" })));
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
		return "{{W|Cloaca Surprise}}";
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
