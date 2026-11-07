using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ConsoleLib.Console;
using Cysharp.Text;
using Qud.API;
using XRL.Language;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.Parts;
using XRL.World.Skills;
using XRL.World.Units;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
[HasWishCommand]
public abstract class GolemQuestSelection
{
	public static bool WISH_ALL;

	public static bool WISH_RANDOM;

	public const string MAKE_SELECTION = "ÿÿÿÿ{{K|<make a selection>}}";

	public const string NO_REQUIRED = "You have nothing that meets the requirement of the ";

	[NonSerialized]
	private GolemQuestSystem _System;

	public GolemQuestSystem System => _System ?? (_System = GolemQuestSystem.Get());

	public abstract string ID { get; }

	public abstract string DisplayName { get; }

	public abstract char Key { get; }

	public string Title => "Choose " + Grammar.A(DisplayName);

	public string Mark
	{
		get
		{
			if (!IsValid())
			{
				return "{{red|[X]}}";
			}
			return "{{green|[û]}}";
		}
	}

	public abstract bool IsValid();

	public abstract void Pick();

	public abstract void Apply(GameObject Object);

	public abstract IEnumerable<GameObjectUnit> YieldAllEffects();

	public abstract IEnumerable<GameObjectUnit> YieldRandomEffects();

	public string GetOptionText()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Choose ");
		Grammar.A(DisplayName, stringBuilder);
		string optionChoice = GetOptionChoice();
		if (optionChoice == null)
		{
			stringBuilder.Compound("ÿÿÿÿ{{K|<make a selection>}}", '\n');
		}
		else
		{
			stringBuilder.Compound(Mark, "\n ");
			if (IsValid())
			{
				stringBuilder.Compound(optionChoice);
			}
			else
			{
				stringBuilder.Compound("{{K|").Append(ColorUtility.StripFormatting(optionChoice)).Append("}}");
			}
			AppendEffects(stringBuilder, 1);
		}
		return stringBuilder.ToString();
	}

	public abstract string GetOptionChoice();

	public virtual void AppendEffects(StringBuilder SB, int Indent = 0)
	{
	}

	public virtual void Save(SerializationWriter Writer)
	{
		Type type = GetType();
		FieldAttributes attr = FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Literal | FieldAttributes.NotSerialized;
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
		Writer.Write(fields.Count((FieldInfo x) => (x.Attributes & attr) == 0));
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if ((fieldInfo.Attributes & attr) <= FieldAttributes.PrivateScope)
			{
				Writer.Write(fieldInfo.Name);
				Writer.WriteObject(fieldInfo.GetValue(this));
			}
		}
	}

	public virtual void Load(SerializationReader Reader)
	{
		Type type = GetType();
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string name = Reader.ReadString();
			object value = Reader.ReadObject();
			try
			{
				type.GetField(name, BindingFlags.Instance | BindingFlags.Public)?.SetValue(this, value);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Error deserializing golem selection field", x);
			}
		}
	}

	public static void ProcessDescription(GameObject Object, GameObject Source = null)
	{
		Description part = Object.GetPart<Description>();
		using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		utf16ValueStringBuilder.Append(part._Short);
		if (Source == null)
		{
			Source = Object;
		}
		string key = Source.GetStringProperty("ArmamentSkill").Coalesce("Cudgel");
		SkillEntry value = SkillFactory.Factory.SkillByClass.GetValue(key);
		utf16ValueStringBuilder.Replace("=armament.skill=", Grammar.Pluralize(value.Name.ToLowerInvariant()));
		utf16ValueStringBuilder.Replace("=armament.skill.snippet=", value.Snippet);
		string text = Source.GetStringProperty("AtzmusSourceDisplayName");
		string text2 = Source.GetStringProperty("AtzmusSourceIndefiniteArticle").Coalesce("");
		if (text.IsNullOrEmpty())
		{
			GameObjectBlueprint aCreatureBlueprintModel = EncountersAPI.GetACreatureBlueprintModel();
			text = aCreatureBlueprintModel.DisplayName();
			text2 = (aCreatureBlueprintModel.HasProperName() ? "" : Grammar.IndefiniteArticle(text));
		}
		utf16ValueStringBuilder.Replace("=atzmus.creature=", text);
		utf16ValueStringBuilder.Replace("=atzmus.creature.an=", text2 + text);
		string stringProperty = Source.GetStringProperty("BodyBlueprint");
		string text3 = GameObjectFactory.Factory.Blueprints.GetValue(stringProperty)?.GetxTag("TextFragments", "PoeticFeatures");
		if (text3 == null)
		{
			text3 = Source.GetBlueprint().GetxTag("TextFragments", "PoeticFeatures").Coalesce("artfulness,gentle stillness,childlike wonder");
		}
		utf16ValueStringBuilder.Replace("=body.features=", Grammar.MakeTheList(text3.Split(',')));
		utf16ValueStringBuilder.Replace("=catalyst=", (LiquidVolume.GetLiquid(Source.GetStringProperty("CatalystLiquid").Coalesce("Water")) ?? LiquidVolume.Liquids.GetRandomElement().Value)?.Name ?? "water");
		string text4 = Source.GetStringProperty("HamsaDisplayName");
		string text5 = Source.GetStringProperty("HamsaIndefiniteArticle").Coalesce("");
		if (text4.IsNullOrEmpty())
		{
			GameObjectBlueprint anItemBlueprintModel = EncountersAPI.GetAnItemBlueprintModel();
			text4 = anItemBlueprintModel.DisplayName();
			text5 = (anItemBlueprintModel.HasProperName() ? "" : Grammar.IndefiniteArticle(text4));
		}
		utf16ValueStringBuilder.Replace("=hamsa=", text4);
		utf16ValueStringBuilder.Replace("=hamsa.an=", text5 + text4);
		part._Short = utf16ValueStringBuilder.ToString();
	}

	protected static bool IsValidHolder(GameObject Object)
	{
		return GolemQuestSystem.IsValidHolder(Object);
	}

	protected List<GameObject> GetValidHolders()
	{
		return System.GetValidHolders();
	}

	[WishCommand("golemquest:alleffects", null)]
	public static void WishAllEffectsGolem()
	{
		Popup.Suppress = true;
		WISH_ALL = true;
		try
		{
			GolemQuestSystem golemQuestSystem = GolemQuestSystem.Require();
			GameObject gameObject = (golemQuestSystem.Body.Material = EncountersAPI.GetACreature());
			GameObject gameObject2 = gameObject;
			GameObject gameObject3 = The.Player.CurrentCell.getClosestPassableCell().AddObject(Vehicle.CreateOwnedBy(GolemBodySelection.GetBodyBlueprintFor(gameObject2).Name, The.Player, true, true));
			gameObject3.SetAlliedLeader<AllyConstructed>(The.Player);
			foreach (KeyValuePair<string, GolemQuestSelection> selection in golemQuestSystem.Selections)
			{
				selection.Value.Apply(gameObject3);
			}
			golemQuestSystem.Body.Material = null;
			gameObject2.Destroy();
		}
		finally
		{
			Popup.Suppress = false;
			WISH_ALL = false;
		}
	}

	[WishCommand("golemquest:random", null)]
	public static void WishRandomEffectsGolem(string Param)
	{
		Popup.Suppress = true;
		WISH_RANDOM = true;
		try
		{
			GameObjectBlueprint value;
			if (int.TryParse(Param, out var result))
			{
				for (int i = 0; i < result; i++)
				{
					CreateWishGolem();
				}
			}
			else if (GameObjectFactory.Factory.Blueprints.TryGetValue(Param, out value))
			{
				CreateWishGolem(null, GameObjectFactory.Factory.CreateObject(value));
			}
		}
		finally
		{
			Popup.Suppress = false;
			WISH_RANDOM = false;
		}
	}

	[WishCommand("golemquest:random", null)]
	public static void WishRandomEffectsGolem()
	{
		Popup.Suppress = true;
		WISH_RANDOM = true;
		try
		{
			CreateWishGolem();
		}
		finally
		{
			Popup.Suppress = false;
			WISH_RANDOM = false;
		}
	}

	[WishCommand("golemquest:finishbuild", null)]
	public static void WishFinishGolem()
	{
		GolemQuestMound golemQuestMound = The.Player.CurrentZone.GetFirstObjectWithPart("GolemQuestMound")?.GetPart<GolemQuestMound>();
		if (golemQuestMound != null && golemQuestMound.CompleteTurn > 0)
		{
			golemQuestMound.CompleteTurn = The.Game.TimeTicks;
			golemQuestMound.CheckCompletion();
		}
		else
		{
			Popup.Show("No mound of scrap and clay was found to complete.");
		}
	}

	[WishCommand("golemquest:finalmound", null)]
	public static void WishFinalMound()
	{
		PlaceFinalMound();
	}

	public static void PlaceFinalMound(Cell Cell = null)
	{
		if (Cell == null)
		{
			Cell = The.PlayerCell.GetFirstEmptyAdjacentCell();
		}
		GameObject gameObject = Cell.ParentZone.FindObject("Golem Mound");
		if (gameObject == null)
		{
			gameObject = Cell.AddObject("Golem Mound");
		}
		else
		{
			Cell = gameObject.CurrentCell;
		}
		gameObject.SetIntProperty("Wish", 1);
		GameObject gameObject2 = Cell.GetFirstEmptyAdjacentCell().AddObject("RelicChest");
		gameObject2.Render.Visible = false;
		GolemArmamentSelection.ReceiveMaterials(gameObject2);
		GolemHamsaSelection.ReceiveMaterials(gameObject2);
		GolemAscensionSelection.ReceiveMaterials(gameObject2);
		GolemAtzmusSelection.ReceiveMaterials(gameObject2);
		GolemCatalystSelection.ReceiveMaterials(gameObject2);
		GolemIncantationSelection.CreateAccomplishments();
		GolemBodySelection.CreateChassis(Cell.ParentZone);
	}

	public static GameObject CreateWishGolem(Cell Cell = null, GameObject Body = null)
	{
		if (Cell == null)
		{
			Cell = The.Player.CurrentCell.getClosestPassableCell();
		}
		GolemQuestSystem golemQuestSystem = GolemQuestSystem.Require();
		GameObject gameObject = (golemQuestSystem.Body.Material = Body ?? EncountersAPI.GetACreature());
		GameObject gameObject3 = gameObject;
		GameObject gameObject4 = Cell.AddObject(Vehicle.CreateOwnedBy(GolemBodySelection.GetBodyBlueprintFor(gameObject3).Name, The.Player, true, true));
		ProcessDescription(gameObject4);
		gameObject4.Inventory.RemoveAll((GameObject x) => !x.IsNatural());
		gameObject4.SetAlliedLeader<AllyConstructed>(The.Player);
		foreach (KeyValuePair<string, GolemQuestSelection> selection in golemQuestSystem.Selections)
		{
			selection.Value.Apply(gameObject4);
		}
		gameObject4.Brain.PerformReequip();
		golemQuestSystem.Body.Material = null;
		gameObject3.Destroy();
		return gameObject4;
	}
}
