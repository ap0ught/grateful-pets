using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XRL.EditorFormats.Map;
using XRL.World;

namespace Overlay.MapEditor;

public class SelectedCellContentsView : MonoBehaviour
{
	public UnityEngine.GameObject CellContentsPrefab;

	public UnityEngine.GameObject ScrollParent;

	public List<UnityEngine.GameObject> Items = new List<UnityEngine.GameObject>();

	private Dictionary<MapFileObjectBlueprint, List<MapFileCell>> Contents = new Dictionary<MapFileObjectBlueprint, List<MapFileCell>>();

	private StringBuilder SB = new StringBuilder();

	public void Set(MapFileRegion value, bool allowReplace = true)
	{
		for (int i = 0; i < Items.Count; i++)
		{
			Object.Destroy(Items[i]);
		}
		Items.Clear();
		Contents.Clear();
		foreach (MapFileObjectReference item in value.AllObjects())
		{
			if (!Contents.TryGetValue(item.blueprint, out var value2))
			{
				value2 = new List<MapFileCell>();
				Contents.Add(item.blueprint, value2);
			}
			value2.Add(item.cell);
		}
		foreach (KeyValuePair<MapFileObjectBlueprint, List<MapFileCell>> content in Contents)
		{
			Items.Add(PooledPrefabManager.Instantiate(CellContentsPrefab));
			MapEditorSelectedObjectsRow component = Items[Items.Count - 1].GetComponent<MapEditorSelectedObjectsRow>();
			component.bp = new MapFileObjectBlueprint(content.Key);
			component.BlueprintName.text = content.Key.Name;
			if (GameObjectFactory.Factory.Blueprints.ContainsKey(content.Key.Name))
			{
				if (!content.Key.Properties.IsNullOrEmpty() || !content.Key.IntProperties.IsNullOrEmpty())
				{
					component.BlueprintName.color = Color.cyan;
				}
				else
				{
					component.BlueprintName.color = Color.white;
				}
			}
			else
			{
				component.BlueprintName.color = Color.red;
			}
			component.Count.text = $"x{content.Value.Count}";
			SB.Clear();
			if (!content.Key.Owner.IsNullOrEmpty())
			{
				SB.Append('[').Append(content.Key.Owner).Append(']');
			}
			if (!content.Key.Part.IsNullOrEmpty())
			{
				SB.Compound('[').Append(content.Key.Part).Append(']');
			}
			component.Owner.text = SB.ToString();
			component.ReplaceButton.SetActive(allowReplace);
			component.transform.SetParent(ScrollParent.transform, worldPositionStays: false);
			Items[Items.Count - 1].SetActive(value: true);
		}
	}
}
