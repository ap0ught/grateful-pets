using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.EditorFormats.Map;

namespace Overlay.MapEditor;

public class MapEditorSelectedObjectsRow : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	public MapFileObjectBlueprint bp;

	public TextMeshProUGUI BlueprintName;

	public TextMeshProUGUI Count;

	public TextMeshProUGUI Owner;

	public GameObject ReplaceButton;

	public void Delete()
	{
		MapEditorView.Instance.DeleteInRegion(bp);
	}

	public void Replace()
	{
		MapEditorView.Instance.ReplaceInRegion(bp);
	}

	public void SetOwner()
	{
		MapEditorView.Instance.SetOwnerInRegion(bp);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		MapEditorView.Instance.SetHoverBlueprint(bp);
		GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		MapEditorView.Instance.SetHoverBlueprint(null);
		GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			MapEditorView.Instance.DisplayContextInRegion(bp);
		}
	}
}
