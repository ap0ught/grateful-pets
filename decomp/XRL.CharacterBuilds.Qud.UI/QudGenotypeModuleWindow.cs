using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:Mutations", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/PickMutations", UICanvasHost = 1)]
public class QudGenotypeModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudGenotypeModule, HorizontalScroller>
{
	public override void RandomSelection()
	{
		int index = Stat.Random(0, prefabComponent.choices.Count - 1);
		prefabComponent.scrollContext.GetContextAt(index).ActivateAndEnable();
		module.SelectGenotype(prefabComponent.choices[index].Id);
	}

	public virtual IEnumerable<ChoiceWithColorIcon> GetSelections()
	{
		foreach (GenotypeEntry genotype in module.genotypes)
		{
			yield return new ChoiceWithColorIcon
			{
				Id = genotype.Name,
				Title = genotype.DisplayName,
				IconPath = genotype.Tile,
				IconDetailColor = (genotype.DetailColor.IsNullOrEmpty() ? The.Color.Brown : ConsoleLib.Console.ColorUtility.ColorMap[genotype.DetailColor[0]]),
				IconForegroundColor = The.Color.Gray,
				Description = genotype.GetFlatChargenInfo(),
				Chosen = IsChoiceSelected
			};
		}
	}

	public bool IsChoiceSelected(ChoiceWithColorIcon choice)
	{
		return module.getSelected() == choice?.Id;
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		UIBreadcrumb uIBreadcrumb = new UIBreadcrumb
		{
			Id = GetType().FullName,
			Title = "Choose Genotype",
			IconPath = UIBreadcrumb.DEFAULT_ICON,
			IconDetailColor = Color.clear,
			IconForegroundColor = The.Color.Gray
		};
		foreach (GenotypeEntry genotype in module.genotypes)
		{
			if (genotype.Name == module.getSelected())
			{
				uIBreadcrumb.Title = genotype.DisplayName;
				uIBreadcrumb.IconPath = genotype.Tile;
				uIBreadcrumb.IconDetailColor = The.Color.DarkBlue;
				uIBreadcrumb.IconForegroundColor = The.Color.Gray;
				break;
			}
		}
		return uIBreadcrumb;
	}

	public void onSelectGenotype(FrameworkDataElement choice)
	{
		if (TutorialManager.AllowSelectGenotype(choice?.Id))
		{
			module.SelectGenotype(choice.Id);
			module.builder.advance();
		}
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		prefabComponent.autoHotkey = true;
		prefabComponent.scrollContext.wraps = false;
		prefabComponent.onSelected.RemoveAllListeners();
		prefabComponent.onSelected.AddListener(onSelectGenotype);
		prefabComponent.BeforeShow(descriptor, GetSelections());
		base.BeforeShow(descriptor);
	}
}
