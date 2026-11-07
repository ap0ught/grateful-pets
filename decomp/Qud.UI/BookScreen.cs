using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleLib.Console;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;

namespace Qud.UI;

[UIView("Book", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "Book", UICanvasHost = 1)]
public class BookScreen : SingletonWindowBase<BookScreen>, ControlManager.IControllerChangedEvent
{
	public class Context : NavigationContext
	{
	}

	public enum SortMode
	{
		AZ,
		Category
	}

	public const string DEFAULT_OPEN_SOUND = "Sounds/Interact/sfx_interact_book_read";

	protected TaskCompletionSource<bool> menucomplete = new TaskCompletionSource<bool>();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> horizNav = new ScrollContext<NavigationContext>();

	public FrameworkScroller[] pageControllers;

	public Context navigationContext = new Context
	{
		buttonHandlers = null
	};

	public UITextSkin titleText;

	public FrameworkScroller hotkeyBar;

	public UITextSkin hotkeyText;

	public UITextSkin leftPageNumber;

	public UITextSkin rightPageNumber;

	private string searchText = "";

	public MarkovBook Book;

	public string BookID;

	public Action<int> onShowPage;

	public Action<int> afterShowPage;

	public UIThreeColorProperties[] traderIcons;

	public UITextSkin[] traderNames;

	private TradeEntry searcher = new TradeEntry("");

	private List<BookLineData> listItems = new List<BookLineData>();

	private int CurrentPage;

	public SortMode sortMode = SortMode.Category;

	public static readonly MenuOption PREV_PAGE = new MenuOption
	{
		Id = "PREV_PAGE",
		KeyDescription = "Previous Page",
		InputCommand = "UI:Navigate/left",
		Description = "Previous Page"
	};

	public static readonly MenuOption NEXT_PAGE = new MenuOption
	{
		Id = "NEXT_PAGE",
		KeyDescription = "Next Page",
		InputCommand = "UI:Navigate/right",
		Description = "Next Page"
	};

	public static List<MenuOption> getItemMenuOptions = new List<MenuOption>
	{
		PREV_PAGE,
		NEXT_PAGE,
		new MenuOption
		{
			Id = "Cancel",
			InputCommand = "Cancel",
			Description = "Close book"
		}
	};

	public UITextSkin[] freeDramsLabels;

	public UITextSkin[] totalLabels;

	private NavigationContext lastContext;

	public static string sortModeDescription => "sort: " + Markup.Color((SingletonWindowBase<BookScreen>.instance.sortMode == SortMode.AZ) ? "w" : "y", "a-z") + "/" + Markup.Color((SingletonWindowBase<BookScreen>.instance.sortMode == SortMode.Category) ? "w" : "y", "by class");

	private int PageCount
	{
		get
		{
			if (Book != null)
			{
				return Book.Pages.Count;
			}
			return BookUI.Books[BookID].Pages.Count;
		}
	}

