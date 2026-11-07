using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using FuzzySharp;
using FuzzySharp.Extractor;
using UnityEngine;
using XRL;
using XRL.Language;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Skills;

namespace Qud.UI;

[UIView("SkillsAndPowersStatusScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menus", UICanvas = "StatusScreens", UICanvasHost = 1)]
public class SkillsAndPowersStatusScreen : BaseStatusScreen<SkillsAndPowersStatusScreen>
{
	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public ButtonBar categoryBar;

	public FrameworkScroller controller;

	public UIThreeColorProperties skillIcon;

	public UIThreeColorProperties smallSkillIcon;

	private Renderable icon;

	public UnityEngine.GameObject requiredSkillsHeader;

	public UITextSkin detailsText;

	public UITextSkin skillNameText;

	public UITextSkin learnedText;

	public UITextSkin requirementsText;

	public UITextSkin requiredSkillsText;

	public UITextSkin statBlockText;

	public UITextSkin nameBlockText;

	public UIThreeColorProperties playerIcon;

	public XRL.World.GameObject GO;

	public UITextSkin spText;

	private SkillsAndPowersLineData searcher;

	private List<SkillsAndPowersLineData> lineData = new List<SkillsAndPowersLineData>(80);

	public override IRenderable GetTabIcon()
	{
		if (icon == null)
		{
			icon = new Renderable("Items/sw_longblade_3.bmp", " ", "&B", null, 'w');
		}
		return icon;
	}

	public override string GetTabString()
	{
		return "Skills";
	}

	public void Left()
	{
		_ = NavigationController.currentEvent;
	}

	public void Right()
	{
	}

