using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Genkit;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Quests;
using XRL.World.ZoneBuilders;

namespace XRL.World.ZoneParts;

[Serializable]
public class ScriptCallToArms : IZonePart
{
	public int n;

	public const int TOPSCORE = 150;

	public const int MIDSCORE = 800;

	public const int ATTACK_TIMER = 250;

	[NonSerialized]
	private ElectricalPowerTransmission generator;

	public List<string> partyTypes = new List<string>();

	public List<Location2D> partyLocations = new List<Location2D>();

	private int partyArrivalTurnOffset = 4;

	public override bool SameAs(IZonePart Part)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == PowerUpdatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		updatePower();
		string track = "Music/Imminent II";
		if (n == 0)
		{
			SoundManager.PlayMusic("Music/Imminent II", "music", Crossfade: false);
			partyArrivalTurnOffset = Stat.Roll(2, 8);
			FlagScoreables();
			generatyPartyLocations();
			ShowWarning();
			The.Game.SetStringGameState("CallToArmsStarted", "1");
			n++;
			GritGateScripts.OpenGritGateDoors(2);
		}
		n++;
		if (n == 250)
		{
			SoundManager.PlayMusic("Music/Attack on Grit Gate");
			track = "Music/Attack on Grit Gate";
			spawnParties(0);
		}
		if (n == 250 + partyArrivalTurnOffset && partyTypes[0] != partyTypes[1])
		{
			spawnParties(1);
		}
		if (n > 250)
		{
			track = "Music/Attack on Grit Gate";
			if (ParentZone.GetFirstObjectWithPropertyOrTag("TemplarWarParty") == null)
			{
				The.Game.FinishQuestStep("A Call to Arms", "Defend Grit Gate");
				The.Game.ForceFinishQuest("Grave Thoughts");
				int intGameState = The.Game.GetIntGameState("CallToArmsScore");
				if (intGameState <= 150)
				{
					The.Game.SetIntGameState("ACallToArms_TopScore", 1);
				}
				else if (intGameState <= 800)
				{
					The.Game.SetIntGameState("ACallToArms_MidScore", 1);
				}
				else
				{
					The.Game.SetIntGameState("ACallToArms_BottomScore", 1);
				}
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.AppendLine("Destruction Score: " + The.Game.GetIntGameState("CallToArmsScore"));
				stringBuilder.AppendLine("  Chromelings: " + The.Game.GetIntGameState("CallToArmsChromelings"));
				stringBuilder.AppendLine("  Barathrumites: " + The.Game.GetIntGameState("CallToArmsBarathrumites"));
				if (The.Game.GetStringGameState("SlynthSettlementFaction") == "Barathrumites")
				{
					stringBuilder.AppendLine("  Slynth: " + The.Game.GetIntGameState("CallToArmsSlynth"));
				}
				stringBuilder.AppendLine("  Bookshelves: " + The.Game.GetIntGameState("CallToArmsBookshelves"));
				stringBuilder.AppendLine("  Books: " + The.Game.GetIntGameState("CallToArmsBooks"));
				stringBuilder.AppendLine("  Crops: " + The.Game.GetIntGameState("CallToArmsCrops"));
				stringBuilder.AppendLine("  Gadgets: " + The.Game.GetIntGameState("CallToArmsGadgets"));
				stringBuilder.AppendLine("  Furniture: " + The.Game.GetIntGameState("CallToArmsFurniture"));
				GeneralAmnestyEvent.Send();
				everyoneReturnHome();
				SoundManager.PlayMusic("Music/Stoic Porridge");
				ParentZone.RemovePart(this);
			}
		}
		else if (The.Player?.CurrentZone == ParentZone)
		{
			SoundManager.PlayMusic(track);
		}
		return base.HandleEvent(E);
	}

	public void everyoneReturnHome()
	{
		foreach (GameObject item in ParentZone.GetZoneFromDirection("D").GetObjectsWithTag("ReturnToGritGateAfterAttack"))
		{
			string propertyOrTag = item.GetPropertyOrTag("ReturnToGritGateAfterAttack");
			if (!propertyOrTag.IsNullOrEmpty())
			{
				string[] array = propertyOrTag.Split(',');
				int x = int.Parse(array[0]);
				int y = int.Parse(array[1]);
				item.TeleportTo(ParentZone.GetCell(25, 21).getClosestPassableCell(), 0);
				The.ActionManager.AddActiveObject(item);
				item.Brain.Goals.Clear();
				item.Brain.PushGoal(new MoveTo(ParentZone.GetCell(x, y)));
			}
		}
		foreach (GameObject item2 in ParentZone.FindObjects((GameObject o) => o.BelongsToFaction("Barathrumites")))
		{
			item2.ModIntProperty("PreventIdleBehavior", -1, RemoveIfZero: true);
			if (item2.HasStringProperty("GritGateDefenseOriginalLocation"))
			{
				string[] array2 = item2.GetStringProperty("GritGateDefenseOriginalLocation").Split(',');
				item2.Brain.PushGoal(new MoveTo(ParentZone.GetCell(Convert.ToInt32(array2[0]), Convert.ToInt32(array2[1]))));
			}
		}
		ZoneRepair zoneRepair = ParentZone.RequirePart<ZoneRepair>();
		zoneRepair.FileName = "GritGate.rpm";
		zoneRepair.TurnsPerObject = 50L;
		zoneRepair.RequiresObjectOfFaction = "Barathrumites";
		zoneRepair.CheckZoneRepair();
	}

	public void updatePower()
	{
		if (generator == null)
		{
			GameObject gameObject = ParentZone.FindObject((GameObject o) => o.Blueprint == "GritGateFusionPowerStation" && o.CurrentCell.Y >= 10);
			if (gameObject != null)
			{
				generator = gameObject.GetPart<ElectricalPowerTransmission>();
			}
		}
		string display;
		if (generator == null)
		{
			int num = 0;
			display = "[{{R|!!! ERROR: POWER SYSTEMS HAVE FAILED !!!}}]";
		}
		else
		{
			int totalDraw = generator.GetTotalDraw();
			int num = 4000 - totalDraw;
			display = ((num >= 0) ? (" Available power: " + (float)num * 0.1f + " amps ") : "[{{W|!!! WARNING: INSUFFICIENT POWER !!!}}]");
		}
		GritGateAmperageImposter.display = display;
	}

	public void scan()
	{
		for (int i = 0; i < partyLocations.Count; i++)
		{
			Location2D p = partyLocations[i];
			if (partyTypes[i] == "*")
			{
				List<Cell> list = ParentZone.GetCell(p).GetAdjacentCells(3).FindAll((Cell c) => c.IsPassable());
				if (list.Count == 0)
				{
					list.Add(ParentZone.GetCell(p));
				}
				list.ShuffleInPlace();
				list[0].SetExplored();
				list[0].AddObject("AlarmCircle");
			}
			else
			{
				List<Cell> list2 = ParentZone.GetCell(p).GetAdjacentCells(1).FindAll((Cell c) => c.IsPassable());
				if (list2.Count == 0)
				{
					list2.Add(ParentZone.GetCell(p));
				}
				list2.ShuffleInPlace();
				list2[0].SetExplored();
				list2[0].AddObject("AlarmCircleSmall");
			}
		}
		ParentZone.GetCell(4, 23).SetExplored();
		ParentZone.GetCell(4, 23).AddObject("GritGatePowerDisplay");
		startBarathrumiteAlertBehaviors();
	}

	public void startBarathrumiteAlertBehaviors()
	{
		string[] list = new string[4] { "aggressive", "fearful", "crops", "mainframe" };
		int num = 0;
		Cell cell = null;
		foreach (Cell reachableCell in ParentZone.GetReachableCells())
		{
			int num2 = reachableCell.PathDistanceTo(ParentZone.GetCell(partyLocations[0])) + reachableCell.PathDistanceTo(ParentZone.GetCell(partyLocations[1]));
			if (num2 > num)
			{
				cell = reachableCell;
				num = num2;
			}
		}
		foreach (GameObject item in ParentZone.FindObjects((GameObject o) => o.BelongsToFaction("Barathrumites") && o.Blueprint != "Otho"))
		{
			try
			{
				string randomElement = item.GetTag("GritGateDefenseBehavior", list.GetRandomElement()).Split(',').GetRandomElement();
				item.RemoveEffect<Asleep>();
				item.SetStringProperty("GritGateDefenseOriginalLocation", item.CurrentCell.X + "," + item.CurrentCell.Y);
				switch (randomElement)
				{
				case "aggressive":
					item.Brain.Goals.Clear();
					item.Brain.PushGoal(new Guard()).PushChildGoal(new MoveTo(ParentZone.GetCell(partyLocations.GetRandomElement()).GetAdjacentCells(6).FindAll((Cell c) => c.IsReachable())
						.GetRandomElement()));
					break;
				case "fearful":
					item.Brain.Goals.Clear();
					item.Brain.PushGoal(new Guard()).PushChildGoal(new MoveTo(cell.GetAdjacentCells(5).FindAll((Cell c) => c.IsReachable()).GetRandomElement()));
					break;
				case "crops":
				{
					GameObject gameObject2 = ParentZone.FindClosestObjectWithTag(item, "LivePlant");
					if (gameObject2 != null)
					{
						item.Brain.Goals.Clear();
						item.Brain.PushGoal(new Guard()).PushChildGoal(new MoveTo(gameObject2.CurrentCell.GetAdjacentCells(3).FindAll((Cell c) => c.IsReachable()).GetRandomElement()));
					}
					break;
				}
				case "mainframe":
				{
					GameObject gameObject = ParentZone.FindClosestObjectWithTag(item, "Mainframe");
					if (gameObject != null)
					{
						item.Brain.Goals.Clear();
						item.Brain.PushGoal(new Guard()).PushChildGoal(new MoveTo(gameObject.CurrentCell.GetAdjacentCells(3).FindAll((Cell c) => c.IsReachable()).GetRandomElement()));
					}
					break;
				}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("A Call to Arms behavior setup for " + item.DebugName, x);
			}
		}
	}

	public void generatyPartyLocations()
	{
		for (int i = 0; i < 2; i++)
		{
			string blueprint = PopulationManager.RollOneFrom("Grit Gate Attack Location").Blueprint;
			Location2D item = null;
			switch (blueprint)
			{
			case "F":
				item = Location2D.Get(40, 21);
				break;
			case "E":
				item = Location2D.Get(75, 4);
				break;
			case "W":
				item = Location2D.Get(6, 7);
				break;
			case "*":
			{
				List<Cell> list = (from c in ParentZone.GetEmptyReachableCells()
					where c.X > 4 && c.X < 76 && c.Y > 4 && c.Y < 20
					select c).ToList();
				if (list.Count == 0)
				{
					list = ParentZone.GetEmptyCells();
				}
				if (list.Count == 0)
				{
					list = ParentZone.GetCells((Cell c) => !c.HasWall());
				}
				if (list.Count == 0)
				{
					list = ParentZone.GetCells();
				}
				item = list.GetRandomElement().Location;
				break;
			}
			}
			partyTypes.Add(blueprint);
			partyLocations.Add(item);
		}
	}

	public void spawnParties(int p)
	{
		if (!The.Game.HasQuest("A Call to Arms"))
		{
			The.Game.StartQuest("A Call to Arms");
		}
		AutoAct.Interrupt();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		ParentZone.FindObjects("AlarmCircle").ForEach((GameObject o) =>
		{
			o.Obliterate();
		});
		ParentZone.FindObjects("AlarmCircleSmall").ForEach((GameObject o) =>
		{
			o.Obliterate();
		});
		ParentZone.FindObjects("GritGatePowerDisplay").ForEach((GameObject o) =>
		{
			o.Obliterate();
		});
		string text = partyTypes[p];
		Location2D location2D = partyLocations[p];
		List<PopulationResult> list = null;
		if (p == 0)
		{
			list = PopulationManager.Generate("Templar War Party Major");
		}
		if (p == 1)
		{
			list = PopulationManager.Generate("Templar War Party Minor");
		}
		int partycount = 0;
		list.ForEach((PopulationResult r) =>
		{
			partycount += r.Number;
		});
		List<Cell> spawnCells = null;
		for (int num = 3; num < 80; num++)
		{
			spawnCells = ParentZone.GetCell(location2D).GetEmptyAdjacentCells(1, num);
			if (spawnCells.Count >= partycount)
			{
				break;
			}
		}
		if (text == "*")
		{
			Zone zoneFromDirection = ParentZone.GetZoneFromDirection("U");
			for (int num2 = location2D.X - 3; num2 < location2D.X + 3; num2++)
			{
				for (int num3 = location2D.Y - 3; num3 < location2D.Y + 3; num3++)
				{
					zoneFromDirection.GetCell(num2, num3)?.ClearWalls();
				}
			}
			for (int num4 = location2D.X - 3; num4 < location2D.X + 3; num4++)
			{
				for (int num5 = location2D.Y - 3; num5 < location2D.Y + 3; num5++)
				{
					Cell cell = ParentZone.GetCell(num4, num5);
					if (25.in100())
					{
						cell.AddObject(PopulationManager.RollOneFrom("Rocky Debris").Blueprint);
					}
				}
			}
			Popup.Show("{{r|The ceiling collapses and rocks come pouring in!}}");
			zoneFromDirection.GetCell(location2D).AddObject("OpenShaft");
			new ForceConnectionsPlus().BuildZone(zoneFromDirection);
		}
		if (text == "F" && !flag)
		{
			flag = true;
			ElectromagneticPulse.EMP(ParentZone.GetCell(location2D), 4, 30);
			foreach (Cell cell3 in ParentZone.GetCells())
			{
				foreach (GameObject item in cell3.GetObjectsInCell())
				{
					if (item.Blueprint == "Forcefield_GritGateRank1")
					{
						item.Destroy();
					}
				}
			}
		}
		if (text == "E" && !flag3)
		{
			flag3 = true;
			ElectromagneticPulse.EMP(ParentZone.GetCell(location2D), 4, 30);
			foreach (Cell cell4 in ParentZone.GetCells())
			{
				foreach (GameObject item2 in cell4.GetObjectsInCell())
				{
					if (item2.Blueprint == "Forcefield_GritGate")
					{
						item2.Destroy();
					}
				}
			}
		}
		if (text == "W" && !flag2)
		{
			flag2 = true;
			for (int num6 = 4; num6 < 8; num6++)
			{
				for (int num7 = 0; num7 < 7; num7++)
				{
					Cell cell2 = ParentZone.GetCell(num6, num7);
					if (cell2 != null)
					{
						cell2.ClearWalls();
						if (20.in100())
						{
							cell2.AddObject(PopulationManager.RollOneFrom("Rocky Debris").Blueprint);
						}
					}
				}
			}
			Zone zoneFromDirection2 = ParentZone.GetZoneFromDirection("N");
			for (int num8 = 4; num8 < 8; num8++)
			{
				for (int num9 = ParentZone.Height - 4; num9 < ParentZone.Height; num9++)
				{
					zoneFromDirection2.GetCell(num8, num9)?.ClearWalls();
				}
			}
			new ForceConnectionsPlus().BuildZone(zoneFromDirection2);
			location2D = Location2D.Get(6, 7);
		}
		list.ForEach((PopulationResult m) =>
		{
			for (int i = 0; i < m.Number; i++)
			{
				spawnCells.RemoveRandomElement().AddObject(m.Blueprint).AddAsActiveObject()
					.SetIntProperty("TemplarWarParty", 1)
					.SetIntProperty("AllowIdleBehavior", 1)
					.Brain.Wanders = false;
			}
		});
		foreach (Cell cell5 in ParentZone.GetCells())
		{
			foreach (GameObject item3 in cell5.GetObjectsInCell())
			{
				if (item3.Blueprint == "GritGateDoor1")
				{
					item3.Destroy();
				}
			}
		}
		if (p == 0 && partyTypes[0] == partyTypes[1])
		{
			spawnParties(1);
		}
		else
		{
			ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true).Shake(1500, 25, Popup._TextConsole);
		}
	}

	public void FlagScoreables()
	{
		Zone.ObjectEnumerator enumerator = ParentZone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			FlagScoreable(current);
		}
	}

	public void FlagScoreable(GameObject o)
	{
		GameObjectBlueprint blueprint = o.GetBlueprint();
		if ((blueprint.InheritsFrom("BarathrumiteRobot") || blueprint.HasTagOrProperty("BarathrumiteRobot")) && !o.HasTag("CallToArmsBarathrumite"))
		{
			o.AddPart(new CallToArmsScore(20, "CallToArmsChromelings"));
		}
		else if ((o.IsCombat && o.BelongsToFaction("Barathrumites")) || o.HasTag("CallToArmsBarathrumite"))
		{
			o.AddPart(new CallToArmsScore(200, "CallToArmsBarathrumites", person: true));
		}
		else if (blueprint.InheritsFrom("BaseSlynth"))
		{
			o.AddPart(new CallToArmsScore(200, "CallToArmsSlynth", person: true));
		}
		else if (blueprint.InheritsFrom("Bookshelf"))
		{
			o.AddPart(new CallToArmsScore(20, "CallToArmsBookshelves"));
			o.AddPart(new BurnMe("Templar"));
			o.Inventory?.Objects?.ForEach(FlagScoreable);
		}
		else if (blueprint.InheritsFrom("Book"))
		{
			o.AddPart(new CallToArmsScore(20, "CallToArmsBooks"));
			o.AddPart(new BurnMe("Templar"));
		}
		else if (o.HasTag("CallToArmsCrop") && !o.IsCombat && o.CurrentCell != null)
		{
			o.AddPart(new CallToArmsScore(20, "CallToArmsCrops"));
			o.AddPart(new BurnMe("Templar"));
		}
		else if (o.HasTag("LivePlant") && !o.IsCombat && o.CurrentCell != null)
		{
			o.AddPart(new BurnMe("Templar"));
		}
		else if (blueprint.InheritsFrom("Furniture"))
		{
			if (blueprint.TechTier >= 2 || o.HasTag("Gadget"))
			{
				o.AddPart(new CallToArmsScore(20, "CallToArmsGadgets"));
			}
			else
			{
				o.AddPart(new CallToArmsScore(20, "CallToArmsFurniture"));
			}
			o.AddPart(new DestroyMe("Templar"));
			o.Inventory?.Objects?.ForEach(FlagScoreable);
		}
	}

	public void ShowWarning()
	{
		ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true).Shake(1500, 25, Popup._TextConsole);
		Popup.Show("The whole compound rumbles around you.");
		Popup.Show("The walls creak, loose objects skid about, and dust is stirred up in iridescent clouds.");
		Popup.Show("Otho yells, '{{W|" + The.Game.PlayerName + "! Come back here!}}'");
		AutoAct.Interrupt();
		foreach (GameObject item in ParentZone.FindObjects((GameObject o) => o.BelongsToFaction("Barathrumites")))
		{
			item.Brain?.Goals?.Clear();
			item.ModIntProperty("PreventIdleBehavior", 1, RemoveIfZero: true);
		}
		The.Game.SetIntGameState("GritGatePower", 1000);
	}

	public static string GenerateResultConversation()
	{
		string text = "=name=, I've assessed the losses we suffered in the attack.\n\n";
		string text2 = "friend";
		string text3 = "deaths";
		string text4 = "";
		if (The.Game.HasStringGameState("CallToArmsPersonsKilled"))
		{
			List<string> list = The.Game.GetStringGameState("CallToArmsPersonsKilled").Split(',').ToList();
			if (list.Count == 1)
			{
				text2 = "friend";
				text3 = "death";
				text4 = list[0];
			}
			else
			{
				HashSet<string> hashSet = new HashSet<string>();
				text2 = "friends";
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != null && !hashSet.Contains(list[i]))
					{
						hashSet.Add(list[i]);
						if (i > 0)
						{
							text4 = ((i >= list.Count - 1) ? (text4 + " and ") : (text4 + ", "));
						}
						text4 += list[i];
					}
				}
			}
		}
		if (text4 != "")
		{
			text = text + "First and foremost, let us lament the " + text3 + " of our " + text2 + ": " + text4 + ". The loss to us is incalculable.\n\n";
			text += "Beyond that, ";
		}
		int intGameState = The.Game.GetIntGameState("CallToArmsChromelings");
		int intGameState2 = The.Game.GetIntGameState("CallToArmsBooks");
		int intGameState3 = The.Game.GetIntGameState("CallToArmsBookshelves");
		int intGameState4 = The.Game.GetIntGameState("CallToArmsCrops");
		int intGameState5 = The.Game.GetIntGameState("CallToArmsGadgets");
		int intGameState6 = The.Game.GetIntGameState("CallToArmsFurniture");
		string text5 = Grammar.CardinalNo(intGameState) + " " + ((intGameState == 1) ? "robot" : "robots") + " perished, " + Grammar.CardinalNo(intGameState2) + " " + ((intGameState2 == 1) ? "book was" : "books were") + " burned, " + Grammar.CardinalNo(intGameState3) + " " + ((intGameState3 == 1) ? "bookshelf was" : "bookshelves were") + " destroyed, " + Grammar.CardinalNo(intGameState4) + " " + ((intGameState4 == 1) ? "crop was" : "crops were") + " ruined, " + Grammar.CardinalNo(intGameState5) + " " + ((intGameState5 == 1) ? "gadget was" : "gadgets were") + " shattered, and " + Grammar.CardinalNo(intGameState6) + " " + ((intGameState6 == 1) ? "piece" : "pieces") + " of furniture " + ((intGameState6 == 1) ? "was" : "were") + " destroyed.";
		if (text4 == "")
		{
			text5 = Grammar.InitCap(text5);
		}
		return text + text5;
	}
}
