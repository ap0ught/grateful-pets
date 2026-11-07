using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using FuzzySharp;
using FuzzySharp.Extractor;
using Genkit;
using Qud.API;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

[UIView("QuestsStatusScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menus", UICanvas = "StatusScreens", UICanvasHost = 1)]
public class QuestsStatusScreen : BaseStatusScreen<QuestsStatusScreen>
{
	public FrameworkScroller controller;

	private Renderable icon;

	public HashSet<string> CollapsedEntries = new HashSet<string>();

	public QuestsLineData searcher = new QuestsLineData();

	private List<QuestsLineData> searchResult = new List<QuestsLineData>();

	public MapScrollerController mapController;

	public UnityEngine.GameObject mapScroller;

	private FilterBar filterBar;

	public override IRenderable GetTabIcon()
	{
		if (icon == null)
		{
			icon = new Renderable("Items/sw_scroll1.bmp", " ", "&y", null, 'Y');
		}
		return icon;
	}

	public override string GetTabString()
	{
		return "Quests";
	}

	public void HandleVPositive()
	{
		CollapsedEntries.Clear();
		UpdateViewFromData();
	}

	public void HandleVNegative()
	{
		CollapsedEntries.Clear();
		foreach (Quest item in from q in QuestsAPI.allQuests()
			where !q.Finished
			select q)
		{
			CollapsedEntries.Add(item.ID);
		}
		UpdateViewFromData();
	}

	public void HandleSelectItem(FrameworkDataElement element)
	{
		if (element is QuestsLineData questsLineData)
		{
			questsLineData.expanded = !questsLineData.expanded;
			if (!questsLineData.expanded)
			{
				CollapsedEntries.Add(questsLineData.quest.ID);
			}
			if (questsLineData.expanded)
			{
				CollapsedEntries.Remove(questsLineData.quest.ID);
			}
			UpdateViewFromData();
		}
	}

	public void HandleHighlightItem(FrameworkDataElement element)
	{
		if (element is QuestsLineData questsLineData && questsLineData != null && questsLineData.quest != null && questsLineData.quest.QuestGiverLocationZoneID != null)
		{
			Location2D location2D = ZoneManager.GetWorldMapLocationForZoneID(questsLineData.quest.QuestGiverLocationZoneID);
			if (location2D == null)
			{
				location2D = Location2D.Get(0, 0);
			}
			mapController.SetTarget(location2D.X, location2D.Y);
		}
	}

	public void OnSearchTextChange(string text)
	{
		searcher.searchText = text?.ToLower();
		UpdateViewFromData();
	}

	public override void UpdateViewFromData()
	{
		foreach (QuestsLineData item in searchResult)
		{
			item.free();
		}
		searchResult.Clear();
		IEnumerable<QuestsLineData> enumerable = from quest in QuestsAPI.allQuests()
			where !quest.Finished
			select PooledFrameworkDataElement<QuestsLineData>.next().set(quest, !CollapsedEntries.Contains(quest.ID));
		if (string.IsNullOrEmpty(searcher.searchText))
		{
			searchResult.AddRange(enumerable);
		}
		else
		{
			searchResult.AddRange(from m in Process.ExtractTop(searcher, enumerable, (QuestsLineData i) => i.searchText, null, enumerable.Count(), 50)
				select m.Value);
		}
		IEnumerable<Quest> enumerable2 = searchResult.Select((QuestsLineData d) => d.quest);
		HashSet<string> hashSet = new HashSet<string>();
		List<MapScrollerController.MapPinData> list = new List<MapScrollerController.MapPinData>();
		foreach (Quest q in enumerable2)
		{
			if (hashSet.Contains(q.QuestGiverLocationZoneID))
			{
				continue;
			}
			hashSet.Add(q.QuestGiverLocationZoneID);
			MapScrollerController.MapPinData mapPinData = new MapScrollerController.MapPinData();
			Location2D location2D = ZoneManager.GetWorldMapLocationForZoneID(q.QuestGiverLocationZoneID);
			if (location2D == null)
			{
				location2D = Location2D.Get(0, 0);
			}
			if (!(location2D != null))
			{
				continue;
			}
			mapPinData.x = location2D.X;
			mapPinData.y = location2D.Y;
			mapPinData.title = "{{W|" + q.QuestGiverLocationName + "}}";
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Quest item2 in enumerable2.Where((Quest l) => l.QuestGiverLocationName == q.QuestGiverLocationName))
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append("\n");
				}
				stringBuilder.Append("{{B|quest:}} " + ConsoleLib.Console.ColorUtility.StripFormatting(item2.DisplayName));
			}
			mapPinData.details = stringBuilder.ToString();
			list.Add(mapPinData);
		}
		if (searchResult.Count == 0)
		{
			searchResult.Add(new QuestsLineData().set(null, expanded: false));
		}
		mapController.SetHighlights(from d in searchResult
			select d?.quest into quest
			where quest != null && !quest.Finished && quest.QuestGiverLocationZoneID != null
			select ZoneManager.GetWorldMapLocationForZoneID(quest?.QuestGiverLocationZoneID) ?? Location2D.Get(0, 0));
		controller.BeforeShow(searchResult);
		mapController.RefreshMap();
		mapController.SetPins(list);
		mapController.mapTarget.gameObject.SetActive(value: false);
	}

	public override void FilterUpdated(string filterText)
	{
		OnSearchTextChange(filterText);
	}

	public override NavigationContext ShowScreen(XRL.World.GameObject GO, StatusScreensScreen parent)
	{
		SingletonWindowBase<QuestsStatusScreen>.instance = this;
		searcher.searchText = "";
		filterBar = parent.filterBar;
		controller.onSelected.RemoveAllListeners();
		controller.onSelected.AddListener(HandleSelectItem);
		controller.onHighlight.RemoveAllListeners();
		controller.onHighlight.AddListener(HandleHighlightItem);
		controller.scrollContext.wraps = false;
		UpdateViewFromData();
		controller.selectedPosition = 0;
		controller.scrollContext.ActivateAndEnable();
		ScrollContext<FrameworkDataElement, NavigationContext> scrollContext = controller.scrollContext;
		if (scrollContext.commandHandlers == null)
		{
			scrollContext.commandHandlers = new Dictionary<string, Action>();
		}
		controller.scrollContext.commandHandlers["V Positive"] = HandleVPositive;
		controller.scrollContext.commandHandlers["V Negative"] = HandleVNegative;
		base.ShowScreen(GO, parent);
		return controller.scrollContext;
	}
}
