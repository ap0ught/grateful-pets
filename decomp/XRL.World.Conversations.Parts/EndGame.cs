using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Text;
using HistoryKit;
using Platform.IO;
using Qud.API;
using Qud.UI;
using UnityEngine;
using UnityEngine.UI;
using XRL.Core;
using XRL.Messages;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Skills.Cooking;
using XRL.World.Tinkering;
using XRL.World.ZoneBuilders;

namespace XRL.World.Conversations.Parts;

[HasWishCommand]
public class EndGame : IConversationPart
{
	private static string[] Regions = new string[3] { "Saltmarsh", "DesertCanyon", "Hills" };

	private static Dictionary<string, string> CallingAliases = new Dictionary<string, string>
	{
		{ "Apostle", "Sibyl" },
		{ "Arconaut", "Caveweird" },
		{ "Greybeard", "Whitebeard" },
		{ "Gunslinger", "Stickwhiz" },
		{ "Marauder", "Corsair" },
		{ "Scholar", "Rabbi" },
		{ "Tinker", "Machinist" },
		{ "Water Merchant", "Waterfreak" },
		{ "Watervine Farmer", "Grassfolk" }
	};

	private static Dictionary<string, string> CallingTiles = new Dictionary<string, string>
	{
		{ "Apostle", "Creatures/chiliad-sibyl.png" },
		{ "Arconaut", "Creatures/chiliad-caveweird.png" },
		{ "Greybeard", "Creatures/chiliad-whitebeard.png" },
		{ "Gunslinger", "Creatures/chiliad-stickwhiz.png" },
		{ "Marauder", "Creatures/chiliad-corsair.png" },
		{ "Nomad", "Creatures/chiliad-nomad.png" },
		{ "Pilgrim", "Creatures/chiliad-pilgrim.png" },
		{ "Scholar", "Creatures/chiliad-rabbi.png" },
		{ "Tinker", "Creatures/chiliad-machinist.png" },
		{ "Warden", "Creatures/chiliad-warden.png" },
		{ "Water Merchant", "Creatures/chiliad-waterfreak.png" },
		{ "Watervine Farmer", "Creatures/chiliad-grassfolk.png" }
	};

	public string Prompt;

	public string Type;

	public string Grade;

	public static bool IsBrightsheol => The.Game.GetStringGameState("EndType") == "Brightsheol";

	public static bool IsMarooned => The.Game.GetStringGameState("EndType") == "Marooned";

	public static bool IsReturn => The.Game.GetStringGameState("EndType") == "Return";

	public static bool IsCovenant => The.Game.GetStringGameState("EndType") == "Covenant";

	public static bool IsAccede => The.Game.GetStringGameState("EndType") == "Accede";

	public static bool IsLaunch => The.Game.GetStringGameState("EndType") == "Launch";

	public static bool IsStarshiib => The.Game.HasDelimitedGameState("EndExtra", ',', "Starshiib");

	public static bool IsArkOpened => The.Game.HasDelimitedGameState("EndExtra", ',', "OpenArk");

	public static bool IsWithBarathrum => The.Game.HasDelimitedGameState("EndExtra", ',', "WithBarathrum");

	public static bool IsAnyNorthSheva
	{
		get
		{
			if (!IsReturn && !IsCovenant && !IsAccede && !IsLaunch)
			{
				return IsMarooned;
			}
			return true;
		}
	}

	public static bool IsSuper => The.Game.GetStringGameState("EndGrade") == "Super";

	public static bool IsUltimate => IsSuper;

	public static bool IsUltra => IsSuper;

	public static bool IsAnyEnding
	{
		get
		{
			if (!IsBrightsheol)
			{
				return IsAnyNorthSheva;
			}
			return true;
		}
	}

