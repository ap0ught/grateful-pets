using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Skills.Cooking;

namespace XRL.World.Parts;

[Serializable]
public class PetEbenshabat : IPart
{
	public override void RegisterActive(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(The.Game, PooledEvent<AfterLevelGainedEvent>.ID);
	}

	public override bool HandleEvent(AfterLevelGainedEvent E)
	{
		if (E.Actor.IsPlayer() && E.Actor == ParentObject.PartyLeader && E.Actor.InSameZone(ParentObject) && Stat.Random(1, 100) <= 40)
		{
			string ingredientTable = "Ingredients" + E.Actor.GetTier();
			List<string> list = new List<string> { "Starapple Preserves" };
			int num = ((Stat.Random(1, 100) <= 50) ? 3 : 2);
			for (int i = 0; i < 100; i++)
			{
				if (list.Count >= num)
				{
					break;
				}
				string item = CookingRecipe.RollOvenSafeIngredient(ingredientTable);
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			CookingRecipe cookingRecipe = CookingGameState.LearnRecipe(CookingRecipe.FromIngredients(list, null, ParentObject.DisplayNameOnlyDirectAndStripped));
			DidXToY("teach", E.Actor, cookingRecipe.GetDisplayName(), "!");
		}
		return base.HandleEvent(E);
	}
}
