using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Annals;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace XRL.World.Capabilities;

[HasWishCommand]
public static class ItemNaming
{
	private const int CHOICE_ENTERED = 0;

	private const int CHOICE_RELIC_STYLE = 1;

	private const int CHOICE_OWN_CULTURE = 2;

	private const int CHOICE_KILL_CULTURE = 3;

	private const int CHOICE_INFLUENCER_CULTURE = 4;

	public static bool Suppress;

	[NonSerialized]
	private static string[] KillBlueprintSources = new string[8] { "LastKillAsMeleeWeaponBlueprint", "LastKillAsMeleeWeaponTurn", "LastKillAsLauncherBlueprint", "LastKillAsLauncherTurn", "LastKillAsProjectileBlueprint", "LastKillAsProjectileTurn", "LastKillAsThrownWeaponBlueprint", "LastKillAsThrownWeaponTurn" };

	public static bool CanBeNamed(GameObject Object, GameObject Owner)
	{
		if (Suppress)
		{
			return false;
		}
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		if (Object.HasProperName)
		{
			return false;
		}
		if (!Object.Physics.IsReal)
		{
			return false;
		}
		if (Object.Render == null)
		{
			return false;
		}
		if (Object.IsTemporary)
		{
			return false;
		}
		if (Object.HasPart<NaturalEquipment>())
		{
			return false;
		}
		if (Object.HasTag("AlwaysStack"))
		{
			return false;
		}
		if (Object.HasTag("Creature"))
		{
			return false;
		}
		if (TinkeringHelpers.ConsiderScrap(Object, Owner))
		{
			return false;
		}
		if (Object.HasTagOrProperty("QuestItem") || Object.GetInventoryCategory() == "Quest Items")
		{
			return false;
		}
		if (!CanBeNamedEvent.Check(Owner, Object))
		{
			return false;
		}
		return true;
	}

	public static int GetNamingChance(GameObject Object, GameObject Owner)
	{
		if (!CanBeNamed(Object, Owner))
		{
			return 0;
		}
		if (Object.Count > 1)
		{
			return 0;
		}
		bool flag = Object.Equipped != null || Object.Implantee != null;
		if (Object.HasPart<Armor>() && !flag)
		{
			return 0;
		}
		if (Object.HasPart<Shield>() && !flag)
		{
			return 0;
		}
		long num = The.CurrentTurn - 100;
		int num2 = Brain.WeaponScore(Object, Owner);
		int num3 = Brain.MissileWeaponScore(Object, Owner);
		if ((num2 >= 5 || num3 >= 5) && !flag)
		{
			if (Owner.IsPlayer())
			{
				if (Object.GetIntProperty("LastKillAsMeleeWeaponByPlayerTurn") < num && Object.GetIntProperty("LastKillAsThrownWeaponByPlayerTurn") < num && Object.GetIntProperty("LastKillAsLauncherByPlayerTurn") < num && Object.GetIntProperty("LastKillAsProjectileByPlayerTurn") < num)
				{
					return 0;
				}
			}
			else if (Object.GetIntProperty("LastKillAsMeleeWeaponTurn") < num && Object.GetIntProperty("LastKillAsThrownWeaponTurn") < num && Object.GetIntProperty("LastKillAsLauncherTurn") < num && Object.GetIntProperty("LastKillAsProjectileTurn") < num)
			{
				return 0;
			}
		}
		double num4 = Object.GetTier() - 1 + Object.GetModificationCount();
		num4 += (double)Math.Min(Brain.ArmorScore(Object, Owner).DiminishingReturns(0.5), 20);
		num4 += (double)Math.Min(Brain.ShieldScore(Object, Owner).DiminishingReturns(1.0), 20);
		int num5 = Object.GetIntProperty("KillsAsMeleeWeapon") - Object.GetIntProperty("AccidentalKillsAsMeleeWeapon") / 2 + Object.GetIntProperty("KillsAsThrownWeapon") - Object.GetIntProperty("AccidentalKillsAsThrownWeapon") / 2 + Object.GetIntProperty("KillsAsLauncher") - Object.GetIntProperty("AccidentalKillsAsLauncher") / 2 + Object.GetIntProperty("KillsAsProjectile") - Object.GetIntProperty("AccidentalKillsAsProjectile") / 2;
		if (num5 > 0)
		{
			num4 += (double)Math.Min(num5.DiminishingReturns(1.0), 50);
		}
		if ((num5 > 0) | flag)
		{
			num4 += (double)Math.Min(num2.DiminishingReturns(1.0), 10);
			num4 += (double)Math.Min(num3.DiminishingReturns(1.0), 10);
		}
		int num6 = Object.GetIntProperty("InventoryActions") + Object.GetIntProperty("ActivatedAbilityCommandsProcessed") + Object.GetIntProperty("SifrahActions");
		if (num6 != 0)
		{
			num4 += (double)Math.Min(num6.DiminishingReturns(1.0) / 4, 20);
		}
		int intProperty = Object.GetIntProperty("ItemNamingBonus");
		if (intProperty != 0)
		{
			num4 += (double)Math.Min(intProperty.DiminishingReturns(1.0), 10);
		}
		Description Part;
		if (Object.HasPart<MakersMark>())
		{
			num4++;
		}
		else if (Object.TryGetPart<Description>(out Part) && !Part.Mark.IsNullOrEmpty())
		{
			num4++;
		}
		if (num4 <= 0.0)
		{
			if (flag)
			{
				num4 = 1.0;
			}
			else if (Object.GetIntProperty("LastInventoryActionTurn") > num)
			{
				num4 = 1.0;
			}
		}
		num4 = GetNamingChanceEvent.GetFor(Owner, Object, num4);
		return Math.Max((int)Math.Round(num4, MidpointRounding.AwayFromZero), 0);
	}

