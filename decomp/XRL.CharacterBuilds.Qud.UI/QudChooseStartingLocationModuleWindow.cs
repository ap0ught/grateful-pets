using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:ChooseStartingLocation", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/ChooseStartingLocation", UICanvasHost = 1)]
public class QudChooseStartingLocationModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudChooseStartingLocationModule, HorizontalScroller>
{
	public static class EventNames
	{
		public static readonly string EID_GET_STARTING_LOCATION_SET = "QudChooseStartingLocationModuleGetStartingLocationSet";
	}

	public string currentSet;

	public IEnumerable<StartingLocationData> GetSelections()
	{
		foreach (StartingLocationData value in module.startingLocations.Values)
		{
			yield return value;
		}
	}

	public void onNext()
	{
		module.onNext();
	}

	public void onBack()
	{
		module.onBack();
	}

	public void onHighlightStartingLocation(FrameworkDataElement element)
	{
		module.data.StartingLocation = element.Id;
		module.setData(module.data);
	}

	public void onSelectStartingLocation(FrameworkDataElement element)
	{
		module.data.StartingLocation = element.Id;
		module.setData(module.data);
		onNext();
	}

	public override void ResetSelection()
	{
		module.setData(new QudChooseStartingLocationModuleData((from s in GetSelections()
			where s.Set == currentSet
			select s).FirstOrDefault().Id));
		prefabComponent.selectedPosition = 0;
	}

	public override void DebugQuickstart(string type)
	{
		module.data.StartingLocation = "Joppa";
		module.setData(module.data);
	}

	public override void RandomSelectionNoUI()
	{
		int index = Stat.Random(0, prefabComponent.choices.Count - 1);
		module.data.StartingLocation = prefabComponent.choices[index].Id;
		module.setData(module.data);
	}

	public override void RandomSelection()
	{
		int index = Stat.Random(0, prefabComponent.choices.Count - 1);
		prefabComponent.scrollContext.GetContextAt(index).ActivateAndEnable();
		module.data.StartingLocation = prefabComponent.choices[index].Id;
		module.setData(module.data);
	}

	public override void Init()
	{
		prefabComponent.selectionPrefab = ((GameObject)Resources.Load("Prefabs/StartingLocationButton")).GetComponent<FrameworkUnityScrollChild>();
		prefabComponent.gridLayout.cellSize = new Vector2(200f, 225f);
		prefabComponent.gridLayout.spacing = new Vector2(20f, 20f);
		base.Init();
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		module.data = new QudChooseStartingLocationModuleData();
		module.data.StartingLocation = "Joppa";
		prefabComponent.autoHotkey = true;
		prefabComponent.scrollContext.wraps = false;
		prefabComponent.onSelected.RemoveAllListeners();
		prefabComponent.onSelected.AddListener(onSelectStartingLocation);
		prefabComponent.onHighlight.AddListener(onHighlightStartingLocation);
		currentSet = module?.builder?.handleUIEvent(EventNames.EID_GET_STARTING_LOCATION_SET) as string;
		prefabComponent.BeforeShow(descriptor, from s in GetSelections()
			where s.Set == currentSet
			select s);
		base.BeforeShow(descriptor);
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		UIBreadcrumb uIBreadcrumb = new UIBreadcrumb
		{
			Id = "StartingLocation",
			Title = "Select Starting Location",
			IconPath = UIBreadcrumb.DEFAULT_ICON,
			IconDetailColor = Color.clear,
			IconForegroundColor = The.Color.Gray
		};
		StartingLocationData startingLocation = module.startingLocation;
		if (startingLocation != null)
		{
			uIBreadcrumb.Title = startingLocation.Name.Split('\n')[0].Strip();
			uIBreadcrumb.IconPath = startingLocation.grid["21"].Tile;
			uIBreadcrumb.IconDetailColor = The.Color[startingLocation.grid["21"].Detail[0]];
			uIBreadcrumb.IconForegroundColor = The.Color[startingLocation.grid["21"].Foreground[0]];
		}
		return uIBreadcrumb;
	}
}
