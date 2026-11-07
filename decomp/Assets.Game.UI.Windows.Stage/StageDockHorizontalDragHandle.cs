using Qud.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Game.UI.Windows.Stage;

public class StageDockHorizontalDragHandle : MonoBehaviour, IDragHandler, IEventSystemHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	public StageDock dock;

	public Texture2D hoverCursor;

	private bool dragging;

	private bool inside;

	private float draggingWidth;

	public void SyncCursor()
	{
		if (dragging || inside)
		{
			CursorManager.instance.currentStyle = CursorManager.Style.ResizeWestEast;
		}
		else
		{
			CursorManager.instance.currentStyle = CursorManager.Style.Pointer;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!dragging)
		{
			draggingWidth = dock.calculatedWidth;
		}
		dragging = true;
		bool flag = false;
		if (GameManager.Instance.DockMovable == 3)
		{
			flag = GameManager.Instance.currentSidebarPosition == GameManager.PreferredSidebarPosition.Left;
		}
		if (GameManager.Instance.DockMovable == 1)
		{
			flag = true;
		}
		if (flag)
		{
			draggingWidth += eventData.delta.x / UIManager.scale;
		}
		else
		{
			draggingWidth -= eventData.delta.x / UIManager.scale;
		}
		dock.preferredWidth = draggingWidth;
		SingletonWindowBase<MessageLogWindow>.instance?.Reflow();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		dragging = false;
		PlayerPrefs.SetFloat("DockPreferredWith", dock.preferredWidth);
		SyncCursor();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		inside = true;
		SyncCursor();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		inside = false;
		SyncCursor();
	}
}
