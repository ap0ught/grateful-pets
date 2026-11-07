using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteInEditMode]
public class HelpRow : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	public Image background;

	public UITextSkin categoryDescription;

	public UITextSkin categoryExpander;

	public UITextSkin description;

	public NavigationContext categoryContext;

	public FrameworkContext frameworkContext;

	public bool selectedMode;

	private ScrollViewCalcs _calcs = new ScrollViewCalcs();

	private static List<string> keysByLength;

	private bool? wasSelected;

	public void SetupContexts(ScrollChildContext scontext)
	{
		if (scontext != null)
		{
			scontext.proxyTo = categoryContext ?? (categoryContext = new NavigationContext());
			categoryContext.parentContext = scontext;
			NavigationContext navigationContext = categoryContext;
			if (navigationContext.axisHandlers == null)
			{
				navigationContext.axisHandlers = new Dictionary<InputAxisTypes, Action>
				{
					{
						InputAxisTypes.NavigationPageYAxis,
						HandleUpDown
					},
					{
						InputAxisTypes.NavigationYAxis,
						HandleUpDown
					}
				};
			}
		}
	}

	public void HandleUpDown()
	{
		XRL.UI.Framework.Event currentEvent = NavigationController.currentEvent;
		if (currentEvent.axisValue > 0)
		{
			if (ScrollViewCalcs.GetScrollViewCalcs(transform as RectTransform, _calcs).isAnyBelowView)
			{
				_calcs.ScrollPageDown();
				currentEvent.Handle();
			}
		}
		else if (currentEvent.axisValue < 0 && ScrollViewCalcs.GetScrollViewCalcs(transform as RectTransform, _calcs).isAnyAboveView)
		{
			_calcs.ScrollPageUp();
			currentEvent.Handle();
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return frameworkContext.context;
	}

	public void setData(FrameworkDataElement data)
	{
		if (!(data is HelpDataRow helpDataRow))
		{
			return;
		}
		categoryDescription.text = "{{C|" + helpDataRow.Description.ToUpper() + "}}";
		categoryDescription.Apply();
		description.text = helpDataRow.HelpText;
		if (keysByLength == null)
		{
			keysByLength = CommandBindingManager.CommandBindings.Keys.ToList();
			keysByLength.Sort((string a, string b) => b.Length - a.Length);
		}
		if (description.text.Contains("~"))
		{
			if (description.text.Contains("~Highlight") && ControlManager.activeControllerType != ControlManager.InputDeviceType.Gamepad)
			{
				description.text = description.text.Replace("~Highlight", "{{W|Alt}}");
			}
			for (int num = 0; num < keysByLength.Count; num++)
			{
				string text = keysByLength[num];
				if (description.text.Contains("~Highlight") || description.text.Contains("~" + text))
				{
					string text2 = ControlManager.getCommandInputFormatted(text);
					if (string.IsNullOrEmpty(text2))
					{
						text2 = ((!CommandBindingManager.CommandsByID.TryGetValue(text, out var value)) ? ("{{hotkey|" + text + " [Unbound]}}") : ("{{hotkey|" + value.DisplayText + " [Unbound]}}"));
					}
					description.text = description.text.Replace("~" + text, text2);
					if (!description.text.Contains("~"))
					{
						break;
					}
				}
			}
		}
		description.Apply();
		description.gameObject.SetActive(!helpDataRow.Collapsed);
		if (helpDataRow.Collapsed)
		{
			categoryExpander.SetText("{{C|[+]}}");
		}
		else
		{
			categoryExpander.SetText("{{C|[-]}}");
		}
	}

	public void Update()
	{
		bool? flag = GetNavigationContext()?.IsActive();
		if (wasSelected != flag)
		{
			wasSelected = flag;
			bool flag2 = (background.enabled = flag == true);
			selectedMode = flag2;
		}
	}
}
