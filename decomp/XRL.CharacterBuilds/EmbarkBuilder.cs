using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Platform.IO;
using Qud.UI;
using UnityEngine;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds;

/// <summary>
/// The main Unity MonoBehaviour that controls the new Embark experience.  
/// There can only ever be one of these because it's attached to the GameManager.
/// </summary>
public class EmbarkBuilder : MonoBehaviour
{
	public static class EventNames
	{
		public static readonly string EditableGameModeQuery = "EditableGameModeQuery";
	}

	/// <summary>
	///     State of the embark builder selection process.
	/// </summary>
	public static TaskCompletionSource<EmbarkInfo> finishedEvent = new TaskCompletionSource<EmbarkInfo>();

	protected EmbarkInfo embarkInfo = new EmbarkInfo();

	protected List<AbstractEmbarkBuilderModule> modules;

	protected List<EmbarkBuilderModuleWindowDescriptor> windowDescriptors;

	public EmbarkBuilderModuleWindowDescriptor activeWindow;

	protected List<AbstractBuilderModuleWindowBase> windows;

	public List<AbstractBuilderModuleWindowBase> createdWindows = new List<AbstractBuilderModuleWindowBase>();

	private List<Predicate<AbstractEmbarkBuilderModule>> enabledChecks;

	public bool SkippingUIUpdates;

	/// <summary>
	///     Static helper to get at the game object.
	/// </summary>
	public new static GameObject gameObject => GameManager.Instance.gameObject;

	public EmbarkInfo info => embarkInfo;

	public IEnumerable<AbstractEmbarkBuilderModule> enabledModules => modules.Where((AbstractEmbarkBuilderModule m) => m.enabled);

	/// <summary>
	///     Start the embark builder.
	/// </summary>
	/// <returns>A task that will resolve to the EmbarkInfo or null.</returns>
	/// <exception cref="T:System.InvalidOperationException">When called from a thread other than the game thread.</exception>
	public static async Task<EmbarkInfo> Begin()
	{
		finishedEvent = new TaskCompletionSource<EmbarkInfo>();
		if (Thread.CurrentThread == GameManager.Instance.uiQueue.threadContext)
		{
			throw new InvalidOperationException("EmbarkBuilder::Begin can only be called from the game thread.");
		}
		GameManager.Instance.CurrentGameView = "EmbarkBuilder";
		await The.UiContext;
		GameManager.Instance.SetActiveLayersForNavCategory("Chargen");
		gameObject.GetComponent<EmbarkBuilder>()?.Destroy();
		GameManager.Instance.gameObject.AddComponent<EmbarkBuilder>().InitModules();
		EmbarkInfo result;
		try
		{
			result = await finishedEvent.Task.ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			await The.UiContext;
			NavigationController.instance.activeContext = null;
			ControlManager.EnableLayer("Adventure");
			ControlManager.EnableLayer("AltAdventure");
			ControlManager.EnableLayer("Chargen");
		}
		return result;
	}

	public string generateCode()
	{
		return CodeCompressor.generateCode(enabledModules);
	}

	public object handleUIEvent(string id, object element = null)
	{
		foreach (AbstractEmbarkBuilderModule enabledModule in enabledModules)
		{
			element = enabledModule.handleUIEvent(id, element);
		}
		return element;
	}

	public void ShowWindow<T>() where T : AbstractBuilderModuleWindowBase
	{
		EmbarkBuilderModuleWindowDescriptor embarkBuilderModuleWindowDescriptor = windowDescriptors.Find((EmbarkBuilderModuleWindowDescriptor desc) => desc.getWindow() is T);
		if (embarkBuilderModuleWindowDescriptor == null)
		{
			throw new Exception("Window of type T not found.");
		}
		ShowWindow(embarkBuilderModuleWindowDescriptor);
	}

