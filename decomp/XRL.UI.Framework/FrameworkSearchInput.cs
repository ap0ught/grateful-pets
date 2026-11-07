using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace XRL.UI.Framework;

public class FrameworkSearchInput : FrameworkContext
{
	public class SearchContext : ScrollChildContext
	{
		public string inputText;
	}

	public Image SearchIcon;

	public Image InputBorder;

	public UITextSkin InputText;

	public UnityEvent<string> OnSearchTextChange = new UnityEvent<string>();

	private bool? isActive;

	private string lastText;

	public string PopupTitle = "Enter search text";

	public new SearchContext context
	{
		get
		{
			return base.context as SearchContext;
		}
		set
		{
			base.context = value;
		}
	}

	public string SearchText => context.inputText;

	public void Awake()
	{
		context = new SearchContext
		{
			buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.AcceptButton,
				Event.Helpers.Handle(ChangeValue)
			} }
		};
	}

	public void EnterAndOpen()
	{
		context.Activate();
		NavigationController.instance.FireInputButtonEvent(InputButtonTypes.AcceptButton);
	}

	public void Update()
	{
		bool flag = context.IsActive();
		if (flag != isActive || context.inputText != lastText)
		{
			isActive = flag;
			lastText = context.inputText;
			Color color = (flag ? ConsoleLib.Console.ColorUtility.colorFromChar('W') : ConsoleLib.Console.ColorUtility.FromWebColor("4A757E"));
			Image inputBorder = InputBorder;
			Color color2 = (SearchIcon.color = color);
			inputBorder.color = color2;
			if (string.IsNullOrWhiteSpace(lastText))
			{
				InputText.SetText("{{K|<search>}}");
			}
			else
			{
				InputText.SetText(lastText);
			}
		}
	}

	public async void ChangeValue()
	{
		string text = await Popup.AskStringAsync(PopupTitle, context.inputText ?? "", 80, 0, null, ReturnNullForEscape: true, EscapeNonMarkupFormatting: true, false);
		context.inputText = text ?? context.inputText ?? "";
		OnSearchTextChange.Invoke(context.inputText);
	}
}
