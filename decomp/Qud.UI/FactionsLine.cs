using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class FactionsLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	public class Context : NavigationContext
	{
		public FactionsLineData data;

		public FactionsStatusScreen.Context screenContext => parents.OfType<FactionsStatusScreen.Context>().FirstOrDefault();

		public FactionsStatusScreen screen => screenContext?.screen;
	}

	private Context context = new Context();

	public Image reputationIndicator;

	public UITextSkin expanderText;

	public UITextSkin barText;

	public UITextSkin barReputationText;

	public UITextSkin detailsText;

	public UnityEngine.GameObject detailsSpacer1;

	public UITextSkin detailsText2;

	public UnityEngine.GameObject detailsSpacer2;

	public UITextSkin detailsText3;

	public UITextSkin check;

	public UITextSkin hotkey;

	public UIThreeColorProperties icon;

	public UnityEngine.GameObject iconSpacer;

	public UnityEngine.GameObject detailsSection;

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

	public FactionsStatusScreen screen => context.screen;

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void XAxis()
	{
		_ = NavigationController.currentEvent;
		context.data.expanded = !context.data.expanded;
		context.screen.ExpansionUpdated();
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
			context.commandHandlers = new Dictionary<string, Action> { 
			{
				"Accept",
				XRL.UI.Framework.Event.Helpers.Handle(ToggleExpand)
			} };
		}
		Context context2 = this.context;
		FactionsLineData data = this.context.data;
		context2.menuOptionDescriptions = ((data != null && data.expanded) ? categoryCollapseOptions : categoryExpandOptions);
	}

	public void ToggleExpand()
	{
		screen?.ToggleExpanded(context.data);
	}

	public void LateUpdate()
	{
		wasSelected = selected;
		wasHighlighted = isHighlighted;
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is FactionsLineData factionsLineData)
		{
			context.data = factionsLineData;
			if (factionsLineData.expanded != detailsSection.activeSelf)
			{
				detailsSection.SetActive(factionsLineData.expanded);
			}
			Color color = ConsoleLib.Console.ColorUtility.colorFromChar(Reputation.GetColor((The.Game?.PlayerReputation?.Get(factionsLineData.id)).GetValueOrDefault()));
			reputationIndicator.color = color;
			expanderText.SetText(factionsLineData.expanded ? "{{C|-}}" : "{{C|+}}");
			barText.SetText(factionsLineData.label);
			barReputationText.SetText("Reputation: " + FactionsScreen.FormatFactionReputation(factionsLineData.id));
			Faction faction = Factions.Get(factionsLineData.id);
			detailsText.SetText(faction.GetFeelingText());
			string rankText = faction.GetRankText();
			rankText = (string.IsNullOrEmpty(rankText) ? faction.GetPetText() : (rankText + " " + faction.GetPetText()));
			rankText = (string.IsNullOrEmpty(rankText) ? faction.GetHolyPlaceText() : (rankText + " " + faction.GetHolyPlaceText()));
			detailsText3.blockWrap = ((Media.sizeClass <= Media.SizeClass.Small) ? 40 : 60);
			detailsText2.SetText(rankText);
			detailsText3.SetText(Faction.GetPreferredSecretDescription(factionsLineData.id));
			icon.FromRenderable(factionsLineData.icon);
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
