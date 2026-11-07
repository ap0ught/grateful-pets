using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World.Parts.Mutation;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:PickMutations", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/PickMutations", UICanvasHost = 1)]
public class QudMutationsModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudMutationsModule, CategoryMenusScroller>
{
	[Serializable]
	public class MLNode
	{
		public MutationCategory Category;

		public MutationEntry Entry;

		public bool bExpand;

		public MLNode ParentNode;

		public int Selected;

		public string Variant;

		public List<MLNode> nodes;

		public BaseMutation Exemplar;

		public bool Valid()
		{
			if (Entry == null)
			{
				return true;
			}
			foreach (MLNode node in nodes)
			{
				if (node.Entry != null && node.Selected > 0 && !Entry.OkWith(node.Entry))
				{
					return false;
				}
			}
			return true;
		}

		public void Randomize()
		{
			Variant = null;
			if (Entry != null)
			{
				if (!Entry.Variant.IsNullOrEmpty())
				{
					Variant = Entry.Variant;
				}
				else if (Entry.HasVariants && Entry.CanSelectVariant)
				{
					BaseMutation baseMutation = Exemplar ?? Entry.Mutation;
					Variant = baseMutation.GetVariants().GetRandomElement(new System.Random());
				}
			}
		}
	}

	public EmbarkBuilderModuleWindowDescriptor windowDescriptor;

	public bool VariantSelectionEnabled;

	private MLNode selectedNode;

	protected const string EMPTY_CHECK = "[ ]";

	protected const string CHECKED = "[■]";

	private StringBuilder sb = new StringBuilder();

	private List<CategoryMenuData> categoryMenus = new List<CategoryMenuData>();

	public static readonly string EID_GET_BASE_MP = "GetBaseMP";

	public static readonly string EID_GET_CATEGORIES = "GetMutationCategories";

	private List<MLNode> mutationNodes = new List<MLNode>();

	private const string VARIANT_SELECT = "Variant";

	public const string SHOW_POINTS = "ShowPoints";

	public override void ResetSelection()
	{
		QudMutationsModuleData qudMutationsModuleData = new QudMutationsModuleData();
		qudMutationsModuleData.mp = (int)module.builder.handleUIEvent(EID_GET_BASE_MP, 0);
		if (qudMutationsModuleData.mp == 0)
		{
			module.setData(null);
			return;
		}
		module.setData(qudMutationsModuleData);
		UpdateNodesFromData();
		UpdateControls();
	}

	public void UpdateDataFromNodes()
	{
		QudMutationsModuleData qudMutationsModuleData = new QudMutationsModuleData();
		qudMutationsModuleData.mp = (int)module.builder.handleUIEvent(EID_GET_BASE_MP, 0);
		foreach (MLNode mutationNode in mutationNodes)
		{
			if (mutationNode.Selected > 0)
			{
				qudMutationsModuleData.mp -= mutationNode.Selected * mutationNode.Entry.Cost;
				QudMutationModuleDataRow qudMutationModuleDataRow = new QudMutationModuleDataRow();
				qudMutationModuleDataRow.Mutation = mutationNode.Entry.Name;
				qudMutationModuleDataRow.Count = mutationNode.Selected;
				qudMutationModuleDataRow.Variant = mutationNode.Variant;
				qudMutationsModuleData.selections.Add(qudMutationModuleDataRow);
			}
		}
		module.setData(qudMutationsModuleData);
	}

	public override GameObject InstantiatePrefab(GameObject prefab)
	{
		prefab.GetComponentInChildren<CategoryMenusScroller>().allowVerticalLayout = false;
		return base.InstantiatePrefab(prefab);
	}

	public void UpdateNodesFromData()
	{
		ClearNodes();
		int num = 0;
		foreach (QudMutationModuleDataRow row in module.data.selections)
		{
			MLNode mLNode = mutationNodes.Find((MLNode n) => n.Entry != null && n.Entry.Name == row.Mutation);
			mLNode.Selected = row.Count;
			mLNode.Variant = row.Variant;
			num += mLNode.Selected * mLNode.Entry.Cost;
		}
	}

