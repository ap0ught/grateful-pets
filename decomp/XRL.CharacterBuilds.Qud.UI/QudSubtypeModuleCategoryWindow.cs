using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:SubtypeCategory", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/PickSubtypeCategory", UICanvasHost = 1)]
public class QudSubtypeModuleCategoryWindow : QudSubtypeModuleWindow
{
	public override bool isWindowEnabled => module.hasCategories();

	public override void RandomSelection()
	{
		var list = GetSelections().SelectMany((FrameworkDataElement cat, int catn) => (cat as IFrameworkDataList).getChildren().Select((FrameworkDataElement choice, int choicen) => new { catn, cat, choice, choicen })).ToList();
		int index = Stat.Random(0, list.Count - 1);
		var anon = list[index];
		prefabComponent.GetPrefabForIndex(anon.catn).GetComponent<FrameworkScroller>().scrollContext.GetContextAt(anon.choicen).ActivateAndEnable();
		module.SelectSubtype(anon.choice.Id);
	}

	public override IEnumerable<FrameworkDataElement> GetSelections()
	{
		return module.GetSelectionCategories();
	}

	public override void Init()
	{
		prefabComponent.selectionPrefab = ((GameObject)Resources.Load("Prefabs/CategoryIconScroller")).GetComponent<FrameworkUnityScrollChild>();
		prefabComponent.GetComponentInChildren<GridLayoutGroup>(includeInactive: true).cellSize = new Vector2(500f, 200f);
	}
}
