using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class TinkeringLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	public class Context : NavigationContext
	{
		public TinkeringLineData data;
	}

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

	public GameObject[] categoryObjects;

	public GameObject[] lineObjects;

	public UITextSkin categoryText;

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

	public bool selected => context?.IsActive() ?? false;

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
		this.context.axisHandlers[InputAxisTypes.NavigationXAxis] = XAxis;
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

	public void XAxis()
	{
		XRL.UI.Framework.Event currentEvent = NavigationController.currentEvent;
		if (context.data.category && context.data.screen != null)
		{
			context.data.screen.categoryCollapsed[context.data.categoryName] = !(currentEvent.axisValue > 0);
			context.data.screen.UpdateViewFromData();
		}
		else if (context.data.categoryName != null && context.data.screen != null)
		{
			if (currentEvent.axisValue < 0)
			{
				context.data.screen.controller.scrollContext.SelectIndex(context.data.screen.controller.scrollContext.selectedPosition - context.data.categoryOffset);
				context.data.screen.SetCategoryExpanded(context.data.categoryName, state: false);
			}
			else
			{
				context.data.screen.SetCategoryExpanded(context.data.categoryName, state: true);
			}
		}
	}

	public void setData(FrameworkDataElement data)
	{
		if (!(data is TinkeringLineData tinkeringLineData))
		{
			return;
		}
		GameObject[] array;
		if (tinkeringLineData.category)
		{
			array = lineObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			array = categoryObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			context.data = tinkeringLineData;
			if (tinkeringLineData.categoryName == "~<none>")
			{
				categoryText.SetText("{{K|You don't have any schematics.}}");
			}
			else
			{
				categoryText.SetText(string.Format("{0} {1} [{2}]", tinkeringLineData.categoryExpanded ? "[-]" : "[+]", tinkeringLineData.categoryName, tinkeringLineData.categoryCount));
			}
			return;
		}
		array = lineObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		array = categoryObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		context.data = tinkeringLineData;
		if (tinkeringLineData.mode == 0)
		{
			text.SetText("    " + tinkeringLineData.data.DisplayName + " [" + tinkeringLineData.costString + "]");
		}
		else if (tinkeringLineData.mode == 1)
		{
			if (tinkeringLineData.modObject == null)
			{
				text.SetText("    <no applicable items>");
				return;
			}
			text.SetText("    " + tinkeringLineData.modObject.DisplayName + " [" + tinkeringLineData.costString + "]");
		}
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
