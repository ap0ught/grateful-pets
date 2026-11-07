using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.World.Parts;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class PreparedCookingRecipieComponentBlueprint : ICookingRecipeComponent, IComposite
{
	public string ingredientBlueprint;

	public string ingredientDisplayName;

	public int amount;

	public PreparedCookingRecipieComponentBlueprint()
	{
	}

	public PreparedCookingRecipieComponentBlueprint(string ingredientType, string displayName = null, int amount = 1)
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
		string text = ((amount > 1) ? "servings" : "serving");
		if (doesPlayerHaveEnough())
		{
			return "{{C|" + amount + "}} " + text + " of " + ingredientDisplayName;
		}
		return "{{r|" + amount + "}} " + text + " of " + ingredientDisplayName;
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
				if (!self.Contains(item.Blueprint))
				{
					continue;
				}
				used.Add(item);
				PreparedCookingIngredient part = item.GetPart<PreparedCookingIngredient>();
				if (num > part.charges)
				{
					num -= part.charges;
					part.ParentObject.SplitFromStack();
					part.ParentObject.Destroy();
					goto IL_0007;
				}
				part.ParentObject.SplitFromStack();
				part.charges -= num;
				if (part.charges == 0)
				{
					part.ParentObject.Destroy();
				}
				else
				{
					part.ParentObject.CheckStack();
				}
				num = 0;
				break;
			}
			break;
		}
		CookingGameState.ResetInventorySnapshot();
	}
}
