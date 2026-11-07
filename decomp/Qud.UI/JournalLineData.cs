using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Genkit;
using Qud.API;
using XRL;
using XRL.UI.Framework;

namespace Qud.UI;

[HasModSensitiveStaticCache]
public class JournalLineData : PooledFrameworkDataElement<JournalLineData>
{
	public string searchText;

	public bool category;

	public bool categoryExpanded;

	public string categoryName;

	public IBaseJournalEntry entry;

	public IRenderable renderable;

	public int categoryOffset;

	public Location2D _mapTarget;

	public int sultanPeriod;

	public JournalStatusScreen screen;

	[ModSensitiveStaticCache(true)]
	public static Dictionary<string, int> sultanPeriods = new Dictionary<string, int>();

	[ModSensitiveStaticCache(true)]
	private static Dictionary<JournalVillageNote, Location2D> villageLocations = new Dictionary<JournalVillageNote, Location2D>();

	public Location2D mapTarget
	{
		get
		{
			if (_mapTarget != null)
			{
				return _mapTarget;
			}
			if (entry is JournalMapNote journalMapNote)
			{
				_mapTarget = Location2D.Get(journalMapNote.ParasangX, journalMapNote.ParasangY);
			}
			else
			{
				IBaseJournalEntry baseJournalEntry = entry;
				JournalVillageNote villageNote = baseJournalEntry as JournalVillageNote;
				if (villageNote != null)
				{
					if (villageLocations == null)
					{
						villageLocations = new Dictionary<JournalVillageNote, Location2D>();
					}
					if (!villageLocations.ContainsKey(villageNote))
					{
						if (villageNote.VillageID == "Joppa")
						{
							_mapTarget = The.ZoneManager.GetZone("JoppaWorld").GetCellsWithObject("TerrainJoppa").First()
								.Location;
						}
						else if (villageNote.VillageID == "The Yd Freehold")
						{
							_mapTarget = The.ZoneManager.GetZone("JoppaWorld").GetCellsWithObject("TerrainPalladiumReef 3l").First()
								.Location;
						}
						else if (villageNote.VillageID == "Kyakukya")
						{
							_mapTarget = The.ZoneManager.GetZone("JoppaWorld").GetCellsWithObject("TerrainKyakukya").First()
								.Location;
						}
						else
						{
							JournalMapNote journalMapNote2 = JournalAPI.GetMapNotes((JournalMapNote note) => note.Text == HistoryAPI.GetEntityName(villageNote.VillageID)).FirstOrDefault();
							if (journalMapNote2 != null)
							{
								_mapTarget = Location2D.Get(journalMapNote2.ParasangX, journalMapNote2.ParasangY);
							}
							else
							{
								_mapTarget = null;
							}
						}
						villageLocations.Add(villageNote, _mapTarget);
						return _mapTarget;
					}
					return villageLocations[villageNote];
				}
			}
			return _mapTarget;
		}
	}

	public JournalLineData set(IBaseJournalEntry entry, JournalStatusScreen screen, bool category = false, bool categoryExpanded = false, string categoryName = null, IRenderable renderable = null, int sultanPeriod = 0, int categoryOffset = 0)
	{
		this.screen = screen;
		_mapTarget = null;
		this.sultanPeriod = sultanPeriod;
		this.entry = entry;
		this.category = category;
		this.categoryExpanded = categoryExpanded;
		this.categoryName = categoryName;
		this.renderable = renderable;
		searchText = entry?.GetDisplayText()?.ToLower();
		this.categoryOffset = categoryOffset;
		if (entry is JournalSultanNote journalSultanNote)
		{
			if (!sultanPeriods.ContainsKey(journalSultanNote.SultanID))
			{
				sultanPeriods.Add(journalSultanNote.SultanID, Convert.ToInt32(HistoryAPI.GetEntityCurrentSnapshot(journalSultanNote.SultanID)?.GetProperty("period", "0") ?? "0"));
			}
			this.sultanPeriod = sultanPeriods[journalSultanNote.SultanID];
		}
		if (entry is JournalRecipeNote journalRecipeNote)
		{
			this.renderable = journalRecipeNote.Recipe.GetRenderable();
		}
		return this;
	}

	public override void free()
	{
		screen = null;
		sultanPeriod = -1;
		categoryOffset = 0;
		_mapTarget = null;
		renderable = null;
		category = false;
		categoryExpanded = false;
		categoryName = null;
		searchText = null;
		entry = null;
		base.free();
	}
}
