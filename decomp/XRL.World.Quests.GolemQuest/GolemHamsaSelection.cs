using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qud.API;
using UnityEngine;
using XRL.UI;
using XRL.Wish;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Tinkering;
using XRL.World.Units;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
[HasWishCommand]
public class GolemHamsaSelection : GolemGameObjectSelection
{
	public const string BASE_ID = "Hamsa";

	public override string ID => "Hamsa";

	public override string DisplayName => "hamsa";

	public override char Key => 'h';

	public override bool IsCarried => true;

	public override int Consumed => 1;

	public override bool IsValid(GameObject Object)
	{
		if (base.IsValid(Object))
		{
			return Object.WeightEach <= 5;
		}
		return false;
	}

	private static IEnumerable<string> GetSemanticTagsOf(GameObjectBlueprint Blueprint)
	{
		foreach (KeyValuePair<string, string> tag in Blueprint.Tags)
		{
			if (tag.Key.StartsWith("Semantic"))
			{
				yield return tag.Key.Substring(8);
			}
		}
	}

	public override IEnumerable<GameObjectUnit> YieldEffectsOf(GameObject Object)
	{
		bool yielded = false;
		foreach (string item in GetSemanticTagsOf(Object.GetBlueprint()))
		{
			if (!GolemMaterialSelection<GameObject, string>.Units.TryGetValue(item, out var value))
			{
				continue;
			}
			using IEnumerator<GameObjectUnit> numr = value(Object).GetEnumerator();
			if (!numr.MoveNext())
			{
				continue;
			}
			yielded = true;
			while (true)
			{
				yield return numr.Current;
				if (numr.MoveNext())
				{
					continue;
				}
				goto end_IL_008c;
			}
			end_IL_008c:;
		}
		if (!yielded)
		{
			yield return UnitTrinket(Object).First();
		}
	}

	[WishCommand("golemquest:hamsa", null)]
	private static void Wish()
	{
		ReceiveMaterials(The.Player);
	}

