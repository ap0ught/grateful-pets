using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class ThePorridge : CookingRecipe
{
	public ThePorridge()
	{
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Fermented Yuckwheat Stem"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Spark Tick Plasma"));
		Components.Add(new PreparedCookingRecipieComponentLiquid("honey"));
		Effects.Add(new CookingRecipeResultProceduralEffect(ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainMedicinal_DiseaseResistUnit" }, new List<string> { "CookingDomainMedicinal_OnEatYuckwheat" }, new List<string> { "CookingDomainElectric_Discharge_ProceduralCookingTriggeredAction" })));
	}

	public override string GetDescription()
	{
		return "+3 to saves vs. disease\nWhenever you eat an unfermented yuckwheat stem, you release an electrical discharge per Electrical Generation at level 5-6.";
	}

	public override string GetApplyMessage()
	{
		return "";
	}

	public override string GetDisplayName()
	{
		return "{{W|The Porridge}}";
	}
}
