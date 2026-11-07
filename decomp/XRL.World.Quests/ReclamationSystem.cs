using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Language;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Quests.GolemQuest;
using XRL.World.ZoneBuilders;

namespace XRL.World.Quests;

[Serializable]
[HasWishCommand]
public class ReclamationSystem : IQuestSystem
{
	public GlobalLocation NephalLocation = new GlobalLocation();

	public List<string> Displaced = new List<string>(16);

	public long LeftTick = long.MaxValue;

	public int NextAttempt;

	public int Attempt;

	public int Stage;

	public int Timer;

	public bool Active;

	[NonSerialized]
	private int RechargeTimer;

	[NonSerialized]
	private HashSet<string> _Perimeter;

	[NonSerialized]
	private Zone CurrentZone;

	[NonSerialized]
	private Dictionary<string, List<Location2D>> Clusters;

	public bool Super
	{
		get
		{
			return Quest.GetProperty("Super", 0) == 1;
		}
		set
		{
			Quest.SetProperty("Super", value ? 1 : 0);
		}
	}

	public HashSet<string> Perimeter
	{
		get
		{
			if (_Perimeter == null)
			{
				_Perimeter = new HashSet<string>(25);
				foreach (Location2D item in Zone.zoneIDTo240x72Location("JoppaWorld.53.3.1.1.10").YieldPerimeter(2))
				{
					_Perimeter.Add(ZoneID.Assemble("JoppaWorld", item));
				}
			}
			return _Perimeter;
		}
	}

	public override bool RemoveWithQuest => false;

	public override void Start()
	{
		NextAttempt++;
		Timer = Stat.Roll(GetProperty("ArrivalTimer"));
		CreateWarleaders();
	}

	public override void FinishStep(QuestStep Step)
	{
		if (Stage == 0 && Step.ID.StartsWith("Warleader") && WarleaderSteps().All((QuestStep x) => x.Finished))
		{
			Timer = 0;
		}
	}

	public override void Finish()
	{
		if (Stage < 2)
		{
			Stage = 2;
		}
		The.ActiveZone.PlayMusic();
		CheckpointingSystem.ManualCheckpoint();
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(ZoneActivatedEvent.ID);
		Registrar.Register(SingletonEvent<EndTurnEvent>.ID);
		Registrar.Register(SingletonEvent<BeforePlayMusicEvent>.ID);
	}

	public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
	{
		Registrar.Register(EnteringZoneEvent.ID);
	}

