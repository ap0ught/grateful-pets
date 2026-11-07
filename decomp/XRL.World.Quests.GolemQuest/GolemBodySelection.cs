using System;
using System.Collections.Generic;
using Qud.API;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.Parts;
using XRL.World.Units;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
[HasWishCommand]
public class GolemBodySelection : GolemGameObjectSelection
{
	public const string BASE_ID = "Body";

	[NonSerialized]
	private static Dictionary<string, string> BodyBySpecies;

	public override string ID => "Body";

	public override string DisplayName => "body";

	public override char Key => 'b';

	public override bool IsCarried => false;

	public GameObjectBlueprint BodyBlueprint => GetBodyBlueprintFor(Material);

	public override bool IsValid(GameObject Object)
	{
		if (!base.IsValid(Object))
		{
			return false;
		}
		if (!Object.IsPlayerLed() || Object.Level < 30)
		{
			return Object.HasIntProperty("GolemChassis");
		}
		return true;
	}

	public override void Apply(GameObject Object)
	{
		base.Apply(Object);
		Object.GetStat("AV").BaseValue += 10;
		Object.GetStat("Strength").BoostStat(3);
		Object.GetStat("Toughness").BoostStat(2);
	}

	public override IEnumerable<GameObjectUnit> YieldEffectsOf(GameObject Object)
	{
		GameObjectBlueprint bodyBlueprintFor = GetBodyBlueprintFor(Object);
		yield return new GameObjectPlaceholderUnit
		{
			Description = bodyBlueprintFor.CachedDisplayNameStripped
		};
	}

	public static Dictionary<string, string> GetBodyBySpecies()
	{
		if (BodyBySpecies != null)
		{
			return BodyBySpecies;
		}
		BodyBySpecies = new Dictionary<string, string>();
		Dictionary<string, List<string>> dictionary = null;
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!blueprint.HasTag("Golem") || blueprint.IsBaseBlueprint())
			{
				continue;
			}
			string text = blueprint.GetPropertyOrTag("SpeciesOverride").Coalesce(blueprint.GetPropertyOrTag("Species").Coalesce(blueprint.GetPropertyOrTag("Class")));
			if (text.IsNullOrEmpty())
			{
				MetricsManager.LogWarning("Blueprint '" + blueprint.Name + "' defined as golem body but lacks a Species or Class tag.");
				continue;
			}
			if (blueprint.GetxTag("TextFragments", "PoeticFeatures").IsNullOrEmpty())
			{
				MetricsManager.LogWarning("Blueprint '" + blueprint.Name + "' defined as golem body but lacks a PoeticFeatures xtag.");
			}
			if (BodyBySpecies.TryGetValue(text, out var value) && value != blueprint.Name)
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<string, List<string>>();
				}
				if (!dictionary.TryGetValue(text, out var value2))
				{
					Dictionary<string, List<string>> dictionary2 = dictionary;
					List<string> list = new List<string> { value };
					value2 = list;
					dictionary2[text] = list;
				}
				if (!value2.Contains(blueprint.Name))
				{
					value2.Add(blueprint.Name);
				}
			}
			else
			{
				BodyBySpecies[text] = blueprint.Name;
			}
		}
		if (!dictionary.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, List<string>> item in dictionary)
			{
				MetricsManager.LogError("Duplicate golem species mapping: " + item.Key + " used by " + string.Join(", ", item.Value));
			}
		}
		return BodyBySpecies;
	}

	public static GameObjectBlueprint GetBodyBlueprintFor(GameObject Object)
	{
		return GetBodyBlueprintFor(Object.Property, Object.GetBlueprint().Tags);
	}

	public static GameObjectBlueprint GetBodyBlueprintFor(GameObjectBlueprint Blueprint)
	{
		return GetBodyBlueprintFor(Blueprint.Props, Blueprint.Tags);
	}

	public static GameObjectBlueprint GetBodyBlueprintFor(Dictionary<string, string> Properties, Dictionary<string, string> Tags)
	{
		Dictionary<string, string> bodyBySpecies = GetBodyBySpecies();
		string text = ((Properties != null) ? Properties.GetValue("SpeciesOverride").Coalesce((Tags != null) ? Tags.GetValue("SpeciesOverride") : null) : null);
		if (!text.IsNullOrEmpty() && bodyBySpecies.TryGetValue(text, out var value) && GameObjectFactory.Factory.Blueprints.TryGetValue(value, out var value2))
		{
			return value2;
		}
		text = ((Properties != null) ? Properties.GetValue("Species").Coalesce((Tags != null) ? Tags.GetValue("Species") : null) : null);
		if (!text.IsNullOrEmpty() && bodyBySpecies.TryGetValue(text, out value) && GameObjectFactory.Factory.Blueprints.TryGetValue(value, out value2))
		{
			return value2;
		}
		text = ((Properties != null) ? Properties.GetValue("Class").Coalesce((Tags != null) ? Tags.GetValue("Class") : null) : null);
		if (!text.IsNullOrEmpty() && bodyBySpecies.TryGetValue(text, out value) && GameObjectFactory.Factory.Blueprints.TryGetValue(value, out value2))
		{
			return value2;
		}
		return GameObjectFactory.Factory.Blueprints.GetValue("Oddity Golem");
	}

	[WishCommand("golemquest:chassis", null)]
	private static void WishChassis()
	{
		CreateChassis(The.ActiveZone);
	}

	public static void CreateChassis(Zone Z)
	{
		Cell cell = Z.GetCell(79, 24);
		GameObjectFactory factory = GameObjectFactory.Factory;
		if (cell.HasObjectWithIntProperty("GolemChassis"))
		{
			return;
		}
		foreach (KeyValuePair<string, string> bodyBySpecy in GetBodyBySpecies())
		{
			GamePartBlueprint part = factory.Blueprints[bodyBySpecy.Value].GetPart("Render");
			GameObject gameObject = factory.CreateSampleObject("Object");
			Render render = gameObject.RequirePart<Render>();
			part.InitializePartInstance(render);
			render.Visible = false;
			render.DisplayName = render.DisplayName.Replace("golem", "chassis");
			gameObject.SetIntProperty("GolemChassis", 1);
			gameObject.SetStringProperty("SpeciesOverride", bodyBySpecy.Key);
			cell.AddObject(gameObject);
		}
	}

	[WishCommand("golemquest:body", null)]
	private static void Wish()
	{
		WishCompanion(EncountersAPI.GetACreature());
	}

	[WishCommand("golemquest:body", null)]
	private static void WishSpec(string Value)
	{
		GameObjectBlueprint value;
		if (Value == "animated")
		{
			WishCompanion(EncountersAPI.GetAnAnimatedObject());
		}
		else if (GameObjectFactory.Factory.Blueprints.TryGetValue(Value, out value))
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateObject(value);
			if (!value.HasPart("Brain"))
			{
				AnimateObject.Animate(gameObject);
			}
			WishCompanion(gameObject);
		}
		else
		{
			Popup.ShowFail("No blueprint by ID '" + Value + "' found.");
		}
	}

	private static void WishCompanion(GameObject Body)
	{
		The.Player.CurrentCell.GetLocalEmptyAdjacentCells().GetRandomElement().AddObject(Body);
		Body.GetStat("Level").BaseValue = 30;
		Body.Brain.Goals.Clear();
		Body.SetAlliedLeader<AllyWish>(The.Player);
	}
}
