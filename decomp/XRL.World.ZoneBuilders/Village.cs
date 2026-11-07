using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Annals;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.World.AI;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Quests;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class Village : VillageBase
{
	public string dynamicCreatureTableName;

	public bool SurfaceRevealer = true;

	private string[] staticPerVillage = new string[4] { "*Storage,*Furniture", "*LiquidStorage,*Furniture", "*Seating,*Furniture", "*Sleep*,*Furniture" };

	private string[] staticPerBuilding = new string[1] { "*LightSource,*Furniture" };

	private Dictionary<string, string> staticVillageResults = new Dictionary<string, string>();

	public string mayorTemplate
	{
		get
		{
			if (villageSnapshot == null)
			{
				return null;
			}
			if (villageSnapshot.GetProperty("mayorTemplate") != "unknown")
			{
				return villageSnapshot.GetProperty("mayorTemplate");
			}
			return "Mayor";
		}
	}

	public GameObject generateWarden(GameObject baseObject, bool GivesRep = false)
	{
		GameObject gameObject;
		if (baseObject != null)
		{
			gameObject = baseObject;
		}
		else
		{
			Func<GameObject> func = FuzzyFunctions.DoThisButRarelyDoThat(() =>
			{
				GameObject aNonLegendaryCreature = EncountersAPI.GetANonLegendaryCreature((GameObjectBlueprint ob) => ob.HasTag(dynamicCreatureTableName) && (ob.HasPart("Body") || ob.HasTagOrProperty("BodySubstitute")) && (ob.HasPart("Combat") || ob.HasTagOrProperty("BodySubstitute")) && !ob.HasTag("Merchant") && !ob.HasTag("ExcludeFromVillagePopulations"));
				return (aNonLegendaryCreature == null) ? null : aNonLegendaryCreature;
			}, () => EncountersAPI.GetANonLegendaryCreature((GameObjectBlueprint ob) => ob.HasPart("Body") && ob.HasPart("Combat") && !ob.HasTag("Merchant") && !ob.HasTag("ExcludeFromVillagePopulations")), "33");
			try
			{
				gameObject = func();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				gameObject = null;
			}
			if (gameObject == null)
			{
				gameObject = getBaseVillager(NoRep: true);
				preprocessVillager(gameObject);
			}
			else
			{
				preprocessVillager(gameObject, foreign: true);
			}
		}
		gameObject.Brain.Mobile = true;
		gameObject.Brain.Factions = "";
		gameObject.Brain.Allegiance.Clear();
		gameObject.Brain.Allegiance.Add("Wardens", 100);
		gameObject.Brain.Allegiance.Hostile = false;
		gameObject.Brain.Allegiance.Calm = true;
		gameObject = HeroMaker.MakeHero(gameObject, "SpecialVillagerHeroTemplate_Warden", -1, "Warden");
		gameObject.RequirePart<Interesting>();
		gameObject.SetIntProperty("VillageWarden", 1);
		gameObject.SetIntProperty("NamedVillager", 1);
		if (isVillageZero)
		{
			gameObject.SetIntProperty("WaterRitualNoSellSkill", 1);
		}
		GivesRep givesRep = gameObject.GetPart<GivesRep>();
		givesRep?.ResetRelatedFactions();
		if (GivesRep)
		{
			gameObject.SetStringProperty("staticFaction1", villageFaction + ",friend,defending their village");
			string propertyOrTag = gameObject.GetPropertyOrTag("NoHateFactions");
			propertyOrTag = ((!propertyOrTag.IsNullOrEmpty()) ? (propertyOrTag + ",Wardens") : "Wardens");
			gameObject.SetStringProperty("NoHateFactions", propertyOrTag);
			if (givesRep == null)
			{
				givesRep = gameObject.AddPart<GivesRep>();
			}
			givesRep.FillInRelatedFactions(Initial: true);
		}
		else if (givesRep != null)
		{
			gameObject.RemovePart(givesRep);
		}
		string text = HistoricStringExpander.ExpandString("<spice.villages.warden.introDialog.!random>");
		gameObject.SetIntProperty("SuppressSimpleConversation", 1);
		ConversationsAPI.addSimpleConversationToObject(gameObject, text, "Live and drink.", null, null, null, true);
		TakeOnRoleEvent.Send(gameObject, "Warden");
		return gameObject;
	}

	public GameObject generateMayor(GameObject baseObject, string specialTemplate = "SpecialVillagerHeroTemplate_Mayor", bool GivesRep = true)
	{
		GameObject gameObject = null;
		if (baseObject != null)
		{
			gameObject = baseObject;
		}
		else
		{
			gameObject = getBaseVillager();
			preprocessVillager(gameObject);
			setVillagerProperties(gameObject);
		}
		if (gameObject.Brain != null)
		{
			gameObject.Brain.SetFactionFeeling(villageFaction, RuleSettings.REPUTATION_LOVED);
			gameObject.Brain.SetFactionFeeling(villagerBaseFaction, RuleSettings.REPUTATION_LOVED);
			gameObject.Brain.SetFactionMembership(villageFaction, 100);
			if (!isVillageZero)
			{
				gameObject.Brain.SetFactionMembership(villagerBaseFaction, 25);
			}
		}
		GivesRep givesRep = gameObject.GetPart<GivesRep>();
		givesRep?.ResetRelatedFactions();
		if (GivesRep)
		{
			string propertyOrTag = gameObject.GetPropertyOrTag("NoHateFactions");
			propertyOrTag = ((!propertyOrTag.IsNullOrEmpty()) ? (propertyOrTag + ",Wardens") : "Wardens");
			gameObject.SetStringProperty("NoHateFactions", propertyOrTag);
			if (givesRep == null)
			{
				givesRep = gameObject.AddPart<GivesRep>();
			}
			givesRep.FillInRelatedFactions(Initial: true);
		}
		else if (givesRep != null)
		{
			gameObject.RemovePart(givesRep);
		}
		gameObject = HeroMaker.MakeHero(gameObject, specialTemplate, -1, mayorTemplate);
		gameObject.RequirePart<Interesting>();
		gameObject.SetStringProperty("Mayor", villageFaction);
		gameObject.SetIntProperty("VillageMayor", 1);
		gameObject.SetIntProperty("NamedVillager", 1);
		gameObject.SetIntProperty("ParticipantVillager", 1);
		gameObject.SetStringProperty("WaterRitual_Skill", signatureSkill ?? RollOneFrom("Village_RandomTaughtSkill"));
		if (signatureDish != null)
		{
			gameObject.AddPart(new TeachesDish(signatureDish, "What a savory smell! Teach me to cook the favorite dish of " + villageName + ".\n"));
		}
		string newValue = ((villageSnapshot.GetList("sacredThings").Count > 0) ? villageSnapshot.GetList("sacredThings").GetRandomElement() : villageSnapshot.GetProperty("defaultSacredThing"));
		string newValue2 = ((villageSnapshot.GetList("profaneThings").Count > 0) ? villageSnapshot.GetList("profaneThings").GetRandomElement() : villageSnapshot.GetProperty("defaultProfaneThing"));
		string message = HistoricStringExpander.ExpandString("<spice.villages.mayor.introDialog.!random>").Replace("*villageName*", villageName).Replace("*sacredThing*", newValue)
			.Replace("*profaneThing*", newValue2);
		gameObject.SetIntProperty("SuppressSimpleConversation", 1);
		AddVillagerConversation(gameObject, message, "Live and drink, =pronouns.formalAddressTerm=.");
		LandingPadsSystem.AddDynamicVillageConversation(gameObject);
		TakeOnRoleEvent.Send(gameObject, "Mayor");
		return gameObject;
	}

	public GameObject generateMerchant(GameObject baseObject)
	{
		GameObject baseVillager;
		if (!isVillageZero && baseObject == null && If.d100(20))
		{
			baseVillager = getBaseVillager(NoRep: true);
			preprocessVillager(baseVillager);
			setVillagerProperties(baseVillager);
			string additionalSpecializationTemplate = (baseVillager.GetBlueprint().DescendsFrom("Dromad") ? "SpecialVillagerHeroTemplate_DromadMerchant" : "SpecialVillagerHeroTemplate_Merchant");
			baseVillager = HeroMaker.MakeHero(baseVillager, additionalSpecializationTemplate, -1, "Merchant");
		}
		else
		{
			if (baseObject != null)
			{
				baseVillager = baseObject;
			}
			else if (isVillageZero)
			{
				baseVillager = GameObjectFactory.Factory.Blueprints["DromadTrader_Village0"].createOne();
				preprocessVillager(baseVillager, foreign: true);
			}
			else
			{
				baseVillager = GameObjectFactory.Factory.Blueprints["DromadTrader" + villageTier].createOne();
				preprocessVillager(baseVillager, foreign: true);
			}
			baseVillager.RemovePart<DromadCaravan>();
			baseVillager.RemovePart<ConversationScript>();
			string text = NameMaker.MakeTitle(baseVillager, null, null, null, null, null, null, null, null, null, "Merchant");
			baseVillager.GiveProperName();
			if (!text.IsNullOrEmpty())
			{
				baseVillager.RequirePart<Titles>().AddTitle(text, -5);
			}
			if (baseVillager.GetSpecies() != "dromad")
			{
				baseVillager.RequirePart<DisplayNameColor>().SetColorByPriority("Y", 30);
				baseVillager.RequirePart<MerchantIconColor>();
			}
		}
		baseVillager.RequirePart<Interesting>();
		baseVillager.SetIntProperty("SuppressSimpleConversation", 1);
		if (baseVillager.GetBlueprint().DescendsFrom("Dromad"))
		{
			AddVillagerConversation(baseVillager, "Welcome, =player.species=. What do you desire?");
		}
		else
		{
			AddVillagerConversation(baseVillager, "Come. Browse my wares, =player.formalAddressTerm=.", "Live and drink, =pronouns.formalAddressTerm=.");
		}
		if (baseVillager.Brain.Allegiance.IsNullOrEmpty())
		{
			baseVillager.Brain.Factions = villageFaction + "-100";
		}
		else if (!baseVillager.Brain.Allegiance.ContainsKey(villageFaction))
		{
			baseVillager.Brain.Allegiance[villageFaction] = 25;
		}
		GenericInventoryRestocker genericInventoryRestocker = baseVillager.RequirePart<GenericInventoryRestocker>();
		genericInventoryRestocker.Clear();
		for (int i = 0; i <= 2 && villageTier > i; i++)
		{
			genericInventoryRestocker.AddTable("Tier" + (villageTier - i).ToStringCached() + "Wares");
		}
		genericInventoryRestocker.PerformRestock(Silent: true);
		baseVillager.SetIntProperty("VillageMerchant", 1);
		baseVillager.SetIntProperty("NamedVillager", 1);
		if (villageSnapshot.hasProperty("worships_faction") && GameObjectFactory.Factory.GetFactionMembers(villageSnapshot.GetProperty("worships_faction")).Count > 0)
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateObject(PopulationManager.RollOneFrom("Figurines " + villageTier).Blueprint, 0, 0, null, null, (GameObject o) =>
			{
				if (o.TryGetPart<RandomFigurine>(out var Part))
				{
					Part.Creature = GameObjectFactory.Factory.GetFactionMembers(villageSnapshot.GetProperty("worships_faction")).GetRandomElement().Name;
				}
			});
			baseVillager.ReceiveObject(gameObject);
		}
		TakeOnRoleEvent.Send(baseVillager, "Merchant");
		return baseVillager;
	}

	public GameObject generateApothecary(GameObject immigrant = null)
	{
		int num = Math.Min(Math.Max(villageTier, 1), 8);
		string additionalSpecializationTemplate = "SpecialVillagerHeroTemplate_Apothecary";
		GameObject gameObject;
		if (immigrant == null && !If.d100(50))
		{
			gameObject = ((!isVillageZero) ? GameObjectFactory.Factory.Blueprints["HumanApothecary" + num].createOne() : GameObjectFactory.Factory.Blueprints["HumanApothecary_Village0"].createOne());
		}
		else
		{
			if (immigrant != null)
			{
				gameObject = immigrant;
			}
			else
			{
				gameObject = getBaseVillager(NoRep: true);
				preprocessVillager(gameObject);
				setVillagerProperties(gameObject);
			}
			GameObject gameObject2 = ((!isVillageZero) ? GameObjectFactory.Factory.Blueprints["HumanApothecary" + num].createOne() : GameObjectFactory.Factory.Blueprints["HumanApothecary_Village0"].createOne());
			foreach (BaseSkill skill in gameObject2.GetPart<XRL.World.Parts.Skills>().SkillList)
			{
				gameObject.AddSkill(skill.Name);
			}
			GenericInventoryRestocker genericInventoryRestocker = gameObject.RequirePart<GenericInventoryRestocker>();
			genericInventoryRestocker.Table = gameObject2.GetPart<GenericInventoryRestocker>()?.Table ?? "Village Apothecary 1";
			genericInventoryRestocker.Chance = 100;
			gameObject.Statistics["XP"].BaseValue = Math.Max(gameObject.Stat("XP"), gameObject2.Stat("XP"));
			gameObject.Statistics["Hitpoints"].BaseValue = Math.Max(gameObject.Stat("Hitpoints"), gameObject2.Stat("Hitpoints"));
			gameObject.Statistics["Intelligence"].BaseValue = Math.Max(gameObject.Stat("Intelligence"), 15);
			gameObject.Statistics["Toughness"].BaseValue = Math.Max(gameObject.Stat("Toughness"), 15);
		}
		gameObject = HeroMaker.MakeHero(gameObject, additionalSpecializationTemplate, -1, "Apothecary");
		gameObject.RequirePart<Interesting>();
		gameObject.RemovePart<ConversationScript>();
		gameObject.SetIntProperty("SuppressSimpleConversation", 1);
		if (villageTier <= 3)
		{
			ConversationsAPI.addSimpleConversationToObject(gameObject, "I've the cure for what ails you.~You don't look so good. You need more yuckwheat and honey in your diet.~Cook your meals with yuckwheat if you feel sick. Catch a disease early enough and you can kill it.~\"Ease the pain, addle the brain.\" Be careful when you chew witchwood bark.", "Live and drink.", null, null, null, true);
		}
		else
		{
			ConversationsAPI.addSimpleConversationToObject(gameObject, "I've the cure for what ails you.~You don't look so good. You need more yuckwheat and honey in your diet.~Cook your meals with yuckwheat if you feel sick. Catch a disease early enough and you can kill it.~\"Ease the pain, addle the brain.\" Be careful when you chew witchwood bark.~In the market for a tonic, =player.formalAddressTerm=? Spend water now or blood later, your choice.~Prickly-boons and yuckwheat for trade.~If you came for the humble pie, you had best not have led any mind-hunters here.~Have you got enough tonics?", "Live and drink.", null, null, null, true);
		}
		if (gameObject.Brain.Allegiance.IsNullOrEmpty())
		{
			gameObject.Brain.Factions = villageFaction + "-100";
		}
		else if (!gameObject.Brain.Allegiance.ContainsKey(villageFaction))
		{
			gameObject.Brain.Allegiance[villageFaction] = 50;
		}
		gameObject.SetIntProperty("VillageApothecary", 1);
		gameObject.SetIntProperty("NamedVillager", 1);
		TakeOnRoleEvent.Send(gameObject, "Apothecary");
		return gameObject;
	}

	public GameObject generateTinker(GameObject immigrant = null)
	{
		string additionalSpecializationTemplate = "SpecialVillagerHeroTemplate_Tinker";
		GameObject gameObject;
		if (immigrant == null && !If.d100(50))
		{
			gameObject = ((!isVillageZero) ? GameObjectFactory.Factory.Blueprints["HumanTinker" + villageTier].createOne() : GameObjectFactory.Factory.Blueprints["HumanTinker_Village0"].createOne());
		}
		else
		{
			if (immigrant != null)
			{
				gameObject = immigrant;
			}
			else
			{
				gameObject = getBaseVillager(NoRep: true);
				preprocessVillager(gameObject);
				setVillagerProperties(gameObject);
			}
			GameObject gameObject2 = ((!isVillageZero) ? GameObjectFactory.Factory.Blueprints["HumanTinker" + villageTier].createOne() : GameObjectFactory.Factory.Blueprints["HumanTinker_Village0"].createOne());
			foreach (BaseSkill skill in gameObject2.GetPart<XRL.World.Parts.Skills>().SkillList)
			{
				gameObject.AddSkill(skill.Name);
			}
			GenericInventoryRestocker genericInventoryRestocker = gameObject.RequirePart<GenericInventoryRestocker>();
			genericInventoryRestocker.Table = gameObject2.GetPart<GenericInventoryRestocker>()?.Table ?? "Village Tinker 1";
			genericInventoryRestocker.Chance = 100;
			gameObject.GetStat("XP").BaseValue = Math.Max(gameObject.GetStatValue("XP"), gameObject2.GetStatValue("XP"));
			gameObject.GetStat("Hitpoints").BaseValue = Math.Max(gameObject.GetStatValue("Hitpoints"), gameObject2.GetStatValue("Hitpoints"));
			gameObject.GetStat("Intelligence").BaseValue = Math.Max(gameObject.GetStatValue("Intelligence"), 16);
		}
		gameObject = HeroMaker.MakeHero(gameObject, additionalSpecializationTemplate, -1, "Tinker");
		gameObject.RequirePart<Interesting>();
		gameObject.RemovePart<ConversationScript>();
		gameObject.SetIntProperty("SuppressSimpleConversation", 1);
		ConversationsAPI.addSimpleConversationToObject(gameObject, "Need a gadget repaired or identified, =player.formalAddressTerm=? Or if you're a tinker =player.reflexive=, perhaps you'd like to peruse my schematics?", "Live and drink, tinker.", null, null, null, true);
		if (gameObject.Brain.Allegiance.IsNullOrEmpty())
		{
			gameObject.Brain.Factions = villageFaction + "-100";
		}
		else if (!gameObject.Brain.Allegiance.ContainsKey(villageFaction))
		{
			gameObject.Brain.Allegiance[villageFaction] = 50;
		}
		gameObject.SetIntProperty("VillageTinker", 1);
		gameObject.SetIntProperty("NamedVillager", 1);
		TakeOnRoleEvent.Send(gameObject, "Tinker");
		return gameObject;
	}

	public GameObject generateImmigrant(string type, string name, string gender, string role, string whyQ, string whyA)
	{
		GameObject gameObject = ((type != null) ? GameObject.Create(type) : GameObject.Create(PopulationManager.RollOneFrom("DynamicInheritsTable:Creature:Tier" + villageTier).Blueprint));
		preprocessVillager(gameObject, foreign: true);
		gameObject.SetStringProperty("HeroNameColor", "&Y");
		setVillagerProperties(gameObject);
		gameObject = role switch
		{
			"mayor" => generateMayor(gameObject, "SpecialVillagerHeroTemplate_" + mayorTemplate), 
			"warden" => generateWarden(gameObject, isVillageZero), 
			"merchant" => generateMerchant(gameObject), 
			"tinker" => generateTinker(gameObject), 
			"apothecary" => generateApothecary(gameObject), 
			_ => HeroMaker.MakeHero(gameObject), 
		};
		gameObject.RequirePart<Interesting>();
		gameObject.SetIntProperty("NamedVillager", 1);
		gameObject.SetIntProperty("ParticipantVillager", 1);
		gameObject.Render.DisplayName = name;
		if (!gender.IsNullOrEmpty())
		{
			gameObject.SetGender(gender);
		}
		if (role != "mayor" && role != "warden")
		{
			gameObject.RemovePart<GivesRep>();
		}
		if (role == "villager")
		{
			gameObject.SetIntProperty("SuppressSimpleConversation", 1);
		}
		AddVillagerConversation(gameObject, gameObject.GetTag("SimpleConversation", "Moon and Sun. Wisdom and will.~May the earth yield for us this season.~Peace, =player.formalAddressTerm=."), "Live and drink.", whyQ, whyA, role != "villager");
		return gameObject;
	}

	public GameObject generatePet(string species, out string name)
	{
		GameObject gameObject = ((species != null) ? GameObject.Create(species) : GameObject.Create(PopulationManager.RollOneFrom("DynamicInheritsTable:BaseAnimal:Tier" + villageTier).Blueprint));
		string text = NameMaker.MakeTitle(gameObject);
		name = gameObject.GiveProperName();
		if (!text.IsNullOrEmpty())
		{
			gameObject.RequirePart<Titles>().AddTitle(text, -5);
		}
		setVillagerProperties(gameObject);
		gameObject.RequirePart<SmartuseForceTwiddles>();
		gameObject.RemovePart<Pettable>();
		Pettable pettable = new Pettable();
		gameObject.AddPart(pettable);
		pettable.PettableIfPositiveFeeling = true;
		pettable.UseFactionForFeelingFloor = villageFaction;
		gameObject.SetIntProperty("VillagePet", 1);
		gameObject.RequirePart<Interesting>().Key = "VillagePet";
		ConversationsAPI.addSimpleConversationToObject(gameObject, gameObject.GetTag("SimpleConversation", "*does not react*"), "Live and drink.");
		return gameObject;
	}

	public List<PopulationResult> ResolveBuildingContents(List<PopulationResult> templateResults)
	{
		List<PopulationResult> list = new List<PopulationResult>(templateResults.Count);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (PopulationResult templateResult in templateResults)
		{
			for (int i = 0; i < templateResult.Number; i++)
			{
				if (!templateResult.Blueprint.StartsWith("*"))
				{
					list.Add(new PopulationResult(templateResult.Blueprint));
					continue;
				}
				if (staticVillageResults.ContainsKey(templateResult.Blueprint) && !Stat.Chance(20))
				{
					list.Add(new PopulationResult(staticVillageResults[templateResult.Blueprint]));
					continue;
				}
				if (dictionary.ContainsKey(templateResult.Blueprint) && !Stat.Chance(20))
				{
					list.Add(new PopulationResult(dictionary[templateResult.Blueprint]));
					continue;
				}
				PopulationResult populationResult = new PopulationResult(null);
				string populationName = "DynamicSemanticTable:" + templateResult.Blueprint.Replace("*", "") + "::" + villageTechTier;
				populationResult.Blueprint = PopulationManager.RollOneFrom(populationName).Blueprint;
				populationResult.Hint = templateResult.Hint;
				if (populationResult.Blueprint.IsNullOrEmpty())
				{
					Debug.LogError("Couldn't resolve object for " + templateResult.Blueprint);
					continue;
				}
				list.Add(populationResult);
				if (staticPerBuilding.Contains(templateResult.Blueprint) && !dictionary.ContainsKey(templateResult.Blueprint))
				{
					dictionary.Add(templateResult.Blueprint, populationResult.Blueprint);
				}
				if (staticPerVillage.Contains(templateResult.Blueprint) && !staticVillageResults.ContainsKey(templateResult.Blueprint))
				{
					staticVillageResults.Add(templateResult.Blueprint, populationResult.Blueprint);
				}
			}
		}
		return list;
	}

	public override void addInitialStructures()
	{
		List<ISultanDungeonSegment> list = new List<ISultanDungeonSegment>();
		int num = 7;
		int num2 = 72;
		int num3 = 7;
		int num4 = 17;
		string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_InitialStructureSegmentation"), null, "Full").Blueprint;
		if (blueprint == "None")
		{
			return;
		}
		string[] array = blueprint.Split(';');
		foreach (string text in array)
		{
			switch (text)
			{
			case "FullHMirror":
			{
				SultanRectDungeonSegment sultanRectDungeonSegment3 = new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22));
				sultanRectDungeonSegment3.mutator = "HMirror";
				list.Add(sultanRectDungeonSegment3);
				continue;
			}
			case "FullVMirror":
			{
				SultanRectDungeonSegment sultanRectDungeonSegment2 = new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22));
				sultanRectDungeonSegment2.mutator = "VMirror";
				list.Add(sultanRectDungeonSegment2);
				continue;
			}
			case "FullHVMirror":
			{
				SultanRectDungeonSegment sultanRectDungeonSegment = new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22));
				sultanRectDungeonSegment.mutator = "HVMirror";
				list.Add(sultanRectDungeonSegment);
				continue;
			}
			case "Full":
				list.Add(new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22)));
				continue;
			}
			if (text.StartsWith("BSP:"))
			{
				int nSegments = Convert.ToInt32(text.Split(':')[1]);
				partition(new Rect2D(2, 2, 78, 24), ref nSegments, list);
			}
			else if (text.StartsWith("Ring:"))
			{
				int num5 = Convert.ToInt32(text.Split(':')[1]);
				list.Add(new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22)));
				if (num5 == 2)
				{
					list.Add(new SultanRectDungeonSegment(new Rect2D(20, 8, 60, 16)));
				}
				if (num5 == 3)
				{
					list.Add(new SultanRectDungeonSegment(new Rect2D(15, 8, 65, 16)));
					list.Add(new SultanRectDungeonSegment(new Rect2D(25, 10, 55, 14)));
				}
			}
			else if (text.StartsWith("Blocks"))
			{
				string[] array2 = text.Split(':')[1].Split(',');
				int num6 = array2[0].RollCached();
				for (int j = 0; j < num6; j++)
				{
					int num7 = array2[1].RollCached();
					int num8 = array2[2].RollCached();
					int num9 = Stat.Random(2, 78 - num7);
					int num10 = Stat.Random(2, 23 - num8);
					int num11 = num9 + num7;
					int num12 = num10 + num8;
					if (num < num9)
					{
						num = num9;
					}
					if (num2 > num11)
					{
						num2 = num11;
					}
					if (num3 < num10)
					{
						num3 = num10;
					}
					if (num4 > num12)
					{
						num4 = num12;
					}
					SultanRectDungeonSegment sultanRectDungeonSegment4 = new SultanRectDungeonSegment(new Rect2D(num9, num10, num9 + num7, num10 + num8));
					if (text.Contains("[HMirror]"))
					{
						sultanRectDungeonSegment4.mutator = "HMirror";
					}
					if (text.Contains("[VMirror]"))
					{
						sultanRectDungeonSegment4.mutator = "VMirror";
					}
					if (text.Contains("[HVMirror]"))
					{
						sultanRectDungeonSegment4.mutator = "HVMirror";
					}
					list.Add(sultanRectDungeonSegment4);
				}
			}
			else if (text.StartsWith("Circle"))
			{
				string[] array3 = text.Split(':')[1].Split(',');
				list.Add(new SultanCircleDungeonSegment(Location2D.Get(array3[0].RollCached(), array3[1].RollCached()), array3[2].RollCached()));
			}
			else if (text.StartsWith("Tower"))
			{
				string[] array4 = text.Split(':')[1].Split(',');
				list.Add(new SultanTowerDungeonSegment(Location2D.Get(array4[0].RollCached(), array4[1].RollCached()), array4[2].RollCached(), array4[3].RollCached()));
			}
		}
		ColorOutputMap colorOutputMap = new ColorOutputMap(80, 25);
		for (int k = 0; k < list.Count; k++)
		{
			string text2 = "";
			text2 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_StructureTemplate")).Blueprint;
			int n = 3;
			string text3 = "";
			string text4 = "";
			text4 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_StructureTemplate")).Blueprint;
			int n2 = 3;
			if (text2.Contains(","))
			{
				string[] array5 = text2.Split(',');
				text2 = array5[0];
				text3 = array5[1];
			}
			WaveCollapseFastModel waveCollapseFastModel = new WaveCollapseFastModel(text2, n, list[k].width(), list[k].height(), periodicInput: true, periodicOutput: false, 8, 0);
			waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
			if (!text3.IsNullOrEmpty())
			{
				waveCollapseFastModel.ClearColors(text3);
			}
			waveCollapseFastModel.UpdateSample(text4.Split(',')[0], n2, periodicInput: true, periodicOutput: false, 8, 0);
			waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
			ColorOutputMap colorOutputMap2 = new ColorOutputMap(waveCollapseFastModel);
			colorOutputMap2.ReplaceBorders(new Color32(byte.MaxValue, 0, 0, byte.MaxValue), new Color32(0, 0, 0, byte.MaxValue));
			if (list[k].mutator == "HMirror")
			{
				colorOutputMap2.HMirror();
			}
			if (list[k].mutator == "VMirror")
			{
				colorOutputMap2.VMirror();
			}
			if (list[k].mutator == "HVMirror")
			{
				colorOutputMap2.HMirror();
				colorOutputMap2.VMirror();
			}
			colorOutputMap.Paste(colorOutputMap2, list[k].x1, list[k].y1);
			waveCollapseFastModel = null;
			MemoryHelper.GCCollect();
		}
		string text5 = RollOneFrom("Village_InitialStructureSegmentationFullscreenMutation");
		int num13 = 0;
		int num14 = 0;
		for (int l = 0; l < list.Count; l++)
		{
			string text6 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_StructureWall")).Blueprint;
			if (text6 == "*auto")
			{
				text6 = GetDefaultWall(zone);
			}
			for (int m = list[l].y1; m < list[l].y2; m++)
			{
				for (int num15 = list[l].x1; num15 < list[l].x2; num15++)
				{
					if (!list[l].contains(num15, m))
					{
						continue;
					}
					int num16 = l + 1;
					while (true)
					{
						if (num16 < list.Count)
						{
							if (list[num16].contains(num15, m))
							{
								break;
							}
							num16++;
							continue;
						}
						Color32 a = colorOutputMap.getPixel(num15, m);
						if (list[l].HasCustomColor(num15, m))
						{
							a = list[l].GetCustomColor(num15, m);
						}
						if (WaveCollapseTools.equals(a, ColorOutputMap.BLACK))
						{
							zone.GetCell(num15 + num13, m + num14).ClearWalls();
							zone.GetCell(num15 + num13, m + num14).AddObject(text6);
							if (text5 == "VMirror" || text5 == "HVMirror")
							{
								zone.GetCell(num15 + num13, zone.Height - (m + num14) - 1).ClearWalls();
								zone.GetCell(num15 + num13, zone.Height - (m + num14) - 1).AddObject(text6);
							}
							if (text5 == "HMirror" || text5 == "HVMirror")
							{
								zone.GetCell(zone.Width - (num15 + num13) - 1, m + num14).ClearWalls();
								zone.GetCell(zone.Width - (num15 + num13) - 1, m + num14).AddObject(text6);
							}
							if (text5 == "HVMirror")
							{
								zone.GetCell(zone.Width - (num15 + num13) - 1, zone.Height - (m + num14) - 1).ClearWalls();
								zone.GetCell(zone.Width - (num15 + num13) - 1, zone.Height - (m + num14) - 1).AddObject(text6);
							}
						}
						break;
					}
				}
			}
		}
	}

	public static void villageClear(Zone Z)
	{
		string tag = Z.GetTerrainObject().GetTag("VillageClearBehavior");
		if (tag.IsNullOrEmpty())
		{
			return;
		}
		string[] array = tag.Split(':');
		if (!(array[0] == "circles"))
		{
			return;
		}
		int num = int.Parse(array[1]);
		for (int i = 0; i < num; i++)
		{
			foreach (Cell item in Z.GetRandomCell().GetCellsInACosmeticCircle(Stat.Random(6, 10)))
			{
				Debug.Log("clearing " + item.X + "," + item.Y);
				item.Clear(null, Important: false, Combat: false, (GameObject o) => o.GetBlueprint().DescendsFrom("Widget"));
			}
		}
	}

	public bool BuildZone(Zone Z)
	{
		bool bDraw = false;
		zone = Z;
		zone.SetZoneProperty("relaxedbiomes", "true");
		zone.SetZoneProperty("faction", villageFaction);
		villageSnapshot = villageEntity.GetCurrentSnapshot();
		region = villageSnapshot.GetProperty("region");
		villagerBaseFaction = villageSnapshot.GetProperty("baseFaction");
		villageName = villageSnapshot.GetProperty("name");
		dynamicCreatureTableName = "DynamicObjectsTable:" + region + "_Creatures";
		Z.SetZoneProperty("villageEntityId", villageEntity.id);
		isVillageZero = villageSnapshot.GetProperty("isVillageZero", "false").EqualsNoCase("true");
		Tier.Constrain(ref villageTier);
		Tier.Constrain(ref villageTechTier);
		generateVillageTheme();
		generateSignatureItems();
		generateSignatureDish();
		generateSignatureLiquid();
		generateSignatureSkill();
		generateStoryType();
		getVillageDoorStyle();
		makeSureThereIsEnoughSpace();
		foreach (Cell cell3 in Z.GetCells())
		{
			for (int num = cell3.Objects.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = cell3.Objects[num];
				if (!gameObject.IsPlayer() && !gameObject.HasTagOrProperty("NoVillageStrip"))
				{
					if (gameObject.HasTagOrProperty("RequireVillagePlacement"))
					{
						gameObject.Physics.CurrentCell = null;
						requiredPlacementObjects.Add(gameObject);
					}
					else if (gameObject.IsCombat || gameObject.HasTagOrProperty("BodySubstitute"))
					{
						gameObject.Physics.CurrentCell = null;
						originalCreatures.Add(gameObject);
					}
					else if (gameObject.IsWall() && gameObject.HasTag("Category_Settlement"))
					{
						gameObject.Physics.CurrentCell = null;
						originalWalls.Add(gameObject);
					}
					else if (gameObject.GetBlueprint().InheritsFrom("Plant") || gameObject.GetBlueprint().InheritsFrom("BasePlant") || gameObject.GetBlueprint().HasTag("PlantLike"))
					{
						gameObject.Physics.CurrentCell = null;
						if (gameObject != null)
						{
							originalPlants.Add(gameObject);
						}
					}
					else if (gameObject.HasPart<LiquidVolume>())
					{
						gameObject.Physics.CurrentCell = null;
						if (gameObject.IsOpenLiquidVolume())
						{
							originalLiquids.Add(gameObject);
						}
					}
					else if (gameObject.GetBlueprint().InheritsFrom("Furniture"))
					{
						gameObject.Physics.CurrentCell = null;
						originalFurniture.Add(gameObject);
					}
					else if (gameObject.GetBlueprint().InheritsFrom("Item"))
					{
						gameObject.Physics.CurrentCell = null;
						originalItems.Add(gameObject);
					}
				}
			}
		}
		villageClear(Z);
		addInitialStructures();
		InfluenceMap regionMap = new InfluenceMap(Z.Width, Z.Height);
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				regionMap.Walls[i, j] = (Z.GetCell(i, j).HasObjectWithTagOrProperty("Wall") ? 1 : 0);
			}
		}
		try
		{
			regionMap.SeedAllUnseeded();
		}
		catch (Exception)
		{
			makeSureThereIsEnoughSpace();
			regionMap.SeedAllUnseeded();
		}
		while (regionMap.LargestSize() > 150)
		{
			regionMap.AddSeedAtMaximaInLargestSeed();
		}
		regionMap.SeedGrowthProbability = new List<int>();
		for (int k = 0; k < regionMap.Seeds.Count; k++)
		{
			regionMap.SeedGrowthProbability.Add(Stat.Random(10, 1000));
		}
		regionMap.Recalculate(bDraw);
		int num2 = Stat.Random(4, 9);
		int num3 = 0;
		int num4 = regionMap.FindClosestSeedTo(Location2D.Get(40, 13), (InfluenceMapRegion influenceMapRegion) => influenceMapRegion.maxRect.ReduceBy(1, 1).Width >= 6 && influenceMapRegion.maxRect.ReduceBy(1, 1).Height >= 6 && influenceMapRegion.AdjacentRegions.Count > 1);
		Location2D location2D = regionMap.Seeds[num4];
		townSquare = regionMap.Regions[num4];
		townSquareLayout = null;
		foreach (InfluenceMapRegion region in regionMap.Regions)
		{
			Rect2D Rect = GridTools.MaxRectByArea(region.GetGrid()).Translate(region.BoundingBox.UpperLeft).ReduceBy(1, 1);
			PopulationLayout populationLayout = new PopulationLayout(Z, region, Rect);
			if (region.AdjacentRegions.Count <= 1 && region.Size >= 9 && !region.IsEdgeRegion() && region != townSquare)
			{
				buildings.Add(populationLayout);
			}
			else if ((Rect.Width >= 5 && Rect.Height >= 5 && num2 > 0) || region == townSquare)
			{
				List<GameObject> list = originalLiquids;
				string liquidBlueprint = ((list == null) ? null : list.GetRandomElement()?.Blueprint) ?? getZoneDefaultLiquid(zone);
				if (region == townSquare)
				{
					townSquareLayout = populationLayout;
					if (fabricateStoryBuilding())
					{
						buildings.Add(populationLayout);
					}
					continue;
				}
				string text = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_BuildingStyle")).Blueprint;
				if (text.StartsWith("wfc,") && !getWfcBuildingTemplate(text.Split(',')[1]).Any((ColorOutputMap t) => t.extrawidth <= Rect.Width && t.extraheight <= Rect.Height))
				{
					text = "squarehut";
				}
				buildings.Add(populationLayout);
				if (text == "burrow")
				{
					FabricateBurrow(populationLayout);
					populationLayout.hasStructure = true;
				}
				if (text == "aerie")
				{
					FabricateAerie(populationLayout);
				}
				if (text == "pond")
				{
					FabricatePond(populationLayout, liquidBlueprint);
				}
				if (text == "islandpond")
				{
					FabricateIslandPond(populationLayout, liquidBlueprint);
					populationLayout.hasStructure = true;
				}
				if (text == "walledpond")
				{
					FabricateWalledPond(populationLayout, liquidBlueprint);
					populationLayout.hasStructure = true;
				}
				if (text == "walledislandpond")
				{
					FabricateWalledIslandPond(populationLayout, liquidBlueprint);
					populationLayout.hasStructure = true;
				}
				if (text == "tent")
				{
					FabricateTent(populationLayout);
					populationLayout.hasStructure = true;
				}
				if (text == "roundhut")
				{
					FabricateHut(populationLayout, isRound: true);
					populationLayout.hasStructure = true;
				}
				if (text == "squarehut")
				{
					FabricateHut(populationLayout, isRound: false);
					populationLayout.hasStructure = true;
				}
				if (text.StartsWith("wfc,"))
				{
					getWfcBuildingTemplate(text.Split(',')[1]).ShuffleInPlace();
					bool flag = false;
					foreach (ColorOutputMap item in getWfcBuildingTemplate(text.Split(',')[1]))
					{
						int num5 = item.width / 2;
						int num6 = item.height / 2;
						if (item.extrawidth > populationLayout.innerRect.Width || item.extraheight > populationLayout.innerRect.Height)
						{
							continue;
						}
						for (int num7 = 0; num7 < item.width; num7++)
						{
							for (int num8 = 0; num8 < item.height; num8++)
							{
								Cell cell = Z.GetCell(populationLayout.position.X - num5 + num7, populationLayout.position.Y - num6 + num8);
								if (cell != null)
								{
									if (ColorExtensionMethods.Equals(item.getPixel(num7, num8), ColorOutputMap.BLACK))
									{
										cell.AddObject(getAVillageWall());
									}
									else
									{
										ColorExtensionMethods.Equals(item.getPixel(num7, num8), ColorOutputMap.RED);
									}
								}
							}
						}
						populationLayout.hasStructure = true;
						flag = true;
						break;
					}
					if (!flag)
					{
						FabricateHut(populationLayout, isRound: false);
						populationLayout.hasStructure = true;
					}
				}
				num2--;
				num3++;
			}
			else if (region.AdjacentRegions.Count == 1 && !region.IsEdgeRegion() && townSquare != region)
			{
				VillageBase.MakeCaveBuilding(Z, regionMap, region);
				buildings.Add(populationLayout);
				populationLayout.hasStructure = true;
			}
		}
		placeStatues();
		regionMap.SeedAllUnseeded(bDraw);
		CarvePathwaysFromLocations(Z, bCarveDoors: true, regionMap, location2D);
		zone.ClearReachableMap(bValue: false);
		zone.BuildReachableMap(location2D.X, location2D.Y);
		SnakeToConnections(Location2D.Get(location2D.X, location2D.Y));
		clearDegenerateDoors();
		applyDoorFilters();
		for (int num9 = 0; num9 < Z.Width; num9++)
		{
			for (int num10 = 0; num10 < Z.Height; num10++)
			{
				regionMap.Walls[num9, num10] = (Z.GetCell(num9, num10).HasObjectWithTag("Wall") ? 1 : 0);
			}
		}
		List<Location2D> list2 = new List<Location2D>();
		foreach (PopulationLayout building2 in buildings)
		{
			Location2D position = building2.position;
			if (position != null)
			{
				list2.Add(position);
			}
		}
		regionMap.Recalculate(bDraw);
		InfluenceMap influenceMap = regionMap.copy();
		using (Pathfinder pathfinder = zone.getPathfinder())
		{
			NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 4, 80, 80, 6, 3, -3, 1, new List<NoiseMapNode>());
			for (int num11 = 0; num11 < zone.Width; num11++)
			{
				for (int num12 = 0; num12 < zone.Height; num12++)
				{
					if (zone.GetCell(num11, num12).HasWall())
					{
						pathfinder.CurrentNavigationMap[num11, num12] = 4999;
					}
					else
					{
						pathfinder.CurrentNavigationMap[num11, num12] = noiseMap.Noise[num11, num12];
					}
				}
			}
			foreach (PopulationLayout building3 in buildings)
			{
				foreach (Location2D cell4 in building3.originalRegion.Cells)
				{
					int x = cell4.X;
					int y = cell4.Y;
					if (x != 0 && x != 79 && y != 0 && y != 24 && Z.GetCell(x, y).IsEmpty())
					{
						int num13 = 0;
						int num14 = 0;
						if (Z.GetCell(x - 1, y).HasWall() || Z.GetCell(x - 1, y).HasObjectWithTag("Door"))
						{
							num14++;
						}
						if (Z.GetCell(x + 1, y).HasWall() || Z.GetCell(x + 1, y).HasObjectWithTag("Door"))
						{
							num14++;
						}
						if (Z.GetCell(x, y - 1).HasWall() || Z.GetCell(x, y - 1).HasObjectWithTag("Door"))
						{
							num13++;
						}
						if (Z.GetCell(x, y + 1).HasWall() || Z.GetCell(x, y + 1).HasObjectWithTag("Door"))
						{
							num13++;
						}
						if ((num13 == 2 && num14 == 0) || (num13 == 0 && num14 == 2))
						{
							influenceMap.Walls[x, y] = 1;
						}
					}
				}
			}
			for (int num15 = 0; num15 < 80; num15++)
			{
				for (int num16 = 0; num16 < 25; num16++)
				{
					if (burrowedDoors.Contains(Location2D.Get(num15, num16)))
					{
						influenceMap.Walls[num15, num16] = 1;
					}
				}
			}
			influenceMap.Recalculate(bDraw);
			foreach (PopulationLayout building4 in buildings)
			{
				string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_BuildingFloor")).Blueprint;
				string text2 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_BuildingPath")).Blueprint;
				if (text2 == "Pond")
				{
					text2 = getZoneDefaultLiquid(zone);
				}
				if (pathfinder.FindPath(building4.position, location2D, Display: false, CardinalDirectionsOnly: true))
				{
					foreach (PathfinderNode step in pathfinder.Steps)
					{
						if (!text2.IsNullOrEmpty())
						{
							zone.GetCell(step.pos).AddObject(text2);
						}
						if (!buildingPaths.Contains(step.pos))
						{
							buildingPaths.Add(step.pos);
						}
					}
				}
				foreach (Location2D cell5 in building4.originalRegion.Cells)
				{
					if (Z.GetCell(cell5).HasWall() || buildingPaths.Contains(cell5))
					{
						continue;
					}
					if (Enumerable.Count(influenceMap.Regions) <= building4.region.Seed)
					{
						building4.outside.Add(cell5);
						int num17 = Z.GetCell(cell5).CountObjectWithTagCardinalAdjacent("Wall");
						if (num17 > 0)
						{
							building4.outsideWall.Add(cell5);
						}
						if (num17 >= 2)
						{
							building4.outsideCorner.Add(cell5);
						}
						continue;
					}
					if (!influenceMap.Regions[building4.region.Seed].Cells.Contains(cell5))
					{
						building4.outside.Add(cell5);
						int num18 = Z.GetCell(cell5).CountObjectWithTagCardinalAdjacent("Wall");
						if (num18 > 0)
						{
							building4.outsideWall.Add(cell5);
						}
						if (num18 >= 2)
						{
							building4.outsideCorner.Add(cell5);
						}
						continue;
					}
					building4.inside.Add(cell5);
					if (!blueprint.IsNullOrEmpty())
					{
						Z.GetCell(cell5).AddObject(blueprint);
					}
					int num19 = Z.GetCell(cell5).CountObjectWithTagCardinalAdjacent("Wall");
					if (num19 > 0)
					{
						building4.insideWall.Add(cell5);
					}
					if (num19 >= 2)
					{
						building4.insideCorner.Add(cell5);
					}
				}
			}
		}
		Dictionary<InfluenceMapRegion, Rect2D> dictionary = new Dictionary<InfluenceMapRegion, Rect2D>();
		Dictionary<InfluenceMapRegion, string> dictionary2 = new Dictionary<InfluenceMapRegion, string>();
		InfluenceMap influenceMap2 = new InfluenceMap(Z.Width, Z.Height);
		influenceMap2.Seeds = new List<Location2D>(regionMap.Seeds);
		Z.SetInfluenceMapWalls(influenceMap2.Walls);
		influenceMap2.Recalculate();
		int num20 = 0;
		for (int num21 = 0; num21 < influenceMap2.Regions.Count; num21++)
		{
			InfluenceMapRegion R = influenceMap2.Regions[num21];
			Rect2D value;
			if (!dictionary.ContainsKey(R))
			{
				value = GridTools.MaxRectByArea(R.GetGrid()).Translate(R.BoundingBox.UpperLeft);
				dictionary.Add(R, value);
			}
			else
			{
				value = dictionary[R];
			}
			if (num21 == num4)
			{
				continue;
			}
			if (list2.Contains(regionMap.Seeds[R.Seed]))
			{
				dictionary2.Add(R, "building");
				PopulationLayout building = buildings.First((PopulationLayout b) => b.position == regionMap.Seeds[R.Seed]);
				string text3 = RollOneFrom("Villages_BuildingTheme_" + villageTheme);
				foreach (PopulationResult item2 in ResolveBuildingContents(PopulationManager.Generate(ResolvePopulationTableName("Villages_BuildingContents_Dwelling_" + text3))))
				{
					PlaceObjectInBuilding(item2, building);
				}
			}
			else if (value.Area >= 4)
			{
				dictionary2.Add(R, "greenspace");
				if (num20 == 0 && signatureHistoricObjectInstance != null)
				{
					string wallObject = "IronFence";
					string blueprint2 = "Iron Gate";
					Z.GetCell(value.Center).AddObject(signatureHistoricObjectInstance);
					ZoneBuilderSandbox.encloseRectWithWall(zone, new Rect2D(value.Center.x - 1, value.Center.y - 1, value.Center.x + 1, value.Center.y + 1), wallObject);
					Z.GetCell(value.Center).GetCellFromDirection(Directions.GetRandomCardinalDirection()).Clear()
						.AddObject(blueprint2);
				}
				else
				{
					string blueprint3 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_GreenspaceContents")).Blueprint;
					int num22 = 20;
					if (blueprint3 == "aquaculture")
					{
						List<GameObject> list3 = originalLiquids;
						string blueprint4 = ((list3 == null) ? null : list3.GetRandomElement()?.Blueprint) ?? getZoneDefaultLiquid(zone);
						GameObject aFarmablePlant = getAFarmablePlant();
						Maze maze = RecursiveBacktrackerMaze.Generate(Math.Max(1, R.BoundingBox.Width / 3 + 1), Math.Max(1, R.BoundingBox.Height / 3 + 1), bShow: false, ZoneBuilderSandbox.GetOracleIntFromString("aquaculture" + num20, 0, 2147483646));
						for (int num23 = R.BoundingBox.x1; num23 <= R.BoundingBox.x2; num23++)
						{
							for (int num24 = R.BoundingBox.y1; num24 <= R.BoundingBox.y2; num24++)
							{
								int num25 = (num23 - R.BoundingBox.x1) / 3;
								int num26 = (num24 - R.BoundingBox.y1) / 3;
								int num27 = (num23 - R.BoundingBox.x1) % 3;
								int num28 = (num24 - R.BoundingBox.y1) % 3;
								bool flag2 = false;
								if (num27 == 1 && num28 == 1)
								{
									flag2 = maze.Cell[num25, num26].AnyOpen();
								}
								if (num27 == 1 && num28 == 0)
								{
									flag2 = maze.Cell[num25, num26].N;
								}
								if (num27 == 1 && num28 == 2)
								{
									flag2 = maze.Cell[num25, num26].S;
								}
								if (num27 == 2 && num28 == 1)
								{
									flag2 = maze.Cell[num25, num26].E;
								}
								if (num27 == 0 && num28 == 1)
								{
									flag2 = maze.Cell[num25, num26].W;
								}
								if (flag2)
								{
									if (R.Cells.Contains(Location2D.Get(num23, num24)) && !buildingPaths.Contains(Location2D.Get(num23, num24)))
									{
										Z.GetCell(num23, num24)?.AddObject(aFarmablePlant.Blueprint, setVillageDomesticatedProperties);
									}
								}
								else if (R.Cells.Contains(Location2D.Get(num23, num24)) && Z.GetCell(num23, num24) != null)
								{
									Z.GetCell(num23, num24).AddObject(blueprint4);
								}
							}
						}
					}
					else if (blueprint3 == "farm" && value.Area >= num22 && value.Width >= 7 && value.Height <= 7)
					{
						value = value.ReduceBy(1, 1).Clamp(1, 1, 78, 23);
						if (value.Width <= 6 || value.Height <= 6)
						{
							continue;
						}
						Location2D location = value.GetRandomDoorCell().location;
						ZoneBuilderSandbox.PlaceObjectOnRect(Z, "BrinestalkFence", value);
						GetCell(Z, location).Clear();
						GetCell(Z, location).AddObject("Brinestalk Gate");
						string cellSide = value.GetCellSide(location.Point);
						Rect2D r = value.ReduceBy(0, 0);
						int num29 = 0;
						if (cellSide == "N")
						{
							num29 = ((Stat.Random(0, 1) == 0) ? 2 : 3);
						}
						if (cellSide == "S")
						{
							num29 = ((Stat.Random(0, 1) != 0) ? 1 : 0);
						}
						if (cellSide == "E")
						{
							num29 = ((Stat.Random(0, 1) != 0) ? 3 : 0);
						}
						if (cellSide == "W")
						{
							num29 = ((Stat.Random(0, 1) == 0) ? 1 : 2);
						}
						if (num29 == 0 || num29 == 1)
						{
							r.y2 = r.y1 + 3;
						}
						else
						{
							r.y1 = r.y2 - 3;
						}
						if (num29 == 0 || num29 == 3)
						{
							r.x2 = r.x1 + 3;
						}
						else
						{
							r.x1 = r.x2 - 3;
						}
						ClearRect(Z, r);
						ZoneBuilderSandbox.PlaceObjectOnRect(Z, getAVillageWall(), r);
						Location2D location2 = r.GetRandomDoorCell(cellSide, 1).location;
						Z.GetCell(location2).Clear();
						Z.GetCell(location2).AddObject(getAVillageDoor());
						burrowedDoors.Add(Location2D.Get(location2.X, location2.Y));
						ZoneBuilderSandbox.PlacePopulationInRect(Z, value.ReduceBy(1, 1), ResolvePopulationTableName("Villages_FarmAnimals"), setVillageDomesticatedProperties);
						ZoneBuilderSandbox.PlacePopulationInRect(Z, r.ReduceBy(1, 1), ResolvePopulationTableName("Villages_FarmHutContents"));
					}
					else if (blueprint3 == "garden" || blueprint3 == "farm")
					{
						int num30 = Stat.Random(1, 4);
						GameObject aFarmablePlant2 = getAFarmablePlant();
						List<GameObject> list4 = originalLiquids;
						string blueprint5 = ((list4 == null) ? null : list4.GetRandomElement()?.Blueprint) ?? getZoneDefaultLiquid(zone);
						if (num30 == 1)
						{
							bool flag3 = Stat.Random(1, 100) <= 33;
							for (int num31 = R.BoundingBox.x1; num31 <= R.BoundingBox.x2; num31++)
							{
								for (int num32 = R.BoundingBox.y1; num32 <= R.BoundingBox.y2; num32++)
								{
									if (num31 % 2 == 0)
									{
										if (R.Cells.Contains(Location2D.Get(num31, num32)) && !buildingPaths.Contains(Location2D.Get(num31, num32)))
										{
											Z.GetCell(num31, num32)?.AddObject(aFarmablePlant2.Blueprint, setVillageDomesticatedProperties);
										}
									}
									else if (flag3 && R.Cells.Contains(Location2D.Get(num31, num32)) && Z.GetCell(num31, num32) != null)
									{
										Z.GetCell(num31, num32).AddObject(blueprint5);
									}
								}
							}
						}
						if (num30 == 2)
						{
							List<GameObject> list5 = originalLiquids;
							string blueprint6 = ((list5 == null) ? null : list5.GetRandomElement()?.Blueprint) ?? getZoneDefaultLiquid(zone);
							bool flag4 = Stat.Random(1, 100) <= 33;
							for (int num33 = R.BoundingBox.x1; num33 <= R.BoundingBox.x2; num33++)
							{
								for (int num34 = R.BoundingBox.y1; num34 <= R.BoundingBox.y2; num34++)
								{
									if (num34 % 2 == 0)
									{
										if (R.Cells.Contains(Location2D.Get(num33, num34)) && !buildingPaths.Contains(Location2D.Get(num33, num34)))
										{
											Z.GetCell(num33, num34)?.AddObject(aFarmablePlant2.Blueprint, setVillageDomesticatedProperties);
										}
									}
									else if (flag4 && R.Cells.Contains(Location2D.Get(num33, num34)) && Z.GetCell(num33, num34) != null)
									{
										Z.GetCell(num33, num34).AddObject(blueprint6);
									}
								}
							}
						}
						if (num30 == 3)
						{
							int num35 = Stat.Random(20, 98);
							for (int num36 = R.BoundingBox.x1; num36 <= R.BoundingBox.x2; num36++)
							{
								for (int num37 = R.BoundingBox.y1; num37 <= R.BoundingBox.y2; num37++)
								{
									if (R.Cells.Contains(Location2D.Get(num36, num37)) && !buildingPaths.Contains(Location2D.Get(num36, num37)) && Stat.Random(1, 100) <= num35)
									{
										Z.GetCell(num36, num37)?.AddObject(aFarmablePlant2.Blueprint, setVillageDomesticatedProperties);
									}
								}
							}
						}
						if (num30 == 4)
						{
							int num38 = Stat.Random(20, 98);
							for (int num39 = R.BoundingBox.x1; num39 <= R.BoundingBox.x2; num39++)
							{
								for (int num40 = R.BoundingBox.y1; num40 <= R.BoundingBox.y2; num40++)
								{
									if (R.Cells.Contains(Location2D.Get(num39, num40)) && !buildingPaths.Contains(Location2D.Get(num39, num40)) && Stat.Random(1, 100) <= num38)
									{
										Z.GetCell(num39, num40)?.AddObject(getAFarmablePlant());
									}
								}
							}
						}
					}
				}
				num20++;
			}
			else if (influenceMap2.SeedToRegionMap[R.Seed].AdjacentRegions.Count == 1)
			{
				dictionary2.Add(R, "cubby");
			}
			else
			{
				dictionary2.Add(R, "hall");
			}
		}
		placeNonTakeableSignatureItems();
		buildings.RemoveAll((PopulationLayout b) => b.inside.Count == 0 && b.outside.Count == 0);
		GameObject gameObject2 = generateVillageOven();
		PlaceObjectInBuilding(gameObject2, buildings.GetRandomElement(), If.OneIn(10) ? "Outside" : "Inside", (Location2D l) => !zone.GetCell(l).HasOpenLiquidVolume() && !zone.GetCell(l).MightBlockPaths());
		if (gameObject2 != null && gameObject2.CurrentCell != null)
		{
			gameObject2.CurrentCell.RemoveObjects((GameObject o) => o.IsOpenLiquidVolume());
		}
		if (villageSnapshot.GetProperty("abandoned") != "true")
		{
			GameObject gameObject3 = null;
			GameObject gameObject4 = null;
			GameObject gameObject5 = null;
			GameObject gameObject6 = null;
			GameObject gameObject7 = null;
			if (villageSnapshot.listProperties.ContainsKey("immigrant_type"))
			{
				List<string> list6 = villageSnapshot.listProperties["immigrant_type"];
				List<string> list7 = villageSnapshot.listProperties["immigrant_name"];
				List<string> list8 = villageSnapshot.listProperties["immigrant_gender"];
				List<string> list9 = villageSnapshot.listProperties["immigrant_role"];
				List<string> list10 = villageSnapshot.listProperties["immigrant_dialogWhy_Q"];
				List<string> list11 = villageSnapshot.listProperties["immigrant_dialogWhy_A"];
				for (int num41 = 0; num41 < list6.Count; num41++)
				{
					string text4 = list6[num41];
					string name;
					if (num41 >= list7.Count)
					{
						Debug.LogWarning("missing immigrant name for " + text4 + " in position " + num41);
						name = "MISSING_NAME";
					}
					else
					{
						name = list7[num41];
					}
					string gender;
					if (num41 >= list8.Count)
					{
						Debug.LogWarning("missing immigrant gender for " + text4 + " in position " + num41);
						gender = null;
					}
					else
					{
						gender = list8[num41];
					}
					string text5;
					if (num41 >= list9.Count)
					{
						Debug.LogWarning("missing immigrant role for " + text4 + " in position " + num41);
						text5 = "villager";
					}
					else
					{
						text5 = list9[num41];
					}
					string whyQ;
					if (num41 >= list10.Count)
					{
						Debug.LogWarning("missing immigrant dialog why Q for " + text4 + " in position " + num41);
						whyQ = "MISSING_QUESTION";
					}
					else
					{
						whyQ = list10[num41];
					}
					string whyA;
					if (num41 >= list11.Count)
					{
						Debug.LogWarning("missing immigrant dialog why A for " + text4 + " in position " + num41);
						whyA = "MISSING_ANSWER";
					}
					else
					{
						whyA = list11[num41];
					}
					try
					{
						GameObject gameObject8 = generateImmigrant(text4, name, gender, text5, whyQ, whyA);
						switch (text5)
						{
						case "mayor":
							gameObject3 = gameObject8;
							break;
						case "merchant":
							gameObject4 = gameObject8;
							break;
						case "tinker":
							gameObject5 = gameObject8;
							break;
						case "apothecary":
							gameObject6 = gameObject8;
							break;
						case "warden":
							gameObject7 = gameObject8;
							break;
						default:
							ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), gameObject8);
							break;
						}
					}
					catch (Exception x2)
					{
						MetricsManager.LogException("Failed to generate immigrant.", x2);
					}
				}
			}
			if (villageSnapshot.GetProperty("government") != "anarchism")
			{
				GameObject baseObject = null;
				if (villageSnapshot.GetProperty("government") == "colonialism")
				{
					baseObject = GameObject.Create(villageSnapshot.GetProperty("colonistType"));
				}
				if (gameObject7 != null)
				{
					ZoneBuilderSandbox.PlaceObject(Z, townSquare, gameObject7);
				}
				else
				{
					ZoneBuilderSandbox.PlaceObject(Z, townSquare, generateWarden(baseObject, isVillageZero));
				}
			}
			GameObject gameObject9 = null;
			if (villageSnapshot.GetProperty("government") == "colonialism")
			{
				gameObject9 = GameObject.Create(villageSnapshot.GetProperty("colonistType"));
				setVillagerProperties(gameObject9);
			}
			if (gameObject3 != null)
			{
				PlaceObjectInBuilding(gameObject3, buildings[0], If.OneIn(100) ? "Outside" : "Inside");
			}
			else
			{
				PlaceObjectInBuilding(generateMayor(gameObject9, "SpecialVillagerHeroTemplate_" + mayorTemplate), buildings[0], If.OneIn(100) ? "Outside" : "Inside");
			}
			if (gameObject4 != null)
			{
				PlaceObjectInBuilding(gameObject4, buildings[(buildings.Count >= 2) ? 1 : 0], If.OneIn(100) ? "Outside" : "Inside");
			}
			else if (isVillageZero || If.Chance(30))
			{
				PlaceObjectInBuilding(generateMerchant(null), buildings[(buildings.Count >= 2) ? 1 : 0], If.OneIn(100) ? "Outside" : "Inside");
			}
			if (isVillageZero)
			{
				if (base.region == "Saltmarsh")
				{
					ZoneBuilderSandbox.PlaceObject(Z, townSquare, GameObject.Create("WatervineFarmerConvert"));
				}
				else if (base.region == "DesertCanyon")
				{
					ZoneBuilderSandbox.PlaceObject(Z, townSquare, GameObject.Create("PigFarmerConvert"));
				}
				else if (base.region == "Hills")
				{
					ZoneBuilderSandbox.PlaceObject(Z, townSquare, GameObject.Create("CannibalConvert"));
				}
				else if (base.region == "Saltdunes")
				{
					ZoneBuilderSandbox.PlaceObject(Z, townSquare, GameObject.Create("IssachariConvert"));
				}
				else
				{
					ZoneBuilderSandbox.PlaceObject(Z, townSquare, GameObject.Create("WatervineFarmerConvert"));
				}
			}
			GameObject gameObject10 = null;
			bool flag5 = false;
			if (gameObject5 != null)
			{
				gameObject10 = gameObject5;
			}
			else if (isVillageZero || If.Chance(25))
			{
				gameObject10 = generateTinker();
				flag5 = true;
			}
			if (gameObject10 != null)
			{
				int index;
				string hint;
				if (buildings.Count >= 3)
				{
					index = 2;
					hint = (If.OneIn(50) ? "Outside" : "AlongInsideWall");
				}
				else if (buildings.Count >= 2)
				{
					index = 1;
					hint = (If.OneIn(50) ? "Outside" : "AlongInsideWall");
				}
				else
				{
					index = 0;
					hint = (If.OneIn(50) ? "Outside" : "AlongInsideWall");
				}
				PlaceObjectInBuilding(gameObject10, buildings[index], hint);
				int num42 = 0;
				for (int num43 = Stat.Random(2, 3); num42 < num43; num42++)
				{
					PlaceObjectInBuilding(GameObject.Create("Workbench"), buildings[index], hint);
				}
				int num44 = 0;
				for (int num45 = Stat.Random(0, 2); num44 < num45; num44++)
				{
					PlaceObjectInBuilding(GameObject.Create("Table"), buildings[index], hint);
				}
			}
			GameObject gameObject11 = null;
			bool flag6 = false;
			if (gameObject6 != null)
			{
				gameObject11 = gameObject6;
			}
			else if (isVillageZero || If.Chance(25))
			{
				gameObject11 = generateApothecary();
				flag6 = true;
			}
			if (gameObject11 != null)
			{
				int index2;
				if (buildings.Count >= 4)
				{
					index2 = 3;
				}
				else
				{
					index2 = ((buildings.Count >= 3) ? 2 : ((buildings.Count >= 2) ? 1 : 0));
				}
				string hint2 = (If.OneIn(50) ? "Outside" : "AlongInsideWall");
				PlaceObjectInBuilding(gameObject11, buildings[index2], hint2);
				int num46 = 0;
				for (int num47 = Stat.Random(1, 2); num46 < num47; num46++)
				{
					PlaceObjectInBuilding(GameObject.Create("Table"), buildings[index2], hint2);
				}
				int num48 = 0;
				for (int num49 = Stat.Random(0, 1); num48 < num49; num48++)
				{
					PlaceObjectInBuilding(GameObject.Create("Alchemist Table"), buildings[index2], hint2);
				}
				int num50 = 0;
				for (int num51 = Stat.Random(2, 3); num50 < num51; num50++)
				{
					PlaceObjectInBuilding(GameObject.Create("Woven Basket"), buildings[index2], hint2);
				}
			}
			int num52 = Stat.Random(4, 10);
			if (!isVillageZero)
			{
				if (!flag5)
				{
					num52++;
				}
				if (!flag6)
				{
					num52++;
				}
			}
			if (villageSnapshot.listProperties.ContainsKey("populationMultiplier"))
			{
				foreach (string item3 in villageSnapshot.listProperties["populationMultiplier"])
				{
					num52 *= int.Parse(item3);
				}
			}
			for (int num53 = 0; num53 < num52; num53++)
			{
				ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), generateVillager());
			}
			if (villageSnapshot.GetProperty("government") == "colonialism")
			{
				int num54 = 0;
				for (int num55 = Stat.Random(2, 3); num54 < num55; num54++)
				{
					GameObject gameObject12 = GameObject.Create(villageSnapshot.GetProperty("colonistType"));
					setVillagerProperties(gameObject12);
					gameObject12.SetIntProperty("SuppressSimpleConversation", 1);
					AddVillagerConversation(gameObject12, gameObject12.GetTag("SimpleConversation", "Moon and Sun. Wisdom and will.~May the earth yield for us this season.~Peace, =player.formalAddressTerm=."));
					ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), gameObject12);
				}
			}
			if (villageSnapshot.GetProperty("government") == "representative democracy")
			{
				int num56 = 0;
				for (int num57 = Stat.Random(2, 4); num56 < num57; num56++)
				{
					ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), generateMayor(null, "SpecialVillagerHeroTemplate_" + mayorTemplate, GivesRep: false));
				}
			}
			ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), generateVillager(bUnique: true));
		}
		if (villageSnapshot.listProperties.ContainsKey("pet_petSpecies"))
		{
			for (int x3 = 0; x3 < villageSnapshot.listProperties["pet_petSpecies"].Count; x3++)
			{
				try
				{
					List<string> petNames = new List<string>();
					int num58 = int.Parse(villageSnapshot.listProperties["pet_number"][x3]);
					for (int num59 = 0; num59 < num58; num59++)
					{
						GameObject obj = generatePet(villageSnapshot.listProperties["pet_petSpecies"][x3], out var name2);
						ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), obj);
						petNames.Add(name2);
					}
					GameObject petSample = GameObject.Create(villageSnapshot.listProperties["pet_petSpecies"][x3]);
					zone.ForeachObjectWithTagOrProperty("Villager", (GameObject o) =>
					{
						string text6 = HistoricStringExpander.ExpandString("<spice.villages.pet.originStory.!random>", null, null, QudHistoryHelpers.BuildContextFromObjectTextFragments(villageSnapshot.listProperties["pet_petSpecies"][x3]));
						text6 = ((int.Parse(villageSnapshot.listProperties["pet_number"][x3]) != 1) ? text6.Replace("@them@", "them").Replace("@they@", "they").Replace("@they're@", "they're")
							.Replace("@they've@", "they've")
							.Replace("@their@", "their")
							.Replace("@Them@", "Them")
							.Replace("@They@", "They")
							.Replace("@They're@", "They're")
							.Replace("@They've@", "They've")
							.Replace("@Their@", "Their")
							.Replace("@has@", "have")
							.Replace("@Name@", Grammar.MakeAndList(petNames)) : text6.Replace("@them@", petSample.them).Replace("@they@", petSample.it).Replace("@they're@", petSample.itis)
							.Replace("@they've@", petSample.ithas)
							.Replace("@their@", petSample.its)
							.Replace("@Them@", petSample.Them)
							.Replace("@They@", petSample.It)
							.Replace("@They're@", petSample.Itis)
							.Replace("@They've@", petSample.Ithas)
							.Replace("@Their@", petSample.Its)
							.Replace("@has@", "has")
							.Replace("@Name@", petNames[0]));
						if (!o.HasTagOrProperty("VillagePet"))
						{
							AddVillagerConversation(o, o.GetTag("SimpleConversation", "Moon and Sun. Wisdom and will.~May the earth yield for us this season.~Peace, =player.formalAddressTerm=."), "Live and drink.", villageSnapshot.listProperties["pet_dialogWhy_Q"][x3], text6, AppendConversation: true);
						}
					});
				}
				catch (Exception x4)
				{
					MetricsManager.LogException("Failed to generate pet.", x4);
				}
			}
		}
		Z.ForeachObjectWithPart("Brain", (GameObject gameObject14) =>
		{
			AllegianceSet allegianceSet = gameObject14.PartyLeader?.Brain.Allegiance;
			if (allegianceSet != null && allegianceSet.TryGetValue(villageFaction, out var Value))
			{
				gameObject14.Brain.Allegiance.TryAdd(villageFaction, Value);
			}
		});
		Z.ForeachObjectWithPart("SecretObject", (GameObject gameObject14) =>
		{
			gameObject14.RemovePart<SecretObject>();
		});
		placeStories();
		Z.ForeachObject((GameObject o) =>
		{
			if ((o.GetBlueprint().HasTag("Furniture") || o.GetBlueprint().HasTag("Vessel")) && o.Physics != null)
			{
				o.Physics.Owner = villageFaction;
			}
			if (villageSnapshot.listProperties.ContainsKey("signatureLiquids") && o.GetBlueprint().HasTag("Vessel") && If.Chance(80))
			{
				LiquidVolume liquidVolume = o.LiquidVolume;
				if (liquidVolume != null)
				{
					liquidVolume.InitialLiquid = villageSnapshot.GetList("signatureLiquids").GetRandomElement();
				}
			}
			if (o.HasStringProperty("GivesDynamicQuest") && o.Brain != null)
			{
				o.Brain.Wanders = false;
				o.Brain.WandersRandomly = false;
			}
		});
		if (villageSnapshot.GetProperty("abandoned") == "true")
		{
			int num60 = 1;
			try
			{
				num60 = Convert.ToInt32(villageSnapshot.GetProperty("ruinScale"));
				if (num60 < 1)
				{
					num60 = 1;
				}
				if (num60 > 4)
				{
					num60 = 4;
				}
			}
			catch (Exception ex2)
			{
				Logger.Exception(ex2);
			}
			if (num60 > 1)
			{
				int ruinLevel = 10;
				if (num60 == 3)
				{
					ruinLevel = 50;
				}
				if (num60 == 4)
				{
					ruinLevel = 100;
				}
				new Ruiner().RuinZone(Z, ruinLevel, bUnderground: false);
				foreach (GameObject originalPlant in originalPlants)
				{
					ZoneBuilderSandbox.PlaceObject(originalPlant, Z);
				}
			}
			if (If.Chance(70))
			{
				foreach (GameObject originalCreature in originalCreatures)
				{
					ZoneBuilderSandbox.PlaceObject(originalCreature, Z);
				}
			}
			Z.ReplaceAll("Torchpost", "Unlit Torchpost");
			Z.ReplaceAll("Sconce", "Unlit Torchpost");
		}
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z);
		if (Z.HasBuilder("RiverBuilder"))
		{
			List<GameObject> list12 = originalLiquids;
			new RiverBuilder(hardClear: false, ((list12 == null) ? null : list12.GetRandomElement()?.Blueprint) ?? getZoneDefaultLiquid(Z), VillageMode: true).BuildZone(Z);
		}
		if (Z.HasBuilder("RoadBuilder"))
		{
			new RoadBuilder(HardClear: false).BuildZone(Z);
		}
		foreach (GameObject requiredPlacementObject in requiredPlacementObjects)
		{
			if (requiredPlacementObject.IsCombat)
			{
				setVillagerProperties(requiredPlacementObject);
			}
			ZoneBuilderSandbox.PlaceObject(requiredPlacementObject, zone);
		}
		string damageChance = ((villageSnapshot.GetProperty("abandoned") == "true") ? Stat.Random(5, 25).ToString() : (10 - villageTechTier).ToString());
		PowerGrid powerGrid = new PowerGrid();
		powerGrid.DamageChance = damageChance;
		if ((10 + villageTechTier * 3).in100())
		{
			powerGrid.MissingConsumers = "1d6";
			powerGrid.MissingProducers = "1d3";
		}
		powerGrid.BuildZone(Z);
		Hydraulics hydraulics = new Hydraulics();
		hydraulics.DamageChance = damageChance;
		if ((10 + villageTechTier * 3).in100())
		{
			hydraulics.MissingConsumers = "1d6";
			hydraulics.MissingProducers = "1d3";
		}
		hydraulics.BuildZone(Z);
		MechanicalPower mechanicalPower = new MechanicalPower();
		mechanicalPower.DamageChance = damageChance;
		if ((20 - villageTechTier).in100())
		{
			mechanicalPower.MissingConsumers = "1d6";
			mechanicalPower.MissingProducers = "1d3";
		}
		mechanicalPower.BuildZone(Z);
		Z.SetMusic("Music/Mehmets Book on Strings");
		Z.FireEvent("VillageInit");
		cleanup();
		new IsCheckpoint().BuildZoneWithKey(Z, villageName);
		Cell cell2 = Z.GetCell(0, 0);
		if (SurfaceRevealer && !cell2.HasObject("VillageSurface") && villageEntity != null)
		{
			GameObject gameObject13 = GameObject.Create("VillageSurface");
			if (gameObject13.TryGetPart<VillageSurface>(out var Part))
			{
				Cell worldCell = Z.GetWorldCell();
				Part.VillageName = villageName;
				Part.RevealKey = "villageReveal_" + villageName;
				Part.RevealLocation = new Vector2i(worldCell.X, worldCell.Y);
				Part.RevealSecret = villageEntity.id;
				Part.IsVillageZero = villageEntity.GetEntityProperty("isVillageZero", -1L).EqualsNoCase("true");
				if (villageEntity.GetEntityProperty("abandoned", -1L).EqualsNoCase("true"))
				{
					Part.RevealString = "You discover the abandoned village of " + villageName + ".";
				}
				else
				{
					Part.RevealString = "You discover the village of " + villageName + ".";
				}
				cell2.AddObject(gameObject13);
			}
		}
		return true;
	}
}