	public void UpdateDetailsFromNode(SPNode node)
	{
		detailsText.SetText(node.Description);
		skillIcon.FromRenderable(node.UIIcon);
		smallSkillIcon.FromRenderable(node.UIIcon);
		skillNameText.SetText(node.Name);
		SPNode.LearnedStatus learnedStatus = node.IsLearned(GO);
		if (learnedStatus == SPNode.LearnedStatus.None)
		{
			learnedText.SetText("{{R|[Unlearned]}}");
		}
		if (learnedStatus == SPNode.LearnedStatus.Partial)
		{
			learnedText.SetText("{{W|[Unlearned]}}");
		}
		if (learnedStatus == SPNode.LearnedStatus.Learned)
		{
			learnedText.SetText("{{G|[Learned]}}");
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (learnedStatus == SPNode.LearnedStatus.Learned)
		{
			stringBuilder.Length = 0;
		}
		else if (node.Skill != null)
		{
			if (GO.GetStatValue("SP") >= node.Skill.Cost)
			{
				stringBuilder.Append($":: {{{{C|{node.Skill.Cost}}}}} SP ::");
			}
			else
			{
				stringBuilder.Append($":: {{{{R|{node.Skill.Cost}}}}} SP ::");
			}
		}
		else if (GO.GetStatValue("SP") >= node.Power.Cost)
		{
			stringBuilder.Append($":: {{{{C|{node.Power.Cost}}}}} SP ::");
		}
		else
		{
			stringBuilder.Append($":: {{{{R|{node.Power.Cost}}}}} SP ::");
		}
		if (learnedStatus == SPNode.LearnedStatus.None && node.Power != null)
		{
			int num = 0;
			foreach (PowerEntryRequirement requirement in node.Power.requirements)
			{
				if (num == 0)
				{
					stringBuilder.Append("\n:: ");
				}
				else
				{
					stringBuilder.Append(" or \n:: ");
				}
				num++;
				requirement.Render(GO, stringBuilder);
				stringBuilder.Append(" ::\n");
			}
		}
		requirementsText.SetText(stringBuilder.ToString());
		if (learnedStatus == SPNode.LearnedStatus.Learned)
		{
			requiredSkillsHeader.SetActive(value: false);
		}
		else
		{
			requiredSkillsHeader.SetActive(value: true);
		}
		if (learnedStatus == SPNode.LearnedStatus.Learned)
		{
			requiredSkillsText.SetText("");
			return;
		}
		if (node.Skill != null)
		{
			requiredSkillsText.SetText("{{K|[none]}}");
			return;
		}
		string text = "";
		if (string.IsNullOrEmpty(node.Power.Requires))
		{
			text = "{{K|[none]}}";
		}
		else
		{
			foreach (string item in node.Power.Requires.CachedCommaExpansion())
			{
				string text2 = item;
				bool flag = false;
				if (SkillFactory.Factory.TryGetFirstEntry(item, out var Entry))
				{
					if (node.Power.IsSkillInitiatory)
					{
						int num2 = node.Power.ParentSkill.PowerList.IndexOf(node.Power);
						if (num2 > 0 && node.Power.ParentSkill.PowerList[num2 - 1] == Entry)
						{
							continue;
						}
					}
					text2 = Entry.Name;
					flag = GO.HasSkill(item);
				}
				else if (MutationFactory.HasMutation(item))
				{
					text2 = MutationFactory.GetMutationEntryByName(item).Name;
					flag = GO.HasPart(item);
				}
				if (flag)
				{
					if (text != "")
					{
						text += "\n";
					}
					text = text + "{{G|[" + text2 + "]}}";
				}
				else
				{
					if (text != "")
					{
						text += "\n";
					}
					text = text + "{{R|[" + text2 + "]}}";
				}
			}
		}
		if (node.Power.Exclusion != null)
		{
			foreach (string item2 in node.Power.Exclusion.CachedCommaExpansion())
			{
				string text3 = item2;
				bool flag2 = false;
				if (SkillFactory.Factory.TryGetFirstEntry(item2, out var Entry2))
				{
					text3 = Entry2.Name;
					flag2 = !GO.HasSkill(item2);
				}
				else if (MutationFactory.HasMutation(item2))
				{
					text3 = MutationFactory.GetMutationEntryByName(item2).Name;
					flag2 = !GO.HasPart(item2);
				}
				if (flag2)
				{
					if (text != "")
					{
						text += "\n";
					}
					text = text + "Ex: {{g|" + text3 + "}}";
				}
				else
				{
					if (text != "")
					{
						text += "\n";
					}
					text = text + "Ex: {{R|" + text3 + "}}";
				}
			}
		}
		requiredSkillsText.SetText(text);
	}

	public void HandleSelectItem(FrameworkDataElement element)
	{
		if (element is SkillsAndPowersLineData skillsAndPowersLineData)
		{
			UpdateDetailsFromNode(skillsAndPowersLineData.entry);
		}
	}

	public void HandleHighlightObject(FrameworkDataElement element)
	{
		if (element is SkillsAndPowersLineData skillsAndPowersLineData)
		{
			UpdateDetailsFromNode(skillsAndPowersLineData.entry);
		}
	}

	public void HandleVPositive()
	{
		foreach (SPNode node in SkillsAndPowersScreen.Nodes)
		{
			if (node.ParentNode == null)
			{
				node.Expand = true;
			}
		}
		UpdateViewFromData();
	}

	public void HandleVNegative()
	{
		foreach (SPNode node in SkillsAndPowersScreen.Nodes)
		{
			if (node.ParentNode == null)
			{
				node.Expand = false;
			}
		}
		UpdateViewFromData();
	}

	public override NavigationContext ShowScreen(XRL.World.GameObject GO, StatusScreensScreen parent)
	{
		filterText = null;
		this.GO = GO;
		if (GO != null)
		{
			nameBlockText.SetText(Grammar.MakePossessive(GO.DisplayName ?? "") + " Skills");
			statBlockText.SetText(string.Format("{{{{K|{{{{g|STR:}}}}{0} ■ {{{{g|AGI}}}}: {1} ■ {{{{g|TOU}}}}: {2} ■ {{{{g|INT}}}}: {3} ■ {{{{g|WIL}}}}: {4} ■ {{{{g|EGO}}}}: {5}}}}}", GO.GetStat("Strength").Value, GO.GetStat("Agility").Value, GO.GetStat("Toughness").Value, GO.GetStat("Intelligence").Value, GO.GetStat("Willpower").Value, GO.GetStat("Ego").Value));
			playerIcon.FromRenderable(GO.RenderForUI());
		}
		categoryBar.SetupContext();
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Add(categoryBar.scrollContext);
		vertNav.contexts.Add(controller.scrollContext);
		vertNav.wraps = false;
		vertNav.Setup();
		controller.onSelected.RemoveAllListeners();
		controller.onSelected.AddListener(HandleSelectItem);
		controller.onHighlight.RemoveAllListeners();
		controller.onHighlight.AddListener(HandleHighlightObject);
		controller.scrollContext.wraps = false;
		controller.scrollContext.SetAxis(InputAxisTypes.NavigationYAxis);
		ScrollContext<FrameworkDataElement, NavigationContext> scrollContext = controller.scrollContext;
		if (scrollContext.axisHandlers == null)
		{
			scrollContext.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		controller.scrollContext.axisHandlers[InputAxisTypes.NavigationXAxis] = XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(Left, Right));
		scrollContext = controller.scrollContext;
		if (scrollContext.commandHandlers == null)
		{
			scrollContext.commandHandlers = new Dictionary<string, Action>();
		}
		controller.scrollContext.commandHandlers["V Positive"] = HandleVPositive;
		controller.scrollContext.commandHandlers["V Negative"] = HandleVNegative;
		base.ShowScreen(GO, parent);
		UpdateData();
		UpdateViewFromData();
		if (lineData.Count > 0)
		{
			UpdateDetailsFromNode(lineData.First().entry);
		}
		controller.selectedPosition = 0;
		controller.scrollContext.ActivateAndEnable();
		return vertNav;
	}

	public void UpdateData()
	{
		SkillsAndPowersScreen.BuildNodes(StatusScreensScreen.GO);
		if (GO != null)
		{
			spText.SetText(string.Format("Skill Points (SP): {{{{C|{0}}}}}", GO.GetStat("SP").Value));
		}
	}

	public bool IsSearching()
	{
		return !filterText.IsNullOrEmpty();
	}

	public override void UpdateViewFromData()
	{
		lineData.ForEach((SkillsAndPowersLineData l) =>
		{
			l.free();
		});
		lineData.Clear();
		lineData.AddRange(from n in SkillsAndPowersScreen.Nodes
			where n.ParentNode == null || n.ParentNode.Expand || IsSearching()
			select PooledFrameworkDataElement<SkillsAndPowersLineData>.next().set(n, this, GO));
		if (searcher == null)
		{
			searcher = new SkillsAndPowersLineData();
			searcher.entry = new SPNode(null, null, Expand: false, null);
		}
		searcher.entry._SearchText = filterText?.ToLower();
		if (filterText.IsNullOrEmpty())
		{
			controller.BeforeShow(lineData);
			return;
		}
		List<SkillsAndPowersLineData> searchResult = (from m in Process.ExtractTop(searcher, lineData, (SkillsAndPowersLineData i) => i.entry.SearchText.ToLower(), null, lineData.Count, 50)
			select m.Value).ToList();
		controller.BeforeShow(lineData.Where((SkillsAndPowersLineData i) => searchResult.Contains(i) || searchResult.Any((SkillsAndPowersLineData r) => r.entry.ParentNode == i.entry)));
	}
}
