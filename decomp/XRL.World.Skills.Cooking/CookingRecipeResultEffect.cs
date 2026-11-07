using System;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class CookingRecipeResultEffect : ICookingRecipeResult, IComposite
{
	public string effect;

	private CookingRecipeResultEffect()
	{
	}

	public CookingRecipeResultEffect(string effect)
	{
		this.effect = effect;
	}

	public string GetCampfireDescription()
	{
		return (Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + effect)) as Effect).GetDescription();
	}

	public string apply(GameObject eater)
	{
		Effect effect = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + this.effect)) as Effect;
		eater.ApplyEffect(effect);
		return effect.GetDescription();
	}
}
