using System.Collections.Generic;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:BuildSummary", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/BuildSummary", UICanvasHost = 1)]
public class QudBuildSummaryModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudBuildSummaryModule, HorizontalScroller>
{
	public IEnumerable<SummaryBlockData> GetSelections()
	{
		List<SummaryBlockData> list = new List<SummaryBlockData>();
		foreach (AbstractEmbarkBuilderModule enabledModule in module.builder.enabledModules)
		{
			SummaryBlockData summaryBlock = enabledModule.GetSummaryBlock();
			if (summaryBlock != null)
			{
				list.Add(summaryBlock);
			}
		}
		list.Sort((SummaryBlockData a, SummaryBlockData b) => a.SortOrder - b.SortOrder);
		foreach (SummaryBlockData item in list)
		{
			yield return item;
		}
	}

	public override void Init()
	{
		prefabComponent.selectionPrefab = ((GameObject)Resources.Load("Prefabs/SummaryTextColumn")).GetComponent<FrameworkUnityScrollChild>();
		prefabComponent.gridLayout.cellSize = new Vector2(300f, 200f);
		prefabComponent.gridLayout.spacing = new Vector2(20f, 20f);
		prefabComponent.descriptionText?.gameObject.DestroyImmediate();
		prefabComponent.descriptionText = null;
		base.Init();
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		prefabComponent.scrollContext.wraps = false;
		prefabComponent.onSelected.RemoveAllListeners();
		prefabComponent.BeforeShow(descriptor, GetSelections());
		base.BeforeShow(descriptor);
	}

	public override void AfterShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		GetOverlayWindow().nextButton.navigationContext.ActivateAndEnable();
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		return new UIBreadcrumb
		{
			Id = GetType().FullName,
			Title = "Summary",
			IconPath = "Items/sw_unfurled_scroll1.bmp",
			IconDetailColor = Color.clear,
			IconForegroundColor = The.Color.Gray
		};
	}

	public override IEnumerable<MenuOption> GetKeyMenuBar()
	{
		if (module.builder.GetModule<QudChartypeModule>().data.type == "Random")
		{
			yield return new MenuOption
			{
				Id = AbstractBuilderModuleWindowBase.RANDOM,
				InputCommand = "CmdChargenRandom",
				KeyDescription = ControlManager.getCommandInputDescription("CmdChargenRandom"),
				Description = "Re-Randomize Selections"
			};
		}
		if (CapabilityManager.AllowClipboardFunctionality)
		{
			yield return new MenuOption
			{
				Id = "CmdExportCode",
				InputCommand = "CmdExportCode",
				KeyDescription = "None",
				Description = "Export Code to Clipboard"
			};
		}
		yield return new MenuOption
		{
			Id = "CmdSaveBuildToLibrary",
			InputCommand = "CmdSaveBuildToLibrary",
			KeyDescription = "None",
			Description = "Save Build To Library"
		};
	}

	public override async void HandleMenuOption(MenuOption menuOption)
	{
		if (menuOption.Id == "CmdSaveBuildToLibrary")
		{
			await QudBuildLibraryModuleWindow.AddBuild(module.builder.generateCode());
		}
		if (menuOption.Id == "CmdExportCode")
		{
			ClipboardHelper.SetClipboardData(module.builder.generateCode());
			Popup.NewPopupMessageAsync("Build code copied to clipboard.");
			return;
		}
		if (menuOption.Id == AbstractBuilderModuleWindowBase.RANDOM)
		{
			module.builder.ShowWindow<QudChartypeModuleWindow>();
			module.builder.SkippingUIUpdates = true;
			try
			{
				module.builder.GetModule<QudChartypeModule>().selectType("Random");
				return;
			}
			finally
			{
				module.builder.SkippingUIUpdates = false;
			}
		}
		base.HandleMenuOption(menuOption);
	}
}
