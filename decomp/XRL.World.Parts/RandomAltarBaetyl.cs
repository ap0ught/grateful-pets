using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Annals;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class RandomAltarBaetyl : IPart
{
	public GameObject DemandObject;

	public string DemandBlueprint;

	public bool DemandIsMod;

	public int DemandCount;

	public string RewardType;

	public int RewardTier;

	public string RewardID;

	public int RewardAttributePointsAmount;

	public int RewardMutationPointsAmount;

	public int RewardSkillPointsAmount;

	public int RewardExperiencePointsAmount;

	public int RewardLicensePointsAmount;

	public int RewardReputationAmount;

	public string RewardReputation;

	public string RewardReputationFaction;

	public string RewardItem;

	public string RewardItemMod;

	public int RewardItemBonusModChance;

	public int RewardItemSetModNumber;

	public int RewardItemMinorBestowalChance;

	public int RewardItemElementBestowalChance;

	public bool Fulfilled;

	[NonSerialized]
	private Cell FromCell;

	private int lastSparkFrame;

	public string MapNoteID => "Baetyl." + ParentObject.ID;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID && ID != BeforeDeathRemovalEvent.ID && ID != BeginConversationEvent.ID && ID != CanSmartUseEvent.ID && ID != EnteredCellEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != PooledEvent<GetPointsOfInterestEvent>.ID)
		{
			return ID == LeftCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!Fulfilled && !E.AutogetOnlyMode && E.Command != "Chat" && JournalAPI.GetMapNote(MapNoteID) == null && !ParentObject.IsHostileTowards(E.Actor))
		{
			E.Command = "Chat";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("chance", 1);
			E.Add("circuitry", 1);
			E.Add("scholarship", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "DemandObject", DemandObject?.DebugName);
		E.AddEntry(this, "DemandBlueprint", DemandBlueprint);
		E.AddEntry(this, "DemandIsMod", DemandIsMod);
		E.AddEntry(this, "DemandCount", DemandCount);
		E.AddEntry(this, "RewardType", RewardType);
		E.AddEntry(this, "RewardTier", RewardTier);
		E.AddEntry(this, "RewardID", RewardID);
		E.AddEntry(this, "Reward", The.ZoneManager.peekCachedObject(RewardID)?.DebugName);
		E.AddEntry(this, "RewardAttributePointsAmount", RewardAttributePointsAmount);
		E.AddEntry(this, "RewardMutationPointsAmount", RewardMutationPointsAmount);
		E.AddEntry(this, "RewardSkillPointsAmount", RewardSkillPointsAmount);
		E.AddEntry(this, "RewardExperiencePointsAmount", RewardExperiencePointsAmount);
		E.AddEntry(this, "RewardLicensePointsAmount", RewardLicensePointsAmount);
		E.AddEntry(this, "RewardReputationAmount", RewardReputationAmount);
		E.AddEntry(this, "RewardReputation", RewardReputation);
		E.AddEntry(this, "RewardReputationFaction", RewardReputationFaction);
		E.AddEntry(this, "RewardItem", RewardItem);
		E.AddEntry(this, "RewardItemMod", RewardItemMod);
		E.AddEntry(this, "RewardItemBonusModChance", RewardItemBonusModChance);
		E.AddEntry(this, "RewardItemSetModNumber", RewardItemSetModNumber);
		E.AddEntry(this, "RewardItemMinorBestowalChance", RewardItemMinorBestowalChance);
		E.AddEntry(this, "RewardItemElementBestowalChance", RewardItemElementBestowalChance);
		E.AddEntry(this, "Fulfilled", Fulfilled);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (!Fulfilled && E.StandardChecks(this, E.Actor) && E.Actor.IsPlayer())
		{
			E.Add(ParentObject, ParentObject.GetReferenceDisplayName());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginConversationEvent E)
	{
		if (E.SpeakingWith == ParentObject && E.Conversation.ID == ParentObject.GetBlueprint().GetPartParameter<string>("ConversationScript", "ConversationID") && CanTalk(E.Actor))
		{
			BaetylWantsSacrifice();
			E.RequestInterfaceExit();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (Visible())
		{
			RemoveJournalNote();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && CanTalk(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		FromCell = E.Cell;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		GenerateDemand();
		GenerateReward();
		if (FromCell != null && FromCell.IsVisible())
		{
			JournalMapNote mapNote = JournalAPI.GetMapNote(MapNoteID);
			if (mapNote != null && (E.Cell.ParentZone == null || E.Cell.ParentZone.ZoneID != mapNote.ZoneID))
			{
				RemoveJournalNote();
			}
		}
		FromCell = null;
		return base.HandleEvent(E);
	}

	public static string GetModDemandName(string part, int num)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(Grammar.Cardinal(num)).Append(' ');
		ModEntry modEntry = ModificationFactory.ModsByPart[part];
		if (modEntry.TinkerDisplayName.Contains("with ") || modEntry.TinkerDisplayName.Contains("of "))
		{
			stringBuilder.Append((num == 1) ? "item" : "items").Append(' ').Append(modEntry.TinkerDisplayName);
		}
		else
		{
			stringBuilder.Append(modEntry.TinkerDisplayName).Append(' ').Append((num == 1) ? "item" : "items");
		}
		return stringBuilder.ToString();
	}

	public string GetDemandName(bool Caps = true)
	{
		string text = (DemandIsMod ? GetModDemandName(DemandBlueprint, DemandCount) : DemandObject.GetDemandName(DemandCount));
		if (Caps)
		{
			text = ColorUtility.ToUpperExceptFormatting(text);
		}
		return text;
	}

	public void GenerateDemand(Zone Z = null)
	{
		if (DemandBlueprint == null)
		{
			if (30.in100())
			{
				int num = 0;
				ModEntry randomElement;
				while (true)
				{
					randomElement = ModificationFactory.ModList.GetRandomElement();
					if (!randomElement.NoSparkingQuest && randomElement.MinTier <= 8)
					{
						break;
					}
					if (++num > 10000)
					{
						throw new Exception("cannot find any baetyl demands, aborting to avoid infinite loop");
					}
				}
				DemandBlueprint = randomElement.Part;
				DemandCount = Stat.Random(3, 6);
				DemandIsMod = true;
				if (RewardTier == 0)
				{
					RewardTier = randomElement.TinkerTier;
					if (randomElement.TinkerAllowed)
					{
						RewardTier += randomElement.Rarity;
					}
					else
					{
						RewardTier += randomElement.Rarity * randomElement.Rarity;
					}
					if (randomElement.CanAutoTinker)
					{
						RewardTier--;
					}
					if (Z == null && ParentObject != null)
					{
						Z = ParentObject.CurrentZone;
					}
					if (Z != null && Z.NewTier > RewardTier)
					{
						RewardTier = Z.NewTier;
					}
					RewardTier += Stat.Random(1, 2);
					Tier.Constrain(ref RewardTier);
				}
			}
			else
			{
				int num2 = 0;
				GameObjectBlueprint randomElement2;
				while (true)
				{
					randomElement2 = GameObjectFactory.Factory.BlueprintList.GetRandomElement();
					if (!randomElement2.HasPart("Brain") && randomElement2.HasPart("Physics") && randomElement2.HasPart("Render") && !randomElement2.Tags.ContainsKey("NoSparkingQuest") && !randomElement2.Tags.ContainsKey("BaseObject") && !randomElement2.Tags.ContainsKey("ExcludeFromDynamicEncounters") && randomElement2.GetPartParameter("Physics", "Takeable", Default: true) && randomElement2.GetPartParameter("Physics", "IsReal", Default: true) && !randomElement2.GetPartParameter<string>("Render", "DisplayName").Contains("[") && (!randomElement2.Props.ContainsKey("SparkingQuestBlueprint") || randomElement2.Name == randomElement2.Props["SparkingQuestBlueprint"]))
					{
						break;
					}
					if (++num2 > 10000)
					{
						throw new Exception("cannot find any baetyl demands, aborting to avoid infinite loop");
					}
				}
				DemandBlueprint = randomElement2.Name;
				DemandCount = Stat.Random(3, 6);
				DemandIsMod = false;
			}
		}
		if (DemandObject != null || DemandIsMod)
		{
			return;
		}
		DemandObject = GameObject.CreateSample(DemandBlueprint);
		if (RewardTier == 0)
		{
			RewardTier = DemandObject.GetTier();
			if (Z == null && ParentObject != null)
			{
				Z = ParentObject.CurrentZone;
			}
			if (Z != null && Z.NewTier > RewardTier)
			{
				RewardTier = Z.NewTier;
			}
			RewardTier += Stat.Random(1, 2);
			Tier.Constrain(ref RewardTier);
		}
	}

	public void GenerateReward(int Value = 100)
	{
		bool flag = !RewardType.IsNullOrEmpty();
		if (flag && Value == 100)
		{
			return;
		}
		RandomAltarBaetylReward randomAltarBaetylReward = null;
		if (flag)
		{
			randomAltarBaetylReward = RandomAltarBaetylRewardManager.GetReward(RewardType);
		}
		else
		{
			randomAltarBaetylReward = RandomAltarBaetylRewardManager.GetRandomReward(The.Player);
			RewardType = randomAltarBaetylReward?.Name;
		}
		if (randomAltarBaetylReward == null)
		{
			randomAltarBaetylReward = RandomAltarBaetylRewardManager.GetRandomReward(The.Player);
			RewardType = randomAltarBaetylReward?.Name;
			if (randomAltarBaetylReward == null)
			{
				return;
			}
			flag = false;
		}
		if (!flag)
		{
			RewardItem = randomAltarBaetylReward.Item;
			RewardItemMod = randomAltarBaetylReward.ItemMod;
			RewardItemBonusModChance = randomAltarBaetylReward.ItemBonusModChance?.RollCached() ?? 0;
			RewardItemSetModNumber = randomAltarBaetylReward.ItemSetModNumber?.RollCached() ?? 0;
			RewardItemMinorBestowalChance = randomAltarBaetylReward.ItemMinorBestowalChance?.RollCached() ?? 0;
			RewardItemElementBestowalChance = randomAltarBaetylReward.ItemElementBestowalChance?.RollCached() ?? 0;
			RewardAttributePointsAmount = randomAltarBaetylReward.AttributePoints?.RollCached() ?? 0;
			RewardMutationPointsAmount = randomAltarBaetylReward.MutationPoints?.RollCached() ?? 0;
			RewardSkillPointsAmount = randomAltarBaetylReward.SkillPoints?.RollCached() ?? 0;
			RewardExperiencePointsAmount = randomAltarBaetylReward.ExperiencePoints?.RollCached() ?? 0;
			RewardLicensePointsAmount = randomAltarBaetylReward.LicensePoints?.RollCached() ?? 0;
			RewardReputation = randomAltarBaetylReward.Reputation;
			RewardReputationAmount = RewardReputation?.RollCached() ?? 0;
			RewardReputationFaction = randomAltarBaetylReward.ReputationFaction;
		}
		GenerateRewardItem(Value);
		GenerateRewardAmounts(Value);
		GenerateRewardReputationFaction(Value);
	}

	private void GenerateRewardItem(int Value = 100)
	{
		if (!RewardID.IsNullOrEmpty())
		{
			if (Value == 100 && !RewardItem.IsNullOrEmpty())
			{
				return;
			}
			The.ZoneManager.UncacheObject(RewardID);
			RewardID = null;
		}
		if (!RewardItem.IsNullOrEmpty())
		{
			GameObject gameObject = GenerateItem(RewardItem, Value);
			if (gameObject != null)
			{
				RewardID = The.ZoneManager.CacheObject(gameObject);
			}
		}
	}

	private void GenerateRewardAmounts(int Value = 100)
	{
		if (Value != 100)
		{
			if (RewardAttributePointsAmount > 0)
			{
				RewardAttributePointsAmount = AdjustByValue(RewardAttributePointsAmount, Value);
			}
			if (RewardMutationPointsAmount > 0)
			{
				RewardMutationPointsAmount = AdjustByValue(RewardMutationPointsAmount, Value);
			}
			if (RewardSkillPointsAmount > 0)
			{
				RewardSkillPointsAmount = AdjustByValue(RewardSkillPointsAmount, Value);
			}
			if (RewardExperiencePointsAmount > 0)
			{
				RewardExperiencePointsAmount = AdjustByValue(RewardExperiencePointsAmount, Value);
			}
			if (RewardLicensePointsAmount > 0)
			{
				RewardLicensePointsAmount = AdjustByValue(RewardLicensePointsAmount, Value);
			}
			if (RewardReputationAmount > 0)
			{
				RewardReputationAmount = VaryRewardReputation(RewardReputationAmount, Value);
			}
		}
	}

	private int VaryRewardReputation(int Amount, int Value = 100)
	{
		return GivesRep.VaryRep(Amount + Value - 100);
	}

	private void GenerateRewardReputationFaction(int Value = 100)
	{
		if (RewardReputationAmount <= 0 || !RewardReputationFaction.IsNullOrEmpty())
		{
			return;
		}
		List<string> list = new List<string>(64);
		foreach (Faction item in Factions.Loop())
		{
			if (item.Visible)
			{
				list.Add(item.Name);
			}
		}
		RewardReputationFaction = list.GetRandomElement();
	}

	private GameObject RetrieveRewardItem()
	{
		return The.ZoneManager.PullCachedObject(RewardID);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BaetylLeaving");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BaetylLeaving" && Visible())
		{
			RemoveJournalNote();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (!Fulfilled && lastSparkFrame != XRLCore.CurrentFrame)
		{
			lastSparkFrame = XRLCore.CurrentFrame;
			if (Stat.RandomCosmetic(1, 120) <= 2)
			{
				for (int i = 0; i < 2; i++)
				{
					ParentObject.ParticleText("&Y" + (char)Stat.RandomCosmetic(191, 198), 0.2f, 20);
				}
				for (int j = 0; j < 2; j++)
				{
					ParentObject.ParticleText("&W\u000f", 0.02f, 10);
				}
				PlayWorldSound("sfx_spark", 0.35f, 0.35f);
			}
		}
		return true;
	}

	public static bool DisplayNameMatches(string B1, string B2)
	{
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints[B1];
		GameObjectBlueprint gameObjectBlueprint2 = GameObjectFactory.Factory.Blueprints[B2];
		string partParameter = gameObjectBlueprint.GetPartParameter<string>("Render", "DisplayName");
		if (partParameter == null)
		{
			return false;
		}
		string partParameter2 = gameObjectBlueprint2.GetPartParameter<string>("Render", "DisplayName");
		if (partParameter2 == null)
		{
			return false;
		}
		if (partParameter == partParameter2)
		{
			return true;
		}
		if (ColorUtility.StripFormatting(partParameter) == ColorUtility.StripFormatting(partParameter2))
		{
			return true;
		}
		return false;
	}

	private int AdjustByValue(int Number, int Value, bool AllowZero = false)
	{
		if (Value != 100)
		{
			if (Value <= 10 && Number > ((!AllowZero) ? 1 : 0))
			{
				Number--;
			}
			if (Value <= 50 && Number > ((!AllowZero) ? 1 : 0))
			{
				Number--;
			}
			if (Value >= 150)
			{
				Number++;
			}
			if (Value >= 300)
			{
				Number++;
			}
		}
		return Number;
	}

	public GameObject GenerateItem(string Spec, int Value, int TierSpec)
	{
		int tier = Tier.Constrain(AdjustByValue(TierSpec, Value));
		if (Spec.Contains("{tier}"))
		{
			Spec = Spec.Replace("{tier}", tier.ToString());
		}
		int bonusModChance = RewardItemBonusModChance * Value / 100;
		int setModNumber = AdjustByValue(RewardItemSetModNumber, Value, AllowZero: true);
		int num = RewardItemMinorBestowalChance * Value / 100;
		int chance = RewardItemElementBestowalChance * Value / 100;
		GameObject obj = null;
		GameObjectFactory.ProcessSpecification(Spec, (GameObject o) =>
		{
			obj?.Obliterate();
			obj = o;
		}, null, 1, bonusModChance, setModNumber, RewardItemMod);
		if (obj == null)
		{
			MetricsManager.LogError($"NULL object returned from spec {Spec} {Value} {TierSpec} in RandomAltarBaetyl::GenerateItem");
			return RelicGenerator.GenerateRelic(tier);
		}
		bool flag = false;
		bool flag2 = false;
		int num2 = 20;
		string type = RelicGenerator.GetType(obj);
		string subtype = RelicGenerator.GetSubtype(type);
		string text = null;
		while (num.in100())
		{
			if (RelicGenerator.ApplyBasicBestowal(obj, type, tier, subtype))
			{
				flag = true;
				num2 += 20;
			}
			num -= 100;
		}
		if (chance.in100())
		{
			if (text == null)
			{
				text = RelicGenerator.SelectElement(obj);
			}
			if (!text.IsNullOrEmpty() && RelicGenerator.ApplyElementBestowal(obj, text, type, tier, subtype))
			{
				flag2 = true;
				num2 += 40;
			}
		}
		if (flag | flag2)
		{
			obj.SetStringProperty("Mods", "None");
			if (text == null)
			{
				text = RelicGenerator.SelectElement(obj) ?? "might";
			}
			string Name = null;
			string Article = null;
			if (15.in100())
			{
				List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote note) => note.Has("ruins") && !note.Has("historic") && note.Text != "some forgotten ruins");
				if (mapNotes.Count > 0)
				{
					Name = HistoricStringExpander.ExpandString("<spice.elements." + text + ".adjectives.!random> " + HistoricStringExpander.ExpandString("<spice.itemTypes." + type + ".!random>") + " of " + mapNotes.GetRandomElement().Text);
					Article = "the";
				}
			}
			if (Name == null)
			{
				GameObject aLegendaryEligibleCreature = EncountersAPI.GetALegendaryEligibleCreature();
				HeroMaker.MakeHero(aLegendaryEligibleCreature);
				Dictionary<string, string> vars = new Dictionary<string, string>
				{
					{ "*element*", text },
					{ "*itemType*", type },
					{
						"*personNounPossessive*",
						Grammar.MakePossessive(HistoricStringExpander.ExpandString("<spice.personNouns.!random>"))
					},
					{
						"*creatureNamePossessive*",
						Grammar.MakePossessive(aLegendaryEligibleCreature.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true))
					}
				};
				Name = HistoricStringExpander.ExpandString("<spice.history.relics.names.!random>", null, null, vars);
				QudHistoryHelpers.ExtractArticle(ref Name, out Article);
			}
			obj.RequirePart<OriginalItemType>();
			Name = QudHistoryHelpers.Ansify(Grammar.MakeTitleCase(Name));
			obj.GiveProperName(Name, Force: true);
			if (!Article.IsNullOrEmpty())
			{
				obj.SetStringProperty("IndefiniteArticle", Article);
				obj.SetStringProperty("DefiniteArticle", Article);
			}
			obj.SetImportant(flag: true);
			AfterPseudoRelicGeneratedEvent.Send(obj, text, type, subtype, tier);
		}
		if (num2 != 0 && obj.TryGetPart<Commerce>(out var Part))
		{
			Part.Value += Part.Value * (double)num2 / 100.0;
		}
		return obj;
	}

	public GameObject GenerateItem(string Spec, int Value = 100)
	{
		return GenerateItem(Spec, Value, RewardTier);
	}

	public bool ItemMatchesDemand(GameObject obj)
	{
		if (DemandIsMod)
		{
			return obj.HasPart(DemandBlueprint);
		}
		if (!(obj.Blueprint == DemandBlueprint) && !(obj.GetPropertyOrTag("SparkingQuestBlueprint") == DemandBlueprint))
		{
			return DisplayNameMatches(obj.Blueprint, DemandBlueprint);
		}
		return true;
	}

	private List<GameObject> SortForSacrifice(List<GameObject> List)
	{
		if (List.Count > 1)
		{
			List.Sort(SortForSacrifice);
		}
		return List;
	}

	private int SortForSacrifice(GameObject A, GameObject B)
	{
		int num = A.ValueEach.CompareTo(B.ValueEach);
		if (num != 0)
		{
			return num;
		}
		int num2 = A.Count.CompareTo(B.Count);
		if (num2 != 0)
		{
			return -num2;
		}
		int num3 = A.WeightEach.CompareTo(B.WeightEach);
		if (num3 != 0)
		{
			return -num3;
		}
		return A.GetCachedDisplayNameForSort().CompareTo(B.GetCachedDisplayNameForSort());
	}

	public void BaetylWantsSacrifice()
	{
		if (ParentObject.IsHostileTowards(The.Player))
		{
			return;
		}
		if (Fulfilled)
		{
			Popup.Show("I AM SATED, MORTAL. BEGONE.");
			return;
		}
		GenerateDemand();
		GenerateReward();
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("PETTY MORTAL! BRING ME ").Append(GetDemandName()).Append(", AND I SHALL REWARD YOU WITH ")
			.Append(GetRewardDescription())
			.Append('.');
		int num = 0;
		int num2 = DemandCount;
		List<GameObject> list = Event.NewGameObjectList();
		List<GameObject> list2 = Event.NewGameObjectList();
		List<GameObject> list3 = Event.NewGameObjectList();
		List<GameObject> list4 = Event.NewGameObjectList();
		foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
		{
			list.AddRange(adjacentCell.GetObjectsInCell());
		}
		foreach (GameObject item in SortForSacrifice(list))
		{
			if (ItemMatchesDemand(item))
			{
				num += item.Count;
				list4.Add(item);
				list2.Add(item);
				if (num >= num2)
				{
					break;
				}
			}
		}
		if (num < num2)
		{
			foreach (GameObject item2 in SortForSacrifice(The.Player.GetInventoryAndEquipment()))
			{
				if (ItemMatchesDemand(item2) && !item2.IsImportant() && (item2.Equipped == null || item2.CanBeUnequipped()))
				{
					num += item2.Count;
					list4.Add(item2);
					list3.Add(item2);
					if (num >= num2)
					{
						break;
					}
				}
			}
		}
		if (num >= num2)
		{
			stringBuilder.Append("\n\nOffer ").Append(ParentObject.t()).Append(' ');
			if (num > num2)
			{
				stringBuilder.Append(num2.ToString()).Append(" out of ");
			}
			if (list2.Count > 0)
			{
				stringBuilder.Append(Grammar.MakeAndList(list2, DefiniteArticles: true)).Append(" nearby");
				if (list3.Count > 0)
				{
					stringBuilder.Append(" and ").Append(Grammar.MakeAndList(list3, DefiniteArticles: false, Serial: true, Reflexive: false, SecondPerson: true, AsPossessed: true));
				}
			}
			else
			{
				stringBuilder.Append(Grammar.MakeAndList(list3, DefiniteArticles: false, Serial: true, Reflexive: false, SecondPerson: true, AsPossessed: true));
			}
			stringBuilder.Append('?');
			if (Popup.ShowYesNo(stringBuilder.ToString()) == DialogResult.Yes)
			{
				foreach (GameObject item3 in list4)
				{
					item3.DustPuff();
					int count = item3.Count;
					if (num2 >= count)
					{
						item3.Obliterate();
					}
					else
					{
						for (int i = 0; i < num2; i++)
						{
							item3.Destroy();
						}
					}
					num2 -= count;
					if (num2 <= 0)
					{
						break;
					}
				}
				int value = 100;
				if (Options.SifrahBaetylOfferings)
				{
					int rating = The.Player.StatMod("Intelligence");
					int difficulty = RewardTier + ParentObject.GetIntProperty("BaetylOfferingSifrahDifficultyModifier");
					BaetylOfferingSifrah baetylOfferingSifrah = new BaetylOfferingSifrah(ParentObject, rating, difficulty);
					baetylOfferingSifrah.Play(ParentObject);
					if (!baetylOfferingSifrah.Abort)
					{
						value = baetylOfferingSifrah.Performance;
					}
				}
				ParentObject.Render.ColorString = "&K^g";
				GenerateReward(value);
				string text = GiveReward(The.Player, IgnorePlayerControl: false, SkipItemMessage: false, SkipAttributePointsMessage: false, SkipMutationPointsMessage: false, SkipSkillPointsMessage: false, SkipExperiencePointsMessage: false, SkipLicensePointsMessage: false, SkipReputationMessage: false, value);
				Fulfilled = true;
				string demandName = GetDemandName(Caps: false);
				JournalAPI.AddAccomplishment("You appeased a baetyl with " + demandName + ", and in return received " + text + ".", "While leading a small army through " + Grammar.GetProsaicZoneName(The.Player.CurrentZone) + ", =name= demanded that a local baetyl use its powers to transmute " + demandName + " into " + GetRewardDescription().ToLower() + ".", "While leading a small army through " + Grammar.GetProsaicZoneName(The.Player.CurrentZone) + ", =name= demanded that a local baetyl use its powers to transmute " + demandName + " into " + GetRewardDescription().ToLower() + ".", null, "general", MuralCategory.AppeasesBaetyl, MuralWeight.Medium, null, -1L);
				Popup.Show("I ACCEPT YOUR OFFERING!\n\nThe sparking baetyl gives you " + text + "!");
			}
		}
		else
		{
			Popup.Show(stringBuilder.ToString());
		}
		UpdateJournalNote();
	}

	public string GiveReward(GameObject Receiver, bool IgnorePlayerControl = false, bool SkipItemMessage = false, bool SkipAttributePointsMessage = false, bool SkipMutationPointsMessage = false, bool SkipSkillPointsMessage = false, bool SkipExperiencePointsMessage = false, bool SkipLicensePointsMessage = false, bool SkipReputationMessage = false, int Value = 100)
	{
		List<string> list = new List<string>();
		if (!RewardID.IsNullOrEmpty())
		{
			GameObject gameObject = RetrieveRewardItem();
			Receiver.ReceiveObject(gameObject);
			if (!SkipItemMessage)
			{
				list.Add(gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: false));
			}
		}
		if (RewardAttributePointsAmount > 0)
		{
			Receiver.GetStat("AP").BaseValue += RewardAttributePointsAmount;
			if (!SkipAttributePointsMessage)
			{
				list.Add(RewardAttributePointsAmount + " attribute " + ((RewardAttributePointsAmount == 1) ? "point" : "points"));
			}
		}
		if (RewardMutationPointsAmount > 0)
		{
			Receiver.GainMP(RewardMutationPointsAmount);
			if (!SkipMutationPointsMessage)
			{
				list.Add(RewardMutationPointsAmount + " mutation " + ((RewardMutationPointsAmount == 1) ? "point" : "points"));
			}
		}
		if (RewardSkillPointsAmount > 0)
		{
			int num = RewardSkillPointsAmount * (50 + (Receiver.BaseStat("Intelligence") - 10) * 4);
			Receiver.GetStat("SP").BaseValue += num;
			if (!SkipSkillPointsMessage)
			{
				list.Add(num + " skill " + ((num == 1) ? "point" : "points"));
			}
		}
		if (RewardExperiencePointsAmount > 0)
		{
			int num2 = RewardExperiencePointsAmount * (50 + Receiver.Stat("Level"));
			Receiver.AwardXP(num2, -1, 0, int.MaxValue, null, ParentObject);
			if (!SkipExperiencePointsMessage)
			{
				list.Add(num2 + " experience " + ((num2 == 1) ? "point" : "points"));
			}
		}
		if (RewardLicensePointsAmount > 0)
		{
			Receiver.ModIntProperty("CyberneticsLicenses", RewardLicensePointsAmount);
			if (!SkipLicensePointsMessage)
			{
				list.Add(RewardLicensePointsAmount + " cybernetics license " + ((RewardLicensePointsAmount == 1) ? "point" : "points"));
			}
		}
		if (RewardReputationAmount > 0 && (IgnorePlayerControl || Receiver.IsPlayerControlled()))
		{
			if (RewardReputationFaction.Contains("|"))
			{
				string[] array = RewardReputationFaction.Split('|');
				bool flag = true;
				string[] array2 = array;
				foreach (string text in array2)
				{
					int amount;
					if (flag)
					{
						amount = RewardReputationAmount;
						flag = false;
					}
					else
					{
						amount = VaryRewardReputation(RewardReputation.RollCached(), Value);
					}
					The.Game.PlayerReputation.Modify(text, amount, "BaetylReward");
					if (!SkipReputationMessage)
					{
						list.Add(amount + " reputation with " + Faction.GetFormattedName(text));
					}
				}
			}
			else
			{
				The.Game.PlayerReputation.Modify(RewardReputationFaction, RewardReputationAmount, "BaetylReward");
				if (!SkipReputationMessage)
				{
					list.Add(RewardReputationAmount + " reputation with " + Faction.GetFormattedName(RewardReputationFaction));
				}
			}
		}
		return Grammar.MakeAndList(list);
	}

	public string GetRewardDescription()
	{
		string text = null;
		if (!RewardType.IsNullOrEmpty())
		{
			RandomAltarBaetylReward reward = RandomAltarBaetylRewardManager.GetReward(RewardType);
			if (reward != null)
			{
				text = reward.Description;
			}
		}
		return text ?? "STUFF";
	}

	public bool CanTalk(GameObject Actor = null)
	{
		if (!IsConversationallyResponsiveEvent.Check(ParentObject, Actor ?? The.Player, Physical: true))
		{
			return false;
		}
		if (ParentObject.IsInStasis())
		{
			return false;
		}
		if (ParentObject.HasEffect<Asleep>())
		{
			return false;
		}
		return true;
	}

	public void UpdateJournalNote()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (Fulfilled)
		{
			stringBuilder.Append("A \"SATED\" baetyl");
		}
		else
		{
			GenerateDemand();
			stringBuilder.Append("A baetyl demanding \"").Append(GetDemandName()).Append("\" and promising \"")
				.Append(GetRewardDescription())
				.Append("\".");
		}
		JournalMapNote mapNote = JournalAPI.GetMapNote(MapNoteID);
		if (mapNote == null)
		{
			JournalAPI.AddMapNote(ParentObject.GetCurrentCell().ParentZone.ZoneID, stringBuilder.ToString(), "Baetyls", null, MapNoteID, revealed: true, sold: false, -1L);
		}
		else
		{
			mapNote.Text = stringBuilder.ToString();
		}
	}

	public void RemoveJournalNote(JournalMapNote note)
	{
		if (note != null)
		{
			JournalAPI.DeleteMapNote(note);
		}
	}

	public void RemoveJournalNote()
	{
		RemoveJournalNote(JournalAPI.GetMapNote(MapNoteID));
	}

	[WishCommand(null, null, Command = "baetylrewarditem")]
	public static bool HandleBaetylRewardWish()
	{
		return Stat.Random(1, 4) switch
		{
			1 => HandleBaetylRewardMeleeWish(), 
			2 => HandleBaetylRewardArmorWish(), 
			3 => HandleBaetylRewardMissileWish(), 
			4 => HandleBaetylRewardArtifactWish(), 
			_ => true, 
		};
	}

	[WishCommand(null, null, Command = "baetylrewardmelee")]
	public static bool HandleBaetylRewardMeleeWish()
	{
		return HandleBaetylRewardWish("@Melee Weapons {tier}R");
	}

	[WishCommand(null, null, Command = "baetylrewardarmor")]
	public static bool HandleBaetylRewardArmorWish()
	{
		return HandleBaetylRewardWish("@Armor {tier}R");
	}

	[WishCommand(null, null, Command = "baetylrewardmissile")]
	public static bool HandleBaetylRewardMissileWish()
	{
		return HandleBaetylRewardWish("@Missile {tier}");
	}

	[WishCommand(null, null, Command = "baetylrewardartifact")]
	public static bool HandleBaetylRewardArtifactWish()
	{
		return HandleBaetylRewardWish("@Artifact {tier}R");
	}

	private static bool HandleBaetylRewardWish(string Spec)
	{
		RandomAltarBaetyl randomAltarBaetyl = new RandomAltarBaetyl();
		randomAltarBaetyl.GenerateDemand(The.Player.CurrentZone);
		GameObject gameObject = randomAltarBaetyl.GenerateItem(Spec);
		Popup.Show("Generated " + gameObject.an() + " as reward for " + randomAltarBaetyl.GetDemandName());
		The.Player.CurrentCell.AddObject(gameObject);
		return true;
	}
}