	public void ShowWindow(EmbarkBuilderModuleWindowDescriptor windowDescriptor)
	{
		windowDescriptor?.show();
		if (windowDescriptor?.getWindow()?.UseOverlay() == true)
		{
			GetOverlayWindow().BeforeShowWithWindow(this, windowDescriptor);
			GetOverlayWindow().Show();
		}
		else
		{
			GetOverlayWindow().Hide();
		}
		NavigationContext navigationContext = windowDescriptor?.getWindow()?.GetNavigationContext();
		if (navigationContext == null)
		{
			NavigationController.instance.activeContext = null;
		}
		else
		{
			navigationContext.ActivateAndEnable();
		}
	}

	public void RefreshActiveWindow()
	{
		ShowWindow(activeWindow);
	}

	public EmbarkBuilderModuleWindowDescriptor GetNextEnabledWindowDescriptor(bool editableOnly)
	{
		int num = windowDescriptors.IndexOf(activeWindow);
		for (num++; num < windowDescriptors.Count; num++)
		{
			if (windowDescriptors[num].enabled && TutorialManager.IsEditableChargenPanel(windowDescriptors[num].name) && (!editableOnly || windowDescriptors[num].module.shouldBeEditable()))
			{
				return windowDescriptors[num];
			}
		}
		return null;
	}

	public EmbarkBuilderModuleWindowDescriptor GetWindowDescriptorByViewID(string id)
	{
		int num = windowDescriptors.IndexOf(activeWindow);
		for (num++; num < windowDescriptors.Count; num++)
		{
			if (windowDescriptors[num].viewID == id)
			{
				return windowDescriptors[num];
			}
		}
		return null;
	}

	public void advanceToSummary()
	{
		EmbarkBuilderModuleWindowDescriptor embarkBuilderModuleWindowDescriptor = activeWindow;
		do
		{
			advance();
			if (activeWindow != embarkBuilderModuleWindowDescriptor)
			{
				embarkBuilderModuleWindowDescriptor = activeWindow;
				continue;
			}
			break;
		}
		while (activeWindow?.viewID != "Chargen/BuildSummary");
	}

	/// <summary>
	///     Checks the state of the current window for data errors and warnings, potentially showing popup messages.
	///     In the event of errors, it will show a dialog box, wait for dismissal and return false.
	///     In the event of warnings, it will ask the player to confirm advancing the screen, returning true or false.
	///     If no errors or warnings, returns true.
	/// </summary>
	/// <returns>If the current window is "ready" to advance</returns>
	public async Task<bool> checkStateAsync()
	{
		string text = activeWindow?.getWindow()?.DataErrors();
		if (text != null)
		{
			await Popup.NewPopupMessageAsync(text, PopupMessage.SingleButton, null, "{{r|Error!}}");
			return false;
		}
		string text2 = activeWindow?.getWindow()?.DataWarnings();
		if (text2 != null && (await Popup.NewPopupMessageAsync(text2 + "\n\nContinue anyway?", PopupMessage.YesNoButton, null, "{{W|Warning!}}")).command != "Yes")
		{
			return false;
		}
		return true;
	}

	public async void advance(bool force = false, bool editableOnly = false, bool deferLayout = false)
	{
		if (force || await checkStateAsync())
		{
			EmbarkBuilderModuleWindowDescriptor nextEnabledWindowDescriptor = GetNextEnabledWindowDescriptor(editableOnly);
			if (nextEnabledWindowDescriptor == null)
			{
				exitWithInfo();
			}
			else if (!deferLayout)
			{
				ShowWindow(nextEnabledWindowDescriptor);
				nextEnabledWindowDescriptor?.window?.AfterShow(nextEnabledWindowDescriptor);
			}
			else
			{
				activeWindow = nextEnabledWindowDescriptor;
			}
		}
	}

	public void advanceToViewId(string viewid)
	{
		EmbarkBuilderModuleWindowDescriptor windowDescriptorByViewID = GetWindowDescriptorByViewID(viewid);
		if (windowDescriptorByViewID == null)
		{
			exitWithInfo();
		}
		else
		{
			ShowWindow(windowDescriptorByViewID);
		}
	}

