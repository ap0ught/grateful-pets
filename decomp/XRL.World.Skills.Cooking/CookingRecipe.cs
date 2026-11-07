using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Cysharp.Text;
using HistoryKit;
using Occult.Engine.CodeGeneration;
using Qud.API;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Skills.Cooking;

[Serializable]
[GenerateSerializationPartial]
public class CookingRecipe : IComposite
{
	public bool Hidden;

	public bool Favorite;

	/// <summary>
	/// Don't read from this. Call GetDisplayName() instead.
	/// </summary>
	public string DisplayName;

	public string ChefName;

	public List<ICookingRecipeComponent> Components = new List<ICookingRecipeComponent>();

	public List<ICookingRecipeResult> Effects = new List<ICookingRecipeResult>();

	[NonSerialized]
	private string CachedDisplayName;

	public Renderable Tile;

	public static List<string> ingredientTileTypes = new List<string>
	{
		"cake", "bread", "loaf", "slaw", "stew", "soup", "brisket", "borscht", "dip", "baklava",
		"compote", "hash", "porridge", "matz", "cookies", "yogurt", "goulash", "rice", "hummus", "knish",
		"broth", "kugel", "latkes", "schnitzel", "pancake", "roast", "shawarma", "flatbread", "meatballs", "pastry",
		"casserole", "dumpling", "doughnut", "tajine", "couscous", "dolma", "kebab", "fillet"
	};

