using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SultanMuralController : IPart
{
	public string PeriodFilter;

	public string Layout = "chronological:1";

	public int RuinedChance = 15;

	public bool initialized;

	public bool Reversed;

	public static Dictionary<string, int[]> muralAscii = new Dictionary<string, int[]>
	{
		{
			"isborn",
			new int[3] { 37, 26, 64 }
		},
		{
			"hasinspiringexperience",
			new int[3] { 64, 13, 14 }
		},
		{
			"treats",
			new int[3] { 134, 132, 134 }
		},
		{
			"createssomething",
			new int[3] { 227, 64, 15 }
		},
		{
			"commitsfolly",
			new int[3] { 168, 64, 63 }
		},
		{
			"weirdthinghappens",
			new int[3] { 64, 21, 21 }
		},
		{
			"endureshardship",
			new int[3] { 16, 64, 17 }
		},
		{
			"bodyexperiencebad",
			new int[3] { 64, 170, 31 }
		},
		{
			"bodyexperiencegood",
			new int[3] { 64, 170, 30 }
		},
		{
			"bodyexperienceneutral",
			new int[3] { 64, 170, 22 }
		},
		{
			"trysts",
			new int[3] { 145, 201, 185 }
		},
		{
			"visitslocation",
			new int[3] { 64, 157, 157 }
		},
		{
			"doesbureaucracy",
			new int[3] { 64, 227, 172 }
		},
		{
			"learnssecret",
			new int[3] { 63, 64, 19 }
		},
		{
			"findsobject",
			new int[3] { 64, 15, 157 }
		},
		{
			"doessomethingrad",
			new int[3] { 19, 64, 19 }
		},
		{
			"doessomethinghumble",
			new int[3] { 64, 250, 210 }
		},
		{
			"doessomethingdestructive",
			new int[3] { 64, 235, 127 }
		},
		{
			"becomesloved",
			new int[3] { 3, 64, 3 }
		},
		{
			"slays",
			new int[3] { 64, 41, 37 }
		},
		{
			"resists",
			new int[3] { 250, 64, 251 }
		},
		{
			"appeasesbaetyl",
			new int[3] { 64, 175, 4 }
		},
		{
			"dies",
			new int[3] { 64, 26, 37 }
		},
		{
			"crownedsultan",
			new int[3] { 134, 15, 64 }
		},
		{
			"wieldsiteminbattle",
			new int[3] { 91, 64, 41 }
		},
		{
			"meetswithcounselors",
			new int[3] { 166, 167, 64 }
		}
	};

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (ParentObject.CurrentZone.IsActive() && !initialized)
		{
			initializeMurals();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!initialized)
		{
			initializeMurals();
		}
		return base.HandleEvent(E);
	}

	public int LeftToRightTopToBottom(List<Cell> a, List<Cell> b)
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
		if (a[0].Location == b[0].Location)
		{
			return 0;
		}
		if (a[0].X == b[0].X)
		{
			return a[0].Y.CompareTo(b[0].Y);
		}
		return a[0].X.CompareTo(b[0].X);
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

	public void initializeMurals()
	{
		if (ParentObject.Physics.CurrentCell == null)
		{
			return;
		}
		System.Random seededRandom = ParentObject.GetSeededRandom("SultanMural");
		initialized = true;
		List<List<Cell>> list = new List<List<Cell>>();
		Zone parentZone = ParentObject.Physics.currentCell.ParentZone;
		string[] supportedPeriods = null;
		if (!string.IsNullOrEmpty(PeriodFilter))
		{
			supportedPeriods = PeriodFilter.Split(',');
		}
		HashSet<Cell> used = new HashSet<Cell>();
		for (int i = 0; i < parentZone.Height; i++)
		{
			for (int j = 0; j < parentZone.Width; j++)
			{
				Cell cell = parentZone.GetCell(j, i);
				if (used.Contains(cell) || !cell.HasObjectWithPart("SultanMural") || cell.HasObjectWithPropertyOrTag("_wasEngraved"))
				{
					continue;
				}
				List<Cell> list2 = new List<Cell>(3);
				list2.Add(cell);
				list2.Add(cell.GetCellFromDirection("E"));
				if (j < parentZone.Width - 1)
				{
					list2.Add(cell.GetCellFromDirection("E").GetCellFromDirection("E"));
				}
				if (list2.All((Cell c) => c != null && c.HasObjectWithPart("SultanMural") && !used.Contains(c) && !c.HasObjectWithPropertyOrTag("_wasEngraved") && (supportedPeriods == null || !c.GetFirstObjectWithPart("SultanMural").HasTag("SultanatePeriod") || supportedPeriods.Contains(c.GetFirstObjectWithPart("SultanMural").GetTag("SultanatePeriod")))))
				{
					list.Add(list2);
					list2.ForEach((Cell c) =>
					{
						used.Add(c);
					});
				}
				else
				{
					blankMural(list2);
				}
			}
		}
		foreach (List<Cell> item in list)
		{
			blankMural(item);
		}
		if (Layout.StartsWith("chronological:"))
		{
			list.Sort(LeftToRightTopToBottom);
			List<HistoricEvent> list3 = new List<HistoricEvent>();
			int period = Convert.ToInt32(Layout.Split(':')[1]);
			list3 = GetMuralEventsForPeriod(period);
			if (Reversed)
			{
				list3.Reverse();
			}
			int num = 0;
			if (list3.Count < list.Count)
			{
				num = (list.Count - list3.Count) / 2;
			}
			for (int num2 = 0; num2 < list3.Count && num2 < list.Count; num2++)
			{
				List<Cell> muralCells = list[num2 + num];
				HistoricEvent ev = list3[num2];
				if (seededRandom.Next(1, 101) <= RuinedChance)
				{
					ruinMural(muralCells, ev);
				}
				else
				{
					updateHistoricMural(muralCells, ev);
				}
			}
		}
		if (Layout.StartsWith("random:"))
		{
			List<HistoricEvent> list4 = new List<HistoricEvent>();
			int period2 = Convert.ToInt32(Layout.Split(':')[1]);
			list4 = GetMuralEventsForPeriod(period2);
			list.ShuffleInPlace(seededRandom);
			list4.ShuffleInPlace(seededRandom);
			for (int num3 = 0; num3 < list4.Count && num3 < list.Count; num3++)
			{
				List<Cell> muralCells2 = list[num3];
				HistoricEvent ev2 = list4[num3];
				if (seededRandom.Next(1, 101) <= RuinedChance)
				{
					ruinMural(muralCells2, ev2);
				}
				else
				{
					updateHistoricMural(muralCells2, ev2);
				}
			}
		}
		ParentObject.Destroy();
	}

	public static void ruinMural(List<Cell> muralCells, HistoricEvent ev)
	{
		if (muralCells.Count != 3)
		{
			Debug.LogError("mural not 3 cells long!");
			return;
		}
		string eventProperty = ev.GetEventProperty("tombInscriptionCategory");
		HistoricEntitySnapshot currentSnapshot = ev.entity.GetCurrentSnapshot();
		string text = currentSnapshot.GetProperty("name", "<unknown name>");
		string randomElementFromListProperty = currentSnapshot.GetRandomElementFromListProperty("cognomen", null, Stat.Rand);
		if (randomElementFromListProperty != null)
		{
			text = text + ", " + randomElementFromListProperty;
		}
		int num = Stat.Random(1, 6);
		for (int i = 0; i < 3; i++)
		{
			string text2 = "c";
			int num2 = 64;
			if (i == 0)
			{
				text2 = "l";
				num2 = 213;
			}
			if (i == 1)
			{
				text2 = "c";
				num2 = 209;
			}
			if (i == 2)
			{
				text2 = "r";
				num2 = 207;
			}
			GameObject firstObjectWithPart = muralCells[i].GetFirstObjectWithPart("SultanMural");
			firstObjectWithPart.SetIntProperty("_wasEngraved", 1);
			firstObjectWithPart.DisplayName = "ruined mural of " + text;
			firstObjectWithPart.Render.RenderString = ((char)num2).ToString();
			firstObjectWithPart.Render.Tile = "Walls/sw_mural_ruined" + num + "_" + text2 + ".bmp";
			firstObjectWithPart.Physics.Category = eventProperty;
			firstObjectWithPart.GetPart<Description>().Short = "The tomb mural depicts a significant event from the life of the ancient sultan " + text + ", but it's no longer legible.";
		}
	}

	public void blankMural(List<Cell> muralCells)
	{
		if (muralCells.Count != 3 || muralCells.Any((Cell C) => C == null))
		{
			foreach (Cell muralCell in muralCells)
			{
				if (muralCell != null)
				{
					GameObject firstObjectWithPart = muralCell.GetFirstObjectWithPart("SultanMural");
					if (firstObjectWithPart != null)
					{
						string text = "c";
						if (muralCell.GetCellFromDirection("W") == null || !muralCell.GetCellFromDirection("W").HasWall())
						{
							text = "l";
						}
						if (muralCell.GetCellFromDirection("E") == null || !muralCell.GetCellFromDirection("E").HasWall())
						{
							text = "r";
						}
						firstObjectWithPart.DisplayName = "ruined mural slate";
						firstObjectWithPart.Render.RenderString = 'ÿ'.ToString();
						firstObjectWithPart.Render.Tile = "Walls/sw_mural_ruined" + Stat.Random(1, 6) + "_" + text + ".bmp";
					}
				}
			}
			return;
		}
		for (int num = 0; num < 3; num++)
		{
			string text2 = "c";
			if (num == 0)
			{
				text2 = "l";
			}
			if (num == 1)
			{
				text2 = "c";
			}
			if (num == 2)
			{
				text2 = "r";
			}
			GameObject firstObjectWithPart2 = muralCells[num].GetFirstObjectWithPart("SultanMural");
			if (firstObjectWithPart2 != null)
			{
				firstObjectWithPart2.DisplayName = "blank mural slate";
				firstObjectWithPart2.Render.RenderString = 'ÿ'.ToString();
				firstObjectWithPart2.Render.Tile = "Walls/sw_mural_blank_" + text2 + ".bmp";
			}
		}
	}

	public void updateHistoricMural(List<Cell> muralCells, HistoricEvent ev)
	{
		if (muralCells.Count != 3)
		{
			Debug.LogError("mural not 3 cells long!");
			return;
		}
		string text = ev.GetEventProperty("tombInscriptionCategory").ToLower();
		HistoricEntitySnapshot currentSnapshot = ev.entity.GetCurrentSnapshot();
		string text2 = currentSnapshot.GetProperty("name", "<unknown name>");
		string randomElementFromListProperty = currentSnapshot.GetRandomElementFromListProperty("cognomen", null, Stat.Rand);
		if (randomElementFromListProperty != null)
		{
			text2 = text2 + ", " + randomElementFromListProperty;
		}
		for (int i = 0; i < 3; i++)
		{
			string text3 = "c";
			if (i == 0)
			{
				text3 = "l";
			}
			if (i == 1)
			{
				text3 = "c";
			}
			if (i == 2)
			{
				text3 = "r";
			}
			GameObject firstObjectWithPart = muralCells[i].GetFirstObjectWithPart("SultanMural");
			firstObjectWithPart.SetIntProperty("_wasEngraved", 1);
			firstObjectWithPart.DisplayName = "mural of " + text2;
			if (muralAscii.ContainsKey(text))
			{
				firstObjectWithPart.Render.RenderString = ((char)muralAscii[text][i]).ToString();
			}
			firstObjectWithPart.Render.Tile = "Walls/sw_mural_" + text + "_" + text3 + ".bmp";
			firstObjectWithPart.Physics.Category = text;
			firstObjectWithPart.GetPart<Description>().Short = "The tomb mural depicts a significant event from the life of the ancient sultan " + text2 + ":\n\n" + ev.GetEventProperty("tombInscription");
			JournalSultanNote journalSultanNote = JournalAPI.SultanNotes.Where((JournalSultanNote e) => e.EventID == ev.id && e.Has("sultanTombPropaganda")).First();
			if (journalSultanNote != null)
			{
				firstObjectWithPart.GetPart<SultanMural>().secretID = journalSultanNote.ID;
				firstObjectWithPart.GetPart<SultanMural>().secretEvent = ev;
			}
		}
	}
}
