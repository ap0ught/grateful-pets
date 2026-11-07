using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class HotandSpiny : CookingRecipe
{
	public HotandSpiny()
	{
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Cured Dawnglider Tail"));
		Components.Add(new PreparedCookingRecipieComponentBlueprint("Spine Fruit Jam"));
		Effects.Add(new CookingRecipeResultProceduralEffect(ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainHeat_UnitResist", "CookingDomainReflect_UnitReflectDamage" })));
	}

	public override string GetDescription()
	{
		return "+10-15 Heat Resist\nReflect 3-4% damage back at your attackers, rounded up.";
	}

	public override string GetApplyMessage()
	{
		return "";
	}

	public override string GetDisplayName()
	{
		return "{{W|Hot and Spiny}}";
	}
}
