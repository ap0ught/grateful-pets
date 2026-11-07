using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World.Parts;

namespace Qud.UI;

public class AbilityManagerLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	public class Context : NavigationContext
	{
		public AbilityManagerLineData data;
	}

	private Context context = new Context();

	public UITextSkin text;

	public UIThreeColorProperties icon;

	public AbilityManagerScreen screen;

	public StringBuilder SB = new StringBuilder();

	public static MenuOption MOVE_DOWN = new MenuOption
	{
		InputCommand = "V Positive",
		Description = "Move Down",
		disabled = true
	};

	public static MenuOption MOVE_UP = new MenuOption
	{
		InputCommand = "V Negative",
		Description = "Move Up",
		disabled = true
	};

	public static MenuOption BIND_KEY = new MenuOption
	{
		InputCommand = "CmdInsert",
		Description = "Bind Key",
		disabled = true
	};

	public static MenuOption UNBIND_KEY = new MenuOption
	{
		InputCommand = "CmdDelete",
		Description = "Unbind Key",
		disabled = true
	};

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

	public static bool dragging = false;

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
			context.axisHandlers = new Dictionary<InputAxisTypes, Action> { 
			{
				InputAxisTypes.NavigationVAxis,
				HandleNavigationVAxis
			} };
		}
		context = this.context;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>
			{
				{ BIND_KEY.InputCommand, HandleRebind },
				{ UNBIND_KEY.InputCommand, HandleRemoveBind }
			};
		}
		if (SingletonWindowBase<AbilityManagerScreen>.instance.sortMode == AbilityManagerScreen.SortMode.Custom)
		{
			this.context.menuOptionDescriptions = new List<MenuOption> { MOVE_UP, MOVE_DOWN, BIND_KEY };
		}
		else
		{
			this.context.menuOptionDescriptions = new List<MenuOption> { BIND_KEY };
		}
		if (!string.IsNullOrEmpty(this.context.data.hotkeyDescription))
		{
			if (this.context.menuOptionDescriptions.Count == 3)
			{
				this.context.menuOptionDescriptions.Add(UNBIND_KEY);
			}
		}
		else if (this.context.menuOptionDescriptions.Count == 4)
		{
			this.context.menuOptionDescriptions.RemoveAt(3);
		}
	}

	public void HandleNavigationVAxis()
	{
		if (context.data.category != null)
		{
			screen.HandleSelectLeft(context.data);
		}
		else if (screen.sortMode == AbilityManagerScreen.SortMode.Custom)
		{
			screen.MoveItem(context.data, NavigationController.currentEvent.axisValue.GetValueOrDefault());
		}
		NavigationController.currentEvent.Handle();
	}

	public async void HandleRebind()
	{
		if (dragging)
		{
			return;
		}
		NavigationController.currentEvent.Handle();
		if (context.data.ability != null)
		{
			NavigationController.currentEvent.Handle();
			await NavigationController.instance.SuspendContextWhile(async () =>
			{
				await AbilityManagerScreen.HandleRebindAsync(context.data.ability);
				return true;
			});
			SingletonWindowBase<AbilityManagerScreen>.instance.Refresh();
			GameManager.Instance.SetActiveLayersForNavCategory("Menu");
		}
	}

	public async void HandleRemoveBind()
	{
		if (dragging)
		{
			return;
		}
		NavigationController.currentEvent.Handle();
		if (context.data.hotkeyDescription != null)
		{
			if (await AbilityManagerScreen.HandleRemoveBindAsync(context.data.ability))
			{
				screen.Refresh();
			}
			GameManager.Instance.SetActiveLayersForNavCategory("Menu");
		}
	}

	public void setData(FrameworkDataElement data)
	{
		if (!(data is AbilityManagerLineData abilityManagerLineData))
		{
			return;
		}
		context.data = abilityManagerLineData;
		if (abilityManagerLineData.category != null)
		{
			string text = (abilityManagerLineData.collapsed ? "-" : "+");
			this.text.SetText("[" + text + "] " + abilityManagerLineData.category);
			icon.gameObject.SetActive(value: false);
			return;
		}
		icon.gameObject.SetActive(value: true);
		icon.FromRenderable(abilityManagerLineData.ability.GetUITile());
		SB.Clear();
		ActivatedAbilityEntry ability = abilityManagerLineData.ability;
		if (!ability.Enabled)
		{
			SB.Append("{{K|" + abilityManagerLineData.quickKey + ") " + ability.DisplayName + (ability.IsAttack ? " [attack]" : "") + " [disabled]}}");
		}
		else if (ability.Cooldown <= 0)
		{
			if (ability.IsRealityDistortionBased && !abilityManagerLineData.realityIsWeak)
			{
				SB.Append("{{K|" + abilityManagerLineData.quickKey + ") " + ability.DisplayName + (ability.IsAttack ? " [attack]" : "") + " [astrally tethered]}}");
			}
			else
			{
				SB.Append(abilityManagerLineData.quickKey + ") " + ability.DisplayName + (ability.IsAttack ? " [{{W|attack}}]" : ""));
			}
		}
		else if (ability.IsRealityDistortionBased && !abilityManagerLineData.realityIsWeak)
		{
			SB.Append("{{K|" + abilityManagerLineData.quickKey + "}}) " + ability.DisplayName + " [{{C|" + ability.CooldownRounds + "}} turn cooldown, astrally tethered]");
		}
		else
		{
			SB.Append("{{K|" + abilityManagerLineData.quickKey + "}}) " + ability.DisplayName + " [{{C|" + ability.CooldownRounds + "}} turn cooldown]");
		}
		if (ability.Toggleable)
		{
			if (ability.ToggleState)
			{
				SB.Append(" {{K|[{{g|Toggled on}}]}}");
			}
			else
			{
				SB.Append(" {{K|[{{y|Toggled off}}]}}");
			}
		}
		if (!string.IsNullOrEmpty(abilityManagerLineData.hotkeyDescription))
		{
			SB.Append(" {{Y|<{{w|" + abilityManagerLineData.hotkeyDescription + "}}>}}");
		}
		this.text.SetText(SB.ToString());
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		dragging = true;
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
		CursorManager.setStyle(CursorManager.Style.Pointer);
		if (!dragging)
		{
			return;
		}
		dragging = false;
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
