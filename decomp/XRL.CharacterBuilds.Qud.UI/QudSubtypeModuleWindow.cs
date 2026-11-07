using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:Subtype", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/PickSubtype", UICanvasHost = 1)]
public class QudSubtypeModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudSubtypeModule, HorizontalScroller>
{
	public override bool isWindowEnabled => !module.hasCategories();

	public override void RandomSelectionNoUI()
	{
		module.SelectSubtype(module.getAvailableSelections().GetRandomElement().Name);
	}

	public override void RandomSelection()
	{
		int index = Stat.Random(0, prefabComponent.choices.Count - 1);
		prefabComponent.scrollContext.GetContextAt(index).ActivateAndEnable();
		module.SelectSubtype(prefabComponent.choices[index].Id);
	}

	public void onSelectSubtype(FrameworkDataElement choice)
	{
		module.SelectSubtype(choice.Id);
		module.builder.advance();
	}

	public string getSubtypeTitle()
	{
		return module.getAvailableSelections().FirstOrDefault()?.SubtypeClass.ChargenTitle ?? "choose subtype";
	}

	public string getSubtypeSingularTitle()
	{
		return module.getAvailableSelections().FirstOrDefault()?.SubtypeClass.SingluarTitle ?? "subtype";
	}

	public virtual IEnumerable<FrameworkDataElement> GetSelections()
	{
		return module.GetSelections();
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		prefabComponent.autoHotkey = true;
		prefabComponent.onSelected.RemoveAllListeners();
		prefabComponent.onSelected.AddListener(onSelectSubtype);
		HorizontalScrollerScroller scrollscroller = prefabComponent as HorizontalScrollerScroller;
		if (scrollscroller != null)
		{
			scrollscroller.onHighlight.RemoveAllListeners();
			scrollscroller.onHighlight.AddListener((FrameworkDataElement a) =>
			{
				scrollscroller.UpdateHighlightText();
			});
		}
		prefabComponent.BeforeShow(descriptor, GetSelections());
		prefabComponent.titleText.SetText(":" + getSubtypeTitle() + ":");
		base.BeforeShow(descriptor);
	}

	public override IEnumerable<MenuOption> GetKeyLegend()
	{
		foreach (MenuOption item in base.GetKeyLegend())
		{
			yield return item;
		}
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		UIBreadcrumb uIBreadcrumb = new UIBreadcrumb
		{
			Id = GetType().FullName,
			Title = Grammar.InitCapWithFormatting(getSubtypeSingularTitle()),
			IconPath = UIBreadcrumb.DEFAULT_ICON,
			HFlip = true,
			IconDetailColor = Color.clear,
			IconForegroundColor = The.Color.Gray
		};
		foreach (SubtypeEntry subtype in module.subtypes)
		{
			if (subtype.Name == module.getSelected())
			{
				uIBreadcrumb.Title = subtype.DisplayName;
				uIBreadcrumb.IconPath = subtype.Tile;
				uIBreadcrumb.IconDetailColor = The.Color.DarkBlue;
				uIBreadcrumb.IconForegroundColor = The.Color.Gray;
				break;
			}
		}
		return uIBreadcrumb;
	}
}