	public static int EXTRA_RECIPE_TILES = 12;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.Write(Hidden);
		Writer.Write(Favorite);
		Writer.WriteOptimized(DisplayName);
		Writer.WriteOptimized(ChefName);
		Writer.WriteComposite(Components);
		Writer.WriteComposite(Effects);
		Writer.Write(Tile);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		Hidden = Reader.ReadBoolean();
		Favorite = Reader.ReadBoolean();
		DisplayName = Reader.ReadOptimizedString();
		ChefName = Reader.ReadOptimizedString();
		Components = Reader.ReadCompositeList<ICookingRecipeComponent>();
		Effects = Reader.ReadCompositeList<ICookingRecipeResult>();
		Tile = (Renderable)Reader.ReadComposite();
	}

	public virtual string GetApplyMessage()
	{
		return "";
	}

	public virtual string GetDisplayName()
	{
		if (CachedDisplayName == null)
		{
			using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
			utf16ValueStringBuilder.Append("{{W|");
			if (ChefName != null)
			{
				utf16ValueStringBuilder.Append(Grammar.MakePossessive(ChefName));
				utf16ValueStringBuilder.Append(' ');
			}
			utf16ValueStringBuilder.Append(DisplayName ?? GetType().Name);
			utf16ValueStringBuilder.Append("}}");
			CachedDisplayName = utf16ValueStringBuilder.ToString();
		}
		return CachedDisplayName;
	}

	public virtual Renderable GetRenderable()
	{
		if (Tile == null)
		{
			Tile = GenerateRecipeTile(this);
		}
		return Tile;
	}

	public virtual CookingRecipe DeepCopy()
	{
		CookingRecipe cookingRecipe = (CookingRecipe)Activator.CreateInstance(GetType());
		cookingRecipe.Hidden = Hidden;
		cookingRecipe.Favorite = Favorite;
		cookingRecipe.DisplayName = DisplayName;
		cookingRecipe.ChefName = ChefName;
		cookingRecipe.Components = new List<ICookingRecipeComponent>(Components);
		cookingRecipe.Effects = new List<ICookingRecipeResult>(Effects);
		cookingRecipe.Tile = new Renderable(Tile);
		return cookingRecipe;
	}

	public int CompareTo(CookingRecipe recipe)
	{
		if (recipe == this)
		{
			return 0;
		}
		bool flag = CheckIngredients();
		bool flag2 = recipe.CheckIngredients();
		if (flag == flag2 && Favorite == recipe.Favorite)
		{
			return GetDisplayName().CompareTo(recipe.GetDisplayName());
		}
		if (Favorite != recipe.Favorite)
		{
			if (Favorite)
			{
				return -1;
			}
			return 1;
		}
		if (flag)
		{
			return -1;
		}
		return 1;
	}

	public virtual bool HasPlants()
	{
		if (Components.Any((ICookingRecipeComponent r) => r.HasPlants()))
		{
			return true;
		}
		return false;
	}

	public virtual bool HasFungi()
	{
		if (Components.Any((ICookingRecipeComponent r) => r.HasFungi()))
		{
			return true;
		}
		return false;
	}

	public virtual string GetCampfireDescription()
	{
		return GetAnnotatedDisplayName(oneline: false, showQuantity: true) + "\n\n" + GetDescription();
	}

	public virtual string GetDescription()
	{
		string text = "";
		foreach (ICookingRecipeResult effect in Effects)
		{
			if (text != "")
			{
				text += "  ";
			}
			text += effect.GetCampfireDescription();
		}
		return text;
	}

	public virtual string GetIngredients()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int num = 0;
		foreach (ICookingRecipeComponent component in Components)
		{
			if (num != 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(component.getDisplayName());
			num++;
		}
		return stringBuilder.ToString();
	}

	public virtual string GetAnnotatedDisplayName()
	{
		return GetAnnotatedDisplayName(true, false);
	}

	public virtual string GetAnnotatedDisplayName(bool oneline = true, bool showQuantity = false)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (Favorite)
		{
			stringBuilder.Append("{{R|").Append('\u0003').Append("}} {{W|");
		}
		if (!CheckIngredients())
		{
			stringBuilder.Append("{{K|").Append(ConsoleLib.Console.ColorUtility.StripFormatting(GetDisplayName())).Append("}}");
		}
		else
		{
			stringBuilder.Append(GetDisplayName());
		}
		if (Favorite)
		{
			stringBuilder.Append("}}");
		}
		if (oneline)
		{
			stringBuilder.Append(" [");
		}
		else
		{
			stringBuilder.Append('\n');
		}
		int num = 0;
		foreach (ICookingRecipeComponent component in Components)
		{
			if (num != 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(component.getDisplayName());
			if (showQuantity)
			{
				stringBuilder.Append("{{K|(");
				stringBuilder.Append(CookingGameState.GetIngredientQuantity(component));
				stringBuilder.Append(")}}");
			}
			num++;
		}
		if (oneline)
		{
			stringBuilder.Append("]");
		}
		return stringBuilder.ToString();
	}

	public virtual bool CheckIngredients(bool displayMessage = false)
	{
		if (displayMessage)
		{
			bool result = true;
			string text = "";
			{
				foreach (ICookingRecipeComponent component in Components)
				{
					if (!component.doesPlayerHaveEnough())
					{
						if (!string.IsNullOrEmpty(text))
						{
							text += "\n";
						}
						text += component.createPlayerDoesNotHaveEnoughMessage();
						Popup.Show(text);
						return false;
					}
				}
				return result;
			}
		}
		foreach (ICookingRecipeComponent component2 in Components)
		{
			if (!component2.doesPlayerHaveEnough())
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool UseIngredients(List<GameObject> used)
	{
		foreach (ICookingRecipeComponent component in Components)
		{
			component.use(used);
		}
		return true;
	}

	public virtual bool ApplyEffectsTo(GameObject target, bool showMessage = true)
	{
		string text = "";
		if (showMessage)
		{
			text = GetApplyMessage();
		}
		foreach (ICookingRecipeResult effect in Effects)
		{
			text += effect.apply(target);
			text += "\n";
		}
		if (showMessage)
		{
			Popup.Show("You start to metabolize the meal, gaining the following effect for the rest of the day:\n\n{{W|" + Campfire.ProcessEffectDescription(text, target) + "}}");
		}
		return true;
	}

	public static CookingRecipe FromIngredients(List<string> ingredients, ProceduralCookingEffect effect = null, string chefName = null)
	{
		List<GameObject> list = new List<GameObject>(ingredients.Count);
		foreach (string ingredient in ingredients)
		{
			list.Add(GameObjectFactory.Factory.CreateObject(ingredient));
		}
		return FromIngredients(list, effect, chefName);
	}

	public static CookingRecipe FromIngredients(List<GameObject> ingredients, ProceduralCookingEffect effect = null, string chefName = null, string dishNames = "generic")
	{
		CookingRecipe cookingRecipe = new CookingRecipe();
		List<string> list = new List<string>(3);
		List<string> list2 = new List<string>(3);
		List<string> list3 = new List<string>(3);
		foreach (GameObject ingredient in ingredients)
		{
			if (ingredient == null)
			{
				Debug.LogError("Null in ingredient list");
			}
			else if (ingredient.HasPart<LiquidVolume>())
			{
				if (ingredient.LiquidVolume.GetPrimaryLiquid() == null)
				{
					continue;
				}
				string preparedCookingIngredientLiquidDomainPairs = ingredient.LiquidVolume.GetPreparedCookingIngredientLiquidDomainPairs();
				string[] array = preparedCookingIngredientLiquidDomainPairs.Split(',').GetRandomElement().Split(':');
				if (array.Length == 2)
				{
					string item = array[1];
					string item2 = array[0];
					if (!list.Contains(item))
					{
						list.Add(item);
					}
					cookingRecipe.Components.Add(new PreparedCookingRecipieComponentLiquid(ingredient.LiquidVolume.GetPrimaryLiquid().ID));
					list2.Add(item2);
					list3.Add(item2);
				}
				else
				{
					Debug.LogWarning("Weird return from liquid: " + preparedCookingIngredientLiquidDomainPairs);
				}
			}
			else if (ingredient.HasPart<PreparedCookingIngredient>())
			{
				PreparedCookingIngredient part = ingredient.GetPart<PreparedCookingIngredient>();
				string domain = part.GetTypeInstance();
				if (!list.Contains(domain))
				{
					list.Add(domain);
				}
				if (part.type == "random" && effect == null)
				{
					GameObject anObjectNoExclusions = EncountersAPI.GetAnObjectNoExclusions((GameObjectBlueprint ob) => ob.GetPartParameter("PreparedCookingIngredient", "type", "").Contains(domain) || (ob.HasTag("LiquidCookingIngredient") && ob.createSample().LiquidVolume.GetPreparedCookingIngredient().Contains(domain)));
					if (anObjectNoExclusions.HasTag("LiquidCookingIngredient"))
					{
						string iD = anObjectNoExclusions.LiquidVolume.GetPrimaryLiquid().ID;
						cookingRecipe.Components.Add(new PreparedCookingRecipieComponentLiquid(iD));
						list2.Add(iD);
						list3.Add(iD);
					}
					cookingRecipe.Components.Add(new PreparedCookingRecipieComponentBlueprint(anObjectNoExclusions.Blueprint, anObjectNoExclusions.GetTag("ServingName", anObjectNoExclusions.DisplayNameOnlyDirect)));
					list2.Add(anObjectNoExclusions.Blueprint);
					list3.Add(null);
				}
				else
				{
					cookingRecipe.Components.Add(new PreparedCookingRecipieComponentBlueprint(ingredient.Blueprint, ingredient.GetTag("ServingName", ingredient.DisplayNameOnlyDirect)));
					list2.Add(ingredient.Blueprint);
					list3.Add(null);
				}
			}
			else
			{
				Debug.LogWarning("Ingredient " + ingredient.Blueprint + " has neither LiquidVolume nor PreparedCookingIngredient parts");
			}
		}
		ProceduralCookingEffect effect2 = null;
		if (effect == null)
		{
			IEnumerable<string[]> enumerable = Algorithms.GeneratePermutations(list);
			List<List<string>> list4 = new List<List<string>>();
			foreach (string[] item5 in enumerable)
			{
				List<string> list5 = new List<string>(item5.Length);
				string[] array2 = item5;
				foreach (string item3 in array2)
				{
					list5.Add(item3);
				}
				list4.Add(list5);
			}
			list4.Add(null);
			list4.ShuffleInPlace();
			for (int num2 = 0; num2 < list4.Count; num2++)
			{
				if (list4[num2] != null && list4[num2].Count == 0)
				{
					throw new InvalidOperationException("There weren't any types in the selected ingredient type permutations? (should be impossible)");
				}
				if (list4[num2] == null || list4[num2].Count == 1)
				{
					effect2 = ProceduralCookingEffect.CreateJustUnits(list);
					break;
				}
				if (list4[num2].Count == 2)
				{
					if (Campfire.HasTriggers(list4[num2][0]) && Campfire.HasActions(list4[num2][1]))
					{
						effect2 = ProceduralCookingEffect.CreateTriggeredAction(list4[num2][0], list4[num2][1]);
						break;
					}
				}
				else if (Campfire.HasUnits(list4[num2][0]) && Campfire.HasTriggers(list4[num2][1]) && Campfire.HasActions(list4[num2][2]))
				{
					effect2 = ProceduralCookingEffect.CreateBaseAndTriggeredAction(list4[num2][0], list4[num2][1], list4[num2][2]);
					break;
				}
			}
		}
		else
		{
			effect2 = effect;
		}
		CookingRecipeResultProceduralEffect item4 = new CookingRecipeResultProceduralEffect(effect2);
		cookingRecipe.Effects.Add(item4);
		cookingRecipe.DisplayName = GenerateRecipeName(list2, list3, dishNames);
		cookingRecipe.ChefName = chefName;
		cookingRecipe.Tile = GenerateRecipeTile(cookingRecipe);
		return cookingRecipe;
	}

	public static string RollOvenSafeIngredient(string ingredientTable)
	{
		int num = 0;
		string blueprint;
		do
		{
			IL_0002:
			blueprint = PopulationManager.RollOneFrom(ingredientTable).Blueprint;
			num++;
			switch (blueprint)
			{
			case "Psychal Gland Paste":
			case "FluxPhial":
			case "CloningDraughtWaterskin_Ingredient":
			case "Drop of Nectar":
				goto IL_0002;
			case "Wild Rice":
				continue;
			}
			break;
		}
		while (num <= 39);
		if (num == 40)
		{
			return "Starapple Preserves";
		}
		return blueprint;
	}

	public static bool RecipeNameIsLike(string recipeName, string ingredient)
	{
		return recipeName.ToLower().Contains(ingredient);
	}

	public static Renderable GenerateRecipeTile(CookingRecipe recipie)
	{
		Renderable renderable = new Renderable();
		foreach (string ingredientTileType in ingredientTileTypes)
		{
			if (RecipeNameIsLike(recipie.GetDisplayName(), ingredientTileType))
			{
				renderable.Tile = "Items/sw_food_" + ingredientTileType + ".png";
			}
		}
		if (renderable.Tile == null)
		{
			List<string> list = new List<string>();
			list.AddRange(ingredientTileTypes.Select((string s) => "Items/sw_food_" + s + ".png"));
			for (int num = 0; num < EXTRA_RECIPE_TILES; num++)
			{
				list.Add($"Items/sw_food_{num}.png");
			}
			renderable.Tile = list.GetRandomElement();
		}
		List<string> list2 = new List<string>();
		list2.AddRange(Crayons.BrightColors);
		list2.AddRange(Crayons.DarkColors);
		renderable.DetailColor = list2.RemoveRandomElement()[0];
		renderable.ColorString = "&" + list2.RemoveRandomElement();
		return renderable;
	}

	public static string GenerateRecipeName(List<string> ingredients, List<string> liquids, string dishNames = "generic")
	{
		string input = "";
		int seed = Stat.Random(0, 2147483646);
		ingredients.ShuffleInPlace(new System.Random(seed));
		liquids.ShuffleInPlace(new System.Random(seed));
		if (ingredients.Count == 1)
		{
			string text = ((liquids[0] == null && GameObjectFactory.Factory.CreateObject(ingredients[0]).HasTag("AggregatedIngredient")) ? GameObjectFactory.Factory.CreateObject(ingredients[0]).GetTag("AggregatedIngredient") : ingredients[0]);
			string value = ((liquids[0] == null) ? GameObjectFactory.Factory.CreateObject(ingredients[0]).DisplayNameOnlyStripped : liquids[0]);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Set("$ingredient1", text);
			dictionary.Set(("$" + text + "DisplayName").Replace(" ", ""), value);
			dictionary.Set("*dishName*", "<spice.cooking.recipeNames.categorizedFoods." + dishNames.Split(',').GetRandomElement() + ".!random>");
			input = HistoricStringExpander.ExpandString("<spice.cooking.recipeNames.oneIngredientMeal.!random>", null, null, dictionary);
		}
		else if (ingredients.Count == 2)
		{
			string text2 = ((liquids[0] == null && GameObjectFactory.Factory.CreateObject(ingredients[0]).HasTag("AggregatedIngredient")) ? GameObjectFactory.Factory.CreateObject(ingredients[0]).GetTag("AggregatedIngredient") : ingredients[0]);
			string text3 = ((liquids[1] == null && GameObjectFactory.Factory.CreateObject(ingredients[1]).HasTag("AggregatedIngredient")) ? GameObjectFactory.Factory.CreateObject(ingredients[1]).GetTag("AggregatedIngredient") : ingredients[1]);
			string value2 = ((liquids[0] == null) ? GameObjectFactory.Factory.CreateObject(ingredients[0]).DisplayNameOnlyStripped : liquids[0]);
			string value3 = ((liquids[1] == null) ? GameObjectFactory.Factory.CreateObject(ingredients[1]).DisplayNameOnlyStripped : liquids[1]);
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			dictionary2.Set("$ingredient1", text2);
			dictionary2.Set(("$" + text2 + "DisplayName").Replace(" ", ""), value2);
			dictionary2.Set("$ingredient2", text3);
			dictionary2.Set(("$" + text3 + "DisplayName").Replace(" ", ""), value3);
			dictionary2.Set("*dishName*", "<spice.cooking.recipeNames.categorizedFoods." + dishNames.Split(',').GetRandomElement() + ".!random>");
			input = HistoricStringExpander.ExpandString("<spice.cooking.recipeNames.twoIngredientMeal.!random>", null, null, dictionary2);
		}
		else if (ingredients.Count == 3)
		{
			string text4 = ((liquids[0] == null && GameObjectFactory.Factory.CreateObject(ingredients[0]).HasTag("AggregatedIngredient")) ? GameObjectFactory.Factory.CreateObject(ingredients[0]).GetTag("AggregatedIngredient") : ingredients[0]);
			string text5 = ((liquids[1] == null && GameObjectFactory.Factory.CreateObject(ingredients[1]).HasTag("AggregatedIngredient")) ? GameObjectFactory.Factory.CreateObject(ingredients[1]).GetTag("AggregatedIngredient") : ingredients[1]);
			string text6 = ((liquids[2] == null && GameObjectFactory.Factory.CreateObject(ingredients[2]).HasTag("AggregatedIngredient")) ? GameObjectFactory.Factory.CreateObject(ingredients[2]).GetTag("AggregatedIngredient") : ingredients[2]);
			string value4 = ((liquids[0] == null) ? GameObjectFactory.Factory.CreateObject(ingredients[0]).DisplayNameOnlyStripped : liquids[0]);
			string value5 = ((liquids[1] == null) ? GameObjectFactory.Factory.CreateObject(ingredients[1]).DisplayNameOnlyStripped : liquids[1]);
			string value6 = ((liquids[2] == null) ? GameObjectFactory.Factory.CreateObject(ingredients[2]).DisplayNameOnlyStripped : liquids[2]);
			Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
			dictionary3.Set("$ingredient1", text4);
			dictionary3.Set(("$" + text4 + "DisplayName").Replace(" ", ""), value4);
			dictionary3.Set("$ingredient2", text5);
			dictionary3.Set(("$" + text5 + "DisplayName").Replace(" ", ""), value5);
			dictionary3.Set("$ingredient3", text6);
			dictionary3.Set(("$" + text6 + "DisplayName").Replace(" ", ""), value6);
			dictionary3.Set("*dishName*", "<spice.cooking.recipeNames.categorizedFoods." + dishNames.Split(',').GetRandomElement() + ".!random>");
			input = HistoricStringExpander.ExpandString("<spice.cooking.recipeNames.threeIngredientMeal.!random>", null, null, dictionary3);
		}
		input = HistoricStringExpander.ExpandString(input);
		return Grammar.MakeTitleCase(input.Replace("- ", "-"));
	}
}