	public void UpdateControls()
	{
		foreach (PrefixMenuOption menu in categoryMenus.SelectMany((CategoryMenuData category) => category.menuOptions))
		{
			MLNode mLNode = mutationNodes?.Find((MLNode n) => n?.Entry?.Name == menu.Id);
			menu.Prefix = FormatNodePrefix(mLNode);
			menu.Description = FormatNodeDescription(mLNode, mLNode.Entry);
			if (mLNode.Entry.HasVariants && mLNode.Entry.CanSelectVariant)
			{
				BaseMutation exemplar = mLNode.Exemplar;
				menu.LongDescription = ((exemplar != null) ? (exemplar.GetDescription() + "\n\n" + exemplar.GetLevelText(1)) : "???");
				menu.Renderable = mLNode.Entry.GetRenderable();
				if (exemplar != null && menu.Renderable is Renderable renderable)
				{
					renderable.Tile = exemplar.GetIcon().getTile();
				}
			}
		}
		prefabComponent.BeforeShow(windowDescriptor, categoryMenus);
		prefabComponent.onHighlight.AddListener(HighlightMutation);
		GetOverlayWindow().UpdateMenuBars(windowDescriptor);
	}

	public override void RandomSelectionNoUI()
	{
		EnsureData();
		while (module.data.mp > 0)
		{
			MLNode randomElement = mutationNodes.Where((MLNode m) => m.Entry != null && m.Entry.Cost <= module.data.mp && m.Selected < m.Entry.Maximum && m.Valid()).GetRandomElement();
			randomElement.Selected++;
			randomElement.Randomize();
			UpdateDataFromNodes();
		}
	}

	public override void RandomSelection()
	{
		while (module.data.mp > 0)
		{
			MLNode randomElement = mutationNodes.Where((MLNode m) => m.Entry != null && m.Entry.Cost <= module.data.mp && m.Selected < m.Entry.Maximum && m.Valid()).GetRandomElement();
			randomElement.Selected++;
			randomElement.Randomize();
			UpdateDataFromNodes();
			UpdateControls();
		}
	}

	public void HighlightMutation(FrameworkDataElement dataElement)
	{
		MLNode mLNode = mutationNodes.Find((MLNode e) => e?.Entry?.Name == dataElement?.Id);
		if (mLNode != null)
		{
			selectedNode = mLNode;
			VariantSelectionEnabled = mLNode.Entry != null && mLNode.Entry.HasVariants && mLNode.Entry.CanSelectVariant;
			GetOverlayWindow().UpdateMenuBars(GetOverlayWindow().currentWindowDescriptor);
		}
	}

	public void SelectMutation(FrameworkDataElement dataElement)
	{
		MLNode mLNode = mutationNodes.Find((MLNode e) => e?.Entry?.Name == dataElement?.Id);
		if (mLNode != null)
		{
			if (mLNode.Selected < mLNode.Entry.Maximum && mLNode.Entry.Cost <= module.data.mp && mLNode.Valid())
			{
				mLNode.Selected++;
			}
			else if (mLNode.Selected > 0)
			{
				mLNode.Selected = 0;
			}
			UpdateDataFromNodes();
			UpdateControls();
		}
	}

	private string FormatNodeDescription(MLNode node, MutationEntry entry)
	{
		sb.Clear();
		BaseMutation exemplar = node.Exemplar;
		exemplar.SetVariant(node.Variant);
		sb.Append(exemplar.GetCreateCharacterDisplayName());
		sb.Append((entry.HasVariants && entry.CanSelectVariant) ? (" [{{W|" + ControlManager.getCommandInputDescription("CmdChargenMutationVariant") + "}}]") : "");
		return sb.ToString();
	}

	private string FormatNodePrefix(MLNode node)
	{
		bool flag = node.Selected > 0;
		bool num = (!node.Valid() || node.Entry.Cost > module.data.mp) && !flag;
		sb.Clear();
		if (num)
		{
			sb.Append("{{K|");
		}
		if (node.Selected == 0)
		{
			sb.Append("[ ]");
		}
		else if (node.Selected == 1 && node.Entry.Maximum == 1)
		{
			sb.Append("[■]");
		}
		else
		{
			sb.Append("[").Append(node.Selected).Append("]");
		}
		sb.Append("[{{");
		sb.Append((node.Entry.Cost > 0) ? "G" : "R");
		sb.Append("|").Append(node.Entry.Cost).Append("}}]");
		if (num)
		{
			sb.Append("}}");
		}
		return sb.ToString();
	}

	private PrefixMenuOption makeMenuOption(MutationEntry entry)
	{
		MLNode mLNode = mutationNodes.Find((MLNode n) => n?.Entry?.Name == entry?.Name);
		BaseMutation exemplar = mLNode.Exemplar;
		return new PrefixMenuOption
		{
			Id = entry.Name,
			Prefix = FormatNodePrefix(mLNode),
			Description = FormatNodeDescription(mLNode, entry),
			LongDescription = ((exemplar != null) ? (exemplar.GetDescription() + "\n\n" + exemplar.GetLevelText(1)) : "???"),
			Renderable = ((exemplar != null) ? exemplar.GetIcon() : entry.GetRenderable())
		};
	}

