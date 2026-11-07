using EnhancedUI.EnhancedScroller;
using Overlay.MapEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.EditorFormats.Map;
using XRL.World;

public class BlueprintCellView : EnhancedScrollerCellView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public Text blueprintID;

	public UIThreeColorProperties image;

	public static BlueprintCellView selectedCell;

	private UnityEngine.GameObject _blueprintReadout;

	private bool pointerIn;

	private MapEditorView mapEditorView => MapEditorView.instance;

	private UnityEngine.GameObject blueprintReadout
	{
		get
		{
			if (_blueprintReadout == null)
			{
				_blueprintReadout = mapEditorView.FindChild("BlueprintReadout");
			}
			return _blueprintReadout;
		}
	}

	public void SetData(BlueprintScrollerData data)
	{
		blueprintID.text = data.ID;
		RefreshCellView();
	}

	public override void RefreshCellView()
	{
		Button component = gameObject.GetComponent<Button>();
		ColorBlock colors = component.colors;
		string text = (mapEditorView.IsSingleObjectBrush() ? mapEditorView.GetBrushText() : "");
		if (blueprintID.text == text)
		{
			colors.normalColor = new Color(0f, 1f, 0f, 0.25f);
			selectedCell = this;
		}
		else
		{
			colors.normalColor = new Color(0f, 0f, 0f, 0f);
		}
		MapFileRegion region = new MapFileRegion(1, 1);
		region.GetOrCreateCellAt(0, 0).Objects.Add(new MapFileObjectBlueprint(blueprintID.text));
		MapFileCellRender mapFileCellRender = new MapFileCellRender();
		mapFileCellRender.RenderBlueprint(GameObjectFactory.Factory.Blueprints[blueprintID.text], new MapFileCellReference(region, 0, 0, null));
		mapFileCellRender.To3C(image);
		component.colors = colors;
	}

	public void OnSelected()
	{
		MapEditorView.instance.SetSelectedBlueprint(blueprintID.text);
	}

	public void Update()
	{
		if (!pointerIn)
		{
			return;
		}
		if (!RectTransformUtility.RectangleContainsScreenPoint(gameObject.GetComponent<RectTransform>(), Input.mousePosition))
		{
			pointerIn = false;
			blueprintReadout.SetActive(value: false);
		}
		else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			if (!blueprintReadout.activeInHierarchy)
			{
				blueprintReadout.SetActive(value: true);
				mapEditorView.FindChild("BlueprintReadout/ReadoutText").SetActive(value: true);
				mapEditorView.FindChild("BlueprintReadout/ReadoutText").GetComponent<Text>().text = GameObjectFactory.Factory.Blueprints[blueprintID.text].BlueprintXML();
			}
		}
		else if (blueprintReadout.activeInHierarchy)
		{
			blueprintReadout.SetActive(value: false);
		}
	}

	public void OnPointerEnter(PointerEventData data)
	{
		pointerIn = true;
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			blueprintReadout.SetActive(value: true);
			mapEditorView.FindChild("BlueprintReadout/ReadoutText").SetActive(value: true);
			mapEditorView.FindChild("BlueprintReadout/ReadoutText").GetComponent<Text>().text = GameObjectFactory.Factory.Blueprints[blueprintID.text].BlueprintXML();
		}
	}

	public void OnPointerExit(PointerEventData data)
	{
		pointerIn = false;
		blueprintReadout.SetActive(value: false);
	}
}
