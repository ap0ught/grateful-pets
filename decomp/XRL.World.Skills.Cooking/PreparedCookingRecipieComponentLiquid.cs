using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class PreparedCookingRecipieComponentLiquid : ICookingRecipeComponent, IComposite
{
	public string liquid;

	public int amount;

	public PreparedCookingRecipieComponentLiquid()
	{
	}

	public PreparedCookingRecipieComponentLiquid(string liquid, int amount = 1)
	{
		this.liquid = liquid;
		this.amount = amount;
	}

	public string getIngredientId()
	{
		return "liquid-" + liquid;
	}

	public bool HasPlants()
	{
		if (liquid == "sap")
		{
			return true;
		}
		if (liquid == "cider")
		{
			return true;
		}
		return false;
	}

	public bool HasFungi()
	{
		return false;
	}

	public string getDisplayName()
	{
		string text = ((amount > 1) ? "drams" : "dram");
		if (doesPlayerHaveEnough())
		{
			return amount + " " + text + " of " + LiquidVolume.GetLiquid(liquid).GetName();
		}
		return "{{r|" + amount + "}} " + text + " of " + LiquidVolume.GetLiquid(liquid).GetName();
	}

	public int PlayerHolding()
	{
		int num = 0;
		foreach (GameObject item in CookingGameState.GetInventorySnapshot())
		{
			LiquidVolume part = item.GetPart<LiquidVolume>();
			if (part != null && part.IsPureLiquid(liquid))
			{
				num += part.Volume;
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
		return "You don't have enough " + LiquidVolume.GetLiquid(liquid).GetName(null) + "&Y.";
	}

	public void use(List<GameObject> used)
	{
		int num = amount;
		while (true)
		{
			IL_0007:
			foreach (GameObject item in CookingGameState.GetInventorySnapshot())
			{
				LiquidVolume part = item.GetPart<LiquidVolume>();
				if (part != null && part.IsPureLiquid(liquid))
				{
					used.Add(item);
					if (num > part.Volume)
					{
						num -= part.Volume;
						part.Volume = 0;
						part.Empty();
						goto IL_0007;
					}
					part.UseDrams(num);
					break;
				}
			}
			break;
		}
		CookingGameState.ResetInventorySnapshot();
	}
}
