using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:Modes", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/Modes", UICanvasHost = 1)]
public class QudGamemodeModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudGamemodeModule, HorizontalScroller>
{
	public static readonly MenuOption QUICKSTART = new MenuOption
	{
		Id = "_Quickstart",
		InputCommand = "CmdDebugQuickstart",
		KeyDescription = "Q",
		Description = "[Debug] Quickstart"
	};

	public IEnumerable<ChoiceWithColorIcon> GetSelections()
	{
		foreach (QudGamemodeModule.GameModeDescriptor value in module.GameModes.Values)
		{
			yield return new ChoiceWithColorIcon
			{
				Id = value.ID,
				Title = value.Title,
				IconPath = value.IconTile,
				IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap[value.IconDetail[0]],
				IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap[value.IconForeground[0]],
				Description = value.Description.Replace("{day_of_year}", DateTime.Now.DayOfYear.ToString()).Replace("{year}", DateTime.Now.Year.ToString()),
				Chosen = IsChoiceSelected
			};
		}
	}

	public bool IsChoiceSelected(ChoiceWithColorIcon choice)
	{
		return choice?.Id == module.GetMode();
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		UIBreadcrumb uIBreadcrumb = new UIBreadcrumb
		{
			Id = "Gamemode",
			Title = "Choose Game Mode",
			IconPath = UIBreadcrumb.DEFAULT_ICON,
			IconDetailColor = Color.clear,
			IconForegroundColor = The.Color.Gray
		};
		foreach (QudGamemodeModule.GameModeDescriptor value in module.GameModes.Values)
		{
			if (value.ID == module.GetMode())
			{
				uIBreadcrumb.Title = value.Title;
				uIBreadcrumb.IconPath = value.IconTile;
				uIBreadcrumb.IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap[value.IconDetail[0]];
				uIBreadcrumb.IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap[value.IconForeground[0]];
				break;
			}
		}
		return uIBreadcrumb;
	}

	public override IEnumerable<MenuOption> GetKeyMenuBar()
	{
		foreach (MenuOption item in base.GetKeyMenuBar())
		{
			yield return item;
		}
		if (Options.ShowQuickstartOption)
		{
			yield return QUICKSTART;
		}
	}

	public override void RandomSelection()
	{
		prefabComponent.scrollContext.GetContextAt(Stat.Random(0, prefabComponent.choices.Count - 1)).ActivateAndEnable();
	}

	public void ChoiceSelected(FrameworkDataElement data)
	{
		module.SelectMode(data.Id);
	}

	public override void HandleMenuOption(MenuOption menuOption)
	{
		base.HandleMenuOption(menuOption);
		if (menuOption == QUICKSTART)
		{
			module.SelectMode(menuOption.Id);
		}
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		TutorialManager.EndTutorial();
		prefabComponent.autoHotkey = true;
		prefabComponent.onSelected.RemoveAllListeners();
		prefabComponent.onSelected.AddListener(ChoiceSelected);
		List<ChoiceWithColorIcon> list = GetSelections().ToList();
		if (!Options.ModernUI)
		{
			ChoiceWithColorIcon choiceWithColorIcon = list.Where((ChoiceWithColorIcon mode) => mode.Id == "Tutorial").FirstOrDefault();
			if (choiceWithColorIcon != null)
			{
				list.Remove(choiceWithColorIcon);
			}
		}
		else if (Options.GetOption("OptionTutorialComplete") == "Yes")
		{
			ChoiceWithColorIcon choiceWithColorIcon2 = list.Where((ChoiceWithColorIcon mode) => mode.Id == "Tutorial").FirstOrDefault();
			if (choiceWithColorIcon2 != null)
			{
				list.Remove(choiceWithColorIcon2);
				list.Add(choiceWithColorIcon2);
			}
		}
		prefabComponent.BeforeShow(descriptor, list);
		base.BeforeShow(descriptor);
	}
}
