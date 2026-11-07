using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XRL.World;

namespace Battlehub.UIControls;

public class BlueprintBrowserTreeView : MonoBehaviour
{
	public TreeView TreeView;

	public InputField text;

	public static bool IsPrefab(Transform This)
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			throw new InvalidOperationException("Does not work in edit mode");
		}
		return This.gameObject.scene.buildIndex < 0;
	}

	private void Awake()
	{
		if (!TreeView)
		{
			Debug.LogError("Set TreeView field");
			return;
		}
		IOrderedEnumerable<GameObjectBlueprint> items = from blueprint in GameObjectFactory.Factory.BlueprintList
			where string.IsNullOrEmpty(blueprint.Inherits)
			orderby blueprint.Name
			select blueprint;
		TreeView.ItemDataBinding += OnItemDataBinding;
		TreeView.SelectionChanged += OnSelectionChanged;
		TreeView.ItemsRemoved += OnItemsRemoved;
		TreeView.ItemExpanding += OnItemExpanding;
		TreeView.ItemBeginDrag += OnItemBeginDrag;
		TreeView.ItemDrop += OnItemDrop;
		TreeView.ItemBeginDrop += OnItemBeginDrop;
		TreeView.ItemEndDrag += OnItemEndDrag;
		TreeView.Items = items;
	}

	private void OnItemBeginDrop(object sender, ItemDropCancelArgs e)
	{
	}

	private void OnDestroy()
	{
		if ((bool)TreeView)
		{
			TreeView.ItemDataBinding -= OnItemDataBinding;
			TreeView.SelectionChanged -= OnSelectionChanged;
			TreeView.ItemsRemoved -= OnItemsRemoved;
			TreeView.ItemExpanding -= OnItemExpanding;
			TreeView.ItemBeginDrag -= OnItemBeginDrag;
			TreeView.ItemBeginDrop -= OnItemBeginDrop;
			TreeView.ItemDrop -= OnItemDrop;
			TreeView.ItemEndDrag -= OnItemEndDrag;
		}
	}

	private void OnItemExpanding(object sender, ItemExpandingArgs e)
	{
		GameObjectBlueprint dataItem = (GameObjectBlueprint)e.Item;
		if (dataItem.hasChildren)
		{
			e.Children = from blueprint in GameObjectFactory.Factory.BlueprintList
				where blueprint.Inherits == dataItem.Name
				orderby blueprint.Name
				select blueprint;
		}
	}

	private void OnSelectionChanged(object sender, SelectionChangedArgs e)
	{
		IEnumerable<GameObjectBlueprint> source = e.NewItems.OfType<GameObjectBlueprint>();
		if (source.Count() > 0)
		{
			GameObjectBlueprint gameObjectBlueprint = source.First();
			text.text = gameObjectBlueprint.BlueprintXML();
		}
	}

	private void OnItemsRemoved(object sender, ItemsRemovedArgs e)
	{
		for (int i = 0; i < e.Items.Length; i++)
		{
		}
	}

	private void OnItemDataBinding(object sender, TreeViewItemDataBindingArgs e)
	{
		if (e.Item is GameObjectBlueprint gameObjectBlueprint)
		{
			e.ItemPresenter.GetComponentInChildren<Text>(includeInactive: true).text = gameObjectBlueprint.Name;
			e.ItemPresenter.GetComponentsInChildren<Image>()[4].sprite = Resources.Load<Sprite>("cube");
			e.HasChildren = gameObjectBlueprint.hasChildren;
		}
	}

	private void OnItemBeginDrag(object sender, ItemArgs e)
	{
	}

	private void OnItemDrop(object sender, ItemDropArgs e)
	{
	}

	private void OnItemEndDrag(object sender, ItemArgs e)
	{
	}

	private void Update()
	{
	}
}
