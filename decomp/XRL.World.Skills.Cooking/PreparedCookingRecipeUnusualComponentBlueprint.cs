using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.World.Parts;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class PreparedCookingRecipeUnusualComponentBlueprint : ICookingRecipeComponent, IComposite
{
	public string ingredientBlueprint;

	public string ingredientDisplayName;

	public int amount;

	public PreparedCookingRecipeUnusualComponentBlueprint()
	{
	}

	public PreparedCookingRecipeUnusualComponentBlueprint(string ingredientType, string displayName = null, int amount = 1)
	{
		ingredientBlueprint = ingredientType;
		if (displayName == null)
		{
			if (ingredientType.Contains('|'))
			{
				ingredientDisplayName = Grammar.MakeOrList(ingredientType.Split('|'));
			}
			else
			{
				ingredientDisplayName = GameObjectFactory.Factory.CreateObject(ingredientBlueprint).DisplayNameOnlyDirect;
			}
		}
		else
		{
			ingredientDisplayName = displayName;
		}
		this.amount = amount;
	}

	public string getIngredientId()
	{
		return "blueprint-" + ingredientBlueprint;
	}

	public bool HasPlants()
	{
		string[] array = ingredientBlueprint.Split('|');
		foreach (string key in array)
		{
			if (GameObjectFactory.Factory.Blueprints[key].HasTag("Plant"))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFungi()
	{
		string[] array = ingredientBlueprint.Split('|');
		foreach (string key in array)
		{
			if (GameObjectFactory.Factory.Blueprints[key].HasTag("Fungus"))
			{
				return true;
			}
		}
		return false;
	}

	public string getDisplayName()
	{
		GameObject gameObject = GameObjectFactory.Factory.Blueprints[ingredientBlueprint].createSample();
		if (doesPlayerHaveEnough())
		{
			if (amount > 1)
			{
				return "{{C|" + amount + "}} " + gameObject.GetPluralName();
			}
			return "{{C|" + amount + "}} " + ingredientDisplayName;
		}
		if (amount > 1)
		{
			return "{{r|" + amount + "}} " + gameObject.GetPluralName();
		}
		return "{{r|" + amount + "}} " + ingredientDisplayName;
	}

	public int PlayerHolding()
	{
		int num = 0;
		string[] self = ingredientBlueprint.Split('|');
		foreach (GameObject item in CookingGameState.GetInventorySnapshot())
		{
			if (self.Contains(item.Blueprint))
			{
				num += item.Count;
			}
		}
		return num;
	}

	public bool doesPlayerHaveEnough()
	{
		return amount <= CookingGameState.GetIngredientQuantity(this);
	}

	public string createPlayerDoesNotHaveEnoughMessage()
	{
		return "You don't have enough servings of " + ingredientDisplayName + ".";
	}

	public void use(List<GameObject> used)
	{
		int num = amount;
		while (true)
		{
			IL_0007:
			string[] self = ingredientBlueprint.Split('|');
			foreach (GameObject item in CookingGameState.GetInventorySnapshot())
			{
				if (self.Contains(item.Blueprint))
				{
					used.Add(item);
					item.GetPart<PreparedCookingIngredient>();
					if (num > 0)
					{
						num--;
						item.SplitFromStack();
						item.Destroy();
						goto IL_0007;
					}
					break;
				}
			}
			break;
		}
		CookingGameState.ResetInventorySnapshot();
	}
}
