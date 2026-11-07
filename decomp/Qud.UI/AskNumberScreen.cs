using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

[UIView("PopupAskNumberScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "AskNumber", UICanvasHost = 1)]
public class AskNumberScreen : SingletonWindowBase<AskNumberScreen>, ControlManager.IControllerChangedEvent
{
	public class Context : NavigationContext
	{
	}

	protected TaskCompletionSource<int?> menucomplete = new TaskCompletionSource<int?>();

	public Context navigationContext = new Context();

	public FrameworkScroller hotkeyBar;

	public UITextSkin numberText;

	public UITextSkin messageText;

	public int Current;

	public int Min;

	public int Max;

	public static List<MenuOption> getItemMenuOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Accept"
		},
		new MenuOption
		{
			Id = "Cancel",
			InputCommand = "Cancel",
			Description = "Cancel"
		}
	};

	public UITextSkin[] freeDramsLabels;

	public UITextSkin[] totalLabels;

	private NavigationContext lastContext;

	public void SetupContext()
	{
		Context context = navigationContext;
		if (context.buttonHandlers == null)
		{
			context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.CancelButton,
				XRL.UI.Framework.Event.Helpers.Handle(Cancel)
			} };
		}
		context = navigationContext;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>
			{
				{
					"Cancel",
					XRL.UI.Framework.Event.Helpers.Handle(Cancel)
				},
				{
					"Accept",
					XRL.UI.Framework.Event.Helpers.Handle(Accept)
				}
			};
		}
		context = navigationContext;
		if (context.axisHandlers == null)
		{
			context.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		navigationContext.axisHandlers.Set(InputAxisTypes.NavigationXAxis, XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(DoPageUp, DoPageDown)));
		navigationContext.axisHandlers.Set(InputAxisTypes.NavigationYAxis, XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(DoScrollDown, DoScrollUp)));
		navigationContext.axisHandlers.Set(InputAxisTypes.NavigationPageYAxis, XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(DoPageDown, DoPageUp)));
	}

	public void DoScrollUp()
	{
		Current++;
		UpdateViewFromData();
	}

	public void DoScrollDown()
	{
		Current--;
		UpdateViewFromData();
	}

	public void DoPageDown()
	{
		Current = Min;
		UpdateViewFromData();
	}

	public void DoPageUp()
	{
		Current = Max;
		UpdateViewFromData();
	}

	protected async Task<int?> showScreen(string Message, int Start = 0, int Min = 0, int Max = int.MaxValue)
	{
		_ = 1;
		try
		{
			await The.UiContext;
			Current = Start;
			messageText.SetText(Message);
			numberText.SetText(Start.ToString());
			this.Min = Min;
			this.Max = Max;
			menucomplete.TrySetCanceled();
			menucomplete = new TaskCompletionSource<int?>();
			MinEvent.UIHold = true;
			BeforeShow();
			Show();
			int? result = await menucomplete.Task;
			Cleanup();
			return result;
		}
		finally
		{
			MinEvent.UIHold = false;
		}
	}

	public static async Task<int?> show(string Message, int Start = 0, int Min = 0, int Max = int.MaxValue)
	{
		return await NavigationController.instance.SuspendContextWhile(() => SingletonWindowBase<AskNumberScreen>.instance.showScreen(Message, Start, Min, Max));
	}

	public void BeforeShow()
	{
		XRL.World.Event.PinCurrentPool();
		SetupContext();
		UpdateMenuBars();
		UpdateTitleBars();
		UpdateViewFromData();
		navigationContext.ActivateAndEnable();
		XRL.World.Event.ResetToPin();
	}

	private void UpdateViewFromData()
	{
		if (Current < Min)
		{
			Current = Min;
		}
		if (Current > Max)
		{
			Current = Max;
		}
		numberText.SetText(Current.ToString());
	}

	public void UpdateTitleBars()
	{
	}

	public IEnumerable<FrameworkDataElement> yieldMenuOptions()
	{
		foreach (MenuOption getItemMenuOption in getItemMenuOptions)
		{
			yield return getItemMenuOption;
		}
	}

	public void UpdateMenuBars()
	{
		hotkeyBar.BeforeShow(null, yieldMenuOptions());
		hotkeyBar.GetNavigationContext().disabled = false;
		hotkeyBar.onSelected.RemoveAllListeners();
		hotkeyBar.onSelected.AddListener(HandleMenuOption);
	}

	public void HandleMenuOption(FrameworkDataElement element)
	{
		if (element.Id == "Accept")
		{
			Accept();
		}
		if (element.Id == "Cancel")
		{
			Cancel();
		}
	}

	public void Cleanup()
	{
	}

	public override void Hide()
	{
		base.Hide();
	}

	public void Accept()
	{
		int value = Convert.ToInt32(numberText.text);
		ControlManager.ResetInput();
		Hide();
		Cleanup();
		menucomplete.TrySetResult(value);
	}

	public void Cancel()
	{
		ControlManager.ResetInput();
		Hide();
		Cleanup();
		menucomplete.TrySetResult(null);
	}

	void ControlManager.IControllerChangedEvent.ControllerChanged()
	{
	}

	public void HandleVAxis(int? val)
	{
	}

	public void HandleXAxis(int? val)
	{
	}

	public void HandleHighlightObject(FrameworkDataElement element)
	{
		_ = element is TradeLineData;
	}

	public void Update()
	{
		if (isCurrentWindow() && navigationContext.IsActive() && lastContext != NavigationController.instance.activeContext)
		{
			lastContext = NavigationController.instance.activeContext;
			UpdateMenuBars();
		}
	}

	public void MoveItem(TradeLineData data, int direction)
	{
	}

	public IEnumerator ShowNextCycle()
	{
		yield return 0;
		yield return 0;
		BeforeShow();
	}
}
