using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:Pregen", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/PickSubtype", UICanvasHost = 1)]
public class QudPregenModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudPregenModule, HorizontalScroller>
{
	public override void RandomSelection()
	{
		prefabComponent.scrollContext.GetContextAt(Stat.Random(0, prefabComponent.choices.Count - 1)).ActivateAndEnable();
	}

	public IEnumerable<ChoiceWithColorIcon> GetSelections()
	{
		string genotype = GetModule<QudGenotypeModule>().data.Genotype;
		IEnumerable<QudPregenModule.QudPregenData> enumerable = module.pregens.Values.Where((QudPregenModule.QudPregenData p) => p.Genotype == genotype);
		foreach (QudPregenModule.QudPregenData item in enumerable)
		{
			yield return new ChoiceWithColorIcon
			{
				Id = item.Name,
				Title = item.Name,
				IconPath = item.Tile,
				HFlip = true,
				IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap[item.Detail[0]],
				IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap[item.Foreground[0]],
				Description = item.Description
			};
		}
	}

	public void onSelectPregen(FrameworkDataElement choice)
	{
		if (TutorialManager.AllowSelectPregen(choice.Id))
		{
			module.SelectPregen(choice.Id);
		}
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		prefabComponent.autoHotkey = true;
		prefabComponent.onSelected.RemoveAllListeners();
		prefabComponent.onSelected.AddListener(onSelectPregen);
		prefabComponent.BeforeShow(descriptor, GetSelections());
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
			Title = "Pregens"
		};
		string text;
		if (module?.data?.Pregen != null)
		{
			QudPregenModule qudPregenModule = module;
			if (qudPregenModule != null && qudPregenModule.pregens.ContainsKey(module?.data?.Pregen))
			{
				text = module?.pregens[module?.data?.Pregen]?.Tile;
				goto IL_00c1;
			}
		}
		text = UIBreadcrumb.DEFAULT_ICON;
		goto IL_00c1;
		IL_00c1:
		if (text == null)
		{
			text = UIBreadcrumb.DEFAULT_ICON;
		}
		uIBreadcrumb.IconPath = text;
		uIBreadcrumb.HFlip = true;
		uIBreadcrumb.IconDetailColor = Color.clear;
		uIBreadcrumb.IconForegroundColor = The.Color.Gray;
		return uIBreadcrumb;
	}
}