	public static void ReceiveMaterials(GameObject Object)
	{
		List<string> list = GolemMaterialSelection<GameObject, string>.Units.Keys.Select((string text) => "Semantic" + text).ToList();
		GameObjectFactory factory = GameObjectFactory.Factory;
		Dictionary<string, (GameObjectBlueprint, int)> dictionary = new Dictionary<string, (GameObjectBlueprint, int)>();
		List<string> list2 = new List<string>();
		foreach (GameObjectBlueprint blueprint in factory.BlueprintList)
		{
			if (!EncountersAPI.IsEligibleForDynamicEncounters(blueprint) || blueprint.IsNatural() || blueprint.CachedDisplayNameStripped.StartsWith("[") || !blueprint.TryGetPartParameter<int>("Physics", "Weight", out var Result) || Result > 20 || (blueprint.TryGetPartParameter<bool>("Physics", "Takeable", out var Result2) && !Result2) || !blueprint.Tags.Any((KeyValuePair<string, string> keyValuePair) => keyValuePair.Key.StartsWith("Semantic")))
			{
				continue;
			}
			if (Result > 5)
			{
				GameObject gameObject = blueprint.createSample();
				bool flag = ItemModding.ModificationApplicable("ModWillowy", gameObject);
				gameObject.Pool();
				if (!flag)
				{
					continue;
				}
			}
			list2.Clear();
			foreach (string item in list)
			{
				if (blueprint.Tags.ContainsKey(item))
				{
					list2.Add(item);
				}
			}
			foreach (string item2 in list2)
			{
				if (!dictionary.TryGetValue(item2, out var value) || list2.Count < value.Item2)
				{
					dictionary[item2] = (blueprint, list2.Count);
				}
			}
		}
		foreach (KeyValuePair<string, (GameObjectBlueprint, int)> item3 in dictionary)
		{
			try
			{
				GameObject gameObject2 = factory.CreateUnmodifiedObject(item3.Value.Item1);
				if (gameObject2 != null)
				{
					gameObject2.Physics.Owner = null;
					gameObject2.MakeUnderstood();
					Object.ReceiveObject(gameObject2);
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("golemquest:hamsa", x);
			}
		}
	}

	[WishCommand("golemquest:hamsa:tree", null)]
	public static void WishList()
	{
		Dictionary<GameObjectBlueprint, List<GameObjectBlueprint>> dictionary = new Dictionary<GameObjectBlueprint, List<GameObjectBlueprint>>();
		HashSet<GameObjectBlueprint> hashSet = new HashSet<GameObjectBlueprint>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			int partParameter = blueprint.GetPartParameter("Physics", "Weight", 10);
			if (blueprint.IsBaseBlueprint() || partParameter > 5 || (blueprint.Tags.Any((KeyValuePair<string, string> x) => x.Key.Contains("Semantic")) && GetSemanticTagsOf(blueprint).Any(GolemMaterialSelection<GameObject, string>.Units.ContainsKey)))
			{
				continue;
			}
			GameObjectBlueprint gameObjectBlueprint = blueprint;
			GameObjectBlueprint shallowParent = blueprint.ShallowParent;
			while (shallowParent != null)
			{
				if (dictionary.TryGetValue(shallowParent, out var value))
				{
					if (!value.Contains(gameObjectBlueprint))
					{
						value.Add(gameObjectBlueprint);
					}
					break;
				}
				dictionary[shallowParent] = new List<GameObjectBlueprint> { gameObjectBlueprint };
				gameObjectBlueprint = shallowParent;
				shallowParent = gameObjectBlueprint.ShallowParent;
				if (gameObjectBlueprint.IsBaseBlueprint())
				{
					hashSet.Add(gameObjectBlueprint);
					break;
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder("Hamsa without defined effects\n-----");
		foreach (GameObjectBlueprint item in hashSet)
		{
			stringBuilder.Append("\n\n").Append(item.Name);
			foreach (GameObjectBlueprint item2 in dictionary[item])
			{
				WishAppendChild(stringBuilder, item2, dictionary);
			}
		}
		string text = stringBuilder.ToString();
		ClipboardHelper.SetClipboardData(text);
		Popup.Show(text);
	}

	private static void WishAppendChild(StringBuilder SB, GameObjectBlueprint Blueprint, Dictionary<GameObjectBlueprint, List<GameObjectBlueprint>> Tree, int Indent = 1)
	{
		SB.Append('\n').Append('-', Indent).Append(' ')
			.Append(Blueprint.Name);
		if (!Tree.TryGetValue(Blueprint, out var value))
		{
			return;
		}
		foreach (GameObjectBlueprint item in value)
		{
			WishAppendChild(SB, item, Tree, Indent + 1);
		}
	}

	static GolemHamsaSelection()
	{
		GolemMaterialSelection<GameObject, string>.Units = new Dictionary<string, UnitGenerator>
		{
			{ "Melee", UnitMelee },
			{ "Missile", UnitMissile },
			{ "Thrown", UnitThrown },
			{ "Protection", UnitProtection },
			{ "Death", UnitDeath },
			{ "Cybernetics", UnitCybernetics },
			{ "Scholarly", UnitSkillPoints },
			{ "Tool", UnitSkillPoints },
			{ "DataDisk", UnitDataDisk },
			{ "Energy", UnitElectricalGeneration },
			{ "Conduit", UnitElectricalGeneration },
			{ "Power", UnitElectricalGeneration },
			{ "LightSource", UnitLightManipulation },
			{ "Medical", UnitRegeneration },
			{ "Seating", UnitRegeneration },
			{ "Scrap", UnitPsychometry },
			{ "Precious", UnitEgo },
			{ "Music", UnitEgo },
			{ "Beauty", UnitEgo },
			{ "Trinket", UnitTrinket },
			{ "Storage", UnitStorage },
			{ "Tonic", UnitTonic },
			{ "Key", UnitPhasing },
			{ "Garbage", UnitGarbage },
			{ "Food", UnitFood },
			{ "Sleep", UnitSleep },
			{ "Corrosion", UnitCorrosion },
			{ "Poison", UnitPoison },
			{ "Normality", UnitNormality },
			{ "Random", UnitRandom },
			{ "Travel", UnitTravel },
			{ "Confusion", UnitConfusion },
			{ "Duplication", UnitDuplication },
			{ "Movement", UnitMovement },
			{ "Floatation", UnitFloatation },
			{ "Maps", UnitMaps },
			{ "Fire", UnitHeat },
			{ "Heat", UnitHeat },
			{ "Ice", UnitCold },
			{ "Cold", UnitCold },
			{ "Time", UnitTime },
			{ "Flight", UnitFlight },
			{ "Figurine", UnitFigurine },
			{ "Explosive", UnitExplosive },
			{ "Stun", UnitStun },
			{ "Electromagnetic", UnitElectromagnetic },
			{ "Repulsive", UnitRepulsive }
		};
	}

	private static IEnumerable<GameObjectUnit> UnitMelee(GameObject Object)
	{
		yield return new GameObjectPartUnit
		{
			Part = new ReduceEnergyCosts
			{
				IncludeTypes = "Melee",
				PercentageReduction = "15",
				WorksOnSelf = true,
				WorksOnEquipper = false,
				IsPowered = false
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitMissile(GameObject Object)
	{
		yield return new GameObjectPartUnit
		{
			Part = new ReduceEnergyCosts
			{
				IncludeTypes = "Missile",
				ExcludeTypes = "Throw",
				PercentageReduction = "25",
				WorksOnSelf = true,
				WorksOnEquipper = false,
				IsPowered = false
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitThrown(GameObject Object)
	{
		yield return new GameObjectPartUnit
		{
			Part = new ReduceEnergyCosts
			{
				IncludeTypes = "Throw",
				PercentageReduction = "50",
				WorksOnSelf = true,
				WorksOnEquipper = false,
				IsPowered = false
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitProtection(GameObject Object)
	{
		Armor armor = Object?.GetPart<Armor>();
		bool flag = armor != null && armor.DV > armor.AV;
		yield return new GameObjectAttributeUnit
		{
			Attribute = (flag ? "DV" : "AV"),
			Value = (flag ? 6 : 6)
		};
	}

	private static IEnumerable<GameObjectUnit> UnitDeath(GameObject Object)
	{
		yield return new GameObjectPartUnit
		{
			Part = new FungalFortitude
			{
				WorksOnSelf = true,
				WorksOnHolder = false,
				WorksOnCarrier = false,
				AVBonus = 1,
				ResistBonus = 4
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitCybernetics(GameObject Object)
	{
		if (Object != null && Object.HasPart<CyberneticsBaseItem>())
		{
			yield return new GameObjectCyberneticsUnit
			{
				Blueprint = Object.Blueprint,
				LicenseStat = "Toughness",
				Removable = false
			};
		}
	}

	private static IEnumerable<GameObjectUnit> UnitSkillPoints(GameObject Object)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "SP",
			Value = 800
		};
	}

	private static IEnumerable<GameObjectUnit> UnitDataDisk(GameObject Object)
	{
		if (Object == null || !Object.TryGetPart<DataDisk>(out var Part) || Part.Data?.Blueprint == null || !GameObjectFactory.Factory.Blueprints.TryGetValue(Part.Data.Blueprint, out var blueprint))
		{
			yield break;
		}
		Object = null;
		foreach (string item in GetSemanticTagsOf(blueprint))
		{
			if (!GolemMaterialSelection<GameObject, string>.Units.TryGetValue(item, out var value))
			{
				continue;
			}
			UnitGenerator unitGenerator = value;
			GameObject gameObject = Object;
			if (gameObject == null)
			{
				GameObject gameObject2;
				Object = (gameObject2 = blueprint.createSample());
				gameObject = gameObject2;
			}
			foreach (GameObjectUnit item2 in unitGenerator(gameObject))
			{
				yield return item2;
			}
		}
		Object?.Obliterate();
	}

	private static IEnumerable<GameObjectUnit> UnitElectricalGeneration(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Electrical Generation",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitLightManipulation(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Light Manipulation",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitRegeneration(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Regeneration",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitPsychometry(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Psychometry",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitPhasing(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Phasing",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitEgo(GameObject Object)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Ego",
			Value = 8
		};
	}

	private static IEnumerable<GameObjectUnit> UnitTrinket(GameObject Object)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "All",
			Value = 2
		};
	}

	private static IEnumerable<GameObjectUnit> UnitStorage(GameObject Object)
	{
		yield return new GameObjectPartUnit
		{
			Part = new CarryBonus
			{
				WorksOnSelf = true,
				WorksOnEquipper = false,
				Amount = 1000,
				Style = "Percent"
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitTonic(GameObject Object)
	{
		yield return new GameObjectSaveModifierUnit
		{
			Vs = "Overdosing",
			Value = SavingThrows.IMMUNITY
		};
	}

	private static IEnumerable<GameObjectUnit> UnitGarbage(GameObject Object)
	{
		yield return new GameObjectPartUnit
		{
			Part = new TrashOracle
			{
				Magnitude = 10
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitFood(GameObject Object)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Hitpoints",
			Value = 25,
			Percent = true
		};
	}

	private static IEnumerable<GameObjectUnit> UnitSleep(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Sleep Gas Generation",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitPoison(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Poison Gas Generation",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitCorrosion(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Corrosive Gas Generation",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitNormality(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Class = "NormalityGasGeneration",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitRandom(GameObject Object)
	{
		yield return new GameObjectGolemQuestRandomUnit
		{
			SelectionID = "Hamsa",
			Amount = 2
		};
	}

	private static IEnumerable<GameObjectUnit> UnitTravel(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Teleportation",
			Level = 6
		};
	}

	private static IEnumerable<GameObjectUnit> UnitConfusion(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Confusion",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitDuplication(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Temporal Fugue",
			Level = 6
		};
	}

	private static IEnumerable<GameObjectUnit> UnitMovement(GameObject Object)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "MoveSpeed",
			Value = -50
		};
	}

	private static IEnumerable<GameObjectUnit> UnitFloatation(GameObject Object)
	{
		yield return new GameObjectBodyPartUnit
		{
			Type = "Floating Nearby",
			InsertAfter = "Floating Nearby",
			Manager = "Golem::Hamsa"
		};
	}

	private static IEnumerable<GameObjectUnit> UnitMaps(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Clairvoyance",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitHeat(GameObject Object)
	{
		return UnitAmp(Object, 75, 0);
	}

	private static IEnumerable<GameObjectUnit> UnitCold(GameObject Object)
	{
		return UnitAmp(Object, 0, 75);
	}

	private static IEnumerable<GameObjectUnit> UnitAmp(GameObject Object, int Heat, int Cold)
	{
		yield return new GameObjectPartUnit
		{
			Part = new ThermalAmp
			{
				ColdDamage = Cold,
				ModifyCold = Cold,
				HeatDamage = Heat,
				ModifyHeat = Heat,
				WorksOnSelf = true,
				WorksOnWearer = false,
				IsPowered = false,
				NameForStatus = null
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitTime(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Time Dilation",
			Level = 8
		};
	}

	private static IEnumerable<GameObjectUnit> UnitFlight(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Wings",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitFigurine(GameObject Object)
	{
		int num = 1000;
		GameObjectBlueprint value = GameObjectFactory.Factory.Blueprints.GetValue(Object?.GetPart<RandomFigurine>()?.Creature);
		if (value == null)
		{
			yield break;
		}
		string partParameter = value.GetPartParameter<string>("Brain", "Factions");
		if (partParameter.IsNullOrEmpty())
		{
			yield break;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		Brain.FillFactionMembership(dictionary, partParameter);
		if (dictionary.IsNullOrEmpty())
		{
			yield break;
		}
		float num2 = 0f;
		int num3 = 0;
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			Faction ifExists = Factions.GetIfExists(item.Key);
			if (ifExists != null && ifExists.Visible)
			{
				num2 += (float)item.Value;
				num3++;
			}
		}
		if (num3 == 0)
		{
			yield break;
		}
		GameObjectUnitAggregate gameObjectUnitAggregate = null;
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			Faction ifExists2 = Factions.GetIfExists(item2.Key);
			if (ifExists2 != null && ifExists2.Visible)
			{
				GameObjectReputationUnit gameObjectReputationUnit = new GameObjectReputationUnit
				{
					Entry = ifExists2,
					Value = Mathf.RoundToInt((float)item2.Value / num2 * (float)num / 5f) * 5,
					Type = "Figurine"
				};
				if (num3 == 1)
				{
					yield return gameObjectReputationUnit;
					yield break;
				}
				if (gameObjectUnitAggregate == null)
				{
					gameObjectUnitAggregate = new GameObjectUnitAggregate();
				}
				gameObjectUnitAggregate.Units.Add(gameObjectReputationUnit);
			}
		}
		yield return gameObjectUnitAggregate;
	}

	private static IEnumerable<GameObjectUnit> UnitExplosive(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Stunning Force",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitStun(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Stunning Force",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitElectromagnetic(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Electromagnetic Pulse",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitRepulsive(GameObject Object)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Socially Repugnant",
			Level = 1
		};
	}
}
