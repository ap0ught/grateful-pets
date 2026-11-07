using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.Wish;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace Sheeter;

[HasWishCommand]
public class Sheeter
{
	private class TagRecord
	{
		public string Tag;

		public string Type;

		public int Score;

		public Dictionary<string, int> Values;
	}

	public class BlueprintColumn
	{
		public FieldInfo Field;

		public BlueprintElement[] Attributes;

		public string Header => Field.Name;
	}

	public static int CalculateAttackDamage(XRL.World.GameObject Attacker)
	{
		int num = 0;
		_ = Attacker.Blueprint == "Barkbiter";
		if (Attacker.Brain == null)
		{
			return 0;
		}
		Attacker.Brain.PerformReequip();
		Body body = Attacker.Body;
		if (body == null)
		{
			return 0;
		}
		XRL.World.GameObject PrimaryWeapon = null;
		bool PickedFromHand = false;
		body.ForeachPart((BodyPart pPart) =>
		{
			if ((pPart.Equipped != null || pPart.DefaultBehavior != null) && (pPart.Primary || PrimaryWeapon == null || (pPart.Type == "Hand" && !PickedFromHand)))
			{
				if (pPart.Equipped != null && pPart.Equipped.HasPart<MeleeWeapon>())
				{
					PrimaryWeapon = pPart.Equipped;
					if (pPart.Type == "Hand")
					{
						PickedFromHand = true;
					}
				}
				else if (pPart.DefaultBehavior != null && pPart.DefaultBehavior.HasPart<MeleeWeapon>())
				{
					PrimaryWeapon = pPart.DefaultBehavior;
					if (!pPart.DefaultBehavior.HasTag("UndesirableWeapon"))
					{
						PickedFromHand = true;
					}
				}
			}
		});
		if (PrimaryWeapon != null)
		{
			MeleeWeapon part = PrimaryWeapon.GetPart<MeleeWeapon>();
			if (part != null)
			{
				int num2 = Stat.RollMin(part.BaseDamage);
				int num3 = Stat.RollMax(part.BaseDamage);
				num += num2 + (num3 - num2) / 2;
			}
			ElementalDamage part2 = PrimaryWeapon.GetPart<ElementalDamage>();
			if (part2 != null)
			{
				int num4 = Stat.RollMin(part2.Damage);
				int num5 = Stat.RollMax(part2.Damage);
				num += (num4 + (num5 - num4) / 2) * part2.Chance / 100;
			}
		}
		int SecondaryChance = GlobalConfig.GetIntSetting("BaseSecondaryAttackChance", 15);
		XRL.World.Event E = new XRL.World.Event("CommandAttack");
		if (Attacker.HasSkill("Dual_Wield_Offhand_Strikes"))
		{
			SecondaryChance = GlobalConfig.GetIntSetting("OffhandStrikesSecondaryAttackChance", 35);
		}
		if (Attacker.HasSkill("Dual_Wield_Ambidexterity"))
		{
			SecondaryChance = GlobalConfig.GetIntSetting("AmbidexteritySecondaryAttackChance", 55);
		}
		if (Attacker.HasSkill("Dual_Wield_Two_Weapon_Fighting"))
		{
			SecondaryChance = GlobalConfig.GetIntSetting("TwoWeaponFightingSecondaryAttackChance", 75);
		}
		if (Attacker.HasProperty("AlwaysOffhand"))
		{
			SecondaryChance = 100;
		}
		if (E.HasParameter("AlwaysOffhand"))
		{
			SecondaryChance = E.GetIntParameter("AlwaysOffhand");
		}
		List<XRL.World.GameObject> WeaponList = new List<XRL.World.GameObject>(8);
		body.ForeachPart((BodyPart pPart) =>
		{
			if (pPart.Equipped != null || pPart.DefaultBehavior != null)
			{
				XRL.World.GameObject gameObject = pPart.Equipped;
				if (gameObject == null || gameObject.GetPart<MeleeWeapon>() == null)
				{
					gameObject = pPart.DefaultBehavior;
				}
				if (!WeaponList.Contains(gameObject))
				{
					MeleeWeapon part5 = gameObject.GetPart<MeleeWeapon>();
					if ((part5 != null && part5.Slot == null) || pPart.Type == null || part5.Slot.Contains(pPart.Type))
					{
						XRL.World.Event obj = XRL.World.Event.New("QueryWeaponSecondaryAttackChance");
						obj.AddParameter("Weapon", gameObject);
						obj.AddParameter("BodyPart", pPart);
						obj.AddParameter("Chance", SecondaryChance);
						obj.AddParameter("Properties", E.GetStringParameter("Properties"));
						gameObject.FireEvent(obj);
						if (Attacker != null)
						{
							obj.ID = "AttackerQueryWeaponSecondaryAttackChance";
							Attacker.FireEvent(obj);
							obj.ID = "AttackerQueryWeaponSecondaryAttackChanceMultiplier";
							Attacker.FireEvent(obj);
						}
						int num10 = obj.GetIntParameter("Chance");
						if (Attacker.HasTag("AttackWithEverything"))
						{
							num10 = 100;
						}
						if (E.HasParameter("AlwaysOffhand"))
						{
							num10 = E.GetIntParameter("AlwaysOffhand");
						}
						while (num10 > 0)
						{
							if (Stat.Random(1, 100) <= num10)
							{
								WeaponList.Add(gameObject);
							}
							num10 -= 100;
						}
					}
				}
			}
		});
		foreach (XRL.World.GameObject item in WeaponList)
		{
			MeleeWeapon part3 = item.GetPart<MeleeWeapon>();
			if (part3 != null)
			{
				int num6 = Stat.RollMin(part3.BaseDamage);
				int num7 = Stat.RollMax(part3.BaseDamage);
				num += num6 + (num7 - num6) / 2;
			}
			ElementalDamage part4 = item.GetPart<ElementalDamage>();
			if (part4 != null)
			{
				int num8 = Stat.RollMin(part4.Damage);
				int num9 = Stat.RollMax(part4.Damage);
				num += (num8 + (num9 - num8) / 2) * part4.Chance / 100;
			}
		}
		return num;
	}

