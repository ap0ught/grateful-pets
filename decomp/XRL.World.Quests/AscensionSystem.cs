using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genkit;
using Qud.API;
using Qud.UI;
using UnityEngine;
using XRL.Rules;
using XRL.Sound;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.AI.Pathfinding;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Quests.GolemQuest;

namespace XRL.World.Quests;

[Serializable]
[HasWishCommand]
public class AscensionSystem : IQuestSystem
{
	public static bool DISABLE_ANIMATIONS;

	public const string BARATHRUM_STEP_ID = "Barathrum";

	public const string ASCEND_STEP_ID = "Ascend";

	public const string ARRIVAL_ZONE = "NorthSheva.39.13.1.0.10";

	public const string MILESTONE_KEY = "AscentMilestone";

	public const string ADVANCE_KEY = "AscentAdvance";

	public const int STAGE_START = 0;

	public const int STAGE_RECLAMATION = 1;

	public const int STAGE_ASCENDING = 2;

	public const int STAGE_ARRIVED = 3;

	public int Stage;

	public long ArrivalTime;

	public int ClimberID;

	[NonSerialized]
	private HashSet<string> _SpindleZones;

	[NonSerialized]
	private GameObject _Climber;

	public override bool RemoveWithQuest => false;

	private HashSet<string> SpindleZones
	{
		get
		{
			if (_SpindleZones == null)
			{
				_SpindleZones = new HashSet<string>(25);
				foreach (Location2D item in Zone.zoneIDTo240x72Location("JoppaWorld.53.3.1.1.10").YieldAdjacent(2))
				{
					_SpindleZones.Add(ZoneID.Assemble("JoppaWorld", item));
				}
			}
			return _SpindleZones;
		}
	}

	public GameObject Climber
	{
		get
		{
			if (!GameObject.Validate(ref _Climber) && ClimberID != 0)
			{
				_Climber = The.ZoneManager.FindObjectByID(ClimberID);
			}
			return _Climber;
		}
		set
		{
			_Climber = value;
			ClimberID = value?.BaseID ?? 0;
		}
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(SingletonEvent<EndTurnEvent>.ID);
		Registrar.Register(ZoneActivatedEvent.ID);
		Registrar.Register(AfterConversationEvent.ID);
		Registrar.Register(PooledEvent<GenericQueryEvent>.ID);
		Registrar.Register(PooledEvent<GenericNotifyEvent>.ID);
	}

	public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
	{
		Registrar.Register(EnteringZoneEvent.ID);
		Registrar.Register(PooledEvent<BeforePilotChangeEvent>.ID);
	}

	public override bool HandleEvent(EnteringZoneEvent E)
	{
		if (Stage == 2 && E.Origin?.ParentZone == The.ActiveZone && E.Cell.ParentZone != The.ActiveZone && !E.System)
		{
			return E.Object.ShowFailure("You can't leave the golem while it is ascending.");
		}
		if (E.Object.IsPlayer())
		{
			if (Stage == 0)
			{
				if (Game.HasQuest("Reclamation"))
				{
					Stage = 1;
				}
				else if (SpindleZones.Contains(E.Cell.ParentZone.ZoneID))
				{
					The.Game.StartQuest("Reclamation");
					Stage = 1;
				}
			}
			else if (Stage == 1 && Game.HasFinishedQuest("Reclamation") && E.Cell.ParentZone.ZoneID == "JoppaWorld.53.3.1.1.10")
			{
				CheckpointingSystem.ManualCheckpoint();
			}
		}
		else if (E.Object.Blueprint == "Barathrum")
		{
			if (Quest == null || Quest.Finished || Stage >= 2)
			{
				E.Object.UnregisterEvent(this, EnteringZoneEvent.ID);
			}
			else if (!(E.Cell.ParentZone is InteriorZone interiorZone) || !interiorZone.ParentObject.HasTagOrProperty("Golem"))
			{
				if (Quest.IsStepFinished("Barathrum"))
				{
					The.Game.FailQuestStep(Quest, "Barathrum", ShowMessage: false);
				}
			}
			else
			{
				The.Game.FinishQuestStep(Quest, "Barathrum");
			}
		}
		return base.HandleEvent(E);
	}

