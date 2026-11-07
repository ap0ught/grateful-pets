using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.WorldBuilders;

namespace XRL.World;

[Serializable]
public class VillageDynamicQuestContext : DynamicQuestContext
{
	public string VillageEntityID;

	public bool isVillageZero;

	[NonSerialized]
	public List<Location2D> usedLocations;

	[NonSerialized]
	private HistoricEntity _VillageEntity;

	[NonSerialized]
	private HistoricEntitySnapshot _VillageSnapshot;

	public HistoricEntity VillageEntity
	{
		get
		{
			if (_VillageEntity == null && !VillageEntityID.IsNullOrEmpty())
			{
				_VillageEntity = The.Game.sultanHistory.GetEntity(VillageEntityID);
			}
			return _VillageEntity;
		}
		set
		{
			_VillageEntity = value;
			VillageEntityID = value?.id;
		}
	}

	public HistoricEntitySnapshot villageSnapshot
	{
		get
		{
			if (_VillageSnapshot == null && !VillageEntityID.IsNullOrEmpty())
			{
				_VillageSnapshot = VillageEntity?.GetCurrentSnapshot();
			}
			return _VillageSnapshot;
		}
		set
		{
			_VillageSnapshot = value;
			VillageEntity = value?.entity;
		}
	}

	public WorldInfo worldInfo => The.Game.GetObjectGameState("JoppaWorldInfo") as WorldInfo;

	public VillageDynamicQuestContext()
	{
	}

	public VillageDynamicQuestContext(HistoricEntity VillageEntity)
	{
		this.VillageEntity = VillageEntity;
		isVillageZero = VillageEntity.HasEntityProperty("isVillageZero", -1L);
	}

	public VillageDynamicQuestContext(HistoricEntitySnapshot villageSnapshot)
	{
		this.villageSnapshot = villageSnapshot;
		isVillageZero = villageSnapshot.hasProperty("isVillageZero");
	}

	public override void Write(SerializationWriter Writer)
	{
		base.Write(Writer);
		Writer.Write(usedLocations);
	}

	public override void Read(SerializationReader Reader)
	{
		base.Read(Reader);
		usedLocations = Reader.ReadLocation2DList();
	}

	public override HistoricEntity originEntity()
	{
		return null;
	}

	public override DynamicQuestReward getQuestReward()
	{
		if (isVillageZero)
		{
			if (questNumber == 0)
			{
				DynamicQuestReward dynamicQuestReward = new DynamicQuestReward
				{
					StepXP = 750
				};
				GameObject gameObject = GameObject.Create("Blank Recoiler");
				gameObject.MakeUnderstood();
				gameObject.Render.DisplayName = villageSnapshot.GetProperty("name") + " recoiler";
				Teleporter part = gameObject.GetPart<Teleporter>();
				part.DestinationZone = villageSnapshot.GetProperty("zoneID");
				part.DestinationX = -1;
				part.DestinationY = -1;
				dynamicQuestReward.rewards.Add(new DynamicQuestRewardElement_GameObject(gameObject));
				dynamicQuestReward.rewards.Add(new DynamicQuestRewardElement_VillageZeroMainQuestHook());
				dynamicQuestReward.postrewards.Add(new DynamicQuestRewardElement_Reputation("villagers of " + villageSnapshot.GetProperty("name"), 50));
				return dynamicQuestReward;
			}
			if (questNumber == 1)
			{
				return new DynamicQuestReward
				{
					StepXP = 1000,
					rewards = 
					{
						(DynamicQuestRewardElement)new DynamicQuestRewardElement_ChoiceFromPopulation("VillageZero_Reward", 3),
						(DynamicQuestRewardElement)new DynamicQuestRewardElement_VillageZeroLoot()
					},
					postrewards = { (DynamicQuestRewardElement)new DynamicQuestRewardElement_Reputation("villagers of " + villageSnapshot.GetProperty("name"), 100) }
				};
			}
		}
		tier = villageSnapshot.Tier;
		return new DynamicQuestReward
		{
			StepXP = tier * 1000,
			rewards = 
			{
				(DynamicQuestRewardElement)new DynamicQuestRewardElement_Reputation("villagers of " + villageSnapshot.GetProperty("name"), 100),
				(DynamicQuestRewardElement)new DynamicQuestRewardElement_ChoiceFromPopulation("VillageTier" + tier + "_QuestReward", 3)
			}
		};
	}