	public override bool HandleEvent(BeforePlayMusicEvent E)
	{
		if (!Active || Quest.Finished || E.Channel != "music")
		{
			return true;
		}
		Zone activeZone = The.ActiveZone;
		if (activeZone == null || !Perimeter.Contains(activeZone.ZoneID))
		{
			return true;
		}
		return E.Track == "Music/Arrival of the Official Party";
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		OnZoneActivated(E.Zone);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		OnEndTurn();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteringZoneEvent E)
	{
		if (Active && Stage < 2 && !E.Forced && !E.System)
		{
			Zone zone = E.Cell.ParentZone;
			if (zone is InteriorZone interiorZone)
			{
				zone = interiorZone.ResolveBasisZone() ?? zone;
			}
			if (!Perimeter.Contains(zone.ZoneID) && Popup.ShowYesNo(GetProperty("MessageLeaving"), "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.No) != DialogResult.Yes)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public void OnZoneActivated(Zone Z)
	{
		if (Z is InteriorZone interiorZone)
		{
			Z = interiorZone.ResolveBasisZone() ?? Z;
		}
		if (Perimeter.Contains(Z.ZoneID))
		{
			SoundManager.PlayMusic("Music/Arrival of the Official Party");
			Active = true;
			The.ActionManager.AllowCachedTurns = true;
			if (Attempt < NextAttempt)
			{
				StartAttempt();
			}
		}
		else if (Active)
		{
			Active = false;
			The.ActionManager.AllowCachedTurns = false;
			if (Stage == 2)
			{
				RestoreFauna();
				Game.FlagSystemForRemoval(this);
			}
			else
			{
				LeftTick = The.Game.TimeTicks;
				FailAttempt();
			}
		}
	}

	public void OnEndTurn()
	{
		if (!Active)
		{
			return;
		}
		bool flag = --RechargeTimer <= 0;
		if (flag)
		{
			RechargeTimer = 100;
		}
		ZoneManager zoneManager = The.ZoneManager;
		foreach (string item in Perimeter)
		{
			if (zoneManager.CachedZones.TryGetValue(item, out var value))
			{
				value.MarkActive();
			}
			else
			{
				value = zoneManager.GetZone(item);
				zoneManager.SetCachedZone(value);
			}
			if (flag)
			{
				RechargeMecha(value);
			}
		}
		if (Stage == 0 && --Timer <= 0)
		{
			SpawnNephilim();
		}
	}

	private void RechargeMecha(Zone Z)
	{
		Zone.ObjectEnumerator enumerator = Z.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (current.HasTag("Mecha") && !current.IsPlayerControlled() && current.TryGetPart<Vehicle>(out var Part) && Part.PilotID != null && current.TryGetPart<EnergyCellSocket>(out var Part2) && Part2.Cell != null && Part2.Cell.TryGetPartDescendedFrom<IEnergyCell>(out var Part3))
			{
				Part3.MaximizeCharge();
			}
		}
	}

	public IEnumerable<QuestStep> WarleaderSteps()
	{
		foreach (KeyValuePair<string, QuestStep> item in Quest.StepsByID)
		{
			QuestStep value = item.Value;
			if (!value.Value.IsNullOrEmpty())
			{
				yield return value;
			}
		}
	}

	public Dictionary<string, string> GetSpawnTables()
	{
		Dictionary<string, string> dictionary = GetDictionary((NextAttempt <= 1) ? "BattlePopulations" : "ReinforcementPopulations");
		Dictionary<string, string> dictionary2 = GetDictionary("FixedSpawnLocations");
		int property = GetProperty("MajorBattles", 0);
		int property2 = GetProperty("MinorBattles", 0);
		List<QuestStep> list = ((NextAttempt <= 1) ? WarleaderSteps().ToList() : null);
		int chance = ((NextAttempt <= 1) ? 100 : GetProperty("ReinforcementChance", 0));
		Dictionary<string, string> dictionary3 = new Dictionary<string, string>(property + property2 + dictionary2.Count + list.Count);
		foreach (KeyValuePair<string, string> item in dictionary2)
		{
			if (!chance.in100())
			{
				dictionary3[item.Key] = null;
			}
			else if (!list.IsNullOrEmpty() && item.Value == "Major")
			{
				dictionary3[item.Key] = list.RemoveRandomElement().Value;
			}
			else
			{
				dictionary3[item.Key] = dictionary[item.Value];
			}
		}
		while (!list.IsNullOrEmpty())
		{
			QuestStep randomElement = list.GetRandomElement();
			string randomElement2 = Perimeter.GetRandomElement();
			if (dictionary3.TryAdd(randomElement2, randomElement.Value))
			{
				list.Remove(randomElement);
			}
		}
		string value = dictionary["Major"];
		for (int i = 0; i < property; i++)
		{
			if (chance.in100())
			{
				string randomElement3 = Perimeter.GetRandomElement();
				if (!dictionary3.TryAdd(randomElement3, value))
				{
					i--;
				}
			}
		}
		value = dictionary["Minor"];
		for (int j = 0; j < property2; j++)
		{
			if (chance.in100())
			{
				string randomElement4 = Perimeter.GetRandomElement();
				if (dictionary3.TryAdd(randomElement4, value))
				{
					j--;
				}
			}
		}
		return dictionary3;
	}

	public void CreateWarleaders()
	{
		List<string> list = new List<string>();
		foreach (QuestStep item in WarleaderSteps())
		{
			if (PopulationManager.ResolvePopulation(item.Value, MissingOkay: true)?.Find((PopulationItem x) => x.Hint == "Warleader") is PopulationObject populationObject)
			{
				GameObject gameObject = GameObject.Create(populationObject.Blueprint);
				if (gameObject.TryGetPart<Vehicle>(out var Part) && Part.Pilot != null)
				{
					Part.Pilot.AddPart(new FinishQuestStepWhenSlain
					{
						Quest = QuestID,
						Step = item.ID
					});
				}
				gameObject.AddPart(new OmonporchBattleWarleader
				{
					Quest = QuestID,
					Step = item.ID
				});
				gameObject.SetImportant(flag: true);
				item.Name = item.Name.Replace("=name=", gameObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true));
				list.Add(gameObject.ID);
				The.ZoneManager.CacheObject(gameObject);
			}
		}
		SetProperty("Warleaders", list);
	}

	public void StartAttempt()
	{
		Attempt++;
		long num = The.Game.TimeTicks - LeftTick;
		if (Attempt == 1 || num >= GetProperty("ReinforcementDelay", 0))
		{
			foreach (KeyValuePair<string, string> spawnTable in GetSpawnTables())
			{
				if (spawnTable.Value == null)
				{
					continue;
				}
				Zone zone = The.ZoneManager.GetZone(spawnTable.Key);
				foreach (PopulationResult item in PopulationManager.Generate(spawnTable.Value))
				{
					PlacePopulation(zone, item);
				}
				if (Attempt > 1)
				{
					RemoveBarathrumites(zone);
				}
				else if (!zone.IsCheckpoint())
				{
					DisplaceFauna(zone);
				}
				ZoneManager.ActivateObjects(zone);
			}
		}
		if (Attempt > 1)
		{
			if (Stage == 0)
			{
				Timer += 500;
			}
			else if (Stage == 1 && !Quest.IsStepFinished("Nephal"))
			{
				NephalOccupy();
			}
			return;
		}
		foreach (string item2 in Perimeter)
		{
			Zone zone2 = The.ZoneManager.GetZone(item2);
			if (!zone2.IsCheckpoint())
			{
				DisplaceFlora(zone2);
			}
		}
	}

	public void RemoveBarathrumites(Zone Zone)
	{
		Zone.ObjectEnumerator enumerator = Zone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (current.IsCombatObject() && current.HasIntProperty("BattleParticipant") && current.IsFactionMember("Barathrumites") && !current.IsPlayerControlled())
			{
				current.RemoveFromContext();
				The.ActionManager.RemoveActiveObject(current);
				current.Pool();
			}
		}
	}

	public void DisplaceFlora(Zone Zone)
	{
		List<int> list = GetList<int>("DisplaceFloraChance");
		if (!list[0].in100())
		{
			return;
		}
		int chance = list[1];
		Zone.ObjectEnumerator enumerator = Zone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (current.Physics != null && current.Render != null && (current.Physics.Solid || current.Render.Occluding) && !current.IsCombatObject() && !current.IsImportant() && current.HasTagOrProperty("Plant") && chance.in100())
			{
				current.Destroy(null, Silent: true);
			}
		}
	}

