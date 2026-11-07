using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class PickGameObjectLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	public class Context : NavigationContext
	{
		public PickGameObjectLineData data;
	}

	private Context context = new Context();

	public UITextSkin text;

	public UITextSkin check;

	public UITextSkin hotkey;

	public UITextSkin rightFloatText;

	public UIThreeColorProperties icon;

	public UnityEngine.GameObject iconSpacer;

	public PickGameObjectScreen screen;

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

	public static List<MenuOption> itemOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Select"
		}
	};

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

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
			context.axisHandlers = new Dictionary<InputAxisTypes, Action>
			{
				{
					InputAxisTypes.NavigationVAxis,
					HandleNavigationVAxis
				},
				{
					InputAxisTypes.NavigationXAxis,
					HandleNavigationXAxis
				}
			};
		}
		if (this.context.data.type == PickGameObjectLineDataType.Category)
		{
			this.context.menuOptionDescriptions = (this.context.data.collapsed ? categoryExpandOptions : categoryCollapseOptions);
		}
		if (this.context.data.type == PickGameObjectLineDataType.Item)
		{
			this.context.menuOptionDescriptions = itemOptions;
		}
	}

	public void HandleNavigationVAxis()
	{
		screen.HandleVAxis(NavigationController.currentEvent.axisValue);
		NavigationController.currentEvent.Handle();
	}

	public void HandleNavigationXAxis()
	{
		screen.HandleXAxis(NavigationController.currentEvent.axisValue);
		NavigationController.currentEvent.Handle();
	}

	public void setData(FrameworkDataElement data)
	{
		if (!(data is PickGameObjectLineData pickGameObjectLineData))
		{
			return;
		}
		if (pickGameObjectLineData.style == PickGameObjectLineDataStyle.Interact)
		{
			check.SetText("");
		}
		context.data = pickGameObjectLineData;
		if (pickGameObjectLineData.go == null)
		{
			string text = ((!pickGameObjectLineData.collapsed) ? "-" : "+");
			this.text.SetText("[" + text + "] {{K|" + pickGameObjectLineData.category + "}}");
			icon.gameObject.SetActive(value: false);
			iconSpacer.SetActive(value: false);
			rightFloatText.SetText("");
			ControlId.Assign(gameObject, "PickGameObject:Category:" + (pickGameObjectLineData?.category ?? "(null)"));
		}
		else
		{
			icon.gameObject.SetActive(value: true);
			iconSpacer.SetActive(value: true);
			icon.FromRenderable(pickGameObjectLineData.go.RenderForUI());
			SB.Clear();
			XRL.World.GameObject go = pickGameObjectLineData.go;
			SB.Append(go.DisplayName);
			if (PickGameObjectScreen.NotePlayerOwned && go.OwnedByPlayer)
			{
				SB.Append(" {{G|[owned by you]}}");
			}
			if (PickGameObjectScreen.ShowContext)
			{
				SB.Append(" [" + go.GetListDisplayContext(The.Player) + "]");
			}
			this.text.SetText(SB.ToString());
			rightFloatText.SetText($"{{{{K|{pickGameObjectLineData.go.GetWeight()}#}}}}");
			ControlId.Assign(gameObject, "PickGameObject:Item:" + (pickGameObjectLineData?.go?.IDIfAssigned ?? "(null)"));
		}
		string text2 = (pickGameObjectLineData.indent ? "   " : "");
		if (!string.IsNullOrEmpty(pickGameObjectLineData.hotkeyDescription))
		{
			hotkey.SetText(text2 + "{{Y|{{w|" + pickGameObjectLineData.hotkeyDescription + "}})}} ");
		}
		else
		{
			hotkey.SetText(text2 + " ");
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (SingletonWindowBase<AbilityManagerScreen>.instance.sortMode != AbilityManagerScreen.SortMode.Custom)
		{
			eventData.pointerDrag = null;
			return;
		}
		CursorManager.instance.currentStyle = CursorManager.Style.ResizeNorthSouth;
		spacers.Clear();
		spacers.AddRange(transform.parent.GetComponentsInChildren<AbilityManagerSpacer>());
		HighlightClosestSpacer(eventData.position);
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
		HighlightClosestSpacer(eventData.position);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		CursorManager.instance.currentStyle = CursorManager.Style.Pointer;
		int num = HighlightClosestSpacer(eventData.position);
		spacers[num].image.enabled = false;
		int num2 = transform.GetSiblingIndex() / 2;
		if (num != num2 && num != num2 + 1)
		{
			if (num < num2)
			{
				screen.MoveItem(context.data, num - num2);
			}
			else
			{
				screen.MoveItem(context.data, num - num2 - 1);
			}
		}
	}
}
