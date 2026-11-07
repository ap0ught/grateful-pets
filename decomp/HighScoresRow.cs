using System;
using System.Collections.Generic;
using Qud.UI;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;

public class HighScoresRow : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	public List<UITextSkin> TextSkins;

	public Image background;

	public FrameworkContext deleteButton;

	public FrameworkContext revisitCodaButton;

	public GameObject revisitCodaButtonObject;

	private FrameworkContext _context;

	private bool hasCoda;

	private bool? wasSelected;

	public FrameworkContext context => _context ?? (_context = GetComponent<FrameworkContext>());

	public void SetupContexts(ScrollChildContext context)
	{
		this.context.context = context;
		if (revisitCodaButton.context?.parentContext != context)
		{
			FrameworkContext frameworkContext = revisitCodaButton;
			if (frameworkContext.context == null)
			{
				NavigationContext navigationContext = (frameworkContext.context = new NavigationContext());
			}
			revisitCodaButton.context.parentContext = context;
		}
		if (deleteButton.context?.parentContext != context)
		{
			FrameworkContext frameworkContext = deleteButton;
			if (frameworkContext.context == null)
			{
				NavigationContext navigationContext = (frameworkContext.context = new NavigationContext());
			}
			deleteButton.context.parentContext = context;
		}
	}

	public void HandleDelete()
	{
		deleteButton?.context.buttonHandlers[InputButtonTypes.AcceptButton]();
	}

	public void HandleRevisitCoda()
	{
		if (hasCoda)
		{
			revisitCodaButton?.context.buttonHandlers[InputButtonTypes.AcceptButton]();
		}
	}

	public void setData(FrameworkDataElement data)
	{
		if (!(data is HighScoresDataElement { entry: var entry } highScoresDataElement))
		{
			return;
		}
		string[] array = entry?.Details?.Split(new char[1] { '\n' }, 7, StringSplitOptions.RemoveEmptyEntries) ?? new string[1] { "" };
		hasCoda = highScoresDataElement.entry.HasCoda();
		TextSkins[0].SetText(string.Format("{{{{{0}|{1} :: Level {2} :: {3}}}}}", hasCoda ? "W" : "C", highScoresDataElement.entry.Name, highScoresDataElement.entry.Level, highScoresDataElement.entry.GameMode));
		int i = 1;
		int num = 0;
		for (; i < TextSkins.Count; i++)
		{
			TextSkins[i].SetText((array.Length <= i) ? "" : array[i + num]);
			if (i == 2)
			{
				num++;
			}
		}
		wasSelected = null;
		Update();
	}

	public void Update()
	{
		bool valueOrDefault = context?.context?.IsActive() == true;
		if (valueOrDefault == wasSelected)
		{
			return;
		}
		wasSelected = valueOrDefault;
		deleteButton.gameObject.SetActive(valueOrDefault);
		revisitCodaButton.gameObject.SetActive(valueOrDefault && hasCoda);
		Color darkCyan = The.Color.DarkCyan;
		darkCyan.a = (valueOrDefault ? 0.25f : 0f);
		background.color = darkCyan;
		bool flag = true;
		foreach (UITextSkin textSkin in TextSkins)
		{
			if (valueOrDefault)
			{
				textSkin.color = The.Color.Gray;
				textSkin.StripFormatting = false;
			}
			else
			{
				ref Color color = ref textSkin.color;
				Color color2;
				if (flag && hasCoda)
				{
					color2 = The.Color.Yellow;
				}
				else
				{
					color2 = (flag ? The.Color.DarkCyan : The.Color.Black);
				}
				color = color2;
				textSkin.StripFormatting = true;
			}
			textSkin.Apply();
			flag = false;
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return null;
	}
}