	public void DisplaceFauna(Zone Zone)
	{
		int property = GetProperty("DisplaceFaunaChance", 0);
		Zone.ObjectEnumerator enumerator = Zone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (current.IsCombatObject() && !current.IsImportant() && !current.IsFactionMember("Barathrumites") && !current.IsFactionMember("Templar") && !current.IsPlayerControlled() && property.in100())
			{
				Cell currentCell = current.CurrentCell;
				current.RemoveFromContext();
				The.ActionManager.RemoveActiveObject(current);
				Displaced.Add(current.ID);
				The.ZoneManager.CacheObject(current);
				Brain brain = current.Brain;
				(brain.StartingCell ?? (brain.StartingCell = new GlobalLocation())).SetCell(currentCell);
			}
		}
	}

	public void RestoreFauna()
	{
		foreach (string item in Displaced)
		{
			GameObject value = The.ZoneManager.CachedObjects.GetValue(item);
			The.ZoneManager.CachedObjects.Remove(item);
			if (value != null)
			{
				Cell cell = value.Brain.StartingCell.ResolveCell();
				if (cell != null)
				{
					value.SystemMoveTo(cell, null, forced: true);
				}
				else
				{
					value.Pool();
				}
			}
		}
		Displaced.Clear();
	}

	public void PlacePopulation(Zone Zone, PopulationResult Result)
	{
		int property = GetProperty("ClusterChance", 0);
		int property2 = GetProperty("ClusterDistance", 0);
		int property3 = GetProperty("ClusterRadius", 4);
		for (int i = 0; i < Result.Number; i++)
		{
			if (Result.Hint == "Warleader")
			{
				List<string> list = GetList("Warleaders");
				foreach (string item in list)
				{
					GameObject value = The.ZoneManager.CachedObjects.GetValue(item);
					if (value != null && value.Blueprint == Result.Blueprint)
					{
						The.ZoneManager.CachedObjects.Remove(item);
						value.SetIntProperty("BattleParticipant", 1);
						PlaceClusteredObject(Zone, value, (property > 0) ? 100 : 0, property2, property3);
						list.Remove(item);
						if (list.Count == 0)
						{
							RemoveProperty("Warleaders");
						}
						break;
					}
				}
			}
			else
			{
				GameObject gameObject = GameObject.Create(Result.Blueprint);
				gameObject.SetIntProperty("BattleParticipant", 1);
				PlaceClusteredObject(Zone, gameObject, property, property2, property3);
			}
		}
	}

	public void PlaceClusteredObject(Zone Zone, GameObject Object, int Chance = 50, int Distance = 0, int Radius = 4)
	{
		Cell cell = null;
		if (Chance.in100())
		{
			if (CurrentZone != Zone)
			{
				Clusters?.Clear();
				CurrentZone = Zone;
			}
			if (Clusters == null)
			{
				Clusters = new Dictionary<string, List<Location2D>>(8);
			}
			string primaryFaction = Object.GetPrimaryFaction();
			if (!Clusters.TryGetValue(primaryFaction, out var value))
			{
				value = new List<Location2D>(16);
				for (int i = 0; i < 100; i++)
				{
					cell = Zone.GetRandomCell(4);
					if (cell.IsEmpty() && cell.IsReachable() && GetClusterDistance(cell) > Distance)
					{
						break;
					}
				}
				foreach (Cell item in cell.IterateAdjacent(Radius, IncludeSelf: true, LocalOnly: true))
				{
					value.Add(item.Location);
				}
				Clusters[primaryFaction] = value;
			}
			for (int j = 0; j < value.Count; j++)
			{
				Location2D randomElement = value.GetRandomElement();
				cell = Zone.GetCell(randomElement);
				if (cell.IsReachable() && cell.IsEmptyFor(Object))
				{
					cell.AddObject(Object);
					return;
				}
			}
		}
		cell = Zone.GetRandomCell();
		if (!cell.IsEmptyFor(Object) || !cell.IsReachable())
		{
			foreach (Cell item2 in cell.IterateAdjacent(10, IncludeSelf: false, LocalOnly: true))
			{
				if (item2.IsEmptyFor(Object) && item2.IsReachable())
				{
					cell = item2;
					break;
				}
			}
		}
		cell.AddObject(Object);
	}

	public int GetClusterDistance(Cell Cell)
	{
		int num = 9999999;
		foreach (List<Location2D> value in Clusters.Values)
		{
			if (!value.IsNullOrEmpty())
			{
				int num2 = Cell.PathDistanceTo(value[0]);
				if (num > num2)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public void FailAttempt()
	{
		NextAttempt++;
		Game.PopupFailQuest(Quest);
		Quest.Name = Blueprint.Name + ", attempt #" + NextAttempt;
		Game.PopupStartQuest(Quest);
		The.ActionManager.RemoveExternalObjects();
	}

	public void NephalOccupy()
	{
		Zone zone = The.ZoneManager.GetZone(GetProperty("NephalRetryZone"));
		List<Zone> list = new List<Zone>(2);
		foreach (string item in Perimeter)
		{
			Zone zone2 = The.ZoneManager.GetZone(item);
			if (zone.ResolvedLocation.ManhattanDistance(zone2.ResolvedLocation) == 1)
			{
				list.Add(zone2);
			}
		}
		Zone.ObjectEnumerator enumerator2 = zone.IterateObjects().GetEnumerator();
		while (enumerator2.MoveNext())
		{
			GameObject current2 = enumerator2.Current;
			if (current2.IsCombatObject() && !current2.IsPlayerControlled())
			{
				Zone randomElement = list.GetRandomElement();
				current2.CurrentCell.RemoveObject(current2);
				ZoneBuilderSandbox.PlaceObjectInArea(randomElement, randomElement.area, current2);
			}
		}
		Cell cell = NephalLocation.ResolveCell();
		GameObject firstObject = cell.GetFirstObject((GameObject x) => x.HasPart(typeof(OmonporchBattleNephilim)));
		cell.RemoveObject(firstObject);
		ZoneBuilderSandbox.PlaceObjectInArea(zone, zone.area, firstObject);
	}

	public GameObject GetValidNephal(string Blueprint = null)
	{
		if (Blueprint.IsNullOrEmpty())
		{
			List<string> list = new List<string>(NephalProperties.Nephilim.Length);
			string[] nephilim = NephalProperties.Nephilim;
			foreach (string text in nephilim)
			{
				if (!NephalProperties.IsFoiled(text))
				{
					list.Add(text);
				}
			}
			if (list.Count > 1)
			{
				list.Remove("Ehalcodon");
			}
			Blueprint = list.GetRandomElement();
		}
		else
		{
			Blueprint = Grammar.ClosestMatch(NephalProperties.Nephilim, Blueprint);
		}
		if (Blueprint.IsNullOrEmpty())
		{
			return null;
		}
		return GameObject.Create(Blueprint);
	}

	public void SpawnNephilim(string Blueprint = null)
	{
		if (!Active || Stage >= 1)
		{
			return;
		}
		Timer = 0;
		Stage = 1;
		string property = GetProperty("ArrivalSound");
		DelimitedEnumeratorChar delimitedEnumeratorChar = property.DelimitedBy(',');
		DelimitedEnumeratorChar enumerator = delimitedEnumeratorChar.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SoundManager.PreloadClipSet(new string(enumerator.Current));
		}
		QuestStep questStep = Quest.StepsByID["Nephal"];
		GameObject validNephal = GetValidNephal(Blueprint);
		if (validNephal == null)
		{
			questStep.Optional = true;
			The.Game.CheckQuestFinishState(Quest);
			return;
		}
		if (validNephal.Blueprint == "Ehalcodon")
		{
			Super = true;
		}
		validNephal.RequirePart<OmonporchBattleNephilim>();
		validNephal.AddPart(new FinishQuestStepWhenSlain
		{
			Quest = "Reclamation",
			Step = "Nephal"
		});
		questStep.Hidden = false;
		questStep.Text = questStep.Text.Replace("=name=", validNephal.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true));
		questStep.Value = validNephal.Blueprint;
		string randomElement = Perimeter.GetRandomElement();
		while (randomElement == "JoppaWorld.53.4.0.0.10")
		{
			randomElement = Perimeter.GetRandomElement();
		}
		Zone zone = The.ZoneManager.GetZone(randomElement);
		ZoneBuilderSandbox.PlaceObjectInArea(zone, zone.area, validNephal);
		string value = The.Player.DescribeDirectionToward(validNephal, General: true);
		delimitedEnumeratorChar = property.DelimitedBy(',');
		enumerator = delimitedEnumeratorChar.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SoundManager.PlaySound(new string(enumerator.Current), 0f, 1f, 0.65f);
		}
		if (CombatJuice.enabled)
		{
			CombatJuice.cameraShake(2f);
		}
		Popup.Show(GetProperty(Super ? "MessageArrivalSuper" : "MessageArrival").StartReplace().AddReplacer("name", validNephal.ShortDisplayName).AddReplacer("direction", value)
			.ToString());
	}

	[WishCommand("reclamation:start", null)]
	public static void WishStart()
	{
		The.Game.StartQuest("Reclamation");
	}

	[WishCommand("reclamation:nephal", null)]
	public static void WishNephal()
	{
		The.Game.GetSystem<ReclamationSystem>().SpawnNephilim();
	}

	[WishCommand("reclamation:nephal", null)]
	public static void WishNephal(string Blueprint)
	{
		The.Game.GetSystem<ReclamationSystem>().SpawnNephilim(Blueprint);
	}

	[WishCommand("reclamation:timer", null)]
	public static void WishTimer()
	{
		int timer = The.Game.GetSystem<ReclamationSystem>().Timer;
		MessageQueue.AddPlayerMessage($"Turns until nephal arrives: {timer}");
	}

	[WishCommand("reclamation:reinforce", null)]
	public static void WishReinforce()
	{
		ReclamationSystem system = The.Game.GetSystem<ReclamationSystem>();
		system.LeftTick = The.Game.TimeTicks - system.GetProperty("ReinforcementDelay", 0);
	}

	[WishCommand("reclamation:heal", null)]
	public static void WishHeal()
	{
		ReclamationSystem system = The.Game.GetSystem<ReclamationSystem>();
		system.LeftTick = The.Game.TimeTicks - system.GetProperty("HealDelay", 0);
	}

	[WishCommand("reclamation:kill", null)]
	public static void WishKill()
	{
		Zone[] array = The.ZoneManager.CachedZones.Values.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Zone.ObjectEnumerator enumerator = array[i].IterateObjects().GetEnumerator();
			while (enumerator.MoveNext())
			{
				GameObject current = enumerator.Current;
				if (current.TryGetPartDescendedFrom<FinishQuestStepWhenSlain>(out var Part) && Part.Quest == "Reclamation")
				{
					current.Die(The.Player);
				}
			}
		}
	}

	[WishCommand("startehalcodon", null)]
	public static void WishTestEhalcodon()
	{
		string[] nephilim = NephalProperties.Nephilim;
		foreach (string text in nephilim)
		{
			if (!(text == "Ehalcodon"))
			{
				if (GameObjectFactory.Factory.Blueprints.TryGetValue("LightCircle" + text, out var value))
				{
					The.Player.ReceiveObject(value.createOne());
				}
				The.Game.TryAddDelimitedGameState(text, ',', "Pacified");
			}
		}
		WishTestBattle();
	}

	public static void WishStage()
	{
		GameObject player = The.Player;
		XRLGame game = The.Game;
		game.CompleteQuest("A Canticle for Barathrum");
		game.CompleteQuest("Decoding the Signal");
		game.CompleteQuest("More Than a Willing Spirit");
		game.CompleteQuest("The Earl of Omonporch");
		game.SetIntGameState("ForcePostEarlSpawn", 1);
		game.CompleteQuest("A Call to Arms");
		game.CompleteQuest("Pax Klanq, I Presume?");
		GritGateScripts.PromoteToJourneyfriend();
		GritGateScripts.OpenRank2Doors();
		int num = Leveler.GetXPForLevel(37) - player.GetStatValue("XP");
		if (num > 0)
		{
			player.AwardXP(num);
		}
		if (!game.GetBooleanGameState("Recame"))
		{
			BodyPart body = player.Body.GetBody();
			bool? dynamic = true;
			body.AddPart("Floating Nearby", 0, null, null, null, null, null, null, null, null, null, null, null, null, null, dynamic);
			game.SetBooleanGameState("Recame", Value: true);
		}
	}

	[WishCommand("startbattle", null)]
	public static void WishTestBattle()
	{
		Popup.Suppress = true;
		ItemNaming.Suppress = true;
		GameObject player = The.Player;
		XRLGame game = The.Game;
		WishStage();
		AscensionSystem.WishSetBarathrumState();
		if (!game.GetBooleanGameState("Recame"))
		{
			BodyPart body = player.Body.GetBody();
			bool? dynamic = true;
			body.AddPart("Floating Nearby", 0, null, null, null, null, null, null, null, null, null, null, null, null, null, dynamic);
			game.SetBooleanGameState("Recame", Value: true);
		}
		Zone zone = The.ZoneManager.GetZone("JoppaWorld.53.4.1.1.10");
		Cell cell = zone.GetCell(26, 16);
		Cell cell2 = zone.GetCell(40, 16);
		player.SystemMoveTo(cell, null, forced: true);
		zone.FastFloodFindFirstBlueprint(40, 16, 10, "Adiyy", player)?.Destroy();
		cell.GetFirstObjectWithPart("Chair")?.GetPart<Chair>().SitDown(player);
		if (!zone.HasObject("Barathrum"))
		{
			cell.getClosestPassableCell().AddObject("Barathrum");
		}
		if (!game.HasFinishedQuest("The Golem"))
		{
			try
			{
				GolemQuestSelection.PlaceFinalMound(cell2);
				game.CompleteQuest("The Golem");
			}
			catch (Exception x)
			{
				MetricsManager.LogException("OmonporchBattle:Start", x);
			}
		}
		game.StartQuest("We Are Starfreight");
		Popup.Suppress = false;
		ItemNaming.Suppress = false;
		if (!game.HasQuest("Reclamation"))
		{
			MapChunkPlacement.PlaceFromFile(zone, "preset_tile_chunks/HamilcrabShop.rpm", 45, 2, 6, 7, 0, 1);
			GameObject gameObject = GameObject.Create("DromadTrader8", 0, 0, null, (GameObject gameObject3) =>
			{
				gameObject3.RemovePart(typeof(DromadCaravan));
			});
			zone.GetCell(49, 5).getClosestEmptyCell().AddObject(gameObject);
			game.StartQuest("Reclamation");
			GameObject gameObject2 = zone.GetCell(39, 12).AddObject("RelicChest");
			for (int num = 1; num <= 8; num++)
			{
				gameObject2.ReceiveObject(RelicGenerator.GenerateRelic(num, RandomName: true));
			}
			zone.GetCell(40, 12).AddObject("RelicChest").ReceivePopulation("FinalSupply");
		}
	}
}