	private void findQuestTargetLocationsInRange(List<GeneratedLocationInfo> list, int minRange, int maxRange, Location2D rangeFrom)
	{
		worldInfo.ForeachGeneratedLocation((GeneratedLocationInfo l) =>
		{
			if (l.distanceTo(rangeFrom) >= minRange && l.distanceTo(rangeFrom) <= maxRange && l.isUndiscovered() && (usedLocations == null || !usedLocations.Contains(l.zoneLocation)) && l.name != "some forgotten ruins")
			{
				list.Add(l);
			}
		});
	}

	public override GeneratedLocationInfo getNearbyUndiscoveredLocation()
	{
		Location2D rangeFrom = Zone.zoneIDTo240x72Location(originZoneId());
		List<GeneratedLocationInfo> list = new List<GeneratedLocationInfo>();
		int num = 12;
		int num2 = 18;
		while (list.Count == 0 && num >= 2)
		{
			findQuestTargetLocationsInRange(list, num, num2, rangeFrom);
			num--;
			num2++;
		}
		if (list.Count > 0)
		{
			GeneratedLocationInfo randomElement = list.GetRandomElement();
			if (usedLocations == null)
			{
				usedLocations = new List<Location2D>();
			}
			usedLocations.Add(randomElement.zoneLocation);
			return randomElement;
		}
		return null;
	}

	public override string originZoneId()
	{
		return villageSnapshot.GetProperty("zoneID");
	}

	public override string getQuestOriginZone()
	{
		return villageSnapshot.GetProperty("name");
	}

	public override string questTargetZone(int minDistance, int maxDistance)
	{
		return null;
	}

	public override string getQuestGiverZone()
	{
		return villageSnapshot.GetProperty("zoneID");
	}

	public bool questGiverFilter(GameObject go)
	{
		if (go.HasProperty("ParticipantVillager") && go.HasProperty("NamedVillager"))
		{
			return !go.HasProperty("GivesDynamicQuest");
		}
		return false;
	}

	public override Func<GameObject, bool> getQuestGiverFilter()
	{
		return questGiverFilter;
	}

	public bool questActorFilter(GameObject go)
	{
		return go.HasProperty("NamedVillager");
	}

	public override Func<GameObject, bool> getQuestActorFilter()
	{
		return questActorFilter;
	}

	public override List<string> getSacredThings()
	{
		List<string> list = new List<string>(villageSnapshot.GetList("sacredThings"));
		if (list.Count > 0)
		{
			return list;
		}
		list.Add(villageSnapshot.GetProperty("defaultSacredThing"));
		return list;
	}

	public bool IsValidQuestDestination(Location2D location)
	{
		int num = villageSnapshot.Tier;
		if (num < 2 || isVillageZero)
		{
			num = 2;
		}
		if (usedLocations != null && usedLocations.Contains(location))
		{
			return false;
		}
		if (XRLCore.Core.Game.ZoneManager.GetZoneTier("JoppaWorld", location.X / 3, location.Y / 3, 10) > num)
		{
			return false;
		}
		if (isVillageZero && XRLCore.Core.Game.ZoneManager.GetZoneTerrain("JoppaWorld", location.X / 3, location.Y / 3).GetBlueprint().DescendsFrom("TerrainFlowerfields"))
		{
			return false;
		}
		List<GeneratedLocationInfo> generatedLocationsAt = worldInfo.GetGeneratedLocationsAt(location);
		if (generatedLocationsAt != null)
		{
			foreach (GeneratedLocationInfo item in generatedLocationsAt)
			{
				if (item.name == "some forgotten ruins")
				{
					return false;
				}
			}
		}
		return true;
	}