	public void back()
	{
		int num = windowDescriptors.IndexOf(activeWindow);
		for (num--; num >= 0; num--)
		{
			if (windowDescriptors[num].enabled && windowDescriptors[num].module.shouldBeEditable() && TutorialManager.IsEditableChargenPanel(windowDescriptors[num].name))
			{
				ShowWindow(windowDescriptors[num]);
				return;
			}
		}
		exitWithoutInfo();
	}

	public async Task SaveCharacterCode()
	{
		string path = DataManager.SavePath("lastcharacter.txt");
		if (!(await Blob.WriteAllTextAsync(DataManager.SavePath("lastcharacter.txt"), generateCode())).WasSuccessful())
		{
			MetricsManager.LogWarning("Could not save \"" + path + "\"");
		}
	}

	public void exitWithInfo()
	{
		Debug.Log("exiting with info");
		try
		{
			SaveCharacterCode().Wait();
			if (Options.ModernUI)
			{
				WorldGenerationScreen.ShowWorldGenerationScreen(258);
			}
			ShowWindow(null);
			foreach (AbstractBuilderModuleWindowBase createdWindow in createdWindows)
			{
				createdWindow.DestroyWindow();
			}
			createdWindows.Clear();
			embarkInfo.modules.AddRange(modules.Where((AbstractEmbarkBuilderModule m) => m.enabled));
			embarkInfo.modules.ForEach((AbstractEmbarkBuilderModule m) =>
			{
				AbstractEmbarkBuilderModuleData data = m.getData();
				if (data != null)
				{
					embarkInfo._data.Add(data);
				}
			});
			NavigationController.instance.activeContext = null;
		}
		finally
		{
			finishedEvent.TrySetResult(embarkInfo);
			this.Destroy();
		}
	}

	public void exitWithoutInfo()
	{
		try
		{
			ShowWindow(null);
			foreach (AbstractBuilderModuleWindowBase createdWindow in createdWindows)
			{
				createdWindow.DestroyWindow();
			}
			createdWindows.Clear();
			Debug.Log("exiting without info");
			embarkInfo = null;
			NavigationController.instance.activeContext = null;
		}
		finally
		{
			finishedEvent.TrySetResult(null);
			this.Destroy();
		}
	}

	public void Update()
	{
	}

	public T GetModule<T>() where T : AbstractEmbarkBuilderModule
	{
		return modules.Find((AbstractEmbarkBuilderModule m) => m.GetType() == typeof(T)) as T;
	}

	public EmbarkBuilderOverlayWindow GetOverlayWindow()
	{
		return UIManager.getWindow<EmbarkBuilderOverlayWindow>("Chargen/Overlay");
	}

	public AbstractBuilderModuleWindowBase createWindow(string viewID, Type windowType)
	{
		Transform parent = GameObject.Find("UI Manager/Chargen").transform;
		AbstractBuilderModuleWindowBase abstractBuilderModuleWindowBase = UIManager.createWindow(viewID, windowType, parent) as AbstractBuilderModuleWindowBase;
		createdWindows.Add(abstractBuilderModuleWindowBase);
		return abstractBuilderModuleWindowBase;
	}

	public IEnumerable<UIBreadcrumb> GetBreadcrumbs()
	{
		foreach (EmbarkBuilderModuleWindowDescriptor windowDescriptor in windowDescriptors)
		{
			if (windowDescriptor.enabled && windowDescriptor.module.shouldBeEditable())
			{
				UIBreadcrumb breadcrumb = windowDescriptor.window.GetBreadcrumb();
				if (breadcrumb != null)
				{
					yield return breadcrumb;
				}
			}
		}
	}

	public async void ShowFromCrumb(FrameworkDataElement choice)
	{
		bool isBeforeActive = true;
		foreach (EmbarkBuilderModuleWindowDescriptor desc in windowDescriptors)
		{
			if (desc == activeWindow)
			{
				isBeforeActive = false;
			}
			if (desc.window.GetBreadcrumb()?.Id == choice.Id)
			{
				if (desc == activeWindow)
				{
					return;
				}
				if (isBeforeActive)
				{
					ShowWindow(desc);
				}
				else if (await checkStateAsync())
				{
					advanceToViewId(desc.viewID);
				}
			}
		}
	}