	public static Dictionary<GameObject, int> GetNamingChances(GameObject Owner)
	{
		List<GameObject> wholeInventoryReadonly = Owner.GetWholeInventoryReadonly();
		Dictionary<GameObject, int> dictionary = null;
		int i = 0;
		for (int count = wholeInventoryReadonly.Count; i < count; i++)
		{
			int namingChance = GetNamingChance(wholeInventoryReadonly[i], Owner);
			if (namingChance > 0)
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<GameObject, int>((wholeInventoryReadonly.Count >= 32) ? 16 : 8);
				}
				dictionary.Add(wholeInventoryReadonly[i], namingChance);
			}
		}
		return dictionary;
	}

	public static bool Opportunity(GameObject Owner, GameObject Kill = null, GameObject InfluencedBy = null, string ZoneID = null, string OpportunityType = "General", int SuppressedByAnyTypeForLevels = 0, int SuppressedBySameTypeForLevels = 0, int SuppressedBySameTypeOnlyIfAtLeast = 0, int ChanceToBypassSuppression = 0, bool Force = false)
	{
		if (Popup.Suppress)
		{
			return false;
		}
		bool flag = false;
		if (SuppressedByAnyTypeForLevels > 0)
		{
			int intProperty = Owner.GetIntProperty("LastItemNamingDoneAtLevel");
			if (intProperty > 0 && Owner.Stat("Level") < intProperty + SuppressedByAnyTypeForLevels)
			{
				flag = true;
			}
		}
		if (!flag && SuppressedBySameTypeForLevels > 0)
		{
			int intProperty2 = Owner.GetIntProperty("Last" + OpportunityType + "ItemNamingDoneAtLevel");
			if (intProperty2 > 0 && Owner.Stat("Level") < intProperty2 + SuppressedBySameTypeForLevels && (SuppressedBySameTypeOnlyIfAtLeast == 0 || Owner.GetIntProperty(OpportunityType + "ItemNamingDoneAtLevel") < SuppressedBySameTypeOnlyIfAtLeast))
			{
				flag = true;
			}
		}
		if (flag && !ChanceToBypassSuppression.in100())
		{
			return false;
		}
		if (!GameObject.Validate(ref Owner))
		{
			return false;
		}
		Dictionary<GameObject, int> namingChances = GetNamingChances(Owner);
		if (namingChances == null || namingChances.Count <= 0)
		{
			return false;
		}
		List<GameObject> list = Event.NewGameObjectList();
		int num = 0;
		while (true)
		{
			foreach (KeyValuePair<GameObject, int> item in namingChances)
			{
				if (Owner.IsPlayer() ? item.Value.in1000(Stat.NamingRnd) : item.Value.in1000())
				{
					list.Add(item.Key);
				}
			}
			if (list.Count > 0)
			{
				break;
			}
			if (!Force || ++num >= 1000)
			{
				return false;
			}
		}
		bool? flag2;
		do
		{
			GameObject gameObject = null;
			if (list.Count > 1)
			{
				if (Owner.IsPlayer())
				{
					List<string> list2 = new List<string>(list.Count);
					List<object> list3 = new List<object>(list.Count);
					List<char> list4 = new List<char>(list.Count);
					list2.Add("nothing");
					list3.Add(null);
					list4.Add('-');
					char c = 'a';
					int i = 0;
					for (int count = list.Count; i < count; i++)
					{
						list2.Add(list[i].DisplayName);
						list3.Add(list[i]);
						list4.Add(c);
						c = (char)(c + 1);
					}
					int num2 = Popup.PickOption("You swell with the inspiration to name an item.", "What would you like to name?", "", "Sounds/UI/ui_notification_question", list2.ToArray(), list4.ToArray());
					if (num2 >= 0)
					{
						gameObject = list3[num2] as GameObject;
					}
				}
				else
				{
					Dictionary<GameObject, int> dictionary = new Dictionary<GameObject, int>(list.Count);
					int j = 0;
					for (int count2 = list.Count; j < count2; j++)
					{
						dictionary.Add(list[j], namingChances[list[j]]);
					}
					gameObject = dictionary.GetRandomElement();
				}
			}
			else if (Owner.IsPlayer())
			{
				if (Popup.ShowYesNo("You swell with the inspiration to name your " + list[0].DisplayNameOnly + ". Do you wish to?", "Sounds/UI/ui_notification_question", AllowEscape: false) == DialogResult.Yes)
				{
					gameObject = list[0];
				}
			}
			else
			{
				gameObject = list[0];
			}
			flag2 = NameItem(gameObject, Owner, Kill, InfluencedBy, ZoneID, OpportunityType);
		}
		while (!flag2.HasValue);
		return flag2 == true;
	}

	private static string FindKillBlueprint(GameObject Object)
	{
		string result = null;
		long num = 0L;
		int i = 0;
		for (int num2 = KillBlueprintSources.Length; i < num2; i += 2)
		{
			string name = KillBlueprintSources[i];
			string name2 = KillBlueprintSources[i + 1];
			string stringProperty = Object.GetStringProperty(name);
			if (!stringProperty.IsNullOrEmpty())
			{
				long longProperty = Object.GetLongProperty(name2, 0L);
				if (longProperty > num)
				{
					result = stringProperty;
					num = longProperty;
				}
			}
		}
		return result;
	}

	private static string FindZoneName(GameObject Owner, string ZoneID = null)
	{
		if (ZoneID == null)
		{
			ZoneID = Owner?.GetCurrentZone()?.ZoneID;
		}
		if (ZoneID == null)
		{
			return null;
		}
		if (The.ZoneManager.GetZoneHasProperName(ZoneID))
		{
			string text = The.ZoneManager.GetZoneBaseDisplayName(ZoneID);
			if (!text.IsNullOrEmpty())
			{
				string zoneDefiniteArticle = The.ZoneManager.GetZoneDefiniteArticle(ZoneID);
				if (!zoneDefiniteArticle.IsNullOrEmpty())
				{
					text = zoneDefiniteArticle + " " + text;
				}
				return text;
			}
		}
		return The.ZoneManager.GetZoneNameContext(ZoneID) ?? ("the " + The.ZoneManager.GetZoneDisplayName(ZoneID, WithIndefiniteArticle: false, WithDefiniteArticle: false, WithStratum: false));
	}

	private static string GenerateRelicStyleName(GameObject Object, GameObject Owner, GameObject Kill, GameObject InfluencedBy, string ZoneID, ref string Element, ref string Type)
	{
		if (Element == null)
		{
			Element = RelicGenerator.SelectElement(Object, Owner, Kill, InfluencedBy);
		}
		if (Type == null)
		{
			Type = RelicGenerator.GetType(Object);
		}
		string text = FindZoneName(Owner, ZoneID);
		string phrase;
		if (!text.IsNullOrEmpty() && 50.in100())
		{
			phrase = "the " + HistoricStringExpander.ExpandString("<spice.elements." + Element + ".adjectives.!random> " + HistoricStringExpander.ExpandString("<spice.itemTypes." + Type + ".!random>") + " of " + text);
			return Grammar.MakeTitleCase(phrase);
		}
		string text2;
		if (Kill != null && Kill.HasProperName && 30.in100())
		{
			text2 = Kill.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: false);
		}
		else
		{
			text2 = ((InfluencedBy != null && InfluencedBy.HasProperName && 50.in100()) ? InfluencedBy.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: false) : Owner.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: false));
		}
		Dictionary<string, string> vars = new Dictionary<string, string>
		{
			{ "*element*", Element },
			{ "*itemType*", Type },
			{
				"*personNounPossessive*",
				Grammar.MakePossessive(HistoricStringExpander.ExpandString("<spice.personNouns.!random>"))
			},
			{
				"*creatureNamePossessive*",
				Grammar.MakePossessive(text2)
			}
		};
		phrase = HistoricStringExpander.ExpandString("<spice.history.relics.names.!random>", null, null, vars);
		if (phrase.Contains(text2))
		{
			phrase = phrase.Replace(text2, "------------");
			phrase = Grammar.MakeTitleCase(phrase);
			return phrase.Replace("------------", text2);
		}
		return Grammar.MakeTitleCase(phrase);
	}

	public static bool? NameItem(GameObject Object, GameObject Owner, GameObject Kill = null, GameObject InfluencedBy = null, string ZoneID = null, string OpportunityType = "General", bool CanBestow = true)
	{
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		string Type = null;
		string Element = null;
		bool bestowalsChecked = false;
		int DidBasicBestowals = 0;
		bool DidElementBestowal = false;
		bool BestowalsPossible = false;
		if (CanBestow)
		{
			CheckBestowals(Owner, Object, Type ?? (Type = RelicGenerator.GetType(Object)), Element ?? (Element = RelicGenerator.SelectElement(Object, Owner, Kill, InfluencedBy)), Kill, InfluencedBy, OpportunityType, out BestowalsPossible, out DidBasicBestowals, out DidElementBestowal);
			bestowalsChecked = true;
		}
		if (Kill == null)
		{
			string text = FindKillBlueprint(Object);
			Kill = ((text != null) ? GameObject.CreateSample(text) : null);
			if (Object.DisplayNameOnly.Contains("["))
			{
				Kill = null;
			}
		}
		string text2 = null;
		string text3 = null;
		bool useAnsify = false;
		if (Owner.IsPlayer())
		{
			List<string> list = new List<string>();
			List<char> list2 = new List<char>();
			List<int> list3 = new List<int>();
			char c = 'a';
			list3.Add(0);
			list2.Add(c++);
			list.Add("Enter a name.");
			list3.Add(1);
			list2.Add(c++);
			list.Add("Name " + Object.them + " based on " + Object.its + " qualities.");
			list3.Add(2);
			list2.Add(c++);
			list.Add("Choose a random name from your own culture.");
			if (Kill != null)
			{
				list3.Add(3);
				list2.Add(c++);
				list.Add("Choose a random name from " + Kill.poss("culture") + ".");
			}
			if (GameObject.Validate(ref InfluencedBy) && InfluencedBy.HasTag("Creature"))
			{
				list3.Add(4);
				list2.Add(c++);
				list.Add("Choose a random name from " + InfluencedBy.poss("culture") + ".");
			}
			while (true)
			{
				int num = Popup.PickOption("", "Rename " + Object.t() + ".", "", "Sounds/UI/ui_notification", list.ToArray(), list2.ToArray(), null, null, null, null, null, 1, 60, 0, -1, !BestowalsPossible);
				if (num < 0)
				{
					return null;
				}
				switch (list3[num])
				{
				case 0:
					text2 = Popup.AskString("Enter a new name for " + Object.t() + ".", "", "Sounds/UI/ui_notification", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -+/#()!@$%*<>'", null, 30);
					if (text2.IsNullOrEmpty())
					{
						continue;
					}
					text2 = text2.Trim();
					if (text2.IsNullOrEmpty())
					{
						continue;
					}
					break;
				case 1:
					text2 = GenerateRelicStyleName(Object, Owner, Kill, InfluencedBy, ZoneID, ref Element, ref Type);
					break;
				case 2:
					text2 = NameMaker.MakeName(Owner, null, null, null, null, null, null, null, null, null, "Item", null, null, FailureOkay: false, SpecialFaildown: true);
					break;
				case 3:
					text2 = NameMaker.MakeName(Kill, null, null, null, null, null, null, null, null, null, "Item", null, null, FailureOkay: false, SpecialFaildown: true);
					break;
				case 4:
					text2 = NameMaker.MakeName(InfluencedBy, null, null, null, null, null, null, null, null, null, "Item", null, null, FailureOkay: false, SpecialFaildown: true);
					break;
				}
				break;
			}
			text3 = Popup.ShowColorPicker("", 0, "You select the name '" + text2 + "' for " + Object.t() + ". Choose a color for " + Object.them + ".", 60, RespectOptionNewlines: false, !BestowalsPossible, null, "", includeNone: true, includePatterns: true, allowBackground: false, text2);
			if (text3 == null)
			{
				return null;
			}
		}
		else
		{
			List<int> list4 = new List<int>(8);
			list4.Add(0);
			list4.Add(1);
			list4.Add(1);
			list4.Add(1);
			list4.Add(2);
			if (Kill != null)
			{
				list4.Add(3);
			}
			if (InfluencedBy != null)
			{
				list4.Add(4);
			}
			switch (list4.GetRandomElement())
			{
			case 0:
			case 2:
				text2 = NameMaker.MakeName(Owner, null, null, null, null, null, null, null, null, null, "Item", null, null, FailureOkay: false, SpecialFaildown: true);
				break;
			case 1:
				text2 = GenerateRelicStyleName(Object, Owner, Kill, InfluencedBy, ZoneID, ref Element, ref Type);
				break;
			case 3:
				text2 = NameMaker.MakeName(Kill, null, null, null, null, null, null, null, null, null, "Item", null, null, FailureOkay: false, SpecialFaildown: true);
				break;
			case 4:
				text2 = NameMaker.MakeName(InfluencedBy, null, null, null, null, null, null, null, null, null, "Item", null, null, FailureOkay: false, SpecialFaildown: true);
				break;
			}
			useAnsify = true;
		}
		return NameItem(Object, Owner, text2, text3, Element, Type, useAnsify, CanBestow, Kill, InfluencedBy, OpportunityType, bestowalsChecked, DidBasicBestowals, DidElementBestowal);
	}

	public static bool NameItem(GameObject Object, GameObject Owner, string Name, string Color = null, string Element = null, string Type = null, bool UseAnsify = false, bool CanBestow = false, GameObject Kill = null, GameObject InfluencedBy = null, string OpportunityType = "General", bool BestowalsChecked = false, int DidBasicBestowals = 0, bool DidElementBestowal = false)
	{
		if (!CanBeNamed(Object, Owner))
		{
			return false;
		}
		QudHistoryHelpers.ExtractArticle(ref Name, out var Article);
		Name = ((!UseAnsify) ? Name.Color(Color) : QudHistoryHelpers.Ansify(Name));
		string displayName = Object.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true);
		string text = Object.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true);
		if (Owner.IsPlayer())
		{
			Popup.Show("You name " + Object.t() + " '" + Name + "'.");
		}
		Object.RequirePart<OriginalItemType>();
		Object.SplitStack(1, Owner);
		Object.GiveProperName(Name, Force: true);
		Object.Render.SetForegroundColor(ColorUtility.GetMainForegroundColor(Name));
		Object.SetImportant(flag: true, force: true);
		if (!Article.IsNullOrEmpty())
		{
			Object.SetStringProperty("IndefiniteArticle", Article);
			Object.SetStringProperty("DefiniteArticle", Article);
		}
		Object.SetIntProperty("Renamed", 1);
		int num = Owner.Stat("Level");
		int intProperty = Owner.GetIntProperty("LastItemNamingDoneAtLevel");
		int intProperty2 = Owner.GetIntProperty("Last" + OpportunityType + "ItemNamingDoneAtLevel");
		Owner.ModIntProperty("ItemNamingDone", 1);
		if (Owner.IsPlayer())
		{
			The.Game.ModIntGameState("PlayerItemNamingDone", 1);
		}
		Owner.SetIntProperty("LastItemNamingDoneAtLevel", num);
		Owner.SetIntProperty("Last" + OpportunityType + "ItemNamingDoneAtLevel", num);
		if (intProperty == num)
		{
			Owner.ModIntProperty("ItemNamingDoneAtLevel", 1);
		}
		else
		{
			Owner.SetIntProperty("ItemNamingDoneAtLevel", 1);
		}
		if (intProperty2 == num || intProperty2 < intProperty)
		{
			Owner.ModIntProperty(OpportunityType + "ItemNamingDoneAtLevel", 1);
		}
		else
		{
			Owner.SetIntProperty(OpportunityType + "ItemNamingDoneAtLevel", 1);
		}
		if (CanBestow && !BestowalsChecked)
		{
			CheckBestowals(Owner, Object, Type, Element, Kill, InfluencedBy, OpportunityType, out var _, out DidBasicBestowals, out DidElementBestowal);
		}
		if (Owner.IsPlayer())
		{
			string text2 = HistoricStringExpander.ExpandString("<spice.professions.!random>");
			JournalAPI.AddAccomplishment("You named " + text + " '" + Name + "'.", "Blessed by divine beings, =name= discovered the legendary " + displayName + " known as '" + Name + "'.", muralWeight: (DidElementBestowal || DidBasicBestowals > 0) ? MuralWeight.High : MuralWeight.Medium, gospelText: "At a remote <spice.professions." + text2 + ".guildhall> near " + JournalAPI.GetLandmarkNearestPlayer().Text + ", =name= met with a group of <spice.professions." + text2 + ".plural> and commissed " + text + " named " + Name + ".", aggregateWith: null, category: "general", muralCategory: MuralCategory.DoesSomethingRad, secretId: null, time: -1L);
		}
		return true;
	}

	private static void CheckBestowals(GameObject Owner, GameObject Object, string Type, string Element, GameObject Kill, GameObject InfluencedBy, string OpportunityType, out bool BestowalsPossible, out int DidBasicBestowals, out bool DidElementBestowal)
	{
		BestowalsPossible = false;
		DidBasicBestowals = 0;
		DidElementBestowal = false;
		int num = 0;
		bool flag = false;
		if (Options.SifrahItemNaming && Owner.IsPlayer())
		{
			BestowalsPossible = true;
			int rating = Owner.StatMod("Ego") + Owner.StatMod("Willpower");
			int difficulty = Tier.Constrain(Object.GetTier());
			ItemNamingSifrah itemNamingSifrah = new ItemNamingSifrah(Object, rating, difficulty);
			itemNamingSifrah.Play(Object);
			num = itemNamingSifrah.BasicBestowals;
			flag = itemNamingSifrah.ElementBestowal;
		}
		else
		{
			int num2 = GetNamingBestowalChanceEvent.GetFor(Owner, Object, GlobalConfig.GetIntSetting("ItemNamingBestowalBaseChance"));
			if (num2 > 0)
			{
				BestowalsPossible = true;
			}
			if (Owner.IsPlayer() ? num2.in100(Stat.NamingRnd) : num2.in100())
			{
				num++;
				if (Owner.IsPlayer() ? num2.in100(Stat.NamingRnd) : num2.in100())
				{
					flag = true;
				}
			}
		}
		int num3 = -1;
		string text = null;
		if (!((num > 0) | flag))
		{
			return;
		}
		if (Type == null)
		{
			Type = RelicGenerator.GetType(Object);
		}
		if (text == null)
		{
			text = RelicGenerator.GetSubtype(Type);
		}
		if (num3 == -1)
		{
			int num4 = Owner.Stat("Level");
			num3 = Tier.Constrain(Stat.Random((num4 < 20) ? 1 : 2, 1 + num4 / 5));
		}
		if (Type == null || text == null)
		{
			return;
		}
		BodyPart bodyPart = null;
		GameObject Object2 = Object.Equipped;
		if (Object.Implantee != null)
		{
			Object2 = null;
		}
		if (Object2 != null)
		{
			bodyPart = Object2.FindEquippedObject(Object);
		}
		bool flag2 = false;
		if (Object2 != null)
		{
			Event obj = Event.New("CommandUnequipObject");
			obj.SetParameter("BodyPart", bodyPart);
			obj.SetFlag("NoStack", State: true);
			flag2 = Object2.FireEvent(obj);
		}
		try
		{
			for (int i = 0; i < num; i++)
			{
				if (RelicGenerator.ApplyBasicBestowal(Object, Type, num3, text, Standard: false, ShowInShortDescription: true))
				{
					DidBasicBestowals++;
				}
			}
			if (DidBasicBestowals == 0)
			{
				flag = true;
			}
			if (flag)
			{
				if (Element == null)
				{
					Element = RelicGenerator.SelectElement(Object, Owner, Kill, InfluencedBy);
				}
				if (!Element.IsNullOrEmpty())
				{
					DidElementBestowal = RelicGenerator.ApplyElementBestowal(Object, Element, Type, num3, text);
				}
			}
			if ((DidBasicBestowals > 0) | DidElementBestowal)
			{
				if (DidElementBestowal)
				{
					Object.SetStringProperty("Mods", "None");
				}
				if (Owner.IsPlayer())
				{
					Popup.Show(Object.Does("seem") + " to have taken on new qualities.");
					InventoryActionEvent.Check(Object, Owner, Object, "Look");
				}
				AfterPseudoRelicGeneratedEvent.Send(Object, Element, Type, text, num3);
			}
		}
		finally
		{
			if (GameObject.Validate(ref Object) && ((bodyPart != null && bodyPart.Equipped == null) & flag2) && GameObject.Validate(ref Object2))
			{
				Object2.FireEvent(Event.New("CommandEquipObject", "Object", Object, "BodyPart", bodyPart));
			}
			if (Object.Equipped == null && Object.InInventory == null && Object.Implantee == null && Object.CurrentCell == null)
			{
				Owner.ReceiveObject(Object);
			}
		}
	}

	public static GameObject GetBaseVersion(GameObject Object)
	{
		if (Object.GetIntProperty("Renamed") > 0 && !Object.IsCombatObject())
		{
			return GameObject.Create(Object.Blueprint);
		}
		return null;
	}

	[WishCommand(null, null, Regex = "^itemnaming(?::([^:]*?)\\s*(?::\\s*([^:]*?))?\\s*)?$")]
	public static bool HandleItemNamingWish(Match match)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		string value = match.Groups[1].Value;
		string value2 = match.Groups[2].Value;
		if (!value.IsNullOrEmpty())
		{
			gameObject = GameObject.Create(value);
			stringBuilder.Append("[Debug: Created " + gameObject.DebugName + " as kill.]\n");
		}
		if (!value2.IsNullOrEmpty())
		{
			gameObject2 = GameObject.Create(value2);
			stringBuilder.Append("[Debug: Created " + gameObject2.DebugName + " as InfluencedBy.]\n");
		}
		if (stringBuilder.Length > 0)
		{
			Popup.Show(stringBuilder.ToString());
		}
		if (!Opportunity(The.Player, gameObject, gameObject2, null, "Wish", 0, 0, 0, 0, Force: true))
		{
			Popup.Show("[Debug: Naming failed.]");
		}
		return true;
	}
}