	public override DynamicQuestDeliveryTarget getQuestDeliveryTarget()
	{
		int num = 0;
		int num2 = 0;
		DynamicQuestDeliveryTarget dynamicQuestDeliveryTarget = null;
		do
		{
			dynamicQuestDeliveryTarget = worldInfo.ResolveDeliveryTargetForVillage(villageSnapshot, Math.Max(12 - num, 2), 18 + num, num2, IsValidQuestDestination);
			num += 3;
			num2++;
		}
		while (dynamicQuestDeliveryTarget == null && num < 100);
		if (dynamicQuestDeliveryTarget != null)
		{
			if (usedLocations == null)
			{
				usedLocations = new List<Location2D>();
			}
			usedLocations.Add(dynamicQuestDeliveryTarget.location);
		}
		return dynamicQuestDeliveryTarget;
	}

	public override GameObject getQuestRemoteInteractable()
	{
		List<GameObject> list = new List<GameObject>();
		if (villageSnapshot.hasProperty("worships_creature"))
		{
			GameObject cachedObjects = XRLCore.Core.Game.ZoneManager.GetCachedObjects(villageSnapshot.GetProperty("worships_creature_id"));
			if (cachedObjects != null)
			{
				GameObject gameObject = GameObject.Create(PopulationManager.RollOneFrom("Village_RandomBaseStatue_*Default").Blueprint);
				gameObject.GetPart<RandomStatue>().SetCreature(cachedObjects.DeepCopy(CopyEffects: false, CopyID: true));
				gameObject.SetStringProperty("QuestVerb", "pray at");
				gameObject.SetStringProperty("QuestEvent", "Prayed");
				gameObject.AddPart(new Shrine());
				list.Add(gameObject);
			}
			else
			{
				MetricsManager.LogWarning("Unable to find a creature for cache id: " + villageSnapshot.GetProperty("worships_creature_id"));
			}
		}
		if (villageSnapshot.hasProperty("despises_creature"))
		{
			GameObject cachedObjects2 = XRLCore.Core.Game.ZoneManager.GetCachedObjects(villageSnapshot.GetProperty("despises_creature_id"));
			if (cachedObjects2 != null)
			{
				GameObject gameObject2 = GameObject.Create(PopulationManager.RollOneFrom("Village_RandomBaseStatue_*Default").Blueprint);
				gameObject2.GetPart<RandomStatue>().SetCreature(cachedObjects2.DeepCopy(CopyEffects: false, CopyID: true));
				if (50.in100())
				{
					gameObject2.AddPart(new ModDesecrated());
					gameObject2.SetStringProperty("QuestVerb", "pray at");
					gameObject2.SetStringProperty("QuestEvent", "Prayed");
				}
				else
				{
					gameObject2.SetStringProperty("QuestVerb", "desecrate");
					gameObject2.SetStringProperty("QuestEvent", "Desecrated");
				}
				gameObject2.AddPart(new Shrine());
				list.Add(gameObject2);
			}
			else
			{
				MetricsManager.LogWarning("Unable to find a creature for cache id: " + villageSnapshot.GetProperty("despises_creature_id"));
			}
		}
		if (villageSnapshot.hasProperty("worships_faction"))
		{
			GameObject gameObject3 = EncountersAPI.GetALegendaryEligibleCreatureFromFaction(villageSnapshot.GetProperty("worships_faction")) ?? EncountersAPI.GetACreatureFromFaction(villageSnapshot.GetProperty("worships_faction"));
			if (gameObject3 != null)
			{
				GameObject gameObject4 = GameObject.Create(PopulationManager.RollOneFrom("Village_RandomBaseStatue_*Default").Blueprint);
				gameObject4.GetPart<RandomStatue>().SetCreature(gameObject3.DeepCopy(CopyEffects: false, CopyID: true));
				gameObject4.SetStringProperty("QuestVerb", "pray at");
				gameObject4.SetStringProperty("QuestEvent", "Prayed");
				gameObject4.AddPart(new Shrine());
				list.Add(gameObject4);
			}
			else
			{
				MetricsManager.LogWarning("Unable to find a creature for faction: " + villageSnapshot.GetProperty("worships_faction"));
			}
		}
		if (villageSnapshot.hasProperty("despises_faction"))
		{
			GameObject gameObject5 = EncountersAPI.GetALegendaryEligibleCreatureFromFaction(villageSnapshot.GetProperty("despises_faction")) ?? EncountersAPI.GetACreatureFromFaction(villageSnapshot.GetProperty("despises_faction"));
			if (gameObject5 != null)
			{
				GameObject gameObject6 = GameObject.Create(PopulationManager.RollOneFrom("Village_RandomBaseStatue_*Default").Blueprint);
				gameObject6.GetPart<RandomStatue>().SetCreature(gameObject5.DeepCopy(CopyEffects: false, CopyID: true));
				if (50.in100())
				{
					gameObject6.AddPart(new ModDesecrated());
					gameObject6.SetStringProperty("QuestVerb", "pray at");
					gameObject6.SetStringProperty("QuestEvent", "Prayed");
				}
				else
				{
					gameObject6.SetStringProperty("QuestVerb", "desecrate");
					gameObject6.SetStringProperty("QuestEvent", "Desecrated");
				}
				gameObject6.AddPart(new Shrine());
				list.Add(gameObject6);
			}
			else
			{
				MetricsManager.LogWarning("Unable to find a creature for faction: " + villageSnapshot.GetProperty("despises_faction"));
			}
		}
		if (villageSnapshot.hasProperty("worships_sultan"))
		{
			GameObject gameObject7 = GameObject.Create("SultanShrine");
			gameObject7.SetStringProperty("ForceSultan", villageSnapshot.GetProperty("worships_sultan_id"));
			gameObject7.SetStringProperty("QuestVerb", "pray at");
			gameObject7.SetStringProperty("QuestEvent", "Prayed");
			gameObject7.AddPart(new Shrine());
			list.Add(gameObject7);
		}
		if (villageSnapshot.hasProperty("despises_sultan"))
		{
			GameObject gameObject8 = GameObject.Create("SultanShrine");
			gameObject8.SetStringProperty("ForceSultan", villageSnapshot.GetProperty("despises_sultan_id"));
			if (50.in100())
			{
				gameObject8.AddPart(new ModDesecrated());
				gameObject8.SetStringProperty("QuestVerb", "pray at");
				gameObject8.SetStringProperty("QuestEvent", "Prayed");
			}
			else
			{
				gameObject8.SetStringProperty("QuestVerb", "desecrate");
				gameObject8.SetStringProperty("QuestEvent", "Desecrated");
			}
			gameObject8.AddPart(new Shrine());
			list.Add(gameObject8);
		}
		int maxTechTier = Convert.ToInt32(villageSnapshot.GetProperty("techTier")) + 2;
		GameObject anObject = EncountersAPI.GetAnObject((GameObjectBlueprint b) => b.TechTier <= maxTechTier && !b.HasTag("NotQuestable") && b.HasTag("QuestableVerb") && !string.IsNullOrEmpty(b.GetTag("QuestableVerb")));
		if (anObject == null)
		{
			anObject = EncountersAPI.GetAnObject((GameObjectBlueprint b) => b.HasTag("QuestableVerb") && !string.IsNullOrEmpty(b.GetTag("QuestableVerb")));
		}
		if (anObject != null)
		{
			string[] array = anObject.GetTag("QuestableVerb").Split(',');
			string[] array2 = anObject.GetTag("QuestableEvent").Split(',');
			int num = Stat.Random(0, array.Count() - 1);
			anObject.SetStringProperty("QuestVerb", array[num]);
			anObject.SetStringProperty("QuestEvent", array2[num]);
			list.Add(anObject);
			list.Add(anObject);
			list.Add(anObject);
		}
		GameObject randomElement = list.GetRandomElement();
		if (randomElement != null)
		{
			randomElement.SetIntProperty("norestock", 1);
			randomElement.SetStringProperty("NeverStack", "1");
		}
		return list.GetRandomElement();
	}