	public void fireBootEvent(string id)
	{
		modules.ForEach((AbstractEmbarkBuilderModule module) =>
		{
			if (module.enabled)
			{
				module.handleBootEvent(id, null, info);
			}
		});
	}

	public T fireBootEvent<T>(string id, T element)
	{
		modules?.ForEach((AbstractEmbarkBuilderModule module) =>
		{
			if (module != null && module.enabled)
			{
				element = (T)(module?.handleBootEvent(id, null, info, element));
			}
		});
		return element;
	}

	public bool IsEditableGameMode()
	{
		try
		{
			return (bool)handleUIEvent(EventNames.EditableGameModeQuery, true);
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Error while processing EditableGameMode", x);
			return true;
		}
	}

	public void RegisterModuleEnabledCheck(Predicate<AbstractEmbarkBuilderModule> filter)
	{
		enabledChecks.Add(filter);
	}

	public bool ModuleEnabledCheck(AbstractEmbarkBuilderModule module)
	{
		foreach (Predicate<AbstractEmbarkBuilderModule> enabledCheck in enabledChecks)
		{
			if (!enabledCheck(module))
			{
				return false;
			}
		}
		return true;
	}

	private void InitModules(bool silent = false)
	{
		enabledChecks = new List<Predicate<AbstractEmbarkBuilderModule>>
		{
			(AbstractEmbarkBuilderModule module) => module.shouldBeEnabled()
		};
		EmbarkBuilderConfiguration.Init();
		modules = new List<AbstractEmbarkBuilderModule>();
		windowDescriptors = new List<EmbarkBuilderModuleWindowDescriptor>();
		windows = new List<AbstractBuilderModuleWindowBase>();
		modules.AddRange(EmbarkBuilderConfiguration.activeModules);
		foreach (AbstractEmbarkBuilderModule module in modules)
		{
			module.builder = this;
			if (ModuleEnabledCheck(module))
			{
				module.enable();
			}
			else
			{
				module.disable();
			}
		}
		foreach (AbstractEmbarkBuilderModule module2 in modules)
		{
			module2.Init();
		}
		if (silent)
		{
			return;
		}
		foreach (AbstractEmbarkBuilderModule module3 in modules)
		{
			module3.assembleWindowDescriptors(windowDescriptors);
		}
		windowDescriptors.ForEach((EmbarkBuilderModuleWindowDescriptor descriptor) =>
		{
			AbstractBuilderModuleWindowBase window = descriptor.getWindow();
			descriptor.windowInit(window);
		});
		ShowWindow(windowDescriptors.First());
	}

	public void NotifyModuleChanges(AbstractEmbarkBuilderModule module, AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
		foreach (AbstractEmbarkBuilderModule module2 in modules)
		{
			if (module2 != module)
			{
				module2.handleModuleDataChange(module, oldValues, newValues);
			}
			bool flag = ModuleEnabledCheck(module2);
			if (flag != module2.enabled)
			{
				if (flag)
				{
					module2.enable();
				}
				else
				{
					module2.disable();
				}
			}
		}
		if (activeWindow?.getWindow()?.UseOverlay() == true && !SkippingUIUpdates)
		{
			GetOverlayWindow().UpdateBreadcrumbs();
		}
	}

	public static EmbarkBuilder FromCode(string code, bool silent = false)
	{
		EmbarkBuilder embarkBuilder = new EmbarkBuilder();
		embarkBuilder.InitModulesFromCode(code, silent);
		return embarkBuilder;
	}

	public void InitModulesFromCode(string code, bool silent = false)
	{
		if (modules == null)
		{
			InitModules(silent);
		}
		CodeCompressor.loadCode(code, modules, silent);
	}

	public void ResetForwardViews()
	{
		foreach (EmbarkBuilderModuleWindowDescriptor item in windowDescriptors.SkipWhile((EmbarkBuilderModuleWindowDescriptor desc) => desc != activeWindow).Skip(1))
		{
			item.getWindow().ResetSelection();
		}
	}
}
