using System;
using XRL.World.Skills.Cooking;

namespace XRL.World.Parts;

[Serializable]
public class TeachesDish : IPart
{
	public string Text;

	public CookingRecipe Recipe;

	public TeachesDish()
	{
	}

	public TeachesDish(CookingRecipe recipe, string text)
	{
		Recipe = recipe;
		Text = text;
	}
}