	public override GameObject getQuestGenericRemoteInteractable()
	{
		List<GameObject> list = new List<GameObject>();
		int maxTechTier = Convert.ToInt32(villageSnapshot.GetProperty("techTier")) + 2;
		GameObject anObject = EncountersAPI.GetAnObject((GameObjectBlueprint b) => b.TechTier <= maxTechTier && !b.HasTag("NotQuestable") && b.HasTag("QuestableVerb") && !string.IsNullOrEmpty(b.GetTag("QuestableVerb")));
		if (anObject == null)
		{
			anObject = EncountersAPI.GetAnObject((GameObjectBlueprint b) => b.HasTag("QuestableVerb") && !string.IsNullOrEmpty(b.GetTag("QuestableVerb")));
		}
		if (anObject != null)
		{
			string[] array = anObject.GetTag("QuestableVerb").Split(',');
			string[] array2 = anObject.GetTag("QuestableEvent").Split(',');
			int num = Stat.Random(0, array.Count() - 1);
			anObject.SetStringProperty("QuestVerb", array[num]);
			anObject.SetStringProperty("QuestEvent", array2[num]);
			list.Add(anObject);
		}
		GameObject randomElement = list.GetRandomElement();
		if (randomElement != null)
		{
			randomElement.SetIntProperty("norestock", 1);
			randomElement.SetIntProperty("QuestItem", 1);
		}
		return randomElement;
	}