	public static void FactionSheeter()
	{
		Sheet sheet = new Sheet(DataManager.SavePath("factionsheet.csv"));
		List<Faction> list = new List<Faction>();
		List<int> list2 = new List<int>();
		List<string> list3 = new List<string>();
		sheet.addColumn("Tier");
		foreach (Faction item in Factions.Loop())
		{
			if (item.Visible && !item.Name.Contains("villagers"))
			{
				list.Add(item);
				list2.Add(0);
				sheet.addColumn(item.Name);
			}
		}
		sheet.endColumns();
		int num = 0;
		for (int i = 1; i <= 8; i++)
		{
			sheet.writeColumn(i);
			for (int j = 0; j < list.Count; j++)
			{
				foreach (GameObjectBlueprint factionMember in GameObjectFactory.Factory.GetFactionMembers(list[j].Name))
				{
					if (factionMember.Tier != i)
					{
						continue;
					}
					if (factionMember.HasTagOrProperty("AggregateWith"))
					{
						if (list3.Contains(factionMember.GetTag("AggregateWith")))
						{
							continue;
						}
						list3.Add(factionMember.GetTag("AggregateWith"));
					}
					num++;
				}
				sheet.writeColumn(num);
				list2[j] += num;
				num = 0;
			}
			sheet.endRow();
		}
		for (int k = 0; k < list.Count + 1; k++)
		{
			sheet.writeColumn("");
		}
		sheet.endRow();
		sheet.writeColumn("");
		for (int l = 0; l < list.Count; l++)
		{
			sheet.writeColumn(list2[l]);
		}
		sheet.finish();
	}

