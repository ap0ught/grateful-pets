using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class PlayerMuralController : IPart
{
	public bool initialized;

	public bool carving;

	public string carvingStage = "waiting";

	public int currentMural;

	public int currentPanel;

	public int turnTick;

	public List<List<Location2D>> murals;

	public List<JournalAccomplishment> playerMuralEventList;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != EnteredCellEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		turnTick++;
		if (turnTick == 2)
		{
			if (carvingStage == "done")
			{
				return true;
			}
			if (currentMural >= murals.Count || currentMural >= playerMuralEventList.Count)
			{
				if (XRLCore.Core.Game.HasIntGameState("ReshephDisguise"))
				{
					Popup.Show("Herododicus says '&WI'm finished, Moloch! Praise =name= Resheph, all who canter in this House!&Y'".Replace("=name=", XRLCore.Core.Game.PlayerName));
				}
				else
				{
					Popup.Show("Herododicus says '&WI'm done!&Y'");
				}
				carvingStage = "done";
				if (XRLCore.Core.Game.HasIntGameState("ReshephDisguise"))
				{
					XRLCore.Core.Game.SetIntGameState("PlayerEngravingsDone_ReshephDisguise", 1);
				}
				else
				{
					XRLCore.Core.Game.SetIntGameState("PlayerEngravingsDone", 1);
				}
				foreach (GameObject item in ParentObject.Physics.CurrentCell.ParentZone.GetObjectsWithPart("ReshephsCrypt"))
				{
					item.RemoveIntProperty("Sealed");
					item.FireEvent("SyncOpened");
					item.Physics.PlayWorldSound("Sounds/Misc/sfx_ancientDoor_open");
				}
			}
			if (carvingStage == "go")
			{
				if (getBiographer().Physics.CurrentCell == getCurrentTargetCell())
				{
					getBiographer().Physics.PlayWorldSound("Sounds/Interact/sfx_interact_engraver");
					getBiographer().Physics.CurrentCell.GetCellFromDirection("N").GetObjectsWithPart("Physics")[0].Sparksplatter();
					updatePlayerMural(murals[currentMural], playerMuralEventList[currentMural], currentPanel);
					currentPanel++;
					if (currentPanel > 2)
					{
						currentPanel = 0;
						currentMural++;
					}
				}
				else if (!getBiographer().Brain.Goals.Items.Any((GoalHandler g) => g.GetType() == typeof(MoveTo) || g.GetType() == typeof(Step)))
				{
					getBiographer().Brain.Goals.Clear();
					getBiographer().Brain.MoveTo(getCurrentTargetCell());
				}
			}
			turnTick = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (ParentObject.CurrentCell.ParentZone.IsActive() && (!initialized || murals == null))
		{
			initializeMurals();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!initialized || murals == null)
		{
			initializeMurals();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginPlayerMural");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginPlayerMural")
		{
			getBiographer().Brain.Wanders = false;
			getBiographer().Brain.WandersRandomly = false;
			getBiographer().Brain.StartingCell = null;
			getBiographer().SetIntProperty("NoStay", 1);
			carvingStage = "go";
			currentMural = 0;
			currentPanel = 0;
			turnTick = 1;
		}
		return true;
	}

	public int RightToLeftTopToBottom(List<Location2D> a, List<Location2D> b)
	{
		if (a[0] == b[0])
		{
			return 0;
		}
		if (a == null)
		{
			return 1;
		}
		if (b == null)
		{
			return -1;
		}
		int y;
		if (a[0].X == b[0].X)
		{
			y = a[0].Y;
			return y.CompareTo(b[0].Y);
		}
		y = b[0].X;
		return y.CompareTo(a[0].X);
	}

	public List<HistoricEvent> GetMuralEventsForPeriod(int period)
	{
		HistoricEntityList entitiesWherePropertyEquals = XRLCore.Core.Game.sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan");
		for (int i = 0; i < entitiesWherePropertyEquals.entities.Count; i++)
		{
			if (int.Parse(entitiesWherePropertyEquals.entities[i].GetCurrentSnapshot().GetProperty("period")) == period)
			{
				return (from e in entitiesWherePropertyEquals.entities[i].events
					where e.HasEventProperty("tombInscription")
					orderby e.year
					select e).ToList().ShuffleInPlace();
			}
		}
		return new List<HistoricEvent>();
	}

	public void clearMurals()
	{
	}

	public void initializeMurals()
	{
		if (initialized)
		{
			return;
		}
		if (ParentObject.Physics.CurrentCell != null)
		{
			murals = new List<List<Location2D>>();
			initialized = true;
			Zone zone = ParentObject.Physics.currentCell.ParentZone;
			HashSet<Cell> used = new HashSet<Cell>();
			for (int i = 0; i < zone.Height; i++)
			{
				for (int j = 0; j < zone.Width - 2; j++)
				{
					Cell cell = zone.GetCell(j, i);
					if (used.Contains(cell) || !cell.HasObjectWithPart("SultanMural"))
					{
						continue;
					}
					List<Location2D> list = new List<Location2D>(3);
					list.Add(cell.Location);
					list.Add(cell.GetCellFromDirection("E").Location);
					list.Add(cell.GetCellFromDirection("E").GetCellFromDirection("E").Location);
					if (list.All((Location2D c) => c != null && zone.GetCell(c).HasObjectWithPart("SultanMural") && !used.Contains(zone.GetCell(c))))
					{
						murals.Add(list);
						list.ForEach((Location2D c) =>
						{
							used.Add(zone.GetCell(c));
						});
						blankMural(list);
					}
				}
			}
			murals.Sort(RightToLeftTopToBottom);
			BallBag<JournalAccomplishment> ballBag = new BallBag<JournalAccomplishment>();
			BallBag<JournalAccomplishment> ballBag2 = new BallBag<JournalAccomplishment>();
			List<JournalAccomplishment> original = JournalAPI.Accomplishments;
			List<JournalAccomplishment> list2 = original.Where((JournalAccomplishment x) => !x.MuralText.IsNullOrEmpty()).ToList();
			HashSet<string> hashSet = new HashSet<string>();
			list2.ShuffleInPlace();
			foreach (JournalAccomplishment item in list2)
			{
				int muralWeight = (int)item.MuralWeight;
				if (!item.AggregateWith.IsNullOrEmpty() && !hashSet.Add(item.AggregateWith))
				{
					ballBag2.Add(item, muralWeight);
				}
				else
				{
					ballBag.Add(item, muralWeight);
				}
			}
			playerMuralEventList = new List<JournalAccomplishment>();
			while (ballBag.Count > 0 && playerMuralEventList.Count < 16)
			{
				playerMuralEventList.Add(ballBag.PickOne());
			}
			while (ballBag2.Count > 0 && playerMuralEventList.Count < 16)
			{
				playerMuralEventList.Add(ballBag2.PickOne());
			}
			playerMuralEventList.Sort((JournalAccomplishment a, JournalAccomplishment b) => original.IndexOf(a).CompareTo(original.IndexOf(b)));
			if (original.Count > 0)
			{
				playerMuralEventList.Remove(original[0]);
				playerMuralEventList.Insert(0, original[0]);
			}
			else
			{
				MetricsManager.LogError("no origial mural available");
			}
			JournalAccomplishment journalAccomplishment = new JournalAccomplishment();
			journalAccomplishment.Category = "Dies";
			journalAccomplishment.MuralCategory = MuralCategory.Dies;
			journalAccomplishment.MuralText = "On the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", " + XRLCore.Core.Game.PlayerName + " died peacefully and was laid to rest in the Tomb of Sultans.";
			journalAccomplishment.GospelText = null;
			playerMuralEventList.Add(journalAccomplishment);
			PlayerMuralGameState.Instance.SetAccomplishments(playerMuralEventList);
		}
		int count = playerMuralEventList.Count;
		int num = 9 - count / 2;
		if (num > 0)
		{
			for (int num2 = 0; num2 < num; num2++)
			{
				murals.RemoveAt(0);
			}
		}
	}

	public void blankMural(List<Location2D> muralCells)
	{
		Zone parentZone = ParentObject.Physics.CurrentCell.ParentZone;
		if (muralCells.Count != 3)
		{
			foreach (Location2D muralCell in muralCells)
			{
				GameObject firstObjectWithPart = parentZone.GetCell(muralCell).GetFirstObjectWithPart("SultanMural");
				string text = "c";
				if (parentZone.GetCell(muralCell).GetCellFromDirection("W") == null || !parentZone.GetCell(muralCell).GetCellFromDirection("W").HasWall())
				{
					text = "l";
				}
				if (parentZone.GetCell(muralCell).GetCellFromDirection("E") == null || !parentZone.GetCell(muralCell).GetCellFromDirection("E").HasWall())
				{
					text = "r";
				}
				firstObjectWithPart.DisplayName = "ruined mural slate";
				firstObjectWithPart.Render.RenderString = 'ÿ'.ToString();
				firstObjectWithPart.Render.Tile = "Walls/sw_mural_ruined" + Stat.Random(1, 6) + "_" + text + ".bmp";
			}
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			string text2 = "c";
			if (i == 0)
			{
				text2 = "l";
			}
			if (i == 1)
			{
				text2 = "c";
			}
			if (i == 2)
			{
				text2 = "r";
			}
			GameObject firstObjectWithPart2 = parentZone.GetCell(muralCells[i]).GetFirstObjectWithPart("SultanMural");
			firstObjectWithPart2.DisplayName = "blank mural slate";
			firstObjectWithPart2.Render.RenderString = 'ÿ'.ToString();
			firstObjectWithPart2.Render.Tile = "Walls/sw_mural_blank_" + text2 + ".bmp";
		}
	}

	public static void SetStateReshephDisguise()
	{
		XRLCore.Core.Game.SetIntGameState("ReshephDisguise", 1);
	}

	public static void RevealMarkOfDeath()
	{
		JournalAPI.RevealObservation("MarkOfDeathSecret", onlyIfNotRevealed: true);
	}

	public static void BeginPlayerMuralSequence()
	{
		IComponent<GameObject>.ThePlayer.CurrentZone.FireEvent("BeginPlayerMural");
	}

	public void updatePlayerMural(List<Location2D> muralCells, JournalAccomplishment a, int panel)
	{
		if (muralCells.Count != 3)
		{
			Debug.LogError("mural not 3 cells long!");
			return;
		}
		string text = a.MuralCategory.ToString().ToLower();
		string text2 = The.Player.BaseDisplayName;
		string text3 = null;
		if (!string.IsNullOrEmpty(text3))
		{
			text2 = text2 + ", " + text3;
		}
		Zone currentZone = ParentObject.CurrentZone;
		for (int i = 0; i < 3; i++)
		{
			if (i == panel)
			{
				string text4 = "c";
				if (i == 0)
				{
					text4 = "l";
				}
				if (i == 1)
				{
					text4 = "c";
				}
				if (i == 2)
				{
					text4 = "r";
				}
				GameObject firstObjectWithPart = currentZone.GetCell(muralCells[i]).GetFirstObjectWithPart("SultanMural");
				Debug.Log(text);
				if (SultanMuralController.muralAscii.ContainsKey(text))
				{
					firstObjectWithPart.Render.RenderString = ((char)SultanMuralController.muralAscii[text][i]).ToString();
				}
				firstObjectWithPart.SetIntProperty("_wasEngraved", 1);
				firstObjectWithPart.DisplayName = "mural of " + text2;
				firstObjectWithPart.Render.Tile = "Walls/sw_mural_" + text + "_" + text4 + ".bmp";
				firstObjectWithPart.Physics.Category = text;
				firstObjectWithPart.GetPart<Description>().Short = "The tomb mural depicts a significant event from the life of the sultan " + text2 + ":\n\n" + a.MuralText;
			}
		}
	}

	private GameObject getBiographer()
	{
		return GameObject.FindByBlueprint("Biographer");
	}

	private Cell getCurrentTargetCell()
	{
		return ParentObject.CurrentZone.GetCell(murals[currentMural][currentPanel]).GetCellFromDirection("S");
	}
}
