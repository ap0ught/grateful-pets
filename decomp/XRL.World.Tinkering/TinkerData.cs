using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Occult.Engine.CodeGeneration;
using XRL.Language;
using XRL.UI;
using XRL.Wish;
using XRL.World.Parts;

namespace XRL.World.Tinkering;

[Serializable]
[HasWishCommand]
[HasGameBasedStaticCache]
[GenerateSerializationPartial]
public sealed class TinkerData : IComposite
{
	public string DisplayName;

	public string Blueprint;

	public string Category;

	public string Type;

	public int Tier;

	public string Cost;

	public string Ingredient;

	public int DescriptionLineCount = 1;

	[NonSerialized]
	private string _PartName;

	[GameBasedStaticCache(true, false, ClearInstance = true)]
	public static List<TinkerData> _TinkerRecipes = new List<TinkerData>();

	[GameBasedStaticCache(true, false)]
	public static List<TinkerData> KnownRecipes = new List<TinkerData>();

	public static Dictionary<TinkerData, string> UnclippedDataDescriptions = new Dictionary<TinkerData, string>();

	public static Dictionary<TinkerData, string> DataDescriptions = new Dictionary<TinkerData, string>();

	public static Dictionary<TinkerData, string> DataNames = new Dictionary<TinkerData, string>();

	public static Dictionary<TinkerData, string> InventoryCateogires = new Dictionary<TinkerData, string>();

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public bool WantFieldReflection => false;

	public string PartName
	{
		get
		{
			if (_PartName == null)
			{
				_PartName = Blueprint.Replace("[mod]", "");
			}
			return _PartName;
		}
	}

	public string UICategory
	{
		get
		{
			if (!InventoryCateogires.ContainsKey(this))
			{
				if (GameObjectFactory.Factory.Blueprints.ContainsKey(Blueprint))
				{
					InventoryCateogires.Add(this, TinkeringHelpers.TinkeredItemInventoryCategory(Blueprint));
				}
				else
				{
					InventoryCateogires.Add(this, "<invalid blueprint>");
				}
			}
			return InventoryCateogires[this];
		}
	}

	public string LongDisplayName
	{
		get
		{
			if (!DataNames.ContainsKey(this))
			{
				if (GameObjectFactory.Factory.Blueprints.ContainsKey(Blueprint))
				{
					DataNames.Add(this, TinkeringHelpers.TinkeredItemDisplayName(Blueprint));
				}
				else
				{
					DataNames.Add(this, "<invalid blueprint>");
				}
			}
			return DataNames[this];
		}
	}

	public string UnclippedDescription
	{
		get
		{
			if (!UnclippedDataDescriptions.ContainsKey(this))
			{
				if (GameObjectFactory.Factory.HasBlueprint(Blueprint))
				{
					GameObject gameObject = GameObject.CreateSample(Blueprint);
					TinkeringHelpers.StripForTinkering(gameObject);
					TinkeringHelpers.ForceToBePowered(gameObject);
					if (gameObject.HasPart<Description>())
					{
						StringBuilder stringBuilder = Event.NewStringBuilder();
						stringBuilder.Append('\n');
						TinkerItem part = gameObject.GetPart<TinkerItem>();
						if (part != null && part.NumberMade > 1)
						{
							stringBuilder.Append("{{rules|Makes a batch of ").Append(Grammar.Cardinal(part.NumberMade)).Append(".}}\n\n");
						}
						Description part2 = gameObject.GetPart<Description>();
						if (part2 != null)
						{
							stringBuilder.Append(part2.GetShortDescription(AsIfKnown: true, NoConfusion: true, "Tinkering"));
						}
						stringBuilder.Append('\n');
						UnclippedDataDescriptions.Add(this, stringBuilder.ToString());
						DescriptionLineCount = UnclippedDataDescriptions[this].Count((char c) => c == '\n');
					}
					else
					{
						UnclippedDataDescriptions.Add(this, "<none>");
					}
					gameObject.Obliterate();
				}
				else
				{
					UnclippedDataDescriptions.Add(this, "<none>");
				}
			}
			return UnclippedDataDescriptions[this];
		}
	}

	public string Description
	{
		get
		{
			if (!DataDescriptions.ContainsKey(this))
			{
				if (GameObjectFactory.Factory.HasBlueprint(Blueprint))
				{
					GameObject gameObject = GameObject.CreateSample(Blueprint);
					TinkeringHelpers.StripForTinkering(gameObject);
					TinkeringHelpers.ForceToBePowered(gameObject);
					if (gameObject.HasPart<Description>())
					{
						StringBuilder stringBuilder = Event.NewStringBuilder();
						stringBuilder.Append('\n');
						TinkerItem part = gameObject.GetPart<TinkerItem>();
						if (part != null && part.NumberMade > 1)
						{
							stringBuilder.Append("{{rules|Makes a batch of ").Append(Grammar.Cardinal(part.NumberMade)).Append(".}}\n\n");
						}
						Description part2 = gameObject.GetPart<Description>();
						if (part2 != null)
						{
							stringBuilder.Append(part2.GetShortDescription(AsIfKnown: true, NoConfusion: true, "Tinkering"));
						}
						stringBuilder.Append('\n');
						DataDescriptions.Add(this, StringFormat.ClipText(stringBuilder.ToString(), 76, KeepNewlines: true));
						DescriptionLineCount = DataDescriptions[this].Count((char c) => c == '\n');
					}
					else
					{
						DataDescriptions.Add(this, "<none>");
					}
					gameObject.Obliterate();
				}
				else
				{
					DataDescriptions.Add(this, "<none>");
				}
			}
			return DataDescriptions[this];
		}
	}

