using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:BuildLibrary", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/BuildLibrary", UICanvasHost = 1)]
public class QudBuildLibraryModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudBuildLibraryModule, HorizontalScroller>
{
	private string selected;

	private bool hasOptions;

	public override void RandomSelection()
	{
		prefabComponent.scrollContext.GetContextAt(Stat.Random(0, prefabComponent.choices.Count - 1)).ActivateAndEnable();
	}

	public void onBack()
	{
		module.onBack();
	}

	public IEnumerable<ChoiceWithColorIcon> GetSelections()
	{
		List<BuildEntry> buildEntries = BuildLibrary.BuildEntries;
		foreach (BuildEntry item in buildEntries)
		{
			yield return new ChoiceWithColorIcon
			{
				Id = "code:" + item.Code,
				Title = item.Name,
				IconPath = item.Tile,
				IconDetailColor = ConsoleLib.Console.ColorUtility.colorFromChar(string.IsNullOrEmpty(item.Detail) ? 'w' : item.Detail[0]),
				IconForegroundColor = ConsoleLib.Console.ColorUtility.colorFromChar(string.IsNullOrEmpty(item.Foreground) ? 'y' : item.Foreground[0]),
				Description = ""
			};
		}
		new List<ChoiceWithColorIcon>();
		yield return new ChoiceWithColorIcon
		{
			Id = "___CreateNew",
			Title = "Add a new build code",
			IconPath = "UI/sw_newchar.bmp",
			IconDetailColor = Color.yellow,
			IconForegroundColor = Color.gray,
			Description = ""
		};
	}

	public async void onSelect(FrameworkDataElement choice)
	{
		if (choice.Id.StartsWith("code:"))
		{
			string text = choice.Id.Split(':')[1];
			module.builder.InitModulesFromCode(text);
			module.setData(new QudBuildLibraryModuleData(text));
			EmbarkBuilder builder = module.builder;
			EmbarkBuilderModuleWindowDescriptor activeWindow = builder.activeWindow;
			do
			{
				builder.advance();
				if (builder.activeWindow != null && builder.activeWindow != activeWindow)
				{
					activeWindow = builder.activeWindow;
					continue;
				}
				break;
			}
			while (builder?.activeWindow?.viewID != "Chargen/BuildSummary");
		}
		else if (choice.Id.StartsWith("___CreateNew"))
		{
			string text2 = await Popup.AskStringAsync("Paste build code:", "", 80, 0, null, ReturnNullForEscape: true, EscapeNonMarkupFormatting: true, false);
			if (text2 != null)
			{
				await AddBuild(text2);
				prefabComponent.BeforeShow(descriptor, GetSelections());
				prefabComponent.onHighlight.AddListener(HighlightBuild);
			}
		}
	}

	public static async Task AddBuild(string code)
	{
		new EmbarkBuilder();
		EmbarkBuilder builder;
		try
		{
			builder = EmbarkBuilder.FromCode(code, silent: true);
		}
		catch
		{
			await Popup.NewPopupMessageAsync("That code appears to be invalid.");
			return;
		}
		if (BuildLibrary.HasBuild(code))
		{
			await Popup.NewPopupMessageAsync("That code is already in your library. It's named " + BuildLibrary.GetBuild(code).Name + ".");
			return;
		}
		string text = await Popup.AskStringAsync("Name this build:", "", 80, 0, null, ReturnNullForEscape: true);
		if (text != null)
		{
			string tile = builder?.GetModule<QudSubtypeModule>()?.data?.Entry?.Tile ?? "Assets_Content_Textures_Creatures_sw_farmer.bmp";
			string foreground = "y";
			string detail = builder?.GetModule<QudSubtypeModule>()?.data?.Entry?.DetailColor ?? "w";
			BuildLibrary.AddBuild(code, text, tile, foreground, detail);
		}
	}

	public override async void HandleMenuOption(MenuOption menuOption)
	{
		if (menuOption?.Id == "CmdChargenItemOptions" && hasOptions)
		{
			BuildEntry build = BuildLibrary.GetBuild(selected.Split(':')[1]);
			QudMenuItem qudMenuItem = await Popup.NewPopupMessageAsync("Manage Build: " + build.Name, PopupMessage.AcceptCancelButton, new List<QudMenuItem>
			{
				new QudMenuItem
				{
					text = "Delete Build",
					command = "delete"
				},
				new QudMenuItem
				{
					text = "Rename Build",
					command = "rename"
				}
			});
			if (qudMenuItem.command != "Cancel")
			{
				if (qudMenuItem.command == "delete")
				{
					BuildLibrary.DeleteBuild(build.Code);
					BeforeShow(descriptor);
				}
				else if (qudMenuItem.command == "rename")
				{
					string text = await Popup.AskStringAsync("Name this build:", "", 80, 0, null, ReturnNullForEscape: true);
					if (text != null)
					{
						build.Name = text;
						BuildLibrary.UpdateBuild(build);
						prefabComponent.BeforeShow(descriptor, GetSelections());
						prefabComponent.onHighlight.AddListener(HighlightBuild);
						BeforeShow(descriptor);
					}
				}
			}
		}
		base.HandleMenuOption(menuOption);
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		prefabComponent.onSelected.RemoveAllListeners();
		prefabComponent.onSelected.AddListener(onSelect);
		prefabComponent.onSelected.AddListener(HighlightBuild);
		prefabComponent.BeforeShow(descriptor, GetSelections());
		prefabComponent.onHighlight.AddListener(HighlightBuild);
		HighlightBuild(prefabComponent.scrollContext.data[prefabComponent.selectedPosition]);
		base.BeforeShow(descriptor);
	}

	public override IEnumerable<MenuOption> GetKeyLegend()
	{
		foreach (MenuOption item in base.GetKeyLegend())
		{
			yield return item;
		}
	}

	public void HighlightBuild(FrameworkDataElement dataElement)
	{
		selected = dataElement?.Id;
		hasOptions = dataElement?.Id?.Contains("code:") == true;
		GetOverlayWindow().UpdateMenuBars(GetOverlayWindow().currentWindowDescriptor);
	}

	public override IEnumerable<MenuOption> GetKeyMenuBar()
	{
		if (hasOptions)
		{
			yield return new MenuOption
			{
				Id = "CmdChargenItemOptions",
				InputCommand = "CmdChargenItemOptions",
				KeyDescription = ControlManager.getCommandInputDescription("CmdChargenItemOptions"),
				Description = "Options"
			};
		}
		foreach (MenuOption item in base.GetKeyMenuBar())
		{
			yield return item;
		}
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		return new UIBreadcrumb
		{
			Id = GetType().FullName,
			Title = "Build Library",
			IconPath = "Items/sw_bookshelf3.bmp",
			IconDetailColor = Color.clear,
			IconForegroundColor = The.Color.Gray
		};
	}
}
