using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class SkillsAndPowersLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	public class Context : NavigationContext
	{
		public SkillsAndPowersLineData data;
	}

	private Context context = new Context();

	public UITextSkin skillText;

	public UITextSkin skillRightText;

	public UITextSkin skillExpander;

	public UIThreeColorProperties skillIcon;

	public UITextSkin powerText;

	public GameObject skillType;

	public GameObject powerType;

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

	public bool wasSelected;

	public bool wasHighlighted;

	public bool isHighlighted;

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

	public bool selected => context?.IsActive() ?? false;

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public async void Accept()
	{
		_ = NavigationController.currentEvent;
		await APIDispatch.RunAndWaitAsync(() =>
		{
			SkillsAndPowersScreen.SelectNode(context.data.entry, context.data.screen.GO);
		});
		context.data.screen.UpdateData();
		context.data.screen.UpdateViewFromData();
	}

	public void XAxis()
	{
		XRL.UI.Framework.Event currentEvent = NavigationController.currentEvent;
		if (context.data.entry.Skill != null)
		{
			if (currentEvent.axisValue < 0)
			{
				if (!context.data.entry.Expand)
				{
					return;
				}
				context.data.entry.Expand = false;
			}
			else if (currentEvent.axisValue > 0)
			{
				if (context.data.entry.Expand)
				{
					return;
				}
				context.data.entry.Expand = true;
			}
		}
		else
		{
			int num = 1 + context.data.entry.ParentNode.Skill.PowerList.IndexOf(context.data.entry.Power);
			context.data.screen.controller.scrollContext.SelectIndex(context.data.screen.controller.selectedPosition - num);
			if (currentEvent.axisValue < 0)
			{
				if (!context.data.entry.ParentNode.Expand)
				{
					return;
				}
				context.data.entry.ParentNode.Expand = false;
			}
			else if (currentEvent.axisValue > 0)
			{
				if (context.data.entry.ParentNode.Expand)
				{
					return;
				}
				context.data.entry.ParentNode.Expand = true;
			}
		}
		context.data.screen.UpdateViewFromData();
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
		this.context.commandHandlers["Accept"] = Accept;
	}

	public void LateUpdate()
	{
		wasSelected = selected;
		wasHighlighted = isHighlighted;
	}

	public void ExpanderClicked()
	{
		if (context.IsActive())
		{
			context.data.entry.Expand = !context.data.entry.Expand;
			context.data.screen.UpdateViewFromData();
		}
	}

	public void BodyClicked()
	{
		if (context.IsActive())
		{
			if (context.data.entry.Skill != null && context.data.screen.GO.HasPart(context.data.entry.Skill.Class))
			{
				ExpanderClicked();
			}
			else
			{
				Accept();
			}
		}
	}

	public void setData(FrameworkDataElement data)
	{
		SkillsAndPowersLineData d = data as SkillsAndPowersLineData;
		if (d == null)
		{
			return;
		}
		context.data = d;
		skillType.SetActive(d.entry.Skill != null);
		powerType.SetActive(d.entry.Power != null);
		if (d.entry.Skill != null)
		{
			skillExpander.SetText(d.entry.Expand ? "[-]" : "[+]");
			Renderable renderable = new Renderable(d.entry.UIIcon);
			if (d.entry.IsLearned(d.go) == SPNode.LearnedStatus.None)
			{
				skillText.SetText(d.entry.Name ?? "");
				renderable.TileColor = "&K";
				renderable.DetailColor = 'k';
				if (d.go.GetStatValue("SP") >= d.entry.Skill.Cost)
				{
					skillRightText.SetText($"Starting Cost {{{{g|[{d.entry.Skill.Cost} sp]}}}}");
				}
				else
				{
					skillRightText.SetText($"Starting Cost {{{{r|[{d.entry.Skill.Cost} sp]}}}}");
				}
			}
			else
			{
				int num = d.entry.powers.Count();
				int num2 = d.entry.powers.Where((SPNode p) => p.IsLearned(d.go) == SPNode.LearnedStatus.Learned).Count();
				if (d.entry.IsLearned(d.go) == SPNode.LearnedStatus.Partial)
				{
					skillText.SetText(" {{g|" + d.entry.Name + "}}");
					if (d.go.GetStatValue("SP") >= d.entry.Skill.Cost)
					{
						skillRightText.SetText($"Starting Cost {{{{g|[{d.entry.Skill.Cost} sp]}}}} [{num2}/{num}]");
					}
					else
					{
						skillRightText.SetText($"Starting Cost {{{{r|[{d.entry.Skill.Cost} sp]}}}} [{num2}/{num}]");
					}
				}
				else
				{
					skillText.SetText(" {{G|" + d.entry.Name + "}}");
					skillRightText.SetText($"{{{{g|Learned}}}} [{num2}/{num}]");
				}
			}
			skillIcon.FromRenderable(renderable);
		}
		else
		{
			powerText.SetText(d.entry.ModernUIText(d.screen.GO) ?? "");
		}
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
}
