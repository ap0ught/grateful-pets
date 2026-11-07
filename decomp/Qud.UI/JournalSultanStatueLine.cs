using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class JournalSultanStatueLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	public class Context : NavigationContext
	{
		public JournalLineData data;
	}

	public Image fadedImage;

	public Image fullImage;

	public Image highlightedBorder;

	public Image fadedBase;

	public Image fullBase;

	public LayoutGroup layoutGroup;

	private Context context = new Context();

	public UITextSkin text;

	public StringBuilder SB = new StringBuilder();

	public static List<MenuOption> categoryExpandOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Expand"
		}
	};

	public static List<MenuOption> categoryCollapseOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Collapse"
		}
	};

	public Image background;

	public bool wasSelected;

	public bool wasHighlighted;

	public bool isHighlighted;

	public bool hasBase;

	private static StringBuilder sb = new StringBuilder();

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

	public bool selected => context?.IsActive() ?? false;

	public void SetHighlight(bool highlightState)
	{
		fadedImage.enabled = !highlightState;
		highlightedBorder.enabled = highlightState;
		if (hasBase)
		{
			fadedBase.enabled = !highlightState;
		}
		ColorUtility.TryParseHtmlString(highlightState ? "#EFD03E" : "#235157", out text.color);
		text.Apply();
	}

	public void SetBase(bool State)
	{
		hasBase = State;
		fullBase.enabled = State;
		fadedBase.enabled = fadedImage.enabled & State;
		if (State)
		{
			fadedImage.rectTransform.localPosition = new Vector3(0f, 6f, 0f);
			fullImage.rectTransform.localPosition = new Vector3(0f, 6f, 0f);
		}
		else
		{
			fadedImage.rectTransform.localPosition = new Vector3(0f, 0f, 0f);
			fullImage.rectTransform.localPosition = new Vector3(0f, 0f, 0f);
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void SetupContexts(ScrollChildContext scrollContext)
	{
		scrollContext.proxyTo = this.context;
		Context context = this.context;
		if (context.axisHandlers == null)
		{
			context.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		context = this.context;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>();
		}
	}

	public void LateUpdate()
	{
		wasSelected = selected;
		wasHighlighted = isHighlighted;
	}

	public void setData(FrameworkDataElement data)
	{
		if (!(data is JournalLineData journalLineData))
		{
			return;
		}
		context.data = journalLineData;
		sb.Clear();
		if (journalLineData.category)
		{
			layoutGroup.padding.left = 16;
			if (journalLineData.categoryExpanded)
			{
				sb.Append("[-] ");
			}
			else
			{
				sb.Append("[+] ");
			}
			sb.Append(journalLineData.categoryName);
		}
		else
		{
			layoutGroup.padding.left = 48;
			if (journalLineData.entry.Tradable)
			{
				sb.Append("{{G|$}} ");
			}
			else
			{
				sb.Append("{{K|$}} ");
			}
			sb.Append(journalLineData.entry.GetDisplayText());
		}
		text.SetText(sb.ToString());
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
	}

	public int HighlightClosestSpacer(Vector2 screenPosition)
	{
		AbilityManagerSpacer abilityManagerSpacer = ((spacers.Count > 0) ? spacers[0] : null);
		float num = float.MaxValue;
		int result = -1;
		Vector3[] array = new Vector3[4];
		int num2 = 0;
		foreach (AbilityManagerSpacer spacer in spacers)
		{
			(spacer.transform as RectTransform).GetWorldCorners(array);
			float magnitude = (new Vector2((array[0].x + array[2].x) / 2f, (array[0].y + array[2].y) / 2f) - screenPosition).magnitude;
			if (magnitude < num)
			{
				abilityManagerSpacer = spacer;
				num = magnitude;
				result = num2;
			}
			spacer.image.enabled = false;
			num2++;
		}
		if (abilityManagerSpacer != null)
		{
			abilityManagerSpacer.image.enabled = true;
		}
		return result;
	}

	public void OnDrag(PointerEventData eventData)
	{
	}

	public void OnEndDrag(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		context.IsActive();
	}
}