	public void OnSearchTextChange(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			searchText = "";
		}
		else
		{
			searchText = text;
		}
		BeforeShow();
	}

	public void SetupContext()
	{
		vertNav.parentContext = navigationContext;
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Clear();
		vertNav.contexts.Add(horizNav);
		horizNav.parentContext = vertNav;
		horizNav.contexts.Clear();
		horizNav.contexts.Add(pageControllers[0].scrollContext);
		horizNav.contexts.Add(pageControllers[1].scrollContext);
		horizNav.SetAxis(InputAxisTypes.NavigationXAxis);
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
					PREV_PAGE.InputCommand,
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						HandleMenuOption(PREV_PAGE);
					})
				},
				{
					NEXT_PAGE.InputCommand,
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						HandleMenuOption(NEXT_PAGE);
					})
				}
			};
		}
		vertNav.Setup();
		horizNav.wraps = true;
		horizNav.Setup();
		vertNav.wraps = true;
		vertNav.Setup();
		pageControllers[0].scrollContext.parentContext = horizNav;
		pageControllers[1].scrollContext.parentContext = horizNav;
	}

	protected async Task<bool> showScreen(MarkovBook Book, string Sound = "Sounds/Interact/sfx_interact_book_read", Action<int> onShowPage = null, Action<int> afterShowPage = null)
	{
		BookID = null;
		this.Book = Book;
		this.onShowPage = onShowPage;
		this.afterShowPage = afterShowPage;
		searchText = "";
		menucomplete.TrySetCanceled();
		menucomplete = new TaskCompletionSource<bool>();
		MinEvent.UIHold = true;
		GameManager.Instance.PushGameView("Book");
		SoundManager.PlaySound(Sound);
		try
		{
			await The.UiContext;
			BeforeShow();
			Show();
			titleText.SetText(Book.Title);
			await CheckPageCallbacks();
			pageControllers[0].scrollContext.ActivateAndEnable();
			bool result = await menucomplete.Task;
			Cleanup();
			return result;
		}
		finally
		{
			GameManager.Instance.PopGameView(bHard: true);
			MinEvent.UIHold = false;
		}
	}

	protected async Task<bool> showScreen(string BookID, string Sound = "Sounds/Interact/sfx_interact_book_read", Action<int> onShowPage = null, Action<int> afterShowPage = null)
	{
		this.BookID = BookID;
		Book = null;
		this.onShowPage = onShowPage;
		this.afterShowPage = afterShowPage;
		searchText = "";
		menucomplete.TrySetCanceled();
		menucomplete = new TaskCompletionSource<bool>();
		MinEvent.UIHold = true;
		GameManager.Instance.PushGameView("Book");
		SoundManager.PlaySound(Sound);
		try
		{
			await The.UiContext;
			BeforeShow();
			Show();
			titleText.SetText(BookUI.Books[BookID].Title);
			await CheckPageCallbacks();
			pageControllers[0].scrollContext.ActivateAndEnable();
			bool result = await menucomplete.Task;
			Cleanup();
			return result;
		}
		finally
		{
			GameManager.Instance.PopGameView(bHard: true);
			MinEvent.UIHold = false;
		}
	}

	public static Task<bool> show(string BookID, string Sound = "Sounds/Interact/sfx_interact_book_read", Action<int> OnShowPage = null, Action<int> AfterShowPage = null)
	{
		return NavigationController.instance.SuspendContextWhile(() => SingletonWindowBase<BookScreen>.instance.showScreen(BookID, Sound, OnShowPage, AfterShowPage));
	}

	public static Task<bool> show(MarkovBook Book, string Sound = "Sounds/Interact/sfx_interact_book_read", Action<int> OnShowPage = null, Action<int> AfterShowPage = null)
	{
		return NavigationController.instance.SuspendContextWhile(() => SingletonWindowBase<BookScreen>.instance.showScreen(Book, Sound, OnShowPage, AfterShowPage));
	}

	public void BeforeShow()
	{
		XRL.World.Event.PinCurrentPool();
		SetupContext();
		ControlManager.GetHotkeySpread(new List<string> { "Menus", "UINav", "Trade", "UI" });
		listItems.ForEach((BookLineData i) =>
		{
			i.free();
		});
		listItems.Clear();
		pageControllers[0].onSelected.RemoveAllListeners();
		pageControllers[0].onHighlight.RemoveAllListeners();
		pageControllers[0].scrollContext.wraps = false;
		UpdateMenuBars();
		CurrentPage = 0;
		UpdateViewFromData();
		XRL.World.Event.ResetToPin();
	}

	public string RenderCurrentPage()
	{
		if (Book == null)
		{
			if (BookID != null)
			{
				if (!BookUI.Books.ContainsKey(BookID))
				{
					BookUI.RenderDynamicBook(BookID);
				}
				return BookUI.Books[BookID].Pages[CurrentPage].RenderForModernUI;
			}
			return "<BookID is null>";
		}
		return Book.Pages[CurrentPage].RenderForModernUI;
	}

	private void UpdateViewFromData()
	{
		pageControllers[0].BeforeShow(new List<BookLineData> { PooledFrameworkDataElement<BookLineData>.next().set(RenderCurrentPage()) });
		leftPageNumber.SetText((CurrentPage + 1).ToString());
		rightPageNumber.SetText(PageCount.ToString());
	}

	private async Task CheckPageCallbacks()
	{
		if (onShowPage != null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				onShowPage(CurrentPage);
			});
		}
		if (afterShowPage != null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				afterShowPage(CurrentPage);
			});
		}
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

	public async void HandleMenuOption(FrameworkDataElement element)
	{
		if (element.Id == "Cancel")
		{
			Cancel();
		}
		if (element == NEXT_PAGE)
		{
			if (CurrentPage < PageCount - 1)
			{
				CurrentPage++;
				UpdateViewFromData();
				SoundManager.PlayUISound("Sounds/Interact/sfx_interact_book_pageTurn");
				await CheckPageCallbacks();
			}
		}
		else if (element == PREV_PAGE && CurrentPage > 0)
		{
			CurrentPage--;
			UpdateViewFromData();
			SoundManager.PlayUISound("Sounds/Interact/sfx_interact_book_pageTurn");
			await CheckPageCallbacks();
		}
	}

	public void Cleanup()
	{
	}

	public override void Hide()
	{
		base.Hide();
	}

	public void Cancel()
	{
		ControlManager.ResetInput();
		Hide();
		Cleanup();
		menucomplete.TrySetResult(result: true);
	}

	void ControlManager.IControllerChangedEvent.ControllerChanged()
	{
	}

	public void Update()
	{
		if (isCurrentWindow() && navigationContext.IsActive() && lastContext != NavigationController.instance.activeContext)
		{
			lastContext = NavigationController.instance.activeContext;
			UpdateMenuBars();
		}
	}

	public IEnumerator ShowNextCycle()
	{
		yield return 0;
		yield return 0;
		BeforeShow();
	}
}
