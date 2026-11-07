using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using Qud.UI;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Collections;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using XRL.World.Skills.Cooking;

namespace XRL.World.Parts;

[Serializable]
public class Campfire : IActivePart
{
	public const string COOKING_PREFIX = "ProceduralCookingIngredient_";

	public string ExtinguishBlueprint;

	public string PresetMeals = "";

	public List<CookingRecipe> specificProcgenMeals;

	[NonSerialized]
	protected List<CookingRecipe> _presetMeals;

	public static readonly string COOK_COMMAND_RECIPE = "CookFromRecipe";

	public static readonly string COOK_COMMAND_WHIP_UP = "CookWhipUp";

	public static readonly string COOK_COMMAND_CHOOSE = "CookChooseIngredients";

	public static readonly string COOK_COMMAND_PRESERVE = "CookPreserve";

	public static readonly string COOK_COMMAND_PRESERVE_EXOTIC = "CookPreserveExotic";

	public static readonly string COOK_COMMAND_PRESET_MEAL = "CookPresetMeal:";

	public static readonly string COOK_COMMAND_STOP_BLEEDING = "CookStopBleeding";

	public static readonly string COOK_COMMAND_TREAT_POISON = "CookTreatPoison";

	public static readonly string COOK_COMMAND_TREAT_ILLNESS = "CookTreatIllness";

	public static readonly string COOK_COMMAND_TREAT_DISEASE_ONSET = "CookTreatDiseaseOnset";

	public List<CookingRecipe> presetMeals => _presetMeals ?? (_presetMeals = ParsePresetMeals());

	public static Stomach pStomach => The.Player.GetPart<Stomach>();

	public static bool hasSkill => The.Player.HasSkill("CookingAndGathering");

	public Campfire()
	{
		WorksOnSelf = true;
	}