	public static bool IsGolemDead()
	{
		List<GameObject> list = VehicleRecord.ResolveRecordsFor(The.Player, null, "Golem");
		if (list.Count <= 0)
		{
			return true;
		}
		bool result = true;
		foreach (GameObject item in list)
		{
			if (item?.CurrentCell != null)
			{
				result = false;
			}
		}
		return result;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnterElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = (NephalProperties.AllFoiled() ? "{{M|[greater victory]}}" : "{{M|[victory]}}");
		return false;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		string text = Prompt;
		if (text.IsNullOrEmpty())
		{
			text = Type.ToUpper();
		}
		string text2 = "";
		string text3 = null;
		if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.PC)
		{
			text2 = "\n\nType " + text + " to confirm.";
			text3 = "";
		}
		if (Popup.AskString("End game?" + text2, text3, "Sounds/UI/ui_notification", null, text, text.Length, 0, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false).ToUpper() == text)
		{
			if (!Type.IsNullOrEmpty())
			{
				The.Game.SetStringGameState("EndType", Type);
			}
			if (!Grade.IsNullOrEmpty())
			{
				The.Game.SetStringGameState("EndGrade", Grade);
			}
			GameManager.Instance.gameQueue.queueTask(Start);
			return base.HandleEvent(E);
		}
		return false;
	}

	public static string ApplyLocation(Zone Z)
	{
		Dictionary<string, CellBlueprint> cellBlueprintsByApplication = WorldFactory.Factory.getWorld("JoppaWorld").CellBlueprintsByApplication;
		string randomElement = Regions.GetRandomElement();
		CellBlueprint value;
		while (!cellBlueprintsByApplication.TryGetValue("Terrain" + randomElement, out value))
		{
			randomElement = Regions.GetRandomElement();
		}
		value.LevelBlueprint[1, 1, 10].Builders.ApplyTo(Z, Z.ZoneID);
		Z.SetZoneProperty("Region", randomElement);
		return randomElement;
	}

	public static HistoricEntity ApplyVillage(Zone Z, string Region)
	{
		VillageCoda villageCoda = new VillageCoda();
		History sultanHistory = The.Game.sultanHistory;
		sultanHistory.currentYear = HistoryAPI.GetFlipYear() + Calendar.GetYear() + 1000;
		HistoricEntity historicEntity = VillageCoda.GenerateVillageEntity(Region, GetBaseFaction().Name);
		sultanHistory.currentYear = Math.Max(sultanHistory.currentYear, historicEntity.lastYear);
		JournalAPI.AddVillageGospels(historicEntity);
		villageCoda.villageEntity = historicEntity;
		Faction faction = VillageBase.CreateVillageFaction(historicEntity.GetCurrentSnapshot());
		faction.EntityID = historicEntity.id;
		faction.EventID = historicEntity.events[0].id;
		villageCoda.villageFaction = faction.Name;
		villageCoda.SurfaceRevealer = false;
		The.ZoneManager.SetZoneName(Z.ZoneID, historicEntity.Name, null, null, null, null, Proper: true);
		ZoneManager.ZoneGenerationContext = Z;
		ZoneManager.zoneGenerationContextTier = Z.NewTier;
		ZoneManager.zoneGenerationContextZoneID = Z.ZoneID;
		NameStyle nameStyle = NameStyles.NameStyleTable["Chiliad Qudish"];
		NameScope item = new NameScope
		{
			Name = "General",
			Priority = 10,
			Combine = false
		};
		try
		{
			nameStyle.Scopes.Add(item);
			villageCoda.BuildZone(Z);
			return historicEntity;
		}
		finally
		{
			nameStyle.Scopes.Remove(item);
		}
	}

	public static Faction GetBaseFaction()
	{
		Faction result = Factions.Get("Beasts");
		float num = float.MinValue;
		bool isAccede = IsAccede;
		foreach (KeyValuePair<string, float> reputationValue in The.Game.PlayerReputation.ReputationValues)
		{
			Faction ifExists = Factions.GetIfExists(reputationValue.Key);
			float value = reputationValue.Value;
			if (ifExists != null && ifExists.Visible && value != num && reputationValue.Value < num == isAccede && ifExists.AnyMembers(VillageCodaBase.IsVillagerEligible, Dynamic: false))
			{
				result = ifExists;
				num = reputationValue.Value;
			}
		}
		return result;
	}

	public static void UnlockAchievement()
	{
		Achievement.CAVES_OF_QUD.Unlock();
		if (IsBrightsheol)
		{
			Achievement.CROSS_BRIGHTSHEOL.Unlock();
		}
		else if (IsCovenant)
		{
			Achievement.END_COVENANT.Unlock();
		}
		else if (IsLaunch)
		{
			Achievement.END_LAUNCH.Unlock();
		}
		else if (IsReturn)
		{
			Achievement.END_RETURN.Unlock();
		}
		else if (IsAccede)
		{
			Achievement.END_ACCEDE.Unlock();
		}
		if (IsUltra && IsAnyNorthSheva)
		{
			Achievement.UNGYRE.Unlock();
		}
	}

	[WishCommand("gocreditscoda", null)]
	public static void Start()
	{
		Start(true);
	}

	public static void Start(bool fade = true)
	{
		if (fade)
		{
			GameManager.Instance.PushGameView("Cinematic");
			FadeToBlack.FadeOut(6f, new Color(0f, 0f, 0f, 1f));
			Thread.Sleep(11000);
		}
		UnlockAchievement();
		RunCredits();
		GoCoda();
	}

	[WishCommand("gocoda", null)]
	public static void GoCodaWish()
	{
		if (!EvaluateState())
		{
			PickState();
		}
		GoCoda();
	}

	[WishCommand("gocredits", null)]
	public static void GoCreditsWish()
	{
		if (!EvaluateState())
		{
			PickState();
		}
		RunCredits();
		FadeToBlack.FadeIn(3f);
	}

	[WishCommand("goend", null)]
	public static void GoEndWish()
	{
		if (!EvaluateState())
		{
			PickState();
		}
		Start(fade: false);
	}

	public static void RunCredits()
	{
		Task.Run(() => RunCreditsAsync()).Wait();
	}

	public static async Task RunCreditsAsync()
	{
		AmbientSoundsSystem.StopAmbientBeds();
		float mintime = 0f;
		ManualResetEvent ev = new ManualResetEvent(initialState: false);
		ev.Reset();
		bool runInBackground = false;
		FadeToBlack.FadeIn(3f, new Color(0f, 0f, 0f, 0f));
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			SoundManager.MusicSources.SetMusicBackground(State: true);
			SoundManager.PlayMusic("Music/Soundscape I", "music", Crossfade: true, 3f, 1f, null, Loop: false);
			runInBackground = Application.runInBackground;
			Application.runInBackground = true;
			UnityEngine.GameObject gameObject = UIManager.mainCanvas.transform.Find("EndgameCredits").gameObject;
			gameObject.SetActive(value: true);
			EndgameCredits component = gameObject.GetComponent<EndgameCredits>();
			BookUI.Books["EndCredits"].Pages[0].Lines.Max((string x) => x.Length);
			using (Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder())
			{
				foreach (string line in BookUI.Books["EndCredits"].Pages[0].Lines)
				{
					utf16ValueStringBuilder.Append(line);
				}
				utf16ValueStringBuilder.Replace("~NBSP", "\u2007");
				string text = utf16ValueStringBuilder.ToString();
				component.creditsTextSkin.SetText(text);
			}
			string text2 = "The End";
			if (IsBrightsheol)
			{
				The.Game.SetStringGameState("FinalEndmark", "brightsheol");
				component.SetEndmark("brightsheol", IsUltimate);
				text2 = ((!IsUltimate) ? "You freed the Spindle for another's ascent and crossed into Brightsheol." : "You annulled the plagues of the Gyre.\n\nThen you freed the Spindle for another's ascent and crossed into Brightsheol");
			}
			else if (IsMarooned)
			{
				The.Game.SetStringGameState("FinalEndmark", "marooned");
				component.SetEndmark("marooned", IsUltimate);
				text2 = ((!IsUltimate) ? "You destroyed Resheph to save the burgeoning world but marooned yourself at the North Sheva." : "You annulled the plagues of the Gyre.\n\nThen you destroyed Resheph to save the burgeoning world but marooned yourself at the North Sheva.");
			}
			else if (IsCovenant)
			{
				The.Game.SetStringGameState("FinalEndmark", "covenant");
				component.SetEndmark("covenant", IsUltimate);
				text2 = ((!IsUltimate) ? "You entered into a covenant with Resheph to help prepare Qud for the Coven's return." : "You annulled the plagues of the Gyre.\n\nThen you entered into a covenant with Resheph to help prepare Qud for the Coven's return.");
			}
			else if (IsReturn)
			{
				The.Game.SetStringGameState("FinalEndmark", "return");
				component.SetEndmark("return", IsUltimate);
				text2 = (IsArkOpened ? ((!IsUltimate) ? "You destroyed Resheph and returned to Qud to help garden the burgeoning world." : "You annulled the plagues of the Gyre.\n\nThen you destroyed Resheph and returned to Qud to help garden the burgeoning world.") : ((!IsUltimate) ? "You rebuked Resheph and returned to Qud to help garden the burgeoning world." : "You annulled the plagues of the Gyre.\n\nThen you returned to Qud to help garden the burgeoning world."));
			}
			else if (IsAccede)
			{
				The.Game.SetStringGameState("FinalEndmark", "accede");
				component.SetEndmark("accede", IsUltimate);
				text2 = ((!IsUltimate) ? "You acceded to Resheph's plan to purge the world of higher life in preparation for the Coven's return." : "You annulled the plagues of the Gyre.\n\nThen you reversed course and acceded to Resheph's plan to purge the world of higher life, in preparation for the Coven's return.");
			}
			else if (IsLaunch)
			{
				The.Game.SetStringGameState("FinalEndmark", "spaceship");
				component.SetEndmark("spaceship", IsUltimate);
				text2 = (IsArkOpened ? (IsWithBarathrum ? ((!IsUltimate) ? "You destroyed Resheph and launched yourself into the dusted cosmos to ply the stars with Barathrum." : "You annulled the plagues of the Gyre.\n\nThen you destroyed Resheph and launched yourself into the dusted cosmos to ply the stars with Barathrum.") : ((!IsUltimate) ? "You destroyed Resheph and launched yourself into the dusted cosmos to ply the stars." : "You annulled the plagues of the Gyre.\n\nThen you destroyed Resheph and launched  yourself into the dusted cosmos to ply the stars.")) : (IsWithBarathrum ? ((!IsUltimate) ? "You launched yourself into the dusted cosmos to ply the stars with Barathrum." : "You annulled the plagues of the Gyre.\n\nThen you launched yourself into the dusted cosmos to ply the stars with Barathrum.") : ((!IsUltimate) ? "You launched yourself into the dusted cosmos to ply the stars." : "You annulled the plagues of the Gyre.\n\nThen you launched yourself into the dusted cosmos to ply the stars.")));
			}
			else
			{
				The.Game.SetStringGameState("FinalEndmark", "tombstone");
				component.SetEndmark("tombstone", IsUltimate);
			}
			The.Game.SetStringGameState("EndgameCause", text2);
			component.tombstoneText.SetText("\n" + text2);
			mintime = component.scrollTime;
			component.complete = () =>
			{
				ev.Set();
				SoundManager.PlayMusic(null, "music", Crossfade: false);
				SoundManager.PlayMusic(null, "music", Crossfade: false);
				MetricsManager.LogInfo("endgame credits complete");
				SoundManager.MusicSources.SetMusicBackground(Options.MusicBackground);
			};
			component.runtime = 0f;
			component.testRun = true;
		});
		ev.WaitOne(145000);
		Thread.Sleep(3000);
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			Application.runInBackground = runInBackground;
			UnityEngine.GameObject gameObject = UIManager.mainCanvas.transform.Find("EndgameCredits").gameObject;
			FadeToBlack.FadeNow(0f, 1f, 1f, Color.black, Color.black);
			gameObject.SetActive(value: false);
		});
	}

	public static GameObject GetSultanBlueprint()
	{
		GameObject gameObject = The.Player;
		if (gameObject.IsOriginalPlayerBody())
		{
			return gameObject;
		}
		if (gameObject.TryGetEffect<Dominated>(out var Effect) && GameObject.Validate(Effect.Dominator))
		{
			gameObject = Effect.Dominator;
			if (gameObject.IsOriginalPlayerBody())
			{
				return gameObject;
			}
		}
		if (gameObject.TryGetPart<Vehicle>(out var Part) && GameObject.Validate(Part.Pilot))
		{
			gameObject = Part.Pilot;
			if (gameObject.IsOriginalPlayerBody())
			{
				return gameObject;
			}
		}
		foreach (KeyValuePair<string, Zone> cachedZone in The.ZoneManager.CachedZones)
		{
			cachedZone.Deconstruct(out var _, out var value);
			Zone.ObjectEnumerator enumerator2 = value.IterateObjects().GetEnumerator();
			while (enumerator2.MoveNext())
			{
				if (enumerator2.Current.IsOriginalPlayerBody())
				{
					return gameObject;
				}
			}
		}
		return gameObject;
	}

	public static void GoCoda()
	{
		FadeToBlack.Fade(0f, 1f, 1f, Color.black, Color.black);
		AmbientSoundsSystem.StopAmbientBeds();
		GameManager.Instance.PushGameView("Empty");
		Loading.SetHideLoadStatus(hidden: true);
		Popup.Suppress = true;
		try
		{
			The.Core.StashScore();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Stashing Score for Coda", x);
		}
		try
		{
			The.Game.IsCoda = true;
			EvaluateState();
			CodaSystem codaSystem = The.Game.RequireSystem<CodaSystem>();
			codaSystem.Sultan = GetSultanBlueprint();
			codaSystem.EndTime = Calendar.TotalTimeTicks;
			VillageCoda.GenerateSultanEntity(codaSystem.Sultan);
			ManualResetEvent ev = new ManualResetEvent(initialState: false);
			GameManager.Instance.uiQueue.queueTask(() =>
			{
				TransitionText(() =>
				{
					ev.Set();
				});
			});
			ev.WaitOne();
			GameObject gameObject = CreatePlayer();
			gameObject.SystemMoveTo(The.PlayerCell);
			The.Game.Player.Body = gameObject;
			long num = XRLCore.FrameTimer.ElapsedMilliseconds + 6000;
			if (The.ZoneManager.GetZone("JoppaWorld.11.22.1.1.10").GetCell(0, 0).AddObject("CodaHost")
				.TryGetPart<Interior>(out var Part))
			{
				string text = ApplyLocation(Part.Zone);
				HistoricEntity historicEntity = ApplyVillage(Part.Zone, text);
				EvaluateFactionReputation();
				MetricsManager.LogInfo("Coda region: " + text);
				gameObject.Brain.TakeBaseAllegiance(The.Player);
				Part.Zone.SetMusic("Music/Onward Romantique");
				Cell pullDownLocation = Part.Zone.GetPullDownLocation(gameObject);
				gameObject.SystemMoveTo(pullDownLocation);
				gameObject.GiveProperName();
				ClearHistory();
				The.Game.TimeTicks += 438000000 + The.Game.TimeOffset;
				ArriveAccomplishment(historicEntity.Name);
				The.Game.SaveGame("Primary");
				The.Game.Checkpoint();
				SaveCoda();
			}
			while (XRLCore.FrameTimer.ElapsedMilliseconds < num)
			{
				The.Core.RenderBase();
			}
		}
		finally
		{
			if (GameManager.Instance.CurrentGameView == "Empty")
			{
				GameManager.Instance.PopGameView();
			}
			float? to = 0f;
			FadeToBlack.Fade(3f, null, to);
			Loading.SetHideLoadStatus(hidden: false);
			Popup.Suppress = false;
		}
	}

	public static void ArriveAccomplishment(string VillageName)
	{
		OpeningStory.AddAccomplishment(VillageName);
	}

	public static void SaveCoda()
	{
		string cacheDirectory = The.Game.GetCacheDirectory("coda.sav.gz");
		if (!File.Exists(cacheDirectory))
		{
			string text = DataManager.SyncedPath("Codas/");
			The.Game.SaveGame("coda");
			Directory.CreateDirectory(text);
			File.Copy(cacheDirectory, Path.Combine(text, "coda-" + The.Game.GameID.ToLower() + ".sav.gz"));
		}
	}

	public static void ClearHistory()
	{
		try
		{
			The.Game.Quests?.Clear();
			XRLCore.Core.Game.Player.Messages.Messages.Clear();
			foreach (IBaseJournalEntry allNote in JournalAPI.GetAllNotes())
			{
				allNote.Revealed = false;
			}
			JournalScreen.ResetRawHash();
			MessageQueue.UnityMessages.Clear();
			MessageLogWindow.GameInit();
			TinkerData.KnownRecipes?.Clear();
			CookingGameState.instance?.knownRecipies?.Clear();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("HistoryClear", x);
		}
	}

	public static void TransitionText(Action action = null)
	{
		UnityEngine.GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<UnityEngine.GameObject>("Prefabs/CodaText"), UnityEngine.GameObject.Find("FadeToBlack").gameObject.transform);
		gameObject.SetActive(value: false);
		FadeText fadeText = gameObject.AddComponent<FadeText>();
		fadeText.After = action;
		fadeText.Skin = gameObject.GetComponent<Image>();
		fadeText.FadeIn = 2f;
		fadeText.Hold = 1f;
		fadeText.FadeOut = 2f;
		fadeText.EndDestroy = false;
		gameObject.SetActive(value: true);
		(gameObject.transform as RectTransform).anchoredPosition = new Vector3(0f, 0f, 1f);
		gameObject.transform.SetAsLastSibling();
	}

	protected static void PostprocessPlayerItem(GameObject Object)
	{
		Object.Seen();
		Object.MakeUnderstood();
		Object.GetPart<EnergyCellSocket>()?.Cell?.MakeUnderstood();
		Object.GetPart<MagazineAmmoLoader>()?.Ammo?.MakeUnderstood();
	}

	public static GameObject CreatePlayer()
	{
		if (VillageCoda.IsRuined)
		{
			return GameObject.Create("GiantAmoeba");
		}
		GameObject gameObject = GameObject.Create("Humanoid");
		GenotypeEntry genotypeEntry = GenotypeFactory.RequireGenotypeEntry("Mutated Human");
		SubtypeEntry randomElement = SubtypeFactory.GetSubtypeClass(genotypeEntry).Categories.GetRandomElement().Subtypes.GetRandomElement();
		gameObject.Render.Tile = (CallingTiles.ContainsKey(randomElement.Name) ? CallingTiles[randomElement.Name] : "Creatures/coda_stranger.png");
		gameObject.Render.DetailColor = randomElement.DetailColor.Coalesce(gameObject.Render.DetailColor);
		gameObject.SetStringProperty("Genotype", genotypeEntry.Name);
		gameObject.SetStringProperty("Culture", "Chiliad Qudish");
		gameObject.RequirePart<Description>().Short = "It's you.";
		if (CallingAliases.TryGetValue(randomElement.Name, out var value))
		{
			gameObject.SetStringProperty("Subtype", value);
		}
		else
		{
			gameObject.SetStringProperty("Subtype", randomElement.Name);
		}
		string key;
		if (!genotypeEntry.Stats.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, GenotypeStat> stat in genotypeEntry.Stats)
			{
				stat.Deconstruct(out key, out var value2);
				string name = key;
				GenotypeStat genotypeStat = value2;
				gameObject.GetStat(name).BaseValue = Stat.Random(genotypeStat.Minimum, genotypeStat.Maximum);
			}
		}
		if (!randomElement.Stats.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, SubtypeStat> stat2 in randomElement.Stats)
			{
				stat2.Deconstruct(out key, out var value3);
				string name2 = key;
				SubtypeStat subtypeStat = value3;
				gameObject.GetStat(name2).BaseValue += subtypeStat.Bonus;
			}
		}
		genotypeEntry.AddSkills(gameObject, randomElement.RemoveSkills);
		randomElement.AddSkills(gameObject, genotypeEntry.RemoveSkills);
		if (!randomElement.Gear.IsNullOrEmpty())
		{
			string[] array = randomElement.Gear.Split(',');
			foreach (string text in array)
			{
				if (PopulationManager.Populations.ContainsKey(text))
				{
					foreach (PopulationResult item in PopulationManager.Populations[text].Generate(new Dictionary<string, string>(), ""))
					{
						gameObject.ReceiveObject(item.Blueprint, item.Number, NoStack: false, 0, 0, null, null, PostprocessPlayerItem);
					}
				}
				else
				{
					Debug.LogError("Unknown gear population table: " + text);
				}
			}
			gameObject.CheckStacks();
		}
		int Points = genotypeEntry.MutationPoints;
		while (Points > 0 && TryAddMutation(gameObject, ref Points))
		{
		}
		return gameObject;
	}

	private static bool TryAddMutation(GameObject Object, ref int Points)
	{
		Mutations mutations = Object.RequirePart<Mutations>();
		List<MutationEntry> mutatePool = mutations.GetMutatePool();
		mutatePool.ShuffleInPlace();
		foreach (MutationEntry item in mutatePool)
		{
			if (item.Cost <= Points)
			{
				mutations.AddMutation(item);
				Points -= item.Cost;
				return true;
			}
		}
		return false;
	}

	public static bool EvaluateState()
	{
		if (IsReturn || IsMarooned)
		{
			The.Game.SetStringGameState("SultanTerm", "freeholder");
			if (!IsSuper)
			{
				The.Game.SetIntGameState("CodaPlagued", 1);
			}
		}
		else if (IsCovenant)
		{
			The.Game.SetStringGameState("SultanTerm", "Resheph");
		}
		else if (IsAccede)
		{
			The.Game.SetIntGameState("CodaRuined", 1);
			The.Game.SetIntGameState("CodaDespised", 1);
		}
		else
		{
			if (!IsLaunch)
			{
				return false;
			}
			The.Game.SetIntGameState("CodaStarshiibLocket", 1);
			if (!IsSuper)
			{
				if (IsArkOpened)
				{
					The.Game.SetIntGameState("CodaPlagued", 1);
				}
				else
				{
					The.Game.SetIntGameState("CodaRuined", 1);
				}
			}
		}
		if (IsSuper)
		{
			The.Game.SetStringGameState("CodaStatueMaterial", "Gold");
		}
		if (The.Game.HasFinishedQuest("Raising Indrix"))
		{
			The.Game.SetIntGameState("CodaAmaranthineDust", 1);
		}
		else
		{
			The.Game.SetIntGameState("CodaAmaranthinePrism", 1);
		}
		if (The.Game.HasQuest("If, Then, Else"))
		{
			if (The.Game.HasDelimitedGameState("TauElse", ',', "KilledByPlayer"))
			{
				The.Game.SetIntGameState("CodaDeadTau", 1);
			}
			else if (The.Game.HasIntGameState("ElseingComplete"))
			{
				The.Game.SetIntGameState("CodaTauNoLonger", 1);
			}
			else if (The.Game.HasDelimitedGameState("TauCompanion", ',', "Dead"))
			{
				The.Game.SetIntGameState("CodaWanderingTau", 1);
			}
		}
		if (The.Game.HasFinishedQuest("Love and Fear"))
		{
			The.Game.SetIntGameState("CodaReturnedSonnet", 1);
		}
		else if (The.Game.HasUnfinishedQuest("Love and Fear"))
		{
			The.Game.SetIntGameState("CodaFoundSonnet", 1);
		}
		if (IsStarshiib)
		{
			The.Game.SetIntGameState("CodaStarshiibLocket", 1);
		}
		EvaluateFactionState();
		return true;
	}

	public static void PickState()
	{
		switch (Popup.PickOption("Pick end game state", null, "", "Sounds/UI/ui_notification", new string[10] { "Return", "Return Ultra", "Covenant", "Covenant Ultra", "Accede", "Accede Ultra", "Launch", "Launch Ultra", "Marooned", "Marooned Ultra" }))
		{
		case 0:
			The.Game.SetStringGameState("EndType", "Return");
			break;
		case 1:
			The.Game.SetStringGameState("EndType", "Return");
			The.Game.SetStringGameState("EndGrade", "Super");
			break;
		case 2:
			The.Game.SetStringGameState("EndType", "Covenant");
			break;
		case 3:
			The.Game.SetStringGameState("EndType", "Covenant");
			The.Game.SetStringGameState("EndGrade", "Super");
			break;
		case 4:
			The.Game.SetStringGameState("EndType", "Accede");
			break;
		case 5:
			The.Game.SetStringGameState("EndType", "Accede");
			The.Game.SetStringGameState("EndGrade", "Super");
			break;
		case 6:
			The.Game.SetStringGameState("EndType", "Launch");
			break;
		case 7:
			The.Game.SetStringGameState("EndType", "Launch");
			The.Game.SetStringGameState("EndGrade", "Super");
			break;
		case 8:
			The.Game.SetStringGameState("EndType", "Marooned");
			break;
		case 9:
			The.Game.SetStringGameState("EndType", "Marooned");
			The.Game.SetStringGameState("EndGrade", "Super");
			break;
		}
		EvaluateState();
	}

	public static bool CheckMarooned()
	{
		if (IsArkOpened && IsGolemDead() && The.Game.HasGameState("StarshipLaunched"))
		{
			The.Game.SetStringGameState("EndType", "Marooned");
			if (NephalProperties.IsFoiled("Ehalcodon"))
			{
				The.Game.SetStringGameState("EndGrade", "Super");
			}
			Start();
			return true;
		}
		return false;
	}

	private static void EvaluateVisibilityProperty()
	{
		string stringGameState = The.Game.GetStringGameState("EndType");
		List<Faction> list = new List<Faction>();
		List<Faction> list2 = new List<Faction>();
		foreach (Faction item in Factions.GetList())
		{
			bool? flag = null;
			string stringProperty = item.GetStringProperty("EndVisibility");
			if (!stringProperty.IsNullOrEmpty())
			{
				if (bool.TryParse(stringProperty, out var result))
				{
					flag = result;
				}
				else if (stringProperty == "Conceal")
				{
					list.Add(item);
				}
				else if (!(stringProperty == "General"))
				{
					flag = ((!stringProperty.HasDelimitedSubstring(',', stringGameState)) ? new bool?(false) : new bool?(true));
				}
				else
				{
					list2.Add(item);
				}
			}
			string stringProperty2 = item.GetStringProperty("EndVisibilityState");
			if (!stringProperty2.IsNullOrEmpty())
			{
				bool flag2 = The.Game.TestGameState(stringProperty2);
				flag = ((!flag.HasValue) ? new bool?(flag2) : (flag2 & flag));
			}
			if (flag.HasValue)
			{
				item.Visible = flag.Value;
			}
		}
		list.Remove(GetBaseFaction());
		for (int num = Stat.Random(3, 4); num > 0; num--)
		{
			Faction faction = list.RemoveRandomElement();
			if (faction != null)
			{
				faction.Visible = false;
			}
		}
		for (int num2 = Stat.Random(3, 4); num2 > 0; num2--)
		{
			Faction faction2 = list2.RemoveRandomElement();
			if (faction2 != null)
			{
				faction2.Visible = true;
			}
		}
	}

	public static void EvaluateFactionState()
	{
		if (VillageCoda.IsRuined)
		{
			foreach (Faction item in Factions.GetList())
			{
				string stringProperty = item.GetStringProperty("EndVisibility");
				item.Visible = stringProperty == "true" || stringProperty.HasDelimitedSubstring(',', "Ruined");
			}
			return;
		}
		EvaluateCults();
		EvaluateVillageFactions();
		EvaluateSpecific();
		EvaluateVisibilityProperty();
	}

	private static void EvaluateCults()
	{
		BallBag<Faction> ballBag = new BallBag<Faction>
		{
			{
				Factions.Get("SultanCult1"),
				400
			},
			{
				Factions.Get("SultanCult2"),
				350
			},
			{
				Factions.Get("SultanCult3"),
				300
			},
			{
				Factions.Get("SultanCult4"),
				250
			},
			{
				Factions.Get("SultanCult5"),
				200
			}
		};
		for (int num = 3; num > 0; num--)
		{
			ballBag.PickOne().Visible = false;
		}
	}

	private static void EvaluateVillageFactions()
	{
		List<Faction> list = new List<Faction>();
		foreach (Faction item in Factions.GetList())
		{
			if (item.GetIntProperty("Village") == 1)
			{
				list.Add(item);
			}
		}
		list.ShuffleInPlace();
		int i = 0;
		for (int num = list.Count / 2; i < num; i++)
		{
			Faction faction = list[i];
			faction.DisplayName = "villagers of " + NameStyles.NameStyleTable["Chiliad Qudish Site"].Generate(null, null, null, null, null, null, null, null, null, null, null, "Site");
			VillageBase.SetVillageFactionEmblem(faction, faction.DisplayName);
		}
	}

	private static void EvaluateSpecific()
	{
		Faction faction = Factions.Get("Barathrumites");
		if (IsCovenant && The.Game.HasBooleanGameState("GemaraSophiaSharedGritGate"))
		{
			faction.DisplayName = "Synagogue at Grit Gate";
			faction.Plural = false;
		}
		else if (IsCovenant && The.Game.HasBooleanGameState("GemaraSophiaSharedMe"))
		{
			faction.DisplayName = "Tabernacle at Grit Gate";
			faction.Plural = false;
		}
		else if (IsCovenant)
		{
			faction.DisplayName = "monastery at Grit Gate";
			faction.Plural = false;
		}
		else if ((IsLaunch && IsSuper) || IsMarooned)
		{
			faction.DisplayName = "Tinkers' Guild";
			faction.Plural = false;
		}
		else if (IsReturn)
		{
			faction.DisplayName = "denizens of the Grit Gate Freehold";
		}
		Faction faction2 = Factions.Get("Templar");
		if (GetBaseFaction() != faction2)
		{
			faction2.Visible = false;
			faction.Interests.RemoveAll((FactionInterest x) => x.Tags == "templar,lair");
		}
		faction2 = Factions.Get("Mechanimists");
		faction2.DisplayName = "Chromaic Church";
		faction2 = Factions.Get("Hindren");
		string value;
		if (!The.Game.HasBooleanGameState("HindrenQuestFullyResolved") || The.Game.HasBooleanGameState("HindrenVillageDoomed"))
		{
			faction2.Visible = false;
		}
		else if (The.Game.StringGameState.TryGetValue("HindrenMysteryOutcomeHindriarch", out value))
		{
			if (value == "Keh")
			{
				faction2.DisplayName = "Hindriarchy of Bey Lah";
			}
			else if (value == "Esk")
			{
				faction2.DisplayName = "denizens of the Bey Lah Freehold";
			}
		}
		if (The.Game.HasFinishedQuest("Raising Indrix"))
		{
			Factions.Get("Chiliad Mamon").Visible = true;
		}
		if (IsSuper)
		{
			faction2 = Factions.Get("Girsh");
			faction2.Visible = false;
			faction2 = Factions.Get("Gyre Wights");
			faction2.Visible = false;
			if (NephalProperties.AnyPacified())
			{
				faction2 = Factions.Get("Nephilim");
				faction2.Visible = true;
			}
		}
	}

	public static void EvaluateFactionReputation()
	{
		foreach (Faction item in Factions.GetList())
		{
			int num = item.GetIntProperty("EndReputation", int.MinValue);
			if (num == int.MinValue)
			{
				int intProperty = item.GetIntProperty("EndReputationMin", int.MinValue);
				int intProperty2 = item.GetIntProperty("EndReputationMax", int.MinValue);
				num = ((intProperty != int.MinValue && intProperty2 != int.MinValue) ? ((int)Math.Round((double)Stat.Roll(intProperty, intProperty2) / 100.0) * 100) : item.InitialPlayerReputation);
			}
			The.Game.PlayerReputation.Set(item, num);
		}
	}
}
