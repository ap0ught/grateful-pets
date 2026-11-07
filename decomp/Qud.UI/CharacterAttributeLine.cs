using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class CharacterAttributeLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	public class Context : NavigationContext
	{
		public CharacterAttributeLineData data;
	}

	private Context context = new Context();

	public UITextSkin attributeText;

	public UITextSkin valueText;

	public UITextSkin modifierText;

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
		if (!(data is CharacterAttributeLineData characterAttributeLineData))
		{
			return;
		}
		context.data = characterAttributeLineData;
		attributeText.SetText((characterAttributeLineData.data == null) ? characterAttributeLineData.stat : characterAttributeLineData.data.GetShortDisplayName());
		string text = "C";
		if (characterAttributeLineData.data != null)
		{
			if (characterAttributeLineData.data.Value > characterAttributeLineData.data.BaseValue)
			{
				text = "G";
			}
			else if (characterAttributeLineData.data.Value < characterAttributeLineData.data.BaseValue)
			{
				text = "r";
			}
		}
		if (characterAttributeLineData.stat == "CP")
		{
			text = "C";
			valueText.SetText($"{{{{{text}|{CharacterStatusScreen.CP}}}}}");
		}
		else if (characterAttributeLineData.data.GetShortDisplayName() == "MS")
		{
			if (characterAttributeLineData.data.Value < characterAttributeLineData.data.BaseValue)
			{
				text = "G";
			}
			else if (characterAttributeLineData.data.Value > characterAttributeLineData.data.BaseValue)
			{
				text = "r";
			}
			valueText.SetText("{{" + text + "|" + (100 - characterAttributeLineData.data.Value + 100).ToStringCached() + "}}");
		}
		else if (characterAttributeLineData.data.GetShortDisplayName() == "AV")
		{
			valueText.SetText($"{{{{{text}|{Stats.GetCombatAV(characterAttributeLineData.go)}}}}}");
		}
		else if (characterAttributeLineData.data.GetShortDisplayName() == "DV")
		{
			valueText.SetText($"{{{{{text}|{Stats.GetCombatDV(characterAttributeLineData.go)}}}}}");
		}
		else if (characterAttributeLineData.data.GetShortDisplayName() == "MA")
		{
			valueText.SetText($"{{{{{text}|{Stats.GetCombatMA(characterAttributeLineData.go)}}}}}");
		}
		else
		{
			valueText.SetText("{{" + text + "|" + characterAttributeLineData.data.Value.ToStringCached() + "}}");
		}
		if (characterAttributeLineData.data != null)
		{
			modifierText?.SetText(((characterAttributeLineData.data.Modifier > -1) ? "{{G|[+" : "{{R|[") + characterAttributeLineData.data.Modifier.ToStringCached() + "]}}");
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
