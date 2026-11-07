using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class PreparedCookingRecipieComponentDomain : ICookingRecipeComponent, IComposite
{
	public string ingredientType;

	public int amount;

	public PreparedCookingRecipieComponentDomain()
	{
	}

	public PreparedCookingRecipieComponentDomain(string ingredientType, int amount = 1)
	{
		this.ingredientType = ingredientType;
		this.amount = amount;
	}

	public string getIngredientId()
	{
		return "prepared-" + ingredientType;
	}

	public bool HasPlants()
	{
		return false;
	}

	public bool HasFungi()
	{
		return false;
	}

	public string getDisplayName()
	{
		string text = ((amount > 1) ? "servings" : "serving");
		if (doesPlayerHaveEnough())
		{
			return "{{C|" + amount + "}} " + text + " of " + ingredientType;
		}
		return "{{r|" + amount + "}} " + text + " of " + ingredientType;
	}

	public int PlayerHolding()
	{
		int num = 0;
		foreach (GameObject item in CookingGameState.GetInventorySnapshot())
		{
			PreparedCookingIngredient part = item.GetPart<PreparedCookingIngredient>();
			if (part != null && part.type == ingredientType)
			{
				num += part.charges;
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
		return "You don't have enough " + ingredientType + ".";
	}

	public void use(List<GameObject> used)
	{
		int num = amount;
		Event e = Event.New("UsedAsIngredient", "Actor", The.Player);
		while (true)
		{
			using List<GameObject>.Enumerator enumerator = CookingGameState.GetInventorySnapshot().GetEnumerator();
			GameObject current;
			PreparedCookingIngredient part;
			do
			{
				if (enumerator.MoveNext())
				{
					current = enumerator.Current;
					part = current.GetPart<PreparedCookingIngredient>();
					continue;
				}
				return;
			}
			while (part == null || !(part.type == ingredientType));
			used.Add(current);
			current.FireEvent(e);
			if (num > part.charges)
			{
				num -= part.charges;
				part.ParentObject.SplitFromStack();
				part.ParentObject.Destroy();
				continue;
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
	}
}