	public void ClearNodes()
	{
		mutationNodes = new List<MLNode>();
		categoryMenus = new List<CategoryMenuData>();
		List<string> list = module.builder.handleUIEvent(EID_GET_CATEGORIES) as List<string>;
		foreach (MutationCategory category in MutationFactory.GetCategories())
		{
			if (category.Entries.All((MutationEntry x) => x.Hidden) || (list != null && !list.Contains(category.Name)))
			{
				continue;
			}
			CategoryMenuData categoryMenuData = new CategoryMenuData();
			categoryMenus.Add(categoryMenuData);
			categoryMenuData.Title = category.DisplayName;
			categoryMenuData.menuOptions = new List<PrefixMenuOption>();
			MLNode mLNode = new MLNode();
			mLNode.bExpand = false;
			mLNode.Category = category;
			mLNode.nodes = mutationNodes;
			mutationNodes.Add(mLNode);
			foreach (MutationEntry entry in category.Entries)
			{
				if (!entry.Hidden)
				{
					MLNode mLNode2 = new MLNode();
					mLNode2.Exemplar = entry.CreateInstance();
					mLNode2.ParentNode = mLNode;
					mLNode2.Entry = entry;
					mLNode2.Variant = entry.Variant;
					mLNode2.nodes = mutationNodes;
					mutationNodes.Add(mLNode2);
				}
			}
			categoryMenuData.menuOptions.AddRange(category.Entries.Where((MutationEntry x) => !x.Hidden).Select(makeMenuOption));
		}
	}

	public void EnsureData()
	{
		if (module.data == null || (module.data.mp == 0 && module.data.selections.Count == 0))
		{
			QudMutationsModuleData qudMutationsModuleData = new QudMutationsModuleData();
			qudMutationsModuleData.mp = (int)module.builder.handleUIEvent(EID_GET_BASE_MP, 0);
			module.setData(qudMutationsModuleData);
		}
		UpdateNodesFromData();
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		if (descriptor != null)
		{
			windowDescriptor = descriptor;
		}
		EnsureData();
		prefabComponent.onSelected.RemoveAllListeners();
		prefabComponent.onSelected.AddListener(SelectMutation);
		UpdateControls();
	}

	public override IEnumerable<MenuOption> GetKeyLegend()
	{
		foreach (MenuOption item in base.GetKeyLegend())
		{
			yield return item;
		}
	}

	public override IEnumerable<MenuOption> GetKeyMenuBar()
	{
		yield return new MenuOption
		{
			Id = "ShowPoints",
			InputCommand = "",
			KeyDescription = null,
			Description = ((module.data.mp < 0) ? ("{{R|Points Remaining: " + module.data.mp + "}}") : ("{{y|Points Remaining: " + module.data.mp + "}}"))
		};
		if (VariantSelectionEnabled)
		{
			yield return new MenuOption
			{
				Id = "Variant",
				InputCommand = "CmdChargenMutationVariant",
				KeyDescription = ControlManager.getCommandInputDescription("CmdChargenMutationVariant"),
				Description = "Choose Variant"
			};
		}
		foreach (MenuOption item in base.GetKeyMenuBar())
		{
			yield return item;
		}
	}

	public override void HandleMenuOption(MenuOption menuOption)
	{
		if (menuOption.Id == "ShowPoints")
		{
			string text = module.GetSummaryBlock()?.Description;
			if (!string.IsNullOrEmpty(text))
			{
				Popup.NewPopupMessageAsync(text, PopupMessage.AcceptButton, null, "Points Remaining: " + module.data.mp).Start();
			}
		}
		else if (menuOption.Id == "Variant")
		{
			SelectVariant();
		}
		else
		{
			base.HandleMenuOption(menuOption);
		}
	}

	public async void SelectVariant()
	{
		BaseMutation baseMutation = selectedNode.Exemplar ?? selectedNode.Entry.Mutation;
		List<string> variants = baseMutation.GetVariants();
		string[] array = new string[variants.Count];
		IRenderable[] array2 = new IRenderable[variants.Count];
		for (int i = 0; i < variants.Count; i++)
		{
			string text = variants[i];
			array[i] = baseMutation.GetVariantName(text);
			array2[i] = baseMutation.GetIcon(text);
		}
		int num = await Popup.PickOptionAsync("Choose variant", null, "", array, null, array2, null, null, 0, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true);
		if (num >= 0)
		{
			selectedNode.Variant = variants[num];
			UpdateDataFromNodes();
			UpdateControls();
		}
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		return new UIBreadcrumb
		{
			Id = GetType().FullName,
			Title = "Mutations",
			IconPath = "Items/sw_horns.bmp",
			IconDetailColor = Color.clear,
			IconForegroundColor = The.Color.Gray
		};
	}
}
