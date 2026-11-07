using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace Qud.UI;

public class TinkeringDetailsLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	public class Context : NavigationContext
	{
		public TinkeringLineData data;
	}

	private Context context = new Context();

	public UIThreeColorProperties icon;

	public UITextSkin text;

	public UITextSkin modBitCostText;

	public UITextSkin descriptionText;

	public UnityEngine.GameObject modHBorder;

	public UnityEngine.GameObject modDescruptionContainer;

	public UITextSkin modDescriptionText;

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

	private static Dictionary<string, XRL.World.GameObject> sampleObjects = new Dictionary<string, XRL.World.GameObject>();

	public UITextSkin requirementsHeaderText;

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
		if (!(data is TinkeringLineData tinkeringLineData))
		{
			return;
		}
		if (tinkeringLineData.category)
		{
			gameObject.SetActive(value: false);
			return;
		}
		gameObject.SetActive(value: true);
		context.data = tinkeringLineData;
		text.SetText(tinkeringLineData.data.DisplayName);
		if (tinkeringLineData.data.Type == "Build")
		{
			icon.gameObject.SetActive(value: true);
			if (!sampleObjects.ContainsKey(tinkeringLineData.data.Blueprint))
			{
				XRL.World.GameObject value = XRL.World.GameObject.CreateSample(tinkeringLineData.data.Blueprint);
				sampleObjects.Add(tinkeringLineData.data.Blueprint, value);
			}
			icon.FromRenderable(sampleObjects[tinkeringLineData.data.Blueprint].RenderForUI(null, AsIfKnown: true));
			descriptionText.SetText(tinkeringLineData.data.UnclippedDescription);
			if (tinkeringLineData.cost != null && tinkeringLineData.data.Type == "Build")
			{
				modBitCostText.SetText("{{K|| Bit Cost |}}\n" + tinkeringLineData.costString);
			}
			if (tinkeringLineData.cost != null && tinkeringLineData.data.Type == "Mod")
			{
				modBitCostText.SetText("{{K || Bit Cost |}}\n" + tinkeringLineData.costString);
			}
			if (tinkeringLineData.cost != null)
			{
				TinkerData data2 = tinkeringLineData.data;
				if (data2 != null && !data2.Ingredient.IsNullOrEmpty())
				{
					bool flag = false;
					string[] array = tinkeringLineData.data.Ingredient.Split(',');
					int num = 14;
					if (array.Length > 1)
					{
						num--;
					}
					modBitCostText.SetText(modBitCostText.text + "\n\n{{K|| Ingredients |}}");
					string[] array2 = array;
					foreach (string blueprint in array2)
					{
						if (The.Player.Inventory.FindObjectByBlueprint(blueprint) != null)
						{
							if (flag)
							{
								modBitCostText.SetText(modBitCostText.text + "\n-or-");
							}
							modBitCostText.SetText(modBitCostText.text + $"\n{{{{G|{'û'}}}}} {TinkeringHelpers.TinkeredItemShortDisplayName(blueprint)}\n");
							flag = true;
						}
					}
					if (!flag)
					{
						array2 = array;
						foreach (string blueprint2 in array2)
						{
							if (flag)
							{
								modBitCostText.SetText(modBitCostText.text + "\n-or-\n");
							}
							modBitCostText.SetText(modBitCostText.text + "\n{{R|X}} " + TinkeringHelpers.TinkeredItemShortDisplayName(blueprint2) + "\n");
							flag = true;
						}
					}
				}
			}
		}
		else if (tinkeringLineData.data.Type == "Mod")
		{
			icon?.gameObject?.SetActive(value: true);
			icon?.FromRenderable(tinkeringLineData.modObject?.RenderForUI(null, AsIfKnown: true));
			descriptionText.SetText(tinkeringLineData.modObject?.GetPart<Description>()?.GetShortDescription(AsIfKnown: true, NoConfusion: true, "Tinkering"));
			modDescriptionText.SetText("{{rules|" + ItemModding.GetModificationDescription(tinkeringLineData.data.Blueprint, tinkeringLineData.modObject) + "}}");
			if (tinkeringLineData.cost != null && tinkeringLineData.data.Type == "Build")
			{
				modBitCostText.SetText("{{K|| Bit Cost |}}\n" + tinkeringLineData.costString);
			}
			if (tinkeringLineData.cost != null && tinkeringLineData.data.Type == "Mod")
			{
				modBitCostText.SetText("{{K || Bit Cost |}}\n" + tinkeringLineData.costString);
			}
			if (tinkeringLineData.cost != null)
			{
				TinkerData data3 = tinkeringLineData.data;
				if (data3 != null && !data3.Ingredient.IsNullOrEmpty())
				{
					bool flag2 = false;
					string[] array3 = tinkeringLineData.data.Ingredient.Split(',');
					int num2 = 14;
					if (array3.Length > 1)
					{
						num2--;
					}
					modBitCostText.SetText(modBitCostText.text + "\n\n{{K|| Ingredients |}}");
					string[] array2 = array3;
					foreach (string blueprint3 in array2)
					{
						if (The.Player.Inventory.FindObjectByBlueprint(blueprint3) != null)
						{
							if (flag2)
							{
								modBitCostText.SetText(modBitCostText.text + "\n-or-");
							}
							modBitCostText.SetText(modBitCostText.text + $"\n{{{{G|{'û'}}}}} {TinkeringHelpers.TinkeredItemShortDisplayName(blueprint3)}\n");
							flag2 = true;
						}
					}
					if (!flag2)
					{
						array2 = array3;
						foreach (string blueprint4 in array2)
						{
							if (flag2)
							{
								modBitCostText.SetText(modBitCostText.text + "\n-or-\n");
							}
							modBitCostText.SetText(modBitCostText.text + "\n{{R|X}} " + TinkeringHelpers.TinkeredItemShortDisplayName(blueprint4) + "\n");
							flag2 = true;
						}
					}
				}
			}
		}
		else
		{
			icon.gameObject.SetActive(value: false);
		}
		modHBorder.SetActive(tinkeringLineData.data.Type == "Mod");
		modDescruptionContainer.SetActive(tinkeringLineData.data.Type == "Mod");
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
