using System;
using System.Collections.Generic;
using System.Linq;
using Qud.API;
using XRL.Core;
using XRL.World.Parts.Mutation;

namespace XRL.World.Skills.Cooking;

[Serializable]
[GameStateSingleton]
public class CookingGameState : IGameStateSingleton, IComposite
{
	public static List<GameObject> inventorySnapshot;

	public static Dictionary<string, int> ingredientQuantity;

	public List<CookingRecipe> knownRecipies = new List<CookingRecipe>();

	public bool WantFieldReflection => false;

	public static CookingGameState instance => XRLCore.Core.Game.GetObjectGameState("CookingGameState") as CookingGameState;

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteComposite(knownRecipies);
	}

	public void Read(SerializationReader Reader)
	{
		knownRecipies = Reader.ReadCompositeList<CookingRecipe>();
	}

	public static List<GameObject> GetInventorySnapshot()
	{
		if (inventorySnapshot == null)
		{
			bool carnivorous = The.Player.HasPart<Carnivorous>();
			inventorySnapshot = The.Player.GetInventoryDirectAndEquipment((GameObject go) => (!carnivorous || (!go.HasTag("Plant") && !go.HasTag("Fungus"))) ? true : false);
		}
		return inventorySnapshot;
	}

	public static void ResetInventorySnapshot()
	{
		inventorySnapshot = null;
		if (ingredientQuantity != null)
		{
			ingredientQuantity.Clear();
		}
	}

	public static int GetIngredientQuantity(ICookingRecipeComponent component)
	{
		if (ingredientQuantity == null)
		{
			ingredientQuantity = new Dictionary<string, int>();
		}
		string ingredientId = component.getIngredientId();
		if (!ingredientQuantity.ContainsKey(ingredientId))
		{
			ingredientQuantity[ingredientId] = component.PlayerHolding();
		}
		return ingredientQuantity[ingredientId];
	}

	public static bool KnowsRecipe(CookingRecipe newRecipe)
	{
		return instance.knownRecipies.Any((CookingRecipe i) => newRecipe != null && i != null && newRecipe.GetDisplayName() == i.GetDisplayName());
	}

	public static bool KnowsRecipe(string ClassName)
	{
		if (ClassName.IsNullOrEmpty())
		{
			return false;
		}
		return instance.knownRecipies.Any((CookingRecipe i) => i.GetType().Name == ClassName);
	}

	public static CookingRecipe LearnRecipe(CookingRecipe newRecipe, GameObject Chef = null)
	{
		JournalAPI.AddRecipeNote(newRecipe, Chef);
		return newRecipe;
	}

	public void Initialize()
	{
	}
}
[Serializable]
[Obsolete]
public class CookingGamestate : IGamestateSingleton, IGamestatePostload
{
	public List<CookingRecipe> knownRecipies = new List<CookingRecipe>();

	public void OnGamestatePostload(XRLGame game, SerializationReader reader)
	{
		CookingGameState cookingGameState = new CookingGameState();
		cookingGameState.knownRecipies = knownRecipies;
		game.ObjectGameState["CookingGameState"] = cookingGameState;
		game.ObjectGameState.Remove("cookingGamestate");
	}
}