	public bool CanFallbackWait()
	{
		if (Game.HasBooleanGameState("ReadTornSheet") || Game.HasBooleanGameState("SawCrimePunishment") || Game.HasBooleanGameState("MakHatesResheph") || Game.HasBooleanGameState("UrsineApprenticeMentioned") || (Game.HasBooleanGameState("TalkedToYlaHaj") && JournalAPI.HasSultanNoteWithTag("rebekahWasHealer")))
		{
			return false;
		}
		if (Game.HasBooleanGameState("WaitArrivePartial1") && Game.HasBooleanGameState("WaitArrivePartial2") && Game.HasBooleanGameState("WaitArrivePartial3"))
		{
			return Game.HasBooleanGameState("WaitArrivePartial4");
		}
		return false;
	}

	public override bool HandleEvent(AfterConversationEvent E)
	{
		if (Stage == 2 && Game.TimeTicks < ArrivalTime && (E.Conversation.HasState("WaitArrive") || CanFallbackWait()) && Popup.ShowYesNo("The control pit dissolves to silence.\n\nDo you want to wait until the ascent is done?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.Cancel) == DialogResult.Yes)
		{
			AutoAct.Setting = ":" + (Game.Turns + (ArrivalTime - Game.TimeTicks) + 1);
			The.Player.PassTurn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericQueryEvent E)
	{
		if (E.Query == "AscendQueryGolem" && E.Subject.TryGetPart<Interior>(out var Part) && !GameObject.Validate(Part.Zone.FindObject("Barathrum")) && Popup.ShowYesNo("Barathrum is not in the golem. Ascend anyway?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.No) != DialogResult.Yes)
		{
			E.Result = false;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericNotifyEvent E)
	{
		if (E.Notify == "AscendNotifyGolem" && E.Subject.TryGetPart<Vehicle>(out var Part))
		{
			SoundManager.PlaySound("sfx_grenade_highExplosive_explode");
			CombatJuice.cameraShake(3f);
			FadeToBlack.FadeOut(2f);
			Thread.Sleep(2000);
			SoundManager.PlaySound("sfx_interact_generictechitem_activate");
			Thread.Sleep(1000);
			SoundManager.PlaySound("sfx_grenade_highExplosive_explode");
			Thread.Sleep(2000);
			SoundManager.PlaySound("sfx_interact_generictechitem_activate");
			Thread.Sleep(2000);
			CombatJuice.cameraShake(3f);
			SoundManager.PlaySound("sfx_grenade_highExplosive_explode");
			Part.Pilot = null;
			Stage = 2;
			Climber = E.Subject;
			Climber.RequirePart<NoDamage>();
			ArrivalTime = Game.TimeTicks + 500;
			The.Game.SetIntGameState("StarfreightAscending", 1);
			SoundManager.PlayMusic("Lattice Loops First");
			LeaveBehindFollowers();
			GameObject barathrum = GetBarathrum();
			if (barathrum != null && barathrum.IsPlayerLed())
			{
				barathrum.Brain.Goals.Clear();
				barathrum.RemovePartsDescendedFrom<AIBehaviorPart>();
				barathrum.RequirePart<PriorityChat>();
				barathrum.RequirePart<AIBarathrumShuttle>().PushGoal(barathrum);
			}
			else
			{
				Game.SetIntGameState("StarfreightAskWait", 1);
			}
			StartAscendAnimation();
			FadeToBlack.FadeIn(3f);
		}
		else if (E.Notify == "AscentMilestone")
		{
			PlayLatticeSecond();
		}
		else if (E.Notify == "AscentAdvance")
		{
			The.Game.TimeTicks = Math.Min(The.Game.TimeTicks + 100, ArrivalTime);
			The.Core.RenderBase();
		}
		return base.HandleEvent(E);
	}

	public void StartAscendAnimation()
	{
		The.ActiveZone.GetCell(0, 0).AddObject("Widget").AddPart<AscensionRenderer>();
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (Stage == 2)
		{
			if (The.Game.GetBooleanGameState("AscentMilestone"))
			{
				SoundManager.PlayMusic("Lattice Loops Second");
			}
			else
			{
				SoundManager.PlayMusic("Lattice Loops First");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Stage == 2)
		{
			if (Game.TimeTicks >= ArrivalTime)
			{
				Stage = 3;
				The.Game.RemoveIntGameState("StarfreightAscending");
				The.Game.RemoveIntGameState("StarfreightAskWait");
				Arrival();
			}
			else if (Game.HasIntGameState("StarfreightAskWait") && ArrivalTime - Game.TimeTicks <= 490 && !AutoAct.IsActive())
			{
				The.Game.RemoveIntGameState("StarfreightAskWait");
				if (Popup.ShowYesNo("Do you want to wait until the ascent is done?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.Cancel) == DialogResult.Yes)
				{
					AutoAct.Setting = ":" + (Game.Turns + (ArrivalTime - Game.TimeTicks) + 1);
					The.Player.PassTurn();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforePilotChangeEvent E)
	{
		if (Stage == 2 && E.NewPilot != null && E.NewPilot.IsPlayer() && E.Vehicle.HasTagOrProperty("Golem"))
		{
			return E.NewPilot.ShowFailure("You can't pilot the golem while it is ascending.");
		}
		return base.HandleEvent(E);
	}

	public override void Start()
	{
		GameObject gameObject = The.ActiveZone.FindFirstObject("Barathrum");
		if (gameObject == null)
		{
			MetricsManager.LogError("StarfreightQuest::Could not find Barathrum to set up quest tracking.");
			return;
		}
		gameObject.RegisterEvent(this, EnteringZoneEvent.ID, 0, Serialize: true);
		gameObject.SetAlliedLeader<AllyAscend>(The.Player);
	}

	public override void Finish()
	{
		The.ActiveZone.FindFirstObject("Barathrum")?.UnregisterEvent(this, EnteringZoneEvent.ID);
	}

	public void Arrival()
	{
		AutoAct.Interrupt();
		Climber?.RemovePart<NoDamage>();
		The.Game.SetBooleanGameState("SpindleAscended", Value: true);
		InteriorZone interior = (InteriorZone)The.ActiveZone;
		Location2D location = interior.ParentObject.CurrentCell.Location;
		Zone zone = The.ZoneManager.GetZone("NorthSheva.39.13.1.0.10");
		List<GameObject> list = zone.FindObjectsWithPart("AscensionCable");
		GameObject selected = list.GetRandomElement();
		int num = int.MaxValue;
		foreach (GameObject item in list)
		{
			int num2 = item.CurrentCell.Location.Distance(location);
			if (num2 < num)
			{
				selected = item;
				num = num2;
			}
		}
		The.Core.RenderBase();
		GameObject barathrum = null;
		bool barathrumAvailable = false;
		if (!DISABLE_ANIMATIONS)
		{
			GameManager.Instance.PushGameView("Cinematic");
			AmbientSoundsSystem.StopAmbientBeds();
			Task.Run(() => StarfreightClimbAnimation.RunSpindleIntroSequence(() =>
			{
				PlayerArrive();
			})).Wait();
			The.Core.RenderDelay(9000, Interruptible: false);
			AmbientSoundsSystem.PlayAmbientBeds(zone);
			FadeToBlack.FadeIn(3f);
			SoundManager.PlayMusic("Music/Another World");
			The.Core.RenderDelay(9000, Interruptible: false);
			GameManager.Instance.PopGameView();
		}
		else
		{
			PlayerArrive();
			The.Core.RenderBase();
		}
		The.Game.FinishQuestStep(Quest, "Ascend", -1, CanFinishQuest: false);
		The.Game.FinishQuest(Quest);
		if (barathrumAvailable)
		{
			BarathrumStartConversation(barathrum);
			Preacher preacher = barathrum.RequirePart<Preacher>();
			preacher.ChatWait = Stat.Random(20, 30);
			preacher.Lines = new string[3] { "Heh heh heh!", "Ho, ho!", "Heh, heh!" };
		}
		The.Player.ForfeitTurn();
		void PlayerArrive()
		{
			interior.MarkActive();
			interior.ParentObject.SystemMoveTo(zone.GetCell(selected.CurrentCell.X, selected.CurrentCell.Y - 1));
			The.Player.SystemMoveTo(zone.GetCell(selected.CurrentCell.X, selected.CurrentCell.Y - 2));
			barathrum = GetBarathrum();
			barathrumAvailable = barathrum != null && (barathrum.CurrentZone.ResolveZoneWorld() == "NorthSheva" || barathrum.IsPlayerLed());
			if (barathrumAvailable)
			{
				barathrum.SystemMoveTo(zone.GetCell(selected.CurrentCell.X + 1, selected.CurrentCell.Y - 2));
			}
		}
	}

	public void LeaveBehindFollowers()
	{
		GameObject player = The.Player;
		foreach (var (_, zone2) in The.ZoneManager.CachedZones)
		{
			if (zone2 is InteriorZone interiorZone && interiorZone.ParentObject.HasTagOrProperty("Golem"))
			{
				continue;
			}
			Zone.ObjectEnumerator enumerator2 = zone2.IterateObjects().GetEnumerator();
			while (enumerator2.MoveNext())
			{
				GameObject current = enumerator2.Current;
				if (current.PartyLeader == player && !current.HasTagOrProperty("Golem"))
				{
					current.PartyLeader = null;
				}
			}
		}
	}

	public GameObject GetBarathrum()
	{
		foreach (KeyValuePair<string, Zone> cachedZone in The.ZoneManager.CachedZones)
		{
			cachedZone.Deconstruct(out var _, out var value);
			Zone.ObjectEnumerator enumerator2 = value.IterateObjects().GetEnumerator();
			while (enumerator2.MoveNext())
			{
				GameObject current = enumerator2.Current;
				if (!(current.Blueprint != "Barathrum"))
				{
					return current;
				}
			}
		}
		return null;
	}

	public void BarathrumStartConversation(GameObject Object)
	{
		Cell currentCell = Object.CurrentCell;
		Cell playerCell = The.PlayerCell;
		if (!currentCell.IsAdjacentTo(playerCell))
		{
			FindPath findPath = new FindPath(currentCell, playerCell);
			if (findPath.Found)
			{
				int i = 0;
				for (int num = findPath.Directions.Count - 1; i < num && Object.Move(findPath.Directions[i], Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: true, DoConfirmations: true, null, null, NearestAvailable: false, 0); i++)
				{
					The.Core.RenderDelay(500, Interruptible: false);
				}
			}
		}
		if (Object.TryGetPart<ConversationScript>(out var Part))
		{
			Part.AttemptConversation(Silent: true);
			Object.RemovePart<PriorityChat>();
			Object.Brain.SetPartyLeader(null);
			Object.Brain.Goals.Clear();
			AIBarathrumShuttle aIBarathrumShuttle = Object.GetPart<AIBarathrumShuttle>();
			if (aIBarathrumShuttle == null)
			{
				Object.RemovePartsDescendedFrom<AIBehaviorPart>();
				aIBarathrumShuttle = Object.AddPart<AIBarathrumShuttle>();
			}
			aIBarathrumShuttle.Stage = 0;
			aIBarathrumShuttle.PushGoal(Object);
			Popup.Show(Object.Does("have") + " left your party.");
		}
	}

	public static void WishSetBarathrumState()
	{
		The.Game.SetBooleanGameState("ReadTornSheet", Value: true);
		The.Game.SetBooleanGameState("SawCrimePunishment", Value: true);
		The.Game.SetBooleanGameState("MakHatesResheph", Value: true);
		The.Game.SetBooleanGameState("UrsineApprenticeMentioned", Value: true);
		The.Game.SetBooleanGameState("TalkedToYlaHaj", Value: true);
		JournalAPI.GetFirstSultanNoteWithTag("rebekahWasHealer")?.Reveal(null, Silent: true);
	}

	[WishCommand("starfreight:spindle", null)]
	public static void WishStart()
	{
		XRLGame game = The.Game;
		GameObject body = game.Player.Body;
		Zone zone = The.ZoneManager.GetZone("JoppaWorld.53.3.1.1.10");
		Popup.Suppress = true;
		ItemNaming.Suppress = true;
		body.SystemMoveTo(zone.GetPullDownLocation(body));
		if (body.CurrentZone.FindObject("Barathrum") == null)
		{
			body.CurrentCell.getClosestPassableCell().AddObject("Barathrum");
		}
		ReclamationSystem.WishStage();
		WishSetBarathrumState();
		if (!game.HasFinishedQuest("The Golem"))
		{
			try
			{
				GolemQuestSelection.WishRandomEffectsGolem();
				Popup.Suppress = true;
				game.CompleteQuest("The Golem");
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Starfreight:Start", x);
			}
		}
		game.StartQuest("We Are Starfreight");
		game.CompleteQuest("Reclamation");
		Popup.Suppress = false;
		ItemNaming.Suppress = false;
	}

	[WishCommand("starfreight:arrive", null)]
	public static void WishArrive()
	{
		AscensionSystem system = The.Game.GetSystem<AscensionSystem>();
		if (system != null && system.Stage == 2)
		{
			The.Game.TimeTicks = Math.Max(The.Game.TimeTicks, system.ArrivalTime);
			The.Player.ForfeitTurn();
		}
	}

	[WishCommand("starfreight:arrivefast", null)]
	public static void WishArriveFast()
	{
		DISABLE_ANIMATIONS = true;
		WishArrive();
	}

	[WishCommand("starfreight:anim", null)]
	public static void WishAnimations()
	{
		DISABLE_ANIMATIONS = !DISABLE_ANIMATIONS;
	}

	[WishCommand("arrivaltest", null)]
	public void ArrivalTest()
	{
		Task.Run(() => StarfreightClimbAnimation.RunSpindleIntroSequence(() =>
		{
		})).Wait();
		The.Core.RenderDelay(6000, Interruptible: false);
	}

	[WishCommand("lattice1", null)]
	public static void PlayLatticeFirst()
	{
		SoundManager.PlayMusic("Lattice Loops First");
	}

	[WishCommand("lattice2", null)]
	public static void PlayLatticeSecond()
	{
		GameManager.Instance.uiQueue.queueTask(async () =>
		{
			AudioEntrySet set = SoundManager.GetClipSet("Lattice Loops Second");
			if (!set.Initialized)
			{
				await set.Load();
			}
			AudioClip clip = set.Next().Clip;
			MusicSource musicSource = SoundManager.RequireMusicSource("music");
			MusicSource musicSource2 = musicSource;
			AudioSource audio = musicSource.Audio;
			float time = audio.time;
			int timeSamples = audio.timeSamples;
			if (audio.isPlaying)
			{
				SoundManager.MusicSources.Remove("music");
				musicSource2.Fade.StartFade(4f);
				musicSource2.Channel = "Crossfade";
				musicSource = SoundManager.RequireMusicSource("music", Reset: true);
				audio = musicSource.Audio;
			}
			musicSource.Track = "Lattice Loops Second";
			musicSource.EntrySet = set;
			musicSource.TargetVolume = musicSource2.TargetVolume;
			audio.Stop();
			audio.volume = 0f;
			audio.clip = clip;
			audio.timeSamples = timeSamples % clip.samples;
			audio.Play();
			musicSource.EventTime = AudioSettings.dspTime + (double)clip.length - (double)(time % clip.length);
		});
	}
}
