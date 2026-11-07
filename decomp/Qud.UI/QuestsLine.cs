using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class QuestsLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IPointerClickHandler, IEventSystemHandler
{
	public class Context : NavigationContext
	{
		public QuestsLineData data;
	}

	private Context context = new Context();

	public TextMeshProUGUI titleTextText;

	public UITextSkin titleText;

	public UITextSkin bodyText;

	public UITextSkin giverText;

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

	public GameObject giverLabel;

	public Image background;

	public bool? wasSelected;

	public GameObject[] detailLayouts;

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
			context.axisHandlers = new Dictionary<InputAxisTypes, Action> { 
			{
				InputAxisTypes.NavigationXAxis,
				XAxis
			} };
		}
		context = this.context;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>();
		}
	}

	public void LateUpdate()
	{
		if (selected != wasSelected)
		{
			background.color = (selected ? ConsoleLib.Console.ColorUtility.FromWebColor("1A3843") : new Color(0f, 0f, 0f, 0f));
			titleText.color = (selected ? ConsoleLib.Console.ColorUtility.FromWebColor("cfc041") : ConsoleLib.Console.ColorUtility.FromWebColor("829EA8"));
		}
		wasSelected = selected;
	}

	public void XAxis()
	{
		XRL.UI.Framework.Event currentEvent = NavigationController.currentEvent;
		if (currentEvent.axisValue < 0)
		{
			if (context.data?.quest?.ID != null)
			{
				QuestsStatusScreen instance = SingletonWindowBase<QuestsStatusScreen>.instance;
				if (instance.CollapsedEntries == null)
				{
					instance.CollapsedEntries = new HashSet<string>();
				}
				SingletonWindowBase<QuestsStatusScreen>.instance.CollapsedEntries.Add(context.data?.quest?.ID);
				SingletonWindowBase<QuestsStatusScreen>.instance.UpdateViewFromData();
			}
		}
		else if (currentEvent.axisValue > 0 && context.data?.quest?.ID != null)
		{
			QuestsStatusScreen instance = SingletonWindowBase<QuestsStatusScreen>.instance;
			if (instance.CollapsedEntries == null)
			{
				instance.CollapsedEntries = new HashSet<string>();
			}
			SingletonWindowBase<QuestsStatusScreen>.instance.CollapsedEntries.Remove(context.data?.quest?.ID);
			SingletonWindowBase<QuestsStatusScreen>.instance.UpdateViewFromData();
		}
	}

	public void setData(FrameworkDataElement data)
	{
		if (!(data is QuestsLineData questsLineData))
		{
			return;
		}
		if (questsLineData.quest == null)
		{
			giverLabel.SetActive(value: false);
			titleText.SetText("You have no active quests.");
			giverText.SetText("");
			bodyText.SetText("");
			return;
		}
		giverLabel.SetActive(value: true);
		context.data = questsLineData;
		questsLineData.line = this;
		int clipWidth = 70;
		if (Media.sizeClass <= Media.SizeClass.Small)
		{
			clipWidth = 45;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in QuestLog.GetLinesForQuest(questsLineData.quest, IncludeTitle: false, Clip: true, clipWidth))
		{
			stringBuilder.AppendLine(item);
		}
		GameObject[] array = detailLayouts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(questsLineData.expanded);
		}
		titleText.SetText((questsLineData.expanded ? "[-]" : "[+]") + " " + ConsoleLib.Console.ColorUtility.StripFormatting(questsLineData.quest.DisplayName));
		giverText.SetText((questsLineData.quest.QuestGiverName ?? "<unknown>") + " / " + (questsLineData.quest.QuestGiverLocationName ?? "<unknown>"));
		bodyText.SetText(stringBuilder.ToString());
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

	public void OnPointerClick(PointerEventData eventData)
	{
		context.IsActive();
	}
}
