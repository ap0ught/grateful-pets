using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using Qud.API;
using Qud.UI;
using UnityEngine;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Anatomy;
using XRL.World.Conversations.Parts;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class ThinWorld : IPart
{
	private FastNoise fastNoise = new FastNoise();

	public int seed;

	private List<int> yhitch = new List<int>();

	private List<Location2D> leftovers = new List<Location2D>();

	private bool triggered;

	private long triggertime;

	private float timer;

	private int xoffset;

	private int yoffset;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public static void SetStateKnowAboutSalum()
	{
		The.Game.SetIntGameState("KnowAboutSalum", 1);
	}

	[WishCommand("crossintobrightsheol", null)]
	public static bool CrossIntoBrightsheol()
	{
		if (!string.Equals(WorldFactory.Factory.getWorld(The.Player.GetCurrentCell().ParentZone.ZoneWorld).Protocol, "THIN"))
		{
			Popup.Show("You cannot cross into Brightsheol from this place.");
		}
		if (Popup.AskString("Crossing into Brightsheol will retire your character. Are you sure you want to do it? Type 'CROSS' to confirm.", "", "Sounds/UI/ui_notification", null, null, 5, 0, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false).ToUpper() == "CROSS")
		{
			GameManager.Instance.PushGameView("GameSummary");
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			scrapBuffer.RenderBase();
			scrapBuffer.Draw();
			scrapBuffer.Draw();
			FadeToBlack.FadeOut(8f, new Color(1f, 1f, 1f));
			Thread.Sleep(13000);
			The.Game.SetStringGameState("EndType", "Brightsheol");
			JournalAPI.AddAccomplishment("You crossed into Brightsheol.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
			float? num = 1f;
			float? to = 1f;
			Color? toColor = new Color(0f, 0f, 0f, 1f);
			FadeToBlack.Fade(3f, num, to, null, toColor);
			Thread.Sleep(3000);
			EndGame.UnlockAchievement();
			EndGame.RunCredits();
			FadeToBlack.FadeIn(3f);
			The.Game.DeathReason = "You crossed into Brightsheol.";
			The.Game.Running = false;
			return true;
		}
		return false;
	}

	public static void RunThinWorldIntroSequence(bool express = false)
	{
		Stopwatch stopwatch = new Stopwatch();
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		List<string> list = new List<string>
		{
			"..................", "", "", "         .   .          .                                    ", "   .     |\\  |  o       |                                .   ", " __|__   | \\ |  .  .--. |--. .  . .--..--. .-.  .-..   __|__ ", "   |     |  \\|  |  |  | |  | |  | |   `--.(   )(   |     |   ", "   '     '   '-' `-'  `-'  `-`--`-'   `--' `-'`-`-`|     '   ", "                                                ._.'         ", "",
			"", " ê 00000000 CRK SHELL", " ê 00000010 GET SEED", " ê 00000020 MOV BSHEOL", " ê 00000030 PUT SEED", " ê 00000040 GRW"
		};
		stopwatch.Start();
		for (int i = 0; i < list.Count; i++)
		{
			scrapBuffer.ScrollUp();
			for (int j = 0; j < list[i].Length; j++)
			{
				scrapBuffer.Goto(j, 24);
				scrapBuffer.Write("&c" + list[i][j]);
				if (stopwatch.ElapsedMilliseconds % 500 < 250 && j != list[i].Length - 1)
				{
					scrapBuffer.Write("&C_");
				}
				GameManager.Instance.uiQueue.queueTask(() =>
				{
					SoundManager.PlaySound("sub_bass_mouseover1");
				});
				Thread.Sleep(Math.Min(70, 1400 / list[i].Length));
				scrapBuffer.Draw();
			}
			Thread.Sleep(50);
		}
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			SoundManager.PlaySound("sfx_sultanSarcophagus_consciousness_uploaded");
		});
		stopwatch.Stop();
		ScreenBuffer scrapBuffer2 = ScreenBuffer.GetScrapBuffer2();
		The.Core.RenderBaseToBuffer(scrapBuffer2);
		Stopwatch stopwatch2 = new Stopwatch();
		stopwatch2.Start();
		int num = 0;
		UnityEngine.Debug.Log("starting sequence");
		int num2 = 17000;
		if (express)
		{
			num2 = 0;
		}
		StringBuilder stringBuilder = new StringBuilder();
		while (stopwatch2.ElapsedMilliseconds < num2)
		{
			if (FadeToBlack.stage == FadeToBlack.FadeToBlackStage.FadedIn && stopwatch2.ElapsedMilliseconds > num2 - 2500)
			{
				UnityEngine.Debug.Log("starting fade");
				FadeToBlack.FadeOut(2.4f);
			}
			num++;
			scrapBuffer.ScrollUp();
			int num3 = Stat.Random(0, 79);
			scrapBuffer.Goto(num3, scrapBuffer.Height - 1);
			for (int num4 = num3; num4 < 80; num4++)
			{
				scrapBuffer.Write("&c" + (char)Stat.RandomCosmetic(1, 254));
			}
			for (int num5 = Math.Max(0, num); num5 < scrapBuffer.Height - 1; num5++)
			{
				stringBuilder.Length = 0;
				stringBuilder.Append("&c");
				stringBuilder.Append((char)Stat.RandomCosmetic(1, 254));
				scrapBuffer.Write(stringBuilder);
			}
			if (num > 80)
			{
				for (int num6 = 0; num6 < scrapBuffer.Width; num6++)
				{
					for (int num7 = 0; num7 < scrapBuffer.Height; num7++)
					{
						if (Stat.RandomCosmetic(0, 10000) <= (stopwatch2.ElapsedMilliseconds - 7000) / 400)
						{
							scrapBuffer[num6, num7].Copy(scrapBuffer2[num6, num7]);
							scrapBuffer[num6, num7].SetBackground('k');
							if (Stat.RandomCosmetic(1, 100) <= 80)
							{
								scrapBuffer[num6, num7].SetForeground('b');
							}
							else
							{
								scrapBuffer[num6, num7].SetForeground('B');
							}
						}
					}
				}
			}
			if (num < 100)
			{
				Thread.Sleep(Math.Max(0, 100 - num));
			}
			else
			{
				Thread.Sleep(10);
			}
			scrapBuffer.Draw();
		}
		while (FadeToBlack.stage == FadeToBlack.FadeToBlackStage.FadingOut)
		{
		}
		scrapBuffer.Clear();
		scrapBuffer.Draw();
		Thread.Sleep(5000);
		FadeToBlack.FadeIn(0f);
		Popup.Show("The darkness recedes, and a new light breaks on the shores of your mind.", null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: false);
		JournalAPI.AddAccomplishment("You crossed into the Thin World, rarefied, and stood before the gate to Brightsheol.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
		SoundManager.PlayMusic("Attenuated Signals");
		scrapBuffer2.Draw();
		FadeToBlack.FadeOut(0f);
		FadeToBlack.FadeIn(6f);
		scrapBuffer2.Draw();
		GameManager.Instance.PopGameView();
	}

	public static void TransitToThinWorld(GameObject sender, bool express = false)
	{
		Thread.Sleep(10);
		XRLGame game = The.Game;
		GameObject oldPlayer = The.Player;
		List<GameObject> objects = oldPlayer.GetCurrentCell().ParentZone.GetObjects((GameObject o) => o.PartyLeader == oldPlayer);
		oldPlayer.RemoveEffect<AmbientRealityStabilized>();
		game.SetInt64GameState("thinWorldTime", Calendar.TotalTimeTicks);
		game.SetStringGameState("thinWorldPlayerBody", game.ZoneManager.CacheObject(oldPlayer, cacheTwiceOk: false, replaceIfAlreadyCached: true));
		game.SetIntGameState("thinWorldFollowerCount", objects.Count);
		for (int num = 0; num < objects.Count; num++)
		{
			game.SetStringGameState("thinWorldFollower" + num, game.ZoneManager.CacheObject(objects[num], cacheTwiceOk: false, replaceIfAlreadyCached: true));
			objects[num].CurrentCell = null;
			objects[num].MakeInactive();
		}
		GameObject who = oldPlayer.DeepCopy(CopyEffects: false, CopyID: true);
		sender.GetPart<Enclosing>().EnterEnclosure(who);
		GameManager.Instance.PushGameView(Options.StageViewID);
		Cell cell = oldPlayer.CurrentCell;
		oldPlayer.CurrentCell.RemoveObject(oldPlayer);
		if (!express)
		{
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			cell.ParentZone.Render(scrapBuffer);
			CombatJuice.cameraShake(0.25f);
			scrapBuffer.Draw();
			Thread.Sleep(10);
			GameManager.Instance.uiQueue.queueTask(() =>
			{
				SoundManager.PlaySound("Sounds/Misc/sfx_ancientDoor_close");
			});
		}
		oldPlayer.MakeInactive();
		if (!express)
		{
			Popup.Show("The colossal lid slams shut. Darkness engulfs you.");
			Thread.Sleep(3000);
			SoundManager.PlaySound("sfx_sultanSarcophagus_crush");
			SoundManager.StopMusic("music", Crossfade: false);
			CombatJuice._cameraShake(0.5f);
			Thread.Sleep(500);
			Popup.ShowSpace("You died.\n\nEntombed in the burial chamber of Resheph, the Last Sultan.", null, "Sounds/UI/ui_notification_death");
			JournalAPI.AddAccomplishment("You died and were entombed in the burial chamber of Resheph, the Last Sultan.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
		}
		else
		{
			SoundManager.StopMusic("music", Crossfade: false);
		}
		The.Core.BuildScore(Real: false, "You were entombed in the burial chamber of Resheph, the Last Sultan.");
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			SoundManager.PlaySound("Sounds/Damage/sfx_damage_stone");
		});
		GameManager.Instance.PushGameView("GameSummary");
		new Stopwatch();
		ScreenBuffer scrapBuffer2 = ScreenBuffer.GetScrapBuffer1();
		for (int num2 = 0; num2 < scrapBuffer2.Height; num2++)
		{
			for (int num3 = 0; num3 < scrapBuffer2.Width; num3++)
			{
				if (Stat.RandomCosmetic(1, 100) <= 10)
				{
					scrapBuffer2[num3, num2].Char = (char)Stat.RandomCosmetic(0, 254);
				}
				Stat.RandomCosmetic(1, 100);
				_ = 10;
			}
		}
		for (int num4 = 0; num4 < 8; num4++)
		{
			scrapBuffer2.Draw();
			Keyboard.getch();
			for (int num5 = 0; num5 < scrapBuffer2.Height; num5++)
			{
				for (int num6 = 0; num6 < scrapBuffer2.Width; num6++)
				{
					if (Stat.RandomCosmetic(1, 100) <= 10)
					{
						scrapBuffer2[num6, num5].Char = (char)Stat.RandomCosmetic(0, 254);
					}
					Stat.RandomCosmetic(1, 100);
					_ = 10;
				}
			}
			CombatJuice.cameraShake(0.25f);
			scrapBuffer2.Draw();
			GameManager.Instance.uiQueue.queueTask(() =>
			{
				SoundManager.PlaySound("Sounds/Damage/sfx_damage_stone");
			});
		}
		The.ZoneManager.SetActiveZone("ThinWorld.6.6.10.6.6");
		GameObject gameObject = The.ZoneManager.ActiveZone.GetCell(40, 22).AddObject(oldPlayer.DeepCopy(CopyEffects: true, CopyID: true));
		gameObject.RequirePart<HologramMaterial>();
		gameObject.RequirePart<RebornOnDeathInThinWorld>();
		gameObject.SetIntProperty("NoStatusColor", 1);
		game.Player.Body = gameObject;
		gameObject.MakeActive();
		gameObject.Energy.BaseValue = 1000;
		RunThinWorldIntroSequence(express);
	}

	public static void SomethingGoesWrong(GameObject go)
	{
		XRL.World.Anatomy.Anatomy randomAnatomy = Anatomies.GetRandomAnatomy();
		int i = 0;
		for (int num = Stat.Random(1, 3); i < num; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				BodyPart randomElement = (from p in go.Body.GetParts()
					where p.Type != "Body" && (!(p.Type == "Head") || !p.Primary) && !p.Abstract
					select p).GetRandomElement();
				AnatomyPart randomElement2 = randomAnatomy.Parts.Where((AnatomyPart p) => p.Type.Type != "Body" && p.Type.Abstract == false).GetRandomElement();
				if (randomElement == null || randomElement2 == null)
				{
					continue;
				}
				BodyPart parentPart = randomElement.GetParentPart();
				if (randomElement.Equipped != null)
				{
					EquipmentAPI.UnequipObject(randomElement.Equipped);
					if (randomElement.Equipped == null)
					{
						randomElement.Dismember(Obliterate: true);
					}
				}
				switch (new BallBag<string>
				{
					{ "parentPart", 70 },
					{ "body", 15 },
					{ "random", 15 }
				}.PluckOne())
				{
				case "parentPart":
					randomElement2.ApplyTo(parentPart);
					break;
				case "body":
					randomElement2.ApplyTo(go.Body._Body);
					break;
				case "random":
					randomElement2.ApplyTo((from p in go.Body.GetParts()
						where !p.Abstract
						select p).GetRandomElement());
					break;
				}
				break;
			}
		}
	}

	public static GameObject ReturnBody(GameObject Object)
	{
		Popup.Suppress = true;
		The.ZoneManager.SetActiveZone(The.Game.GetStringGameState("Recorporealization_ZoneID"));
		Popup.Suppress = false;
		GameObject gameObject = The.ZoneManager.ActiveZone.FindObject("Full-Scale Recompositer");
		Cell cell = null;
		if (gameObject != null)
		{
			cell = gameObject.CurrentCell;
		}
		if (cell == null)
		{
			cell = The.ZoneManager.ActiveZone.GetEmptyCells().GetRandomElement();
		}
		if (cell == null)
		{
			cell = (from c in The.ZoneManager.ActiveZone.GetCells()
				where c.IsPassable()
				select c).GetRandomElement();
		}
		if (cell == null)
		{
			cell = The.ZoneManager.ActiveZone.GetCells().GetRandomElement();
		}
		GameObject body = The.Game.Player.Body;
		cell.AddObject(Object);
		The.Game.Player.Body = Object;
		Object.SetActive();
		Object.Energy.BaseValue = 1000;
		Object.ApplyEffect(new Dazed(Stat.Random(180, 220)));
		The.Game.TimeTicks = The.Game.GetInt64GameState("thinWorldTime", The.Game.TimeTicks) + 3600;
		if (body != Object)
		{
			body.MakeInactive();
			body.Obliterate();
		}
		MessageQueue.Suppress = true;
		Popup.Suppress = true;
		try
		{
			BodyPart body2 = The.Player.Body.GetBody();
			bool? dynamic = true;
			body2.AddPart("Floating Nearby", 0, null, null, null, null, null, null, null, null, null, null, null, null, null, dynamic);
			Stomach part = The.Player.GetPart<Stomach>();
			if (part != null)
			{
				part.Water = RuleSettings.WATER_PARCHED - 1;
			}
			The.Player.RemovePart<Tattoos>();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("exception cleaning the player body on recoming", x);
		}
		return gameObject;
	}

	public static void ReturnToQud()
	{
		XRLGame game = The.Game;
		GameObject body = The.Game.Player.Body;
		body = game.ZoneManager.peekCachedObject(game.GetStringGameState("thinWorldPlayerBody"));
		if (body == null)
		{
			body = The.Game.Player.Body;
		}
		if (game.ZoneManager.CachedObjects.ContainsKey(game.GetStringGameState("thinWorldPlayerBody")))
		{
			game.ZoneManager.CachedObjects[game.GetStringGameState("thinWorldPlayerBody")] = null;
			game.ZoneManager.CachedObjects.Remove(game.GetStringGameState("thinWorldPlayerBody"));
		}
		SoundManager.StopMusic("music", Crossfade: true, 5f);
		FadeToBlack.FadeOut(5f);
		Thread.Sleep(8000);
		GameObject gameObject = ReturnBody(body);
		The.Game.FinishQuestStep("Tomb of the Eaters", "Disable the Spindle's Magnetic Field");
		try
		{
			int intGameState = game.GetIntGameState("thinWorldFollowerCount");
			for (int i = 0; i < intGameState; i++)
			{
				string text = "thinWorldFollower" + i;
				GameObject gameObject2 = game.ZoneManager.peekCachedObject(game.GetStringGameState(text));
				if (gameObject2 != null)
				{
					Cell connectedSpawnLocation = The.Player.GetCurrentCell().GetConnectedSpawnLocation();
					if (connectedSpawnLocation != null)
					{
						game.ZoneManager.CachedObjects.Remove(game.GetStringGameState("thinWorldFollower" + i));
						connectedSpawnLocation.AddObject(gameObject2);
						if (gameObject2.PartyLeader != The.Player)
						{
							gameObject2.PartyLeader = The.Player;
						}
						gameObject2.MakeActive();
						gameObject2.ApplyEffect(new Dazed(Stat.Random(180, 220)));
					}
				}
				game.ZoneManager.UncacheObject(text);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("exception returning followers from brightsheol", x);
		}
		try
		{
			GameObject gameObject3 = The.ZoneManager.ActiveZone.FindObject("Recoming Reliquary");
			GameObject[] array;
			if (gameObject3 != null)
			{
				foreach (GameObject item in The.Player.GetInventoryAndEquipment())
				{
					if (item.Equipped == The.Player)
					{
						EquipmentAPI.UnequipObject(item);
					}
				}
				Inventory inventory = gameObject3.Inventory;
				array = The.Player.Inventory.Objects.ToArray();
				foreach (GameObject gameObject4 in array)
				{
					if (!inventory.Objects.Contains(gameObject4))
					{
						inventory.AddObject(gameObject4);
					}
				}
				The.Player.Inventory.Objects.Clear();
				return;
			}
			List<Cell> connectedSpawnLocations = The.Player.CurrentCell.GetConnectedSpawnLocations(6);
			if (connectedSpawnLocations == null || connectedSpawnLocations.Count <= 0)
			{
				return;
			}
			foreach (GameObject item2 in The.Player.GetInventoryAndEquipment())
			{
				if (item2.Equipped == The.Player)
				{
					EquipmentAPI.UnequipObject(item2);
				}
			}
			array = The.Player.Inventory.Objects.ToArray();
			foreach (GameObject gameObject5 in array)
			{
				connectedSpawnLocations.GetRandomElement().AddObject(gameObject5);
			}
			The.Player.Inventory.Objects.Clear();
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("ThinWorld return", x2);
		}
		finally
		{
			MessageQueue.Suppress = false;
			Popup.Suppress = false;
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			scrapBuffer.Clear();
			scrapBuffer.Draw();
			FadeToBlack.FadeIn(0f);
			Popup.Show("The matter of your new body starts to thicken and clot. You're inlaid with ribbons of bone, tissue, nerve, and flesh.", null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: false);
			if (gameObject == null)
			{
				Popup.Show("Something went wrong.", null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: false);
				try
				{
					SomethingGoesWrong(The.Player);
				}
				catch
				{
				}
			}
			scrapBuffer.Draw();
			FadeToBlack.FadeOut(0f);
			scrapBuffer.RenderBase();
			scrapBuffer.Draw();
			FadeToBlack.FadeIn(8f);
			Thread.Sleep(8000);
			Popup.Show("The neoteric body is differently charged. Tying inside you is another of the secret knots that bind the world.");
			Popup.Show("You gain an additional {{rules|Floating Nearby}} slot!");
			JournalAPI.AddAccomplishment("You were reconstituted in the material world with a stronger magnetic charge.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
			IComponent<GameObject>.TheGame.SetBooleanGameState("Recame", Value: true);
			Achievement.RECAME.Unlock();
			CheckpointingSystem.ManualCheckpoint(The.ActiveZone, "Gyl_Recame");
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		int num = 80;
		if (The.Player != null)
		{
			if (num > 0)
			{
				Cell cell = The.Player.CurrentCell;
				cell?.ParentZone?.AddLight(cell.X, cell.Y, num, LightLevel.Light);
			}
			if (num < 3)
			{
				Cell cell2 = The.Player.CurrentCell;
				cell2?.ParentZone?.AddExplored(cell2.X, cell2.Y, 3);
			}
		}
		return base.HandleEvent(E);
	}

	public double sampleSimplexNoise(string type, int x, int y, int z, int amplitude, float frequencyMultiplier = 1f)
	{
		if (seed == 0)
		{
			seed = Stat.Random(0, 2147483646);
		}
		fastNoise.SetSeed(seed);
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.25f * frequencyMultiplier);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(0.7f);
		fastNoise.SetFractalGain(1f);
		return Math.Ceiling((double)fastNoise.GetNoise(x, y, z) * (double)amplitude);
	}

	public double sampleSimplexNoiseRange(string type, int x, int y, int z, float low, float high, float frequencyMultiplier = 1f)
	{
		if (seed == 0)
		{
			seed = Stat.Random(0, 2147483646);
		}
		fastNoise.SetSeed(seed);
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.25f * frequencyMultiplier);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(0.7f);
		fastNoise.SetFractalGain(1f);
		return (fastNoise.GetNoise(x, y, z) + 1f) / 2f * (high - low) + low;
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (!triggered && (float)XRLCore.FrameTimer.ElapsedMilliseconds - timer >= 200f)
		{
			if (Stat.RandomCosmetic(1, 12) <= 1)
			{
				triggered = true;
				triggertime = XRLCore.FrameTimer.ElapsedMilliseconds;
				yhitch.Clear();
				int num = Stat.RandomCosmetic(-2, 2);
				for (int i = 0; i < 25; i++)
				{
					yhitch.Add(num);
					num += Stat.RandomCosmetic(-2, 2);
				}
			}
			timer = XRLCore.FrameTimer.ElapsedMilliseconds;
			xoffset = Stat.RandomCosmetic(-100000, 100000);
			yoffset = Stat.RandomCosmetic(-100000, 100000);
		}
		E.WantsToPaint = true;
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		for (int i = 0; i < buffer.Height; i++)
		{
			for (int j = 0; j < buffer.Width; j++)
			{
				try
				{
					char foregroundCode = buffer.Buffer[j, i].ForegroundCode;
					char backgroundCode = buffer.Buffer[j, i].BackgroundCode;
					char detailCode = buffer.Buffer[j, i].DetailCode;
					if (foregroundCode != 'b' && foregroundCode != 'B' && foregroundCode != 'k')
					{
						if (foregroundCode >= 'a' && foregroundCode <= 'z')
						{
							buffer.Buffer[j, i].SetForeground('b');
						}
						if (foregroundCode >= 'A' && foregroundCode <= 'Z')
						{
							buffer.Buffer[j, i].SetForeground('B');
						}
					}
					if (backgroundCode != 'b' && backgroundCode != 'B' && backgroundCode != 'k')
					{
						if (backgroundCode >= 'a' && backgroundCode <= 'z')
						{
							buffer.Buffer[j, i].SetBackground('b');
						}
						if (backgroundCode >= 'A' && backgroundCode <= 'Z')
						{
							buffer.Buffer[j, i].SetBackground('B');
						}
					}
					if (detailCode != 'b')
					{
						switch (detailCode)
						{
						case 'a':
						case 'b':
						case 'c':
						case 'd':
						case 'e':
						case 'f':
						case 'g':
						case 'h':
						case 'i':
						case 'j':
						case 'l':
						case 'm':
						case 'n':
						case 'o':
						case 'p':
						case 'q':
						case 'r':
						case 's':
						case 't':
						case 'u':
						case 'v':
						case 'w':
						case 'x':
						case 'y':
						case 'z':
							buffer.Buffer[j, i].SetDetail('b');
							break;
						case 'B':
						case 'k':
							continue;
						}
						if (detailCode >= 'A' && detailCode <= 'Z')
						{
							buffer.Buffer[j, i].SetDetail('B');
						}
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Thin world::OnPaint 1", x);
				}
			}
		}
		if (triggered)
		{
			long num = (The.Game.WallTime.ElapsedMilliseconds - triggertime) / 8;
			for (int k = 0; k < buffer.Height; k++)
			{
				for (int l = 0; l < buffer.Width; l++)
				{
					try
					{
						double num2 = (double)num - sampleSimplexNoiseRange("render", l + xoffset, k + yoffset + yhitch[k], 0, 0f, 160f, 0.25f) + (double)l + (double)(k / 10);
						if (!(num2 > 0.0))
						{
							continue;
						}
						if (num2 <= 3.0)
						{
							buffer.Buffer[l, k].Tile = null;
							buffer.Buffer[l, k].Char = buffer.Buffer[l, k].BackupChar;
							if (Stat.RandomCosmetic(1, 100) <= 50)
							{
								buffer.Buffer[l, k].SetForeground('B');
							}
							if (Stat.RandomCosmetic(1, 100) <= 50)
							{
								buffer.Buffer[l, k].SetForeground('b');
							}
							if (Stat.Random(1, 20) <= 1)
							{
								leftovers.Add(Location2D.Get(l, k));
							}
						}
						if (num2 <= 6.0)
						{
							buffer.Buffer[l, k].SetForeground('B');
						}
						if (num2 <= 9.0 && Stat.RandomCosmetic(1, 100) <= 50)
						{
							buffer.Buffer[l, k].SetForeground('b');
						}
					}
					catch (Exception x2)
					{
						MetricsManager.LogException("Thin world::OnPaint 2", x2);
					}
				}
			}
			if (num > 160)
			{
				triggered = false;
			}
		}
		if (leftovers.Count > 0)
		{
			try
			{
				foreach (Location2D leftover in leftovers)
				{
					if (Stat.Random(1, 100) <= 40)
					{
						buffer[leftover.X, leftover.Y].Tile = null;
						buffer[leftover.X, leftover.Y].Char = buffer.Buffer[leftover.X, leftover.Y].BackupChar;
					}
					if (Stat.Random(1, 100) <= 40)
					{
						buffer[leftover.X, leftover.Y].SetForeground('B');
						if (Stat.RandomCosmetic(1, 100) <= 10)
						{
							buffer[leftover.X, leftover.Y].SetForeground('b');
						}
					}
				}
				leftovers.RemoveAt(0);
			}
			catch (Exception x3)
			{
				MetricsManager.LogException("Thin world::OnPaint 3", x3);
			}
		}
		base.OnPaint(buffer);
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}