	public static List<TinkerData> TinkerRecipes
	{
		get
		{
			if (_TinkerRecipes.Count == 0)
			{
				ModificationFactory.CheckInit();
				foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
				{
					TinkerData tinkerData = TinkerItem.LoadBlueprint(blueprint);
					if (tinkerData != null)
					{
						_TinkerRecipes.Add(tinkerData);
					}
				}
				foreach (ModEntry mod in ModificationFactory.ModList)
				{
					if (mod.TinkerAllowed)
					{
						_TinkerRecipes.Add(new TinkerData
						{
							Blueprint = "[mod]" + mod.Part,
							DisplayName = mod.TinkerDisplayName,
							Cost = "",
							Ingredient = mod.TinkerIngredient,
							Tier = mod.TinkerTier,
							Category = mod.TinkerCategory,
							Type = "Mod"
						});
					}
				}
			}
			return _TinkerRecipes;
		}
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(DisplayName);
		Writer.WriteOptimized(Blueprint);
		Writer.WriteOptimized(Category);
		Writer.WriteOptimized(Type);
		Writer.WriteOptimized(Tier);
		Writer.WriteOptimized(Cost);
		Writer.WriteOptimized(Ingredient);
		Writer.WriteOptimized(DescriptionLineCount);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public void Read(SerializationReader Reader)
	{
		DisplayName = Reader.ReadOptimizedString();
		Blueprint = Reader.ReadOptimizedString();
		Category = Reader.ReadOptimizedString();
		Type = Reader.ReadOptimizedString();
		Tier = Reader.ReadOptimizedInt32();
		Cost = Reader.ReadOptimizedString();
		Ingredient = Reader.ReadOptimizedString();
		DescriptionLineCount = Reader.ReadOptimizedInt32();
	}

	public static TinkerData LegacyRead(SerializationReader reader)
	{
		return new TinkerData
		{
			DisplayName = reader.ReadString(),
			Blueprint = reader.ReadString(),
			Category = reader.ReadString(),
			Type = reader.ReadString(),
			Tier = reader.ReadInt32(),
			Cost = reader.ReadString(),
			Ingredient = reader.ReadString(),
			DescriptionLineCount = reader.ReadInt32()
		};
	}

	public bool CanMod(string ModTag)
	{
		if (!ModificationFactory.ModsByPart.TryGetValue(PartName, out var value))
		{
			return false;
		}
		if (!value.TinkerAllowed)
		{
			return false;
		}
		List<string> list = ModTag.CachedCommaExpansion();
		string[] tableList = value.TableList;
		foreach (string item in tableList)
		{
			if (list.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool Known()
	{
		return KnownRecipes.Any((TinkerData r) => r == this || r.Blueprint == Blueprint);
	}

	public static void UnlearnRecipe(string blueprint)
	{
		for (int i = 0; i < KnownRecipes.Count; i++)
		{
			if (KnownRecipes[i].Blueprint == blueprint)
			{
				KnownRecipes.RemoveAt(i);
				break;
			}
		}
	}

	[WishCommand(null, null)]
	public static bool DataDisk(string blueprint)
	{
		if (blueprint.StartsWith("mod:"))
		{
			blueprint = "[mod]" + blueprint.Substring(4);
		}
		blueprint = blueprint.ToLower();
		GameObject gameObject = createDataDisk(TinkerRecipes.FirstOrDefault((TinkerData t) => t.Blueprint.ToLower() == blueprint));
		The.Player.ReceiveObject(gameObject);
		return true;
	}

	[WishCommand(null, null)]
	public static bool DataDisk()
	{
		int index = Popup.PickOption("Pick a Blueprint:", null, "", "Sounds/UI/ui_notification", TinkerRecipes.ConvertAll((TinkerData t) => t.Blueprint).ToArray());
		GameObject gameObject = createDataDisk(TinkerRecipes[index]);
		The.Player.ReceiveObject(gameObject);
		return true;
	}

	public static GameObject createDataDisk(string blueprint)
	{
		if (blueprint.StartsWith("mod:"))
		{
			string modBlueprint = "[mod]" + blueprint.Substring(4);
			return createDataDisk(TinkerRecipes.FirstOrDefault((TinkerData t) => t.Type == "Mod" && t.Blueprint == modBlueprint));
		}
		return createDataDisk(TinkerRecipes.FirstOrDefault((TinkerData t) => t.Type == "Build" && t.Blueprint == blueprint));
	}

	public static GameObject createDataDisk(TinkerData data)
	{
		return GameObjectFactory.Factory.CreateObject("DataDisk", (GameObject o) =>
		{
			o.GetPart<DataDisk>().Data = data;
		});
	}

	public static void LearnBlueprint(string blueprint)
	{
		TinkerData tinkerData = TinkerRecipes.FirstOrDefault((TinkerData t) => t.Type == "Build" && t.Blueprint == blueprint);
		if (tinkerData != null && !KnownRecipes.Contains(tinkerData))
		{
			KnownRecipes.Add(tinkerData);
		}
	}

	public static void LearnMod(string blueprint)
	{
		string ModBlueprint = "[mod]" + blueprint;
		TinkerData tinkerData = TinkerRecipes.FirstOrDefault((TinkerData t) => t.Type == "Mod" && t.Blueprint == ModBlueprint);
		if (tinkerData != null && !KnownRecipes.Contains(tinkerData))
		{
			KnownRecipes.Add(tinkerData);
		}
	}

	public static bool RecipeKnown(TinkerData Data)
	{
		foreach (TinkerData knownRecipe in KnownRecipes)
		{
			if (knownRecipe.Blueprint == Data.Blueprint)
			{
				return true;
			}
		}
		return false;
	}
}
