using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using ModelShark;
using TMPro;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[RequireComponent(typeof(FrameworkContext))]
public class AttributeSelectionControl : MonoBehaviour, IFrameworkControl
{
	public AttributeDataElement data;

	public TextMeshProUGUI attribute;

	public TextMeshProUGUI value;

	public TextMeshProUGUI modifier;

	public FrameworkContext addButton;

	public FrameworkContext subtractButton;

	public TooltipTrigger tooltip;

	public ScrollContext<int, NavigationContext> navContext = new ScrollContext<int, NavigationContext>();

	private bool? _wasActive;

	public bool isActive => _wasActive == true;

	public void raise()
	{
		data.raise();
		Updated();
	}

	public void lower()
	{
		data.lower();
		Updated();
	}

	public void setAttributeText(string value)
	{
		attribute.text = value;
	}

	public void setValueText(string value)
	{
		this.value.text = value;
	}

	public void setModifierText(string value)
	{
		modifier.text = value;
	}

	public void Updated()
	{
		int score = data.Value;
		attribute.text = data.Attribute.Substring(0, 3).ToUpper();
		value.text = score.ToString();
		TextMeshProUGUI textMeshProUGUI = value;
		Dictionary<char, Color> colorMap = ConsoleLib.Console.ColorUtility._ColorMap;
		char key;
		if (data.Bonus > 0)
		{
			key = 'G';
		}
		else
		{
			key = ((data.Bonus < 0) ? 'R' : 'c');
		}
		textMeshProUGUI.color = colorMap[key];
		tooltip.SetText("BodyText", Sidebar.FormatToRTF(data.BonusSource));
		_wasActive = false;
		modifier.text = "[" + Stat.GetScoreModifier(score) + "]";
		subtractButton.gameObject.SetActive(value: true);
		addButton.gameObject.SetActive(value: true);
		GetComponent<TitledIconButton>().SetTitle("[" + data.APToRaise + "pts]");
		data.Updated();
	}

	public void Update()
	{
		bool flag = navContext.IsActive();
		if (_wasActive != flag)
		{
			_wasActive = flag;
			if (!flag && tooltip.IsDisplayed())
			{
				Debug.Log("Hide");
				tooltip.HidePopup();
			}
		}
		if (flag && !string.IsNullOrEmpty(data?.BonusSource) && !tooltip.IsDisplayed())
		{
			Debug.Log("Show");
			tooltip.ShowManually(bForceDisplay: true);
		}
	}

	public void setData(FrameworkDataElement d)
	{
		GetComponent<FrameworkContext>().context = navContext;
		data = d as AttributeDataElement;
		data.control = this;
		Updated();
		ScrollChildContext scrollChildContext = addButton.RequireContext<ScrollChildContext>();
		scrollChildContext.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
		{
			InputButtonTypes.AcceptButton,
			XRL.UI.Framework.Event.Helpers.Handle(raise)
		} };
		ScrollChildContext scrollChildContext2 = subtractButton.RequireContext<ScrollChildContext>();
		scrollChildContext2.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
		{
			InputButtonTypes.AcceptButton,
			XRL.UI.Framework.Event.Helpers.Handle(lower)
		} };
		navContext.contexts = new List<NavigationContext> { scrollChildContext, scrollChildContext2 };
		navContext.wraps = false;
		navContext.data = new List<int> { 1, -1 };
		navContext.SetAxis(InputAxisTypes.NavigationYAxis);
		navContext.axisHandlers.Add(InputAxisTypes.NavigationPageYAxis, XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(negative: raise, positive: lower)));
		navContext.axisHandlers.Add(InputAxisTypes.NavigationVAxis, XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(raise, lower)));
	}

	public NavigationContext GetNavigationContext()
	{
		return navContext;
	}
}