	public Campfire(List<CookingRecipe> specificProcgenMeals)
		: this()
	{
		this.specificProcgenMeals = specificProcgenMeals;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != GetCookingActionsEvent.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetPointsOfInterestEvent>.ID && ID != InventoryActionEvent.ID)
		{
			return ID == SingletonEvent<RadiatesHeatEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (Cook())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null && !cell.IsSolidForOtherThan(E.Actor, ParentObject))
			{
				string propertyOrTag = ParentObject.GetPropertyOrTag("PointOfInterestKey");
				bool flag = true;
				string explanation = null;
				if (!propertyOrTag.IsNullOrEmpty())
				{
					PointOfInterest pointOfInterest = E.Find(propertyOrTag);
					if (pointOfInterest != null)
					{
						if (ParentObject.DistanceTo(E.Actor) < pointOfInterest.GetDistanceTo(E.Actor))
						{
							E.Remove(pointOfInterest);
							explanation = "nearest";
						}
						else
						{
							flag = false;
							pointOfInterest.Explanation = "nearest";
						}
					}
				}
				if (flag)
				{
					E.Add(ParentObject, ParentObject.GetReferenceDisplayName(), explanation, propertyOrTag, null, null, null, 1);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatEvent E)
	{
		if (!ParentObject.HasTagOrProperty("CampfireHeatSelfOnly") && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return true;
		}
		if (!ExtinguishBlueprint.IsNullOrEmpty())
		{
			GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("LiquidVolume", CanExtinguish);
			if (firstObjectWithPart != null)
			{
				Extinguish(null, firstObjectWithPart);
				return true;
			}
		}
		if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ParentObject.Physics != null && ParentObject.Physics.Temperature < 600 && Stat.Random5(1, 100) <= 10)
			{
				ParentObject.TemperatureChange(150, ParentObject, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, 5);
			}
			if (cell.Objects.Count > 1 && !ParentObject.HasTagOrProperty("CampfireHeatSelfOnly"))
			{
				int phase = ParentObject.GetPhase();
				int i = 0;
				for (int count = cell.Objects.Count; i < count; i++)
				{
					GameObject gameObject = cell.Objects[i];
					if (gameObject != ParentObject)
					{
						Physics physics = gameObject.Physics;
						if (physics != null && physics.Temperature < 600 && (!gameObject.IsCombat || Stat.Random5(1, 100) <= 10))
						{
							gameObject.TemperatureChange(150, ParentObject, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, phase);
						}
					}
				}
			}
		}
		if (!ExtinguishBlueprint.IsNullOrEmpty() && ParentObject.IsFrozen())
		{
			Extinguish();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Cook", "cook", "Cook", null, 'c', FireOnActor: false, 10);
		if (!ExtinguishBlueprint.IsNullOrEmpty())
		{
			E.AddAction("Extinguish", "extinguish", "ExtinguishCampfire", null, 'x', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	protected List<CookingRecipe> ParsePresetMeals()
	{
		List<CookingRecipe> list = new List<CookingRecipe>();
		if (!PresetMeals.IsNullOrEmpty())
		{
			string[] array = PresetMeals.Split(',');
			foreach (string text in array)
			{
				CookingRecipe item = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Skills.Cooking." + text)) as CookingRecipe;
				list.Add(item);
			}
		}
		else if (specificProcgenMeals != null)
		{
			list.AddRange(specificProcgenMeals);
		}
		return list;
	}

	public static string EnabledDisplay(bool Enabled, string Display)
	{
		return (Enabled ? "" : "&K") + Display;
	}

	public override bool HandleEvent(GetCookingActionsEvent E)
	{
		bool flag = CookingGameState.instance.knownRecipies.Count > 0;
		bool flag2 = IsHungry(The.Player);
		bool flag3 = The.Player.HasSkill("CookingAndGathering");
		List<GameObject> inventoryDirect = The.Player.GetInventoryDirect((GameObject go) => go.HasPart<PreservableItem>() && !go.HasTag("ChooseToPreserve") && !go.IsTemporary && !go.IsImportant());
		List<GameObject> inventoryDirect2 = The.Player.GetInventoryDirect((GameObject go) => go.HasPart<PreservableItem>() && go.HasTag("ChooseToPreserve") && !go.IsTemporary && go.Understood());
		if (presetMeals.Count > 0)
		{
			for (int num = 0; num < presetMeals.Count; num++)
			{
				CookingRecipe cookingRecipe = presetMeals[num];
				string display = ParentObject.GetTag("PresetMealMessage") ?? ("Eat " + cookingRecipe?.GetDisplayName().Replace("&W", "&Y").Replace("{{W|", "{{Y|"));
				E.AddAction(COOK_COMMAND_PRESET_MEAL + num, Command: COOK_COMMAND_PRESET_MEAL + num, Key: (num >= 5) ? ' ' : ((char)(97 + num)), Display: EnabledDisplay(flag2, display), PreferToHighlight: null, FireOnActor: false, Default: 100 + presetMeals.Count - num, Priority: 100 + presetMeals.Count - num);
			}
		}
		E.AddAction(COOK_COMMAND_WHIP_UP, Command: COOK_COMMAND_WHIP_UP, Display: EnabledDisplay(flag2, "Whip up a meal."), PreferToHighlight: null, Key: 'm', FireOnActor: false, Default: 10, Priority: 90);
		E.AddAction(COOK_COMMAND_CHOOSE, EnabledDisplay(flag3 & flag2, "Choose ingredients to cook with."), COOK_COMMAND_CHOOSE, null, 'i', FireOnActor: false, 0, 80);
		E.AddAction(COOK_COMMAND_RECIPE, EnabledDisplay(flag3 & flag & flag2, "Cook from a recipe."), COOK_COMMAND_RECIPE, null, 'r', FireOnActor: false, 0, 70);
		E.AddAction(COOK_COMMAND_PRESERVE, EnabledDisplay(flag3 && inventoryDirect.Count > 0, "Preserve your fresh foods."), COOK_COMMAND_PRESERVE, null, 'f', FireOnActor: false, 0, 60);
		if (flag3 && inventoryDirect2.Count > 0)
		{
			E.AddAction(COOK_COMMAND_PRESERVE_EXOTIC, EnabledDisplay(Enabled: true, "Preserve your exotic foods."), COOK_COMMAND_PRESERVE_EXOTIC, null, 'x', FireOnActor: false, 0, 50);
		}
		if (The.Player.HasSkill("Physic_Nostrums"))
		{
			E.AddAction(COOK_COMMAND_STOP_BLEEDING, EnabledDisplay(NostrumsCanStopBleeding(), "Stop bleeding."), COOK_COMMAND_STOP_BLEEDING, null, 'b', FireOnActor: false, 0, 40);
			E.AddAction(COOK_COMMAND_TREAT_POISON, EnabledDisplay(NostrumsCanTreatPoison(), "Treat poison."), COOK_COMMAND_TREAT_POISON, null, 'p', FireOnActor: false, 0, 30);
			E.AddAction(COOK_COMMAND_TREAT_ILLNESS, EnabledDisplay(NostrumsCanTreatIllness(), "Treat illness."), COOK_COMMAND_TREAT_ILLNESS, null, 'l', FireOnActor: false, 0, 20);
			E.AddAction(COOK_COMMAND_TREAT_DISEASE_ONSET, EnabledDisplay(NostrumsCanTreatDiseaseOnset(), "Treat disease onset."), COOK_COMMAND_TREAT_DISEASE_ONSET, null, 'd', FireOnActor: false, 0, 10);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		try
		{
			if (E.Command == "Cook")
			{
				if (!Cook())
				{
					return false;
				}
			}
			else if (E.Command == "ExtinguishCampfire")
			{
				if (!ExtinguishBlueprint.IsNullOrEmpty() && E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
				{
					Extinguish(E.Actor);
				}
			}
			else
			{
				if (E.Command == COOK_COMMAND_WHIP_UP)
				{
					if (CookFromIngredients(random: true))
					{
						E.RequestInterfaceExit();
					}
					return true;
				}
				if (E.Command == COOK_COMMAND_CHOOSE)
				{
					if (CookFromIngredients(random: false))
					{
						E.RequestInterfaceExit();
					}
					return true;
				}
				if (E.Command == COOK_COMMAND_RECIPE)
				{
					if (CookFromRecipe())
					{
						E.RequestInterfaceExit();
					}
					return true;
				}
				if (E.Command == COOK_COMMAND_PRESERVE)
				{
					Preserve();
					return true;
				}
				if (E.Command == COOK_COMMAND_PRESERVE_EXOTIC)
				{
					PreserveExotic();
					return true;
				}
				if (E.Command.StartsWith(COOK_COMMAND_PRESET_MEAL))
				{
					int index = Convert.ToInt32(E.Command.Substring(COOK_COMMAND_PRESET_MEAL.Length));
					CookPresetMeal(index);
					E.RequestInterfaceExit();
					return true;
				}
				if (E.Command == COOK_COMMAND_STOP_BLEEDING)
				{
					NostrumsStopBleeding();
					return true;
				}
				if (E.Command == COOK_COMMAND_TREAT_POISON)
				{
					NostrumsTreatPoison();
					return true;
				}
				if (E.Command == COOK_COMMAND_TREAT_ILLNESS)
				{
					NostrumsTreatIllness();
					return true;
				}
				if (E.Command == COOK_COMMAND_TREAT_DISEASE_ONSET)
				{
					NostrumsTreatDiseaseOnset();
					return true;
				}
			}
			return base.HandleEvent(E);
		}
		finally
		{
			AbilityBar.UpdateActiveEffects();
		}
	}

	public bool IsHalfIngredient(string type)
	{
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type];
		if (gameObjectBlueprint.GetTag("Triggers").IsNullOrEmpty() && gameObjectBlueprint.GetTag("Actions").IsNullOrEmpty())
		{
			return true;
		}
		return false;
	}

	public static bool HasTriggers(string type)
	{
		if (GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type].GetTag("Triggers").IsNullOrEmpty())
		{
			return false;
		}
		return true;
	}

	public static bool HasActions(string type)
	{
		if (GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type].GetTag("Actions").IsNullOrEmpty())
		{
			return false;
		}
		return true;
	}

	public static bool HasUnits(string type)
	{
		if (GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type].GetTag("Units").IsNullOrEmpty())
		{
			return false;
		}
		return true;
	}

	public ProceduralCookingEffect GenerateEffectFromTypeList(List<string> selectedIngredientTypes)
	{
		ProceduralCookingEffect result = null;
		IEnumerable<string[]> enumerable = Algorithms.GeneratePermutations(selectedIngredientTypes);
		List<List<string>> list = new List<List<string>>();
		foreach (string[] item2 in enumerable)
		{
			List<string> list2 = new List<string>();
			string[] array = item2;
			foreach (string item in array)
			{
				list2.Add(item);
			}
			list.Add(list2);
		}
		list.Add(null);
		list.ShuffleInPlace();
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] != null && list[j].Count == 0)
			{
				throw new InvalidOperationException("There weren't any types in the selected ingredient type permutations? (should be impossible)");
			}
			if (list[j] == null || list[j].Count == 1)
			{
				result = ProceduralCookingEffect.CreateJustUnits(selectedIngredientTypes);
				break;
			}
			if (list[j].Count == 2)
			{
				if (HasTriggers(list[j][0]) && HasActions(list[j][1]))
				{
					result = ProceduralCookingEffect.CreateTriggeredAction(list[j][0], list[j][1]);
					break;
				}
			}
			else if (HasUnits(list[j][0]) && HasTriggers(list[j][1]) && HasActions(list[j][2]))
			{
				result = ProceduralCookingEffect.CreateBaseAndTriggeredAction(list[j][0], list[j][1], list[j][2]);
				break;
			}
		}
		return result;
	}

	public List<ProceduralCookingEffect> GenerateEffectsFromTypeList(List<string> selectedIngredientTypes, int n)
	{
		List<ProceduralCookingEffect> list = new List<ProceduralCookingEffect>();
		IEnumerable<string[]> enumerable = Algorithms.GeneratePermutations(selectedIngredientTypes);
		List<List<string>> list2 = new List<List<string>>();
		foreach (string[] item2 in enumerable)
		{
			List<string> list3 = new List<string>();
			string[] array = item2;
			foreach (string item in array)
			{
				list3.Add(item);
			}
			list2.Add(list3);
		}
		list2.Add(null);
		list2.ShuffleInPlace();
		for (int j = 0; j < list2.Count; j++)
		{
			if (list2[j] != null && list2[j].Count == 0)
			{
				throw new InvalidOperationException("There weren't any types in the selected ingredient type permutations? (should be impossible)");
			}
			if (list2[j] == null || list2[j].Count == 1)
			{
				ProceduralCookingEffect effect = ProceduralCookingEffect.CreateJustUnits(selectedIngredientTypes);
				if (!list.Any((ProceduralCookingEffect e) => e.SameAs(effect)))
				{
					list.Add(effect);
					n--;
					if (n <= 0)
					{
						return list;
					}
				}
			}
			else if (list2[j].Count == 2)
			{
				if (!HasTriggers(list2[j][0]) || !HasActions(list2[j][1]))
				{
					continue;
				}
				ProceduralCookingEffectWithTrigger effect2 = ProceduralCookingEffect.CreateTriggeredAction(list2[j][0], list2[j][1]);
				if (!list.Any((ProceduralCookingEffect e) => e.SameAs(effect2)))
				{
					list.Add(effect2);
					n--;
					if (n <= 0)
					{
						return list;
					}
				}
			}
			else
			{
				if (!HasUnits(list2[j][0]) || !HasTriggers(list2[j][1]) || !HasActions(list2[j][2]))
				{
					continue;
				}
				ProceduralCookingEffectWithTrigger effect3 = ProceduralCookingEffect.CreateBaseAndTriggeredAction(list2[j][0], list2[j][1], list2[j][2]);
				if (!list.Any((ProceduralCookingEffect e) => e.SameAs(effect3)))
				{
					list.Add(effect3);
					n--;
					if (n <= 0)
					{
						return list;
					}
				}
			}
		}
		return list;
	}

	private static void MakeFromStoredByPlayer(GameObject obj)
	{
		obj.SetIntProperty("FromStoredByPlayer", 1);
	}

	public static bool PerformPreserve(GameObject go, StringBuilder sb, GameObject who, bool Capitalize = true, bool Single = false)
	{
		bool flag = go.GetIntProperty("StoredByPlayer") > 0;
		Action<GameObject> action = null;
		if (flag)
		{
			action = MakeFromStoredByPlayer;
		}
		int count = go.Count;
		if (Single && count > 1)
		{
			go = go.RemoveOne();
			count = go.Count;
		}
		if (count == 1)
		{
			sb.Append(Capitalize ? go.A : go.a).Append(go.DisplayNameOnlyDirect);
		}
		else if (go.IsPlural)
		{
			sb.Append(count).Append(' ').Append(go.DisplayNameOnlyDirect);
		}
		else
		{
			sb.Append(count).Append(' ').Append(Grammar.Pluralize(go.DisplayNameOnlyDirect));
		}
		sb.Append(" into ");
		string result = go.GetPart<PreservableItem>().Result;
		int num = 0;
		PreservableItem part = go.GetPart<PreservableItem>();
		PreparedCookingIngredient part2 = go.GetPart<PreparedCookingIngredient>();
		int num2 = 1;
		if (part2 != null)
		{
			num2 = part2.charges;
		}
		if (part != null)
		{
			num2 = part.Number;
		}
		num2 *= go.Count;
		go.Obliterate();
		string result2 = part.Result;
		int number = num2;
		Action<GameObject> afterObjectCreated = action;
		who.TakeObject(result2, number, NoStack: false, Silent: true, 0, null, 0, 0, null, null, null, afterObjectCreated);
		num += num2;
		GameObject gameObject = GameObject.CreateSample(result);
		string tagOrStringProperty = gameObject.GetTagOrStringProperty("ServingType", "serving");
		string tagOrStringProperty2 = gameObject.GetTagOrStringProperty("ServingName", gameObject.DisplayNameOnlyDirect);
		sb.Append(num).Append(' ').Append((num == 1) ? tagOrStringProperty : Grammar.Pluralize(tagOrStringProperty))
			.Append(" of ")
			.Append(tagOrStringProperty2);
		return true;
	}

	public static bool IsValidCookingIngredient(GameObject obj)
	{
		try
		{
			if (obj.IsTemporary && !obj.HasPropertyOrTag("CanCookTemporary"))
			{
				return false;
			}
			if (obj.IsImportant())
			{
				return false;
			}
			if (obj.HasPart<PreparedCookingIngredient>())
			{
				return true;
			}
			LiquidVolume liquidVolume = obj.LiquidVolume;
			if (liquidVolume != null && !liquidVolume.EffectivelySealed() && !liquidVolume.GetPreparedCookingIngredient().IsNullOrEmpty() && obj.GetEpistemicStatus() != 0)
			{
				return true;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Campfire::IsValidCookingIngredient", x);
		}
		return false;
	}

	public static List<GameObject> GetValidCookingIngredients(GameObject who)
	{
		return who.GetInventoryDirectAndEquipmentAndAdjacentCells(IsValidCookingIngredient);
	}

	public static List<GameObject> GetValidCookingIngredients()
	{
		return GetValidCookingIngredients(The.Player);
	}

	public static bool IsHungry(GameObject Object)
	{
		if (Object.TryGetPart<Stomach>(out var Part))
		{
			if (Part.HungerLevel <= 0)
			{
				return Part.CookCount < 3;
			}
			return true;
		}
		return false;
	}

	public bool Preserve()
	{
		if (!hasSkill)
		{
			Popup.Show("You don't have the Cooking and Gathering skill.");
			return false;
		}
		List<GameObject> inventoryDirect = The.Player.GetInventoryDirect((GameObject go) => go.HasPart<PreservableItem>() && !go.HasTag("ChooseToPreserve") && !go.IsTemporary && !go.IsImportant());
		if (inventoryDirect.Count == 0)
		{
			Popup.Show("You don't have anything to preserve.");
			return false;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("You preserved:\n\n");
		int num = 0;
		IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_interact_preserve_foods");
		int num2 = 0;
		for (int count = inventoryDirect.Count; num2 < count; num2++)
		{
			PerformPreserve(inventoryDirect[num2], stringBuilder, The.Player);
			stringBuilder.Append('.');
			if (num2 < count - 1)
			{
				stringBuilder.Append('\n');
			}
			num++;
		}
		Popup.Show(stringBuilder.ToString());
		return true;
	}

	public bool PreserveExotic()
	{
		if (!hasSkill)
		{
			Popup.Show("You don't have the Cooking and Gathering skill.");
			return false;
		}
		List<GameObject> inventoryDirect = The.Player.GetInventoryDirect((GameObject go) => go.HasPart<PreservableItem>() && go.HasTag("ChooseToPreserve") && !go.IsTemporary && go.Understood());
		if (inventoryDirect.Count == 0)
		{
			Popup.Show("You don't have anything to preserve.");
			return false;
		}
		for (; inventoryDirect.Count != 0; inventoryDirect = The.Player.GetInventoryDirect((GameObject go) => go.HasPart<PreservableItem>() && go.HasTag("ChooseToPreserve") && !go.IsTemporary && go.Understood()))
		{
			StringBuilder stringBuilder = new StringBuilder();
			int defaultSelected = 0;
			bool[] array = new bool[inventoryDirect.Count];
			string[] array2 = new string[inventoryDirect.Count];
			IRenderable[] array3 = new IRenderable[inventoryDirect.Count];
			int num = 0;
			foreach (GameObject item in inventoryDirect)
			{
				array[num] = false;
				array2[num] = item.DisplayName;
				array3[num] = item.RenderForUI();
				num++;
			}
			int num2 = Popup.PickOption("Choose exotic foods to preserve.", null, "", "Sounds/UI/ui_notification", array2, null, array3, null, null, null, null, 0, 60, defaultSelected, -1, AllowEscape: true);
			if (num2 < 0)
			{
				return true;
			}
			GameObject gameObject = inventoryDirect[num2];
			if (!gameObject.ConfirmUseImportant(null, "preserve"))
			{
				continue;
			}
			int num3 = 1;
			if (gameObject.Count > 1)
			{
				int? num4 = Popup.AskNumber(gameObject.DisplayNameOnlyDirect + ": how many do you want to preserve? (max = " + gameObject.Count + ")", "Sounds/UI/ui_notification", "", gameObject.Count, 0, gameObject.Count);
				if (!num4.HasValue || num4 == 0)
				{
					continue;
				}
				try
				{
					num3 = Convert.ToInt32(num4);
					if (num3 > gameObject.Count)
					{
						num3 = gameObject.Count;
					}
				}
				catch
				{
					continue;
				}
				if (num3 <= 0)
				{
					continue;
				}
			}
			stringBuilder.Append("You preserved:\n\n");
			IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_interact_preserve_exotic");
			PerformPreserve(gameObject.Split(num3, NoRemove: true), stringBuilder, The.Player);
			stringBuilder.Append('.');
			Popup.Show(stringBuilder.ToString());
		}
		return true;
	}

	public void AfterCooked()
	{
		Event e = Event.New("CookedAt", "Actor", The.Player, "Object", ParentObject);
		The.Player.FireEvent(e);
		ParentObject.FireEvent(e);
	}

	public bool CookPresetMeal(int index)
	{
		if (!IsHungry(The.Player))
		{
			Popup.Show("You aren't hungry. Instead, you relax by the warmth of the fire.");
			return false;
		}
		IComponent<GameObject>.PlayUISound("Human_Eating");
		Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
		The.Player.FireEvent("ClearFoodEffects");
		The.Player.CleanEffects();
		CookingRecipe cookingRecipe = presetMeals[index];
		if (The.Player.HasPart(typeof(Carnivorous)) && (cookingRecipe.HasPlants() || cookingRecipe.HasFungi()))
		{
			if (50.in100())
			{
				Popup.Show("Ugh, you feel sick.");
				The.Player.ApplyEffect(new Ill(100));
			}
		}
		else
		{
			cookingRecipe.ApplyEffectsTo(The.Player);
			ClearHunger();
		}
		pStomach.CookCount++;
		AfterCooked();
		return true;
	}

	public static int IngredientSort(GameObject a, GameObject b)
	{
		LiquidVolume liquidVolume = a.LiquidVolume;
		LiquidVolume liquidVolume2 = b.LiquidVolume;
		string text = liquidVolume?.GetPreparedCookingIngredient();
		string text2 = liquidVolume2?.GetPreparedCookingIngredient();
		if (!text.IsNullOrEmpty() && !text2.IsNullOrEmpty())
		{
			int num = ProducesLiquidEvent.Check(a, text).CompareTo(ProducesLiquidEvent.Check(b, text2));
			if (num != 0)
			{
				return -num;
			}
			int num2 = WantsLiquidCollectionEvent.Check(a, The.Player, text).CompareTo(WantsLiquidCollectionEvent.Check(b, The.Player, text2));
			if (num2 != 0)
			{
				return num2;
			}
			int num3 = liquidVolume.Volume.CompareTo(liquidVolume2.Volume);
			if (num3 != 0)
			{
				return -num3;
			}
		}
		int num4 = a.HasTagOrProperty("WaterContainer").CompareTo(b.HasTagOrProperty("WaterContainer"));
		if (num4 != 0)
		{
			return -num4;
		}
		int num5 = a.ValueEach.CompareTo(b.ValueEach);
		if (num5 != 0)
		{
			return num5;
		}
		return a.Blueprint.CompareTo(b.Blueprint);
	}

	public bool CookFromIngredients(bool random)
	{
		if (!IsHungry(The.Player))
		{
			Popup.Show("You aren't hungry. Instead, you relax by the warmth of the fire.");
			return false;
		}
		bool flag = pStomach.HungerLevel > 0;
		int bonus = 0;
		List<GameObject> validCookingIngredients = GetValidCookingIngredients();
		validCookingIngredients.Sort(IngredientSort);
		List<(int, GameObject, string)> list = new List<(int, GameObject, string)>(validCookingIngredients.Count);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (GameObject item6 in validCookingIngredients)
		{
			if (!The.Player.HasPart<Carnivorous>() || (!item6.HasTag("Plant") && !item6.HasTag("Fungus")))
			{
				string liquidIngredient1 = item6.LiquidVolume?.GetPreparedCookingIngredient();
				int num = item6.LiquidVolume?.Volume ?? item6.Count;
				string name;
				if (item6.LiquidVolume == null)
				{
					item6.Count = 1;
					name = item6.GetDisplayName(1120);
					item6.Count = num;
				}
				else
				{
					stringBuilder.Clear();
					item6.LiquidVolume.AppendLiquidName(stringBuilder);
					name = "a dram of " + stringBuilder.ToString();
				}
				int num2 = list.FindIndex(((int Count, GameObject go, string name) o) =>
				{
					string text4 = o.go.LiquidVolume?.GetPreparedCookingIngredient();
					return (!liquidIngredient1.IsNullOrEmpty() && !text4.IsNullOrEmpty()) ? (liquidIngredient1 == text4) : (o.name == name);
				});
				if (num2 != -1)
				{
					(int, GameObject, string) item = list[num2];
					list.Remove(item);
					item.Item1 += num;
					list.Add(item);
				}
				else
				{
					list.Add((num, item6, name));
				}
			}
		}
		foreach (GameObject item7 in validCookingIngredients)
		{
			item7.ResetNameCache();
		}
		int chance = 0;
		int num3 = 3;
		List<string> list2 = new List<string>();
		List<GameObject> list3 = Event.NewGameObjectList();
		List<GameObject> list4 = Event.NewGameObjectList();
		if (!random && !The.Player.HasSkill("CookingAndGathering"))
		{
			Popup.Show("You don't have the Cooking and Gathering skill.");
			return false;
		}
		if (random)
		{
			if (!The.Player.HasSkill("CookingAndGathering"))
			{
				List<string> list5 = new List<string>();
				list.ShuffleInPlace();
				foreach (var item8 in list)
				{
					GameObject item2 = item8.Item2;
					using ScopeDisposedList<string> scopeDisposedList = ScopeDisposedList<string>.GetFromPool();
					if (item2.TryGetPart<PreparedCookingIngredient>(out var Part))
					{
						Part.GetTypeOptions(scopeDisposedList);
					}
					else
					{
						scopeDisposedList.AddRange(item2.LiquidVolume.GetPreparedCookingIngredient().CachedCommaExpansion());
					}
					if (scopeDisposedList.Count <= 0)
					{
						continue;
					}
					scopeDisposedList.ShuffleInPlace();
					if (!list5.Contains(scopeDisposedList[0]) && chance.in100() && !list2.Contains(scopeDisposedList[0]))
					{
						list2.Add(scopeDisposedList[0]);
						list3.Add(item2);
						if (list2.Count >= num3)
						{
							break;
						}
					}
					list5.Add(scopeDisposedList[0]);
				}
			}
		}
		else
		{
			int num4 = 2;
			if (The.Player.HasSkill("CookingAndGathering_Spicer"))
			{
				num4++;
			}
			int defaultSelected = 0;
			List<bool> list6 = new List<bool>();
			int num5 = 0;
			list.Sort(((int Count, GameObject go, string name) a, (int Count, GameObject go, string name) b) => ConsoleLib.Console.ColorUtility.CompareExceptFormattingAndCase(a.name, b.name));
			foreach (var item9 in list)
			{
				_ = item9;
				list6.Add(item: false);
			}
			while (true)
			{
				List<string> list7 = new List<string>();
				List<IRenderable> list8 = new List<IRenderable>();
				string text = "C";
				if (num5 > num4)
				{
					text = "R";
				}
				string text2 = "";
				if (num5 < num4)
				{
					text2 = "\n{{y|[up to " + (num4 - num5) + " remaining]}}";
				}
				else if (num5 == num4)
				{
					text2 = "\n{{y|[0 remaining]}}";
				}
				list7.Add("{{W|Cook with the {{" + text + "|" + num5 + "}} selected ingredients.}}" + text2);
				list8.Add(null);
				int num6 = 0;
				foreach (var item10 in list)
				{
					int item3 = item10.Item1;
					GameObject item4 = item10.Item2;
					string item5 = item10.Item3;
					string text3 = $" {{{{K|x{item3}}}}}";
					if (list6[num6])
					{
						list7.Add("{{y|[{{G|X}}]}}   " + item5 + text3);
					}
					else
					{
						list7.Add("[ ]   " + item5 + text3);
					}
					list8.Add(item4.RenderForUI());
					num6++;
				}
				int num7 = Popup.PickOption("Choose ingredients to cook with.", null, "", "Sounds/UI/ui_notification", list7.ToArray(), null, list8.ToArray(), null, null, null, null, 0, 60, defaultSelected, 6, AllowEscape: true);
				if (num7 < 0)
				{
					return false;
				}
				if (num7 > 0)
				{
					defaultSelected = num7;
					list6[num7 - 1] = !list6[num7 - 1];
					num5 = ((!list6[num7 - 1]) ? (num5 - 1) : (num5 + 1));
				}
				else if (num7 != 0 || num5 <= num4)
				{
					break;
				}
			}
			for (int num8 = 0; num8 < list6.Count; num8++)
			{
				if (!list6[num8])
				{
					continue;
				}
				GameObject gameObject = null;
				string type;
				if (list[num8].Item2.TryGetPart<PreparedCookingIngredient>(out var Part2))
				{
					type = Part2.GetTypeInstanceAvoidingList(list2);
					if (Part2.type == "random" || Part2.type == "randomHighTier")
					{
						gameObject = EncountersAPI.GetAnObjectNoExclusions((GameObjectBlueprint ob) => EncountersAPI.IsEligibleForDynamicEncounters(ob) && (ob.GetPartParameter("PreparedCookingIngredient", "type", "").Contains(type) || (ob.HasTag("LiquidCookingIngredient") && ob.createSample().LiquidVolume.GetPreparedCookingIngredient().Contains(type))));
					}
				}
				else
				{
					type = list[num8].Item2.LiquidVolume.GetPreparedCookingIngredient().Split(',').GetRandomElement();
				}
				if (type != null && !list2.Contains(type))
				{
					list2.Add(type);
				}
				list3.Add(list[num8].Item2);
				if (gameObject != null)
				{
					list4.Add(gameObject);
				}
				else
				{
					list4.Add(list[num8].Item2);
				}
			}
		}
		if (list2.Count > 0)
		{
			The.Player.FireEvent("ClearFoodEffects");
			The.Player.CleanEffects();
			ProceduralCookingEffect proceduralCookingEffect = null;
			if (!random && The.Player.HasEffect<Inspired>())
			{
				List<ProceduralCookingEffect> list9 = GenerateEffectsFromTypeList(list2, 3);
				List<string> list10 = new List<string>();
				foreach (ProceduralCookingEffect item11 in list9)
				{
					list10.Add(ProcessEffectDescription(item11.GetTemplatedProceduralEffectDescription(), The.Player));
				}
				Popup.Show(DescribeMeal(list3));
				if (flag)
				{
					if (!RollTasty(bonus, The.Player.HasPart<Carnivorous>(), ForceTastyBasedOnIngredients(list2)))
					{
						Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
					}
				}
				else
				{
					IComponent<GameObject>.PlayUISound("Human_Eating");
					Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
				}
				IComponent<GameObject>.PlayUISound("HarmonicaRiff_GoodFood_3");
				int index = Popup.PickOption("You let inspiration guide you toward a mouthwatering dish.", null, "", "Sounds/UI/ui_notification", list10.ToArray(), null, null, null, null, null, null, 1);
				proceduralCookingEffect = list9[index];
				CookingRecipe cookingRecipe = CookingRecipe.FromIngredients(list4, proceduralCookingEffect, The.Player.BaseDisplayName);
				CookingGameState.LearnRecipe(cookingRecipe, The.Player);
				Popup.Show("You create a new recipe for {{|" + cookingRecipe.GetDisplayName() + "}}!", null, "sfx_cookingRecipe_create");
				JournalAPI.AddAccomplishment("You invented a mouthwatering dish called {{|" + cookingRecipe.GetDisplayName() + "}}.", "In a moment of divine inspiration, the Carbide Chef =name= invented the mouthwatering dish called {{|" + cookingRecipe.GetDisplayName() + "}}.", "At a remote tavern near " + JournalAPI.GetLandmarkNearestPlayer().Text + ", =name= met with a group of kippers and, divinely inspired by <spice.elements." + The.Player.GetMythicDomain() + ".practices.!random>, invented the dish called " + cookingRecipe.GetDisplayName() + ".", null, "general", MuralCategory.CreatesSomething, MuralWeight.Low, null, -1L);
				Achievement.RECIPES_100.Progress.Increment();
				The.Player.RemoveEffect<Inspired>();
				AfterCooked();
			}
			else
			{
				proceduralCookingEffect = GenerateEffectFromTypeList(list2);
				Popup.Show(DescribeMeal(list3));
				if (flag)
				{
					if (!RollTasty(bonus, The.Player.HasPart<Carnivorous>(), ForceTastyBasedOnIngredients(list2)))
					{
						Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
					}
				}
				else
				{
					IComponent<GameObject>.PlayUISound("Human_Eating");
					Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
				}
				AfterCooked();
			}
			pStomach.CookCount++;
			ClearHunger();
			if (proceduralCookingEffect != null)
			{
				proceduralCookingEffect.Init(The.Player);
				Popup.Show("You start to metabolize the meal, gaining the following effect for the rest of the day:\n\n{{W|" + ProcessEffectDescription(proceduralCookingEffect.GetProceduralEffectDescription(), The.Player) + "}}");
				proceduralCookingEffect.Duration = 1;
				The.Player.ApplyEffect(proceduralCookingEffect);
			}
		}
		else
		{
			pStomach.CookCount++;
			ClearHunger();
			Popup.Show(DescribeMeal(list3));
			TutorialManager.OnTrigger("AteRandom");
			if (flag)
			{
				if (!RollTasty(bonus, The.Player.HasPart<Carnivorous>()))
				{
					IComponent<GameObject>.PlayUISound("Human_Eating");
					Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
					AfterCooked();
				}
			}
			else
			{
				IComponent<GameObject>.PlayUISound("Human_Eating");
				Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
				AfterCooked();
			}
		}
		Event e = Event.New("UsedAsIngredient", "Actor", The.Player);
		foreach (GameObject item12 in list3)
		{
			item12.FireEvent(e);
			if (item12.HasPart<PreparedCookingIngredient>())
			{
				PreparedCookingIngredient part = item12.GetPart<PreparedCookingIngredient>();
				if (part.HasTag("AlwaysStack"))
				{
					item12.Destroy();
					continue;
				}
				item12.SplitFromStack();
				part.charges--;
				if (part.charges <= 0)
				{
					item12.Destroy();
				}
				else
				{
					item12.CheckStack();
				}
			}
			else
			{
				item12.LiquidVolume?.UseDram();
			}
		}
		return true;
	}

	public bool CookFromRecipe()
	{
		if (!hasSkill)
		{
			Popup.Show("You don't have the Cooking and Gathering skill.");
			return false;
		}
		if (!IsHungry(The.Player))
		{
			Popup.Show("You aren't hungry. Instead, you relax by the warmth of the fire.");
			return false;
		}
		bool flag = pStomach.HungerLevel > 0;
		int bonus = 0;
		CookingGameState.ResetInventorySnapshot();
		bool flag2 = false;
		int defaultSelected = 0;
		while (true)
		{
			int num = 0;
			List<Tuple<string, CookingRecipe>> list = new List<Tuple<string, CookingRecipe>>();
			foreach (CookingRecipe knownRecipy in CookingGameState.instance.knownRecipies)
			{
				if (!knownRecipy.Hidden && (!The.Player.HasPart<Carnivorous>() || (!knownRecipy.HasPlants() && !knownRecipy.HasFungi())))
				{
					if (!flag2 && !knownRecipy.CheckIngredients())
					{
						num++;
					}
					else
					{
						list.Add(new Tuple<string, CookingRecipe>(knownRecipy.GetCampfireDescription() + "\n\n", knownRecipy));
					}
				}
			}
			if (list.Count <= 0)
			{
				if (num > 0)
				{
					flag2 = true;
					continue;
				}
				Popup.Show("You don't know any recipes.");
				return false;
			}
			list.Sort((Tuple<string, CookingRecipe> a, Tuple<string, CookingRecipe> b) => ConsoleLib.Console.ColorUtility.CompareExceptFormattingAndCase(a.Item1, b.Item1));
			string text = "";
			if (num > 0)
			{
				text = text + "&K< " + num + " hidden for missing ingredients >";
				list.Add(new Tuple<string, CookingRecipe>("Show " + num + " hidden recipes missing ingredients", null));
			}
			int num2 = Popup.PickOption("Choose a recipe", text, Popup.SPACING_DARK_LINE.Replace('=', '÷'), "Sounds/UI/ui_notification", TupleUtilities<string, CookingRecipe>.GetFirstArray(list), null, null, null, null, null, null, 1, 72, defaultSelected, -1, AllowEscape: true, RespectOptionNewlines: true);
			if (num2 >= 0 && num2 < list.Count && list[num2].Item2 == null)
			{
				flag2 = true;
				continue;
			}
			if (num2 < 0)
			{
				break;
			}
			defaultSelected = num2;
			while (true)
			{
				string text2 = "Add to favorite recipes";
				if (list[num2].Item2.Favorite)
				{
					text2 = "Remove from favorite recipes";
				}
				int num3 = Popup.PickOption("", list[num2].Item2.GetCampfireDescription(), "", "Sounds/UI/ui_notification", new string[4] { "Cook", text2, "Forget", "Back" }, null, null, null, null, null, null, 0, 72, 0, -1, AllowEscape: true, RespectOptionNewlines: true);
				if (num3 < 0 || num3 == 3)
				{
					break;
				}
				if (num3 == 1)
				{
					list[num2].Item2.Favorite = !list[num2].Item2.Favorite;
					continue;
				}
				if (num3 == 2)
				{
					if (Popup.ShowYesNo("Are you sure you want to forget this recipe?") == DialogResult.Yes)
					{
						list[num2].Item2.Hidden = true;
					}
					break;
				}
				if (!list[num2].Item2.CheckIngredients(displayMessage: true))
				{
					break;
				}
				List<GameObject> list2 = new List<GameObject>();
				if (!list[num2].Item2.UseIngredients(list2))
				{
					break;
				}
				The.Player.FireEvent("ClearFoodEffects");
				The.Player.CleanEffects();
				if (flag)
				{
					if (!RollTasty(bonus, The.Player.HasPart<Carnivorous>(), ForceTastyBasedOnIngredients(list2)))
					{
						IComponent<GameObject>.PlayUISound("Human_Eating");
						Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
						AfterCooked();
					}
				}
				else
				{
					IComponent<GameObject>.PlayUISound("Human_Eating");
					Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
					AfterCooked();
				}
				if (list[num2].Item2.ApplyEffectsTo(The.Player))
				{
					pStomach.CookCount++;
					ClearHunger();
				}
				return true;
			}
		}
		return false;
	}

	public bool Cook()
	{
		if (!The.Player.CheckFrozen())
		{
			return false;
		}
		if (The.Player.AreHostilesNearby())
		{
			Popup.Show("You can't cook with hostile creatures nearby.");
			return false;
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != ActivePartStatus.Operational)
		{
			switch (activePartStatus)
			{
			case ActivePartStatus.SwitchedOff:
				Popup.Show(ParentObject.Does("are") + " turned off.");
				break;
			case ActivePartStatus.Unpowered:
				Popup.Show(ParentObject.Does("do") + " not have enough charge to operate.");
				break;
			case ActivePartStatus.NotHanging:
				Popup.Show(ParentObject.Does("need") + " to be hung up first.");
				break;
			default:
				Popup.Show(ParentObject.Does("do") + " not seem to be working.");
				break;
			}
			return false;
		}
		if (!The.Player.CanChangeMovementMode("Cooking", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			UnityEngine.GameObject.Find("CampfireSounds").GetComponent<CampfireSounds>().Open();
		});
		try
		{
			InventoryAction inventoryAction;
			do
			{
				Dictionary<string, InventoryAction> dictionary = new Dictionary<string, InventoryAction>(16);
				GetCookingActionsEvent.SendToActorAndObject(The.Player, ParentObject, dictionary);
				inventoryAction = EquipmentAPI.ShowInventoryActionMenu(dictionary, The.Player, ParentObject, Distant: false, TelekineticOnly: false, "{{W|The fire breathes its warmth on your bones.}}", new InventoryAction.Comparer
				{
					priorityFirst = true
				});
				if (inventoryAction == null)
				{
					return true;
				}
			}
			while (!inventoryAction.Process(ParentObject, The.Player).InterfaceExitRequested());
			return true;
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Campfire", x);
		}
		finally
		{
			GameManager.Instance.uiQueue.queueTask(() =>
			{
				UnityEngine.GameObject.Find("CampfireSounds").GetComponent<CampfireSounds>().Close();
			});
		}
		return true;
	}

	private static bool CanExtinguish(GameObject obj)
	{
		LiquidVolume liquidVolume = obj.LiquidVolume;
		if (liquidVolume.Volume >= 10)
		{
			return liquidVolume.IsOpenVolume();
		}
		return false;
	}

	public static GameObject FindExtinguishingPool(Cell C)
	{
		GameObject gameObject = C?.GetFirstObjectWithPart("LiquidVolume", CanExtinguish);
		if (gameObject == null)
		{
			return null;
		}
		if (C.HasBridge())
		{
			return null;
		}
		return gameObject;
	}

	public static GameObject FindExtinguishingPool(GameObject obj)
	{
		return FindExtinguishingPool(obj?.CurrentCell);
	}

	public GameObject FindExtinguishingPool()
	{
		return FindExtinguishingPool(ParentObject);
	}

	public static void ClearHunger()
	{
		The.Player.GetPart<Stomach>()?.ClearHunger();
	}

	public static bool ForceTastyBasedOnIngredients(List<string> ingredients)
	{
		return ingredients.Contains("tastyMinor");
	}

	public static bool ForceTastyBasedOnIngredients(List<GameObject> ingredientObjects)
	{
		foreach (GameObject ingredientObject in ingredientObjects)
		{
			PreparedCookingIngredient part = ingredientObject.GetPart<PreparedCookingIngredient>();
			if (part != null && part.HasTypeOption("tastyMinor"))
			{
				return true;
			}
			LiquidVolume liquidVolume = ingredientObject.LiquidVolume;
			if (liquidVolume != null && liquidVolume.GetPreparedCookingIngredient().Contains("tastyMinor"))
			{
				return true;
			}
		}
		return false;
	}

	private static Effect RandomTastyEffect(string tastyMessage)
	{
		return Stat.Random(0, 6) switch
		{
			0 => (Effect)new BasicCookingEffect_Hitpoints(tastyMessage), 
			1 => new BasicCookingEffect_MA(tastyMessage), 
			2 => new BasicCookingEffect_MS(tastyMessage), 
			3 => new BasicCookingEffect_Quickness(tastyMessage), 
			4 => new BasicCookingEffect_RandomStat(tastyMessage), 
			5 => new BasicCookingEffect_Regeneration(tastyMessage), 
			6 => new BasicCookingEffect_ToHit(tastyMessage), 
			_ => null, 
		};
	}

	public static bool RollTasty(int Bonus = 0, bool bCarnivore = false, bool bAlwaysSucceed = false)
	{
		if (The.Player == null)
		{
			return false;
		}
		if (bAlwaysSucceed || (10 + Bonus).in100())
		{
			string tastyMessage = ((!bCarnivore) ? "You eat the meal. It's tastier than usual." : "You gorge on the succulent meat. It's tastier than usual.");
			IComponent<GameObject>.PlayUISound("Human_Eating_WithGulp");
			The.Player.ApplyEffect(RandomTastyEffect(tastyMessage));
			return true;
		}
		return false;
	}

	public static string[] RollIngredients(int Amount, IReadOnlyList<GameObject> Objects = null, System.Random Random = null)
	{
		Dictionary<string, string> variableCache = HistoricStringExpander.GetVariableCache();
		string objectTypeForZone = ZoneManager.GetObjectTypeForZone(The.Player.Physics.CurrentCell.ParentZone.ZoneID);
		string tag = GameObjectFactory.Factory.Blueprints[objectTypeForZone].GetTag("LairOwnerTable", "GenericLairOwner");
		int num = 0;
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		GameObject gameObject3 = null;
		GameObject gameObject4 = null;
		try
		{
			do
			{
				gameObject?.Destroy();
				num++;
				gameObject = GameObject.Create(PopulationManager.RollOneFrom((num < 20) ? tag : "LairOwners_Jungle").Blueprint);
			}
			while (!Axe_Dismember.HasAnyDismemberableBodyPart(gameObject, The.Player, null, assumeDecapitate: true));
			variableCache["$terrain"] = GameObjectFactory.Factory.Blueprints[objectTypeForZone].GetTag("Terrain");
			variableCache["$creaturePossessive"] = Grammar.MakePossessive(gameObject.DisplayNameOnlyStripped);
			variableCache["$creatureBodyPart"] = Axe_Dismember.GetDismemberableBodyPart(gameObject, The.Player).GetOrdinalName();
			variableCache["$bookName"] = "{{|" + (gameObject2 = EncountersAPI.CreateARandomDescendantOf("Book")).DisplayNameOnlyStripped + "}}";
			variableCache["$gasName"] = "{{|" + (gameObject3 = EncountersAPI.CreateARandomDescendantOf("Gas")).DisplayNameOnlyStripped + "}}";
			variableCache["$tonicName"] = "{{|" + (gameObject4 = EncountersAPI.CreateARandomDescendantOf("Tonic")).DisplayNameOnlyDirect + "}}";
		}
		catch (Exception x)
		{
			MetricsManager.LogException("HistoricVariables [Terrain: " + objectTypeForZone + ", Creature: " + gameObject?.DebugName + ", Book: " + gameObject2?.DebugName + ", Gas: " + gameObject3?.DebugName + ", Tonic: " + gameObject4?.DebugName + "]", x);
			return new string[1] { "a pinch of salt" };
		}
		finally
		{
			gameObject?.Destroy();
			gameObject2?.Destroy();
			gameObject3?.Destroy();
			gameObject4?.Destroy();
		}
		string[] array = new string[Amount];
		for (int i = 0; i < Amount; i++)
		{
			try
			{
				if (Objects != null && i < Objects.Count)
				{
					LiquidVolume liquidVolume = Objects[i].LiquidVolume;
					if (liquidVolume != null && liquidVolume.HasPreparedCookingIngredient())
					{
						array[i] = "a dram of " + liquidVolume.GetLiquidName();
					}
					else
					{
						array[i] = Objects[i].an(int.MaxValue, null, "CookingIngredient", AsIfKnown: false, Single: true, NoConfusion: true);
					}
				}
				else
				{
					array[i] = HistoricStringExpander.ExpandString("<spice.cooking.ingredient.!random>", null, null, variableCache, Random);
				}
			}
			catch (Exception x2)
			{
				string text = Objects.ElementAtOrDefault(i)?.DebugName ?? "unknown";
				MetricsManager.LogException("MealIngredient [" + text + "]", x2);
				array[i] = "{invalid ingredient: " + text + "}";
			}
		}
		return array;
	}

	public static string DescribeMeal(IReadOnlyList<GameObject> mealObjects)
	{
		string text = "unknown";
		try
		{
			text = Grammar.MakeAndList(RollIngredients(Stat.Random(3, 4), mealObjects));
			return Grammar.ConvertAtoAn(HistoricStringExpander.ExpandString("<spice.cooking.cookTemplate.!random>").Replace("*ingredients*", text));
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error generating meal description", x);
			return "{invalid meal: " + text + "}";
		}
	}

	public static string ProcessEffectDescription(string description, GameObject go)
	{
		description = description.TrimEnd('\r', '\n');
		if (go == null || go.IsPlayer())
		{
			return Grammar.CapAfterNewlines(Grammar.InitCap(description.Replace("@thisCreature", "you").Replace("@s", "").Replace("@es", "")
				.Replace("@is", "are")
				.Replace("@they", "you")
				.Replace("@their", "your")
				.Replace("@them", "you")));
		}
		return Grammar.CapAfterNewlines(Grammar.InitCap(description.Replace("@thisCreature", go.IsPlural ? "these creatures" : "this creature").Replace("@s", go.IsPlural ? "" : "s").Replace("@es", go.IsPlural ? "" : "es")
			.Replace("@is", go.Is)
			.Replace("@they", go.it)
			.Replace("@their", go.its)
			.Replace("@them", go.them)));
	}

	private List<GameObject> GetPlayerAndAdjacentCompanions()
	{
		List<GameObject> list = Event.NewGameObjectList();
		list.Add(The.Player);
		list.AddRange(The.Player.GetCompanionsReadonly(1));
		return list;
	}

	private bool AnyPlayerOrAdjacentCompanion(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return true;
		}
		if (Filter(The.Player))
		{
			return true;
		}
		return The.Player.AnyCompanion(1, Filter);
	}

	private List<GameObject> GetMedicinalIngredients()
	{
		List<GameObject> list = Event.NewGameObjectList();
		foreach (GameObject item in The.Player.GetInventory())
		{
			if (item.HasTagOrProperty("Medicinal"))
			{
				list.Add(item);
			}
			else if (item.GetPart<PreparedCookingIngredient>()?.type == "regenLowtier")
			{
				list.Add(item);
			}
		}
		if (list.Count > 1)
		{
			list.Sort((GameObject a, GameObject b) => a.Value.CompareTo(b.Value));
		}
		return list;
	}

	public bool NostrumsCanStopBleeding()
	{
		return AnyPlayerOrAdjacentCompanion((GameObject who) => who.HasEffect((Bleeding fx) => !fx.Internal));
	}

	public void NostrumsStopBleeding()
	{
		List<GameObject> playerAndAdjacentCompanions = GetPlayerAndAdjacentCompanions();
		int num = 0;
		foreach (GameObject item in playerAndAdjacentCompanions)
		{
			bool flag = false;
			bool flag2 = false;
			if (item._Effects != null)
			{
				int i = 0;
				for (int count = item.Effects.Count; i < count; i++)
				{
					if (item.Effects[i] is Bleeding bleeding)
					{
						if (bleeding.Internal)
						{
							flag2 = true;
						}
						else
						{
							flag = true;
						}
					}
				}
			}
			if (!(flag | flag2))
			{
				continue;
			}
			if (!item.IsPlayer() && !The.Player.PhaseMatches(item))
			{
				Popup.ShowFail("You try to staunch the wounds of " + item.t() + ", but your limbs pass through " + item.them + ".");
			}
			else if (!item.IsPlayer() && item.IsInStasis())
			{
				Popup.ShowFail("You try to staunch the wounds of " + item.t() + ", but cannot affect " + item.them + ".");
			}
			else if (flag)
			{
				IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_interact_campfire_treat_bleeding");
				if (flag2)
				{
					if (item.IsPlayer())
					{
						Popup.Show("You staunch your wounds, though some are too deep to treat.");
					}
					else
					{
						Popup.Show("You staunch the wounds of " + item.t() + ", though some are too deep to treat.");
					}
				}
				else if (item.IsPlayer())
				{
					Popup.Show("You staunch your wounds.");
				}
				else
				{
					Popup.Show("You staunch the wounds of " + item.t() + ".");
				}
				int num2 = 0;
				int count2 = item.Effects.Count;
				int num3 = 0;
				while (num2 < count2 && num3 < 1000)
				{
					if (item.Effects[num2] is Bleeding { Internal: false } bleeding2)
					{
						bleeding2.StopMessageUsePopup = false;
						item.RemoveEffect(bleeding2);
						num2 = -1;
						count2 = item.Effects.Count;
					}
					num2++;
					num3++;
				}
			}
			else
			{
				Popup.Show(item.Poss("wounds") + " are too deep to treat.");
			}
			num++;
		}
		if (num == 0)
		{
			if (playerAndAdjacentCompanions.Count == 1)
			{
				Popup.ShowFail("You are not bleeding.");
			}
			else if (playerAndAdjacentCompanions.Count == 2)
			{
				Popup.ShowFail("Neither you nor " + playerAndAdjacentCompanions[1].t() + " are bleeding.");
			}
			else
			{
				Popup.ShowFail("Neither you nor any of your nearby companions are bleeding.");
			}
		}
	}

	public bool NostrumsCanTreatPoison()
	{
		return AnyPlayerOrAdjacentCompanion((GameObject who) => who.HasEffectOfType(67174400));
	}

	public void NostrumsTreatPoison()
	{
		List<GameObject> playerAndAdjacentCompanions = GetPlayerAndAdjacentCompanions();
		List<GameObject> list = Event.NewGameObjectList(playerAndAdjacentCompanions, (GameObject who) => who.HasEffectOfType(67174400));
		int count = list.Count;
		while (list.Count > 0)
		{
			GameObject gameObject;
			if (list.Count > 1)
			{
				gameObject = Popup.PickGameObject("Treat whom " + ((list.Count == count) ? "first" : "next") + "?", list, AllowEscape: true);
				if (gameObject == null)
				{
					break;
				}
			}
			else
			{
				gameObject = list[0];
			}
			list.Remove(gameObject);
			List<GameObject> medicinalIngredients = GetMedicinalIngredients();
			if (medicinalIngredients.Count == 0)
			{
				Popup.ShowFail("You have no medicinal ingredients with which to treat the poison coursing through " + gameObject.t() + ".");
				break;
			}
			GameObject gameObject2 = Popup.PickGameObject("Select an ingredient to use.", medicinalIngredients, AllowEscape: true);
			if (gameObject2 == null)
			{
				break;
			}
			if (!The.Player.PhaseMatches(gameObject))
			{
				Popup.ShowFail("You try to cure the poison coursing through " + gameObject.t() + ", but your limbs pass through " + gameObject.them + ".");
				continue;
			}
			if (gameObject.IsInStasis())
			{
				Popup.ShowFail("You try to cure the poison coursing through " + gameObject.t() + ", but cannot affect " + gameObject.them + ".");
				continue;
			}
			int num = gameObject.RemoveEffectsOfType(67174400);
			if (num > 0)
			{
				IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_interact_campfire_treat_poison");
				Popup.Show("You cure the " + ((num == 1) ? "poison" : "poisons") + " coursing through " + gameObject.t() + " with a balm made from " + gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				gameObject2.Destroy();
			}
			else
			{
				Popup.ShowFail("You try to cure the poison coursing through " + gameObject.t() + ", but your cures are ineffective.");
			}
		}
		if (count != 0)
		{
			return;
		}
		if (playerAndAdjacentCompanions.Any((GameObject who) => who.HasEffectOfType(65536)))
		{
			List<GameObject> list2 = Event.NewGameObjectList(playerAndAdjacentCompanions, (GameObject who) => who.HasEffectOfType(65536));
			if (list2.Contains(The.Player))
			{
				if (list2.Count == 1)
				{
					Popup.ShowFail("The poison affecting you is too strong to be cured by your nostrums.");
				}
				else if (list2.Count == 2)
				{
					Popup.ShowFail("The poison affecting you and " + list2[1].t() + " is too strong to be cured by your nostrums.");
				}
				else
				{
					Popup.ShowFail("The poison affecting you and your companions is too strong to be cured by your nostrums.");
				}
			}
			else
			{
				Popup.ShowFail("The poison affecting " + Grammar.MakeAndList(list2) + " is too strong to be cured by your nostrums.");
			}
		}
		else if (playerAndAdjacentCompanions.Count == 1)
		{
			Popup.ShowFail("You are not poisoned.");
		}
		else if (playerAndAdjacentCompanions.Count == 2)
		{
			Popup.ShowFail("Neither you nor " + playerAndAdjacentCompanions[1].t() + " are poisoned.");
		}
		else
		{
			Popup.ShowFail("Neither you nor any of your nearby companions are poisoned.");
		}
	}

	public bool NostrumsCanTreatIllness()
	{
		return AnyPlayerOrAdjacentCompanion((GameObject who) => who.HasEffect<Ill>());
	}

	public void NostrumsTreatIllness()
	{
		List<GameObject> playerAndAdjacentCompanions = GetPlayerAndAdjacentCompanions();
		List<GameObject> list = Event.NewGameObjectList(playerAndAdjacentCompanions, (GameObject who) => who.HasEffect<Ill>());
		int count = list.Count;
		while (list.Count > 0)
		{
			GameObject gameObject;
			if (list.Count > 1)
			{
				gameObject = Popup.PickGameObject("Treat whom " + ((list.Count == count) ? "first" : "next") + "?", list, AllowEscape: true);
				if (gameObject == null)
				{
					break;
				}
			}
			else
			{
				gameObject = list[0];
			}
			list.Remove(gameObject);
			List<GameObject> medicinalIngredients = GetMedicinalIngredients();
			if (medicinalIngredients.Count == 0)
			{
				Popup.ShowFail("You have no medicinal ingredients with which to treat " + gameObject.poss("illness") + ".");
				break;
			}
			GameObject gameObject2 = Popup.PickGameObject("Select an ingredient to use.", medicinalIngredients, AllowEscape: true);
			if (gameObject2 == null)
			{
				break;
			}
			if (!The.Player.PhaseMatches(gameObject))
			{
				Popup.ShowFail("You try to cure " + gameObject.poss("illness") + ", but your limbs pass through " + gameObject.them + ".");
			}
			else if (gameObject.IsInStasis())
			{
				Popup.ShowFail("You try to " + gameObject.poss("illness") + ", but cannot affect " + gameObject.them + ".");
			}
			else
			{
				IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_interact_campfire_treat_illness");
				gameObject.RemoveAllEffects<Ill>();
				Popup.Show("You cure " + gameObject.poss("illness") + " with a balm made from " + gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				gameObject2.Destroy();
			}
		}
		if (count == 0)
		{
			if (playerAndAdjacentCompanions.Count == 1)
			{
				Popup.ShowFail("You are not ill.");
			}
			else if (playerAndAdjacentCompanions.Count == 2)
			{
				Popup.ShowFail("Neither you nor " + playerAndAdjacentCompanions[1].t() + " are ill.");
			}
			else
			{
				Popup.ShowFail("Neither you nor any of your nearby companions are ill.");
			}
		}
	}

	public bool NostrumsCanTreatDiseaseOnset()
	{
		return AnyPlayerOrAdjacentCompanion((GameObject who) => GetDiseaseOnsetEvent.GetFor(who) != null);
	}

	public void NostrumsTreatDiseaseOnset()
	{
		List<GameObject> playerAndAdjacentCompanions = GetPlayerAndAdjacentCompanions();
		List<GameObject> list = Event.NewGameObjectList();
		List<Effect> list2 = new List<Effect>();
		int i = 0;
		for (int count = playerAndAdjacentCompanions.Count; i < count; i++)
		{
			GameObject gameObject = playerAndAdjacentCompanions[i];
			Effect effect = GetDiseaseOnsetEvent.GetFor(gameObject);
			if (effect != null)
			{
				list.Add(gameObject);
				list2.Add(effect);
			}
		}
		int count2 = list.Count;
		int chance = 20;
		while (list.Count > 0)
		{
			GameObject gameObject;
			if (list.Count > 1)
			{
				gameObject = Popup.PickGameObject("Treat whom " + ((list.Count == count2) ? "first" : "next") + "?", list, AllowEscape: true);
				if (gameObject == null)
				{
					break;
				}
			}
			else
			{
				gameObject = list[0];
			}
			int index = list.IndexOf(gameObject);
			list.RemoveAt(index);
			Effect effect = list2[index];
			list2.RemoveAt(index);
			if (gameObject.HasEffect<BoostedImmunity>())
			{
				Popup.ShowFail(gameObject.Does("have", int.MaxValue, null, null, "already") + " boosted immunity from a nostrum.");
				continue;
			}
			List<GameObject> medicinalIngredients = GetMedicinalIngredients();
			if (medicinalIngredients.Count == 0)
			{
				Popup.ShowFail("You have no medicinal ingredients with which to treat " + gameObject.poss(effect.GetDescription()) + ".");
				break;
			}
			GameObject gameObject2 = Popup.PickGameObject("Select an ingredient to use.", medicinalIngredients, AllowEscape: true);
			if (gameObject2 == null)
			{
				break;
			}
			if (!The.Player.PhaseMatches(gameObject))
			{
				Popup.ShowFail("You try to cure " + gameObject.poss("diease onset") + ", but your limbs pass through " + gameObject.them + ".");
			}
			else if (gameObject.IsInStasis())
			{
				Popup.ShowFail("You try to " + gameObject.poss("disease onset") + ", but cannot affect " + gameObject.them + ".");
			}
			else if (chance.in100())
			{
				IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_interact_campfire_treat_disease");
				Popup.Show("You cure " + gameObject.poss(effect.GetDescription()) + " with a balm made from " + gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				gameObject.RemoveEffect(effect);
				gameObject2.Destroy();
			}
			else if (gameObject.ApplyEffect(new BoostedImmunity()))
			{
				IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_interact_campfire_treat_disease");
				Popup.Show("You boost " + gameObject.poss("immunity") + " with a balm made from " + gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				gameObject2.Destroy();
			}
			else
			{
				Popup.Show("You try to boost " + gameObject.poss("immunity") + " with a balm made from " + gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", but it is ineffective.");
				gameObject2.Destroy();
			}
		}
		if (count2 == 0)
		{
			if (playerAndAdjacentCompanions.Count == 1)
			{
				Popup.ShowFail("You are not suffering from the onset of a disease.");
			}
			else if (playerAndAdjacentCompanions.Count == 2)
			{
				Popup.ShowFail("Neither you nor " + playerAndAdjacentCompanions[1].t() + " are suffering from the onset of a disease.");
			}
			else
			{
				Popup.ShowFail("Neither you nor any of your nearby companions are suffering from the onset of a disease.");
			}
		}
	}

	public void Extinguish(GameObject Actor = null, GameObject Object = null)
	{
		if (Actor != null)
		{
			IComponent<GameObject>.XDidYToZ(Actor, "extinguish", ParentObject);
		}
		else if (Object != null)
		{
			EmitMessage(ParentObject.Does("are") + " extinguished by " + Object.t() + ".");
		}
		PlayWorldSound("Sounds/Interact/sfx_interact_campfire_extinguish");
		bool flag = ParentObject.HasProperty("PlayerCampfire");
		GameObject gameObject = ParentObject.ReplaceWith(ExtinguishBlueprint);
		if ((gameObject != null) & flag)
		{
			gameObject.SetIntProperty("PlayerCampfire", 1);
			gameObject.SetStringProperty("PointOfInterestKey", "PlayerCampfire");
		}
	}
}