	public static void MonsterSheeter()
	{
		Sheet sheet = new Sheet(DataManager.SavePath("monstersheet.csv"));
		sheet.addColumn("Name");
		sheet.addColumn("Level");
		sheet.addColumn("Tier");
		sheet.addColumn("Role");
		sheet.addStatColumn("Melee");
		sheet.addStatColumn("StrMod");
		sheet.addStatColumn("Hitpoints");
		sheet.addStatColumn("AV");
		sheet.addStatColumn("DV");
		sheet.addStatColumn("MA");
		sheet.addColumn("Elec");
		sheet.addColumn("Heat");
		sheet.addColumn("Cold");
		sheet.addColumn("Acid");
		sheet.addStatColumn("QN");
		sheet.addStatColumn("MS");
		sheet.addColumn("XP");
		sheet.endColumns();
		Sheet sheet2 = new Sheet(DataManager.SavePath("monstersheet_short.csv"));
		sheet2.addColumn("Name");
		sheet2.addColumn("Level");
		sheet2.addColumn("Tier");
		sheet2.addColumn("Role");
		sheet2.addShortStatColumn("Melee");
		sheet2.addShortStatColumn("StrMod");
		sheet2.addShortStatColumn("Hitpoints");
		sheet2.addShortStatColumn("AV");
		sheet2.addShortStatColumn("DV");
		sheet2.addShortStatColumn("MA");
		sheet2.addColumn("Elec");
		sheet2.addColumn("Heat");
		sheet2.addColumn("Cold");
		sheet2.addColumn("Acid");
		sheet2.addShortStatColumn("QN");
		sheet2.addShortStatColumn("MS");
		sheet2.addColumn("XP");
		sheet2.endColumns();
		int num = 0;
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			XRL.World.Event.ResetPool();
			try
			{
				num++;
				if (num % 10 == 0)
				{
					Debug.Log(num + " of " + GameObjectFactory.Factory.BlueprintList.Count + " sheeted out...");
				}
				if (blueprint.Name.Contains("Trader") || blueprint.HasPart("Tier1Wares") || blueprint.HasPart("Tier2Wares") || blueprint.HasPart("Tier3Wares") || blueprint.HasPart("Tier4Wares") || blueprint.HasPart("Tier5Wares") || blueprint.HasPart("Tier6Wares") || blueprint.HasPart("Tier7Wares") || blueprint.HasPart("Tier8Wares") || blueprint.HasPart("YurlWares") || blueprint.Builders.ContainsKey("DataDiskWares") || blueprint.HasPart("DataDiskWares") || blueprint.HasPart("GlowpadOasisWares") || blueprint.HasPart("ScrapWares") || blueprint.Builders.ContainsKey("Tier1Wares") || blueprint.Builders.ContainsKey("Tier2Wares") || blueprint.Builders.ContainsKey("Tier3Wares") || blueprint.Builders.ContainsKey("Tier4Wares") || blueprint.Builders.ContainsKey("Tier5Wares") || blueprint.Builders.ContainsKey("Tier6Wares") || blueprint.Builders.ContainsKey("Tier7Wares") || blueprint.Builders.ContainsKey("Tier8Wares") || blueprint.Builders.ContainsKey("YurlWares") || blueprint.Builders.ContainsKey("GlowpadOasisWares") || blueprint.Builders.ContainsKey("ScrapWares") || !blueprint.HasStat("ElectricResistance") || !blueprint.HasStat("Level") || !blueprint.HasPart("Combat"))
				{
					continue;
				}
				XRL.World.Event.ResetPool();
				int tier = blueprint.Tier;
				sheet.writeColumn(blueprint.Name);
				sheet2.writeColumn(blueprint.Name);
				if (blueprint.HasStat("Level"))
				{
					sheet.writeColumn(blueprint.GetStat("Level", new Statistic()).Value);
					sheet2.writeColumn(blueprint.Name);
				}
				else
				{
					sheet.writeColumn("-1");
					sheet2.writeColumn("-1");
				}
				sheet.writeColumn(tier);
				sheet2.writeColumn(tier);
				string text = "unassiged";
				if (blueprint.Tags.ContainsKey("Role") && !string.IsNullOrEmpty(blueprint.Tags["Role"]))
				{
					text = blueprint.Tags["Role"];
				}
				if (blueprint.Props.ContainsKey("Role") && !string.IsNullOrEmpty(blueprint.Props["Role"]))
				{
					text = blueprint.Props["Role"];
				}
				sheet.writeColumn(text);
				sheet2.writeColumn(text);
				List<XRL.World.GameObject> list = new List<XRL.World.GameObject>();
				for (int i = 0; i < 100; i++)
				{
					list.Add(blueprint.createOne());
				}
				sheet.writeStats(list.Map((XRL.World.GameObject o) => CalculateAttackDamage(o)));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => o.StatMod("Strength")));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("Hitpoints")));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatAV(o)));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatDV(o)));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatMA(o)));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => CalculateAttackDamage(o)));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => o.StatMod("Strength")));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("Hitpoints")));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatAV(o)));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatDV(o)));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatMA(o)));
				sheet.writeColumn(blueprint.GetStat("ElectricResistance", new Statistic()).Value);
				sheet.writeColumn(blueprint.GetStat("HeatResistance", new Statistic()).Value);
				sheet.writeColumn(blueprint.GetStat("ColdResistance", new Statistic()).Value);
				sheet.writeColumn(blueprint.GetStat("AcidResistance", new Statistic()).Value);
				sheet2.writeColumn(blueprint.GetStat("ElectricResistance", new Statistic()).Value);
				sheet2.writeColumn(blueprint.GetStat("HeatResistance", new Statistic()).Value);
				sheet2.writeColumn(blueprint.GetStat("ColdResistance", new Statistic()).Value);
				sheet2.writeColumn(blueprint.GetStat("AcidResistance", new Statistic()).Value);
				sheet.writeStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("Speed")));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("MoveSpeed")));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("Speed")));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("MoveSpeed")));
				int num2 = 0;
				if (blueprint.GetStat("XPValue", new Statistic()).sValue == "*XPValue")
				{
					num2 = Convert.ToInt32(blueprint.GetProp("*XPValue", "0"));
				}
				else
				{
					string valueOrSValue = blueprint.GetStat("XPValue", new Statistic()).ValueOrSValue;
					if (!(blueprint.GetStat("XPValue", new Statistic()).sValue == "*XP"))
					{
						num2 = ((!(valueOrSValue != "")) ? (-1) : Convert.ToInt32(valueOrSValue));
					}
					else
					{
						float num3 = Convert.ToInt32(blueprint.GetStat("Level", new Statistic()).Value);
						num3 /= 2f;
						num2 = text switch
						{
							"Minion" => (int)(num3 * 20f), 
							"Leader" => (int)(num3 * 100f), 
							"Hero" => (int)(num3 * 200f), 
							_ => (int)(num3 * 50f), 
						};
					}
				}
				sheet.writeColumn(num2);
				sheet2.writeColumn(num2);
				sheet.endRow();
				sheet2.endRow();
			}
			catch (Exception ex)
			{
				Debug.LogError("error blueprint=" + blueprint.Name);
				throw ex;
			}
		}
		sheet.finish();
		sheet2.finish();
	}

	private static void AddUnique(Dictionary<string, TagRecord> Records, IDictionary StringMap, string Type)
	{
		foreach (DictionaryEntry item in StringMap)
		{
			string tag = item.Key.ToString();
			string key = Type + item.Key;
			if (!Records.TryGetValue(key, out var value))
			{
				TagRecord tagRecord = new TagRecord
				{
					Tag = tag,
					Values = new Dictionary<string, int>(),
					Type = Type
				};
				value = tagRecord;
				Records[key] = tagRecord;
			}
			value.Score++;
			if (item.Value != null && (item.Value is string || item.Value.GetType().IsPrimitive))
			{
				string text = item.Value.ToString();
				if (!text.IsNullOrEmpty())
				{
					value.Values.TryGetValue(text, out var value2);
					value.Values[text] = value2 + 1;
				}
			}
		}
	}

	[WishCommand("blueprintsheeter", null)]
	public static void BlueprintSheetWish()
	{
		GenerateBlueprintSheet(typeof(RowBlueprint));
	}

	[WishCommand("blueprintsheeter", null)]
	public static void BlueprintSheetWish(string Parameter)
	{
		if (Parameter.EqualsNoCase("all"))
		{
			GenerateAllBlueprintSheet();
		}
		else
		{
			GenerateBlueprintSheet(ModManager.ResolveType(Parameter, IgnoreCase: false, ThrowOnError: true));
		}
	}

	public static void GenerateAllBlueprintSheet()
	{
		string[] allTags = GetAllTags();
		int num = allTags.Length;
		List<GameObjectBlueprint> list = new List<GameObjectBlueprint>(GameObjectFactory.Factory.BlueprintList);
		list.Sort((GameObjectBlueprint a, GameObjectBlueprint b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
		string text = XRLCore.SavePath + "/Trash/Text/";
		Directory.CreateDirectory(text);
		using StreamWriter streamWriter = new StreamWriter(text + "AllTagBlueprintSheet.csv");
		streamWriter.Write("\"Blueprint\",\"DisplayName\"");
		for (int num2 = 0; num2 < num; num2++)
		{
			streamWriter.Write(',');
			streamWriter.Write('"');
			streamWriter.Write(allTags[num2].Replace("\"", "\"\""));
			streamWriter.Write('"');
		}
		foreach (GameObjectBlueprint item in list)
		{
			streamWriter.Write("\n");
			streamWriter.Write('"');
			streamWriter.Write(item.Name);
			streamWriter.Write('"');
			streamWriter.Write(',');
			streamWriter.Write('"');
			streamWriter.Write(item.CachedDisplayNameStripped);
			streamWriter.Write('"');
			for (int num3 = 0; num3 < num; num3++)
			{
				streamWriter.Write(",");
				streamWriter.Write('"');
				if (item.Tags.ContainsKey(allTags[num3]))
				{
					streamWriter.Write("true");
				}
				streamWriter.Write('"');
			}
		}
	}

	public static string[] GetAllTags()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			foreach (KeyValuePair<string, string> tag in blueprint.Tags)
			{
				hashSet.Add(tag.Key);
			}
		}
		string[] array = hashSet.ToArray();
		Array.Sort(array, StringComparer.Ordinal);
		return array;
	}

	public static void GenerateBlueprintSheet(Type Type)
	{
		BlueprintColumn[] sheetColumns = GetSheetColumns(Type);
		List<string[]> list = new List<string[]>();
		GameObjectFactory factory = GameObjectFactory.Factory;
		int num = sheetColumns.Length;
		foreach (GameObjectBlueprint blueprint in factory.BlueprintList)
		{
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				string text = "";
				BlueprintElement[] attributes = sheetColumns[i].Attributes;
				for (int j = 0; j < attributes.Length; j++)
				{
					string text2 = attributes[j].GetFrom(blueprint);
					if (!text2.IsNullOrEmpty() && (text.IsNullOrEmpty() || text2 != "true"))
					{
						text = text2;
					}
				}
				array[i] = text;
			}
			list.Add(array);
		}
		list.Sort((string[] a, string[] b) => string.Compare(a[0], b[0], StringComparison.Ordinal));
		string text3 = XRLCore.SavePath + "/Trash/Text/";
		Directory.CreateDirectory(text3);
		using StreamWriter streamWriter = new StreamWriter(text3 + "BlueprintSheet.csv");
		for (int num2 = 0; num2 < num; num2++)
		{
			if (num2 != 0)
			{
				streamWriter.Write(",");
			}
			streamWriter.Write('"');
			streamWriter.Write(sheetColumns[num2].Header.Replace("\"", "\"\""));
			streamWriter.Write('"');
		}
		foreach (string[] item in list)
		{
			streamWriter.Write("\n");
			for (int num3 = 0; num3 < num; num3++)
			{
				if (num3 != 0)
				{
					streamWriter.Write(",");
				}
				streamWriter.Write('"');
				streamWriter.Write(item[num3].Replace("\"", "\"\""));
				streamWriter.Write('"');
			}
		}
	}

	public static BlueprintColumn[] GetSheetColumns(Type Type)
	{
		FieldInfo[] fields = Type.GetFields();
		BlueprintColumn[] array = new BlueprintColumn[fields.Length];
		for (int i = 0; i < fields.Length; i++)
		{
			array[i] = new BlueprintColumn
			{
				Field = fields[i],
				Attributes = fields[i].GetCustomAttributes<BlueprintElement>().ToArray()
			};
			BlueprintElement[] attributes = array[i].Attributes;
			foreach (BlueprintElement blueprintElement in attributes)
			{
				if (blueprintElement.Key.IsNullOrEmpty())
				{
					blueprintElement.Key = fields[i].Name;
				}
			}
		}
		return array;
	}

	[WishCommand("toptags", null)]
	public static void TopTagWish()
	{
		GameObjectFactory factory = GameObjectFactory.Factory;
		Dictionary<string, TagRecord> dictionary = new Dictionary<string, TagRecord>();
		foreach (GameObjectBlueprint blueprint in factory.BlueprintList)
		{
			AddUnique(dictionary, blueprint.Tags, "Tag");
			AddUnique(dictionary, blueprint.Props, "Property");
			AddUnique(dictionary, blueprint.IntProps, "IntProperty");
			AddUnique(dictionary, blueprint.Parts, "Part");
			AddUnique(dictionary, blueprint.xTags, "XTag");
		}
		string text = XRLCore.SavePath + "/Trash/Text/";
		Directory.CreateDirectory(text);
		StringBuilder stringBuilder = new StringBuilder();
		List<TagRecord> list = new List<TagRecord>(dictionary.Values);
		List<(string, int)> list2 = new List<(string, int)>();
		list.Sort((TagRecord a, TagRecord b) => b.Score.CompareTo(a.Score));
		foreach (TagRecord item in list)
		{
			stringBuilder.Compound(item.Tag, '\n').Append(" [").Append(item.Score)
				.Append(']')
				.Append('[')
				.Append(item.Type)
				.Append(']');
			foreach (KeyValuePair<string, int> value in item.Values)
			{
				list2.Add((value.Key, value.Value));
			}
			list2.Sort(((string, int) a, (string, int) b) => b.Item2.CompareTo(a.Item2));
			foreach (var item2 in list2)
			{
				stringBuilder.Compound(" -- ", '\n').Append(item2.Item1).Append(" [")
					.Append(item2.Item2)
					.Append(']');
			}
			list2.Clear();
		}
		File.WriteAllText(text + "TopTags.txt", stringBuilder.ToString());
	}
}
