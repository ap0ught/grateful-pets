using System;
using System.Collections.Generic;
using System.Text;
using Qud.API;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class JournalLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	public class Context : NavigationContext
	{
		public JournalLineData data;
	}

	public LayoutGroup layoutGroup;

	private Context context = new Context();

	public UITextSkin text;

	public GameObject imageContainer;

	public UIThreeColorProperties image;

	public GameObject headerContainer;

	public UITextSkin headerText;

	public GameObject textContainer;

	public JournalStatusScreen screen;

	public Image background;

	public bool wasSelected;

	public bool wasHighlighted;

	public bool isHighlighted;

	private static StringBuilder sb = new StringBuilder();

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

	public bool selected => context?.IsActive() ?? false;

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void XAxis()
	{
		XRL.UI.Framework.Event currentEvent = NavigationController.currentEvent;
		if (context.data.category)
		{
			context.data.screen.categoryCollapsed[context.data.categoryName] = !(currentEvent.axisValue > 0);
			context.data.screen.UpdateViewFromData();
		}
		else if (context.data.categoryName != null && currentEvent.axisValue < 0)
		{
			context.data.screen.controller.scrollContext.SelectIndex(context.data.screen.controller.scrollContext.selectedPosition - context.data.categoryOffset);
			context.data.screen.categoryCollapsed[context.data.categoryName] = true;
			context.data.screen.UpdateViewFromData();
		}
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
			context.commandHandlers = new Dictionary<string, Action> { { "CmdDelete", HandleDelete } };
		}
	}

	public void HandleDelete()
	{
		screen.HandleDelete(context.data);
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
		imageContainer.SetActive(journalLineData.renderable != null);
		if (journalLineData.renderable != null)
		{
			image.FromRenderable(journalLineData.renderable);
		}
		screen = journalLineData.screen;
		context.data = journalLineData;
		sb.Clear();
		if (journalLineData.category)
		{
			layoutGroup.padding.left = 16;
			if (journalLineData.categoryName != JournalStatusScreen.NO_ENTRIES_TEXT)
			{
				if (journalLineData.categoryExpanded)
				{
					sb.Append("[-] ");
				}
				else
				{
					sb.Append("[+] ");
				}
			}
			sb.Append(journalLineData.categoryName);
			headerContainer.SetActive(value: true);
			headerText.SetText(sb.ToString());
			text.SetText("");
			return;
		}
		if (journalLineData.entry is JournalRecipeNote { Recipe: not null } journalRecipeNote)
		{
			layoutGroup.padding.left = 16;
			headerContainer.SetActive(value: true);
			headerText.SetText(journalRecipeNote.Recipe.GetDisplayName());
			sb.Append("{{K|Ingredients:}} ");
			sb.AppendLine(journalRecipeNote.Recipe.GetIngredients());
			string[] array = journalRecipeNote.Recipe.GetDescription().Split("\n");
			for (int i = 0; i < array.Length; i++)
			{
				if (i != 0)
				{
					sb.Append("\n");
				}
				if (i == 0)
				{
					sb.Append("{{K|Effects:}}     {{K|/}} {{y|");
					sb.Append(array[i]);
					sb.Append("}}");
				}
				else
				{
					sb.Append("             {{K|/}} {{y|");
					sb.Append(array[i]);
					sb.Append("}}");
				}
			}
			sb.Append(" \n \n");
			text.SetText(sb.ToString());
			return;
		}
		headerContainer.SetActive(value: false);
		layoutGroup.padding.left = 48;
		if (journalLineData.entry is JournalMapNote journalMapNote)
		{
			sb.Append(journalMapNote.Tracked ? "[X] " : "[ ] ");
		}
		if (journalLineData.entry != null)
		{
			if (journalLineData.entry.Tradable)
			{
				sb.Append("{{G|$}} ");
			}
			else
			{
				sb.Append("{{K|$}} ");
			}
		}
		IBaseJournalEntry entry = journalLineData.entry;
		int num;
		if (entry == null)
		{
			num = 0;
		}
		else
		{
			num = (entry.Has("sultanTombPropaganda") ? 1 : 0);
			if (num != 0)
			{
				sb.Append("{{w|[tomb engraving] ");
			}
		}
		sb.Append(journalLineData.entry?.GetDisplayText() ?? "");
		if (num != 0)
		{
			sb.Append("}}");
		}
		if (Media.sizeClass > Media.SizeClass.Small || journalLineData.screen.CurrentCategory == 2)
		{
			text.SetText(sb.ToString());
		}
		else
		{
			text.SetText(StringFormat.ClipText(sb.ToString(), 45));
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
