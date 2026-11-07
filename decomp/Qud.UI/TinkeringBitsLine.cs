using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World.Tinkering;

namespace Qud.UI;

public class TinkeringBitsLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	public class Context : NavigationContext
	{
		public TinkeringBitsLineData data;
	}

	private Context context = new Context();

	public Image usedIndicator;

	public UITextSkin text;

	public UITextSkin amountText;

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
		if (!(data is TinkeringBitsLineData tinkeringBitsLineData))
		{
			return;
		}
		context.data = tinkeringBitsLineData;
		text.SetText("{{" + tinkeringBitsLineData.bit + "}}");
		BitType bitType = BitType.BitMap[tinkeringBitsLineData.cBit];
		if (tinkeringBitsLineData.activeCost == null || !tinkeringBitsLineData.activeCost.ContainsKey(tinkeringBitsLineData.cBit))
		{
			amountText.SetText($"{{{{K|{tinkeringBitsLineData.amount}}}}}");
			usedIndicator.color = new Color(0f, 0f, 0f, 0f);
			return;
		}
		usedIndicator.color = ConsoleLib.Console.ColorUtility.ColorMap[bitType.Color];
		int num = tinkeringBitsLineData.activeCost[tinkeringBitsLineData.cBit];
		if (num == 0)
		{
			amountText.SetText($"{{{{K|{tinkeringBitsLineData.amount}}}}}");
		}
		else if (tinkeringBitsLineData.amount >= num)
		{
			amountText.SetText($"{{{{G|{tinkeringBitsLineData.amount}}}}}");
		}
		else
		{
			amountText.SetText($"{{{{R|{tinkeringBitsLineData.amount}}}}}");
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