	public override GameObject getQuestDeliveryItem()
	{
		bool flag = villageSnapshot.hasProperty("signatureItem") && GameObject.CreateSample(villageSnapshot.GetProperty("signatureItem")).IsTakeable();
		bool flag2 = false;
		if (villageSnapshot.hasProperty("signatureHistoricObjectType"))
		{
			GameObject gameObject = GameObject.CreateSample(villageSnapshot.GetProperty("signatureHistoricObjectType"));
			flag2 = gameObject.IsTakeable() && gameObject.Weight <= 100;
		}
		GameObject gameObject2 = HistoryKit.Switch.RandomWhere(flag, flag2) switch
		{
			0 => GameObject.Create(villageSnapshot.GetProperty("signatureItem")), 
			1 => GameObject.Create(villageSnapshot.GetProperty("signatureHistoricObjectType")), 
			_ => EncountersAPI.GetAnItem((GameObjectBlueprint ob) => ob.Tier <= villageSnapshot.Tier && ob.GetPartParameter("Physics", "Takeable", Default: true) && ob.GetPartParameter("Physics", "Weight", 0) <= 100 && !ob.HasTag("ExcludeFromQuests")), 
		};
		gameObject2.Render.SetForegroundColor(villageSnapshot.GetList("palette")[0]);
		gameObject2.Render.DetailColor = villageSnapshot.GetList("palette")[1];
		gameObject2.SetIntProperty("norestock", 1);
		gameObject2.SetIntProperty("QuestItem", 1);
		gameObject2.SetStringProperty("NeverStack", "1");
		return gameObject2;
	}

	public override string getQuestItemNameMutation(string name)
	{
		string text = (villageSnapshot.hasListProperty("itemAdjectiveRoots") ? Grammar.Adjectify(villageSnapshot.GetList("itemAdjectiveRoots").GetRandomElement()) : HistoricStringExpander.ExpandString("<spice.commonPhrases.sacred.!random>"));
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("*adj*", text);
		dictionary.Add("*adj.cap*", Grammar.MakeTitleCase(text));
		dictionary.Add("*itemName*", name);
		dictionary.Add("*itemName.cap*", Grammar.MakeTitleCase(name));
		dictionary.Add("*noun.cap*", Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.nouns.!random>")));
		return HistoricStringExpander.ExpandString("<spice.quests.questContext.itemNameMutation.!random>", null, null, dictionary);
	}

	public override string assassinationTargetId()
	{
		return null;
	}

	public override GameObject getALostItem()
	{
		return null;
	}
}
