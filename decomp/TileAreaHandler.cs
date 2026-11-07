using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileAreaHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IMoveHandler, IPointerExitHandler, IScrollHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	public bool PointerInside;

	public float hoverTimer;

	public float hoverDelay = 0.5f;

	private int skipRelease;

	private Camera mainCamera;

	private Vector3 lastMousePosition = Vector3.zero;

	private bool dragging = true;

	public void OnMove(AxisEventData eventData)
	{
		hoverTimer = 0f;
	}

	public void StartHover(Vector2 position)
	{
		if (!GameManager.Instance.MouseInput)
		{
			return;
		}
		RaycastHit[] array = Physics.RaycastAll(GameManager.MainCamera.GetComponent<Camera>().ScreenPointToRay(position));
		foreach (RaycastHit raycastHit in array)
		{
			TileBehavior component = raycastHit.collider.gameObject.GetComponent<TileBehavior>();
			if (component != null)
			{
				GameManager.Instance.TileHover(component.x, component.y);
			}
		}
	}

	public void EndHover()
	{
		hoverTimer = 0f;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.dragging || !GameManager.Instance.MouseInput || GameManager.MainCamera == null)
		{
			return;
		}
		RaycastHit[] array = Physics.RaycastAll(GameManager.MainCamera.GetComponent<Camera>().ScreenPointToRay(eventData.position));
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			TileBehavior component = raycastHit.collider.gameObject.GetComponent<TileBehavior>();
			if (component != null)
			{
				if (eventData.button == PointerEventData.InputButton.Left)
				{
					if (!Input.GetKey(UnityEngine.KeyCode.LeftControl) && !Input.GetKey(UnityEngine.KeyCode.RightControl))
					{
						GameManager.Instance.OnTileClicked("LeftClick", component.x, component.y);
					}
				}
				else
				{
					GameManager.Instance.OnTileClicked("RightClick", component.x, component.y);
				}
			}
			BorderTileBehavior component2 = raycastHit.collider.gameObject.GetComponent<BorderTileBehavior>();
			if (component2 != null)
			{
				Keyboard.PushMouseEvent($"CmdMoveToBorder:{component2.x},{component2.y}");
			}
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		GameManager.Instance.OnScroll(eventData.scrollDelta);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		PointerInside = true;
		hoverTimer = 0f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		PointerInside = false;
		hoverTimer = 0f;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!GameManager.Instance.MouseInput || GameManager.MainCamera == null)
		{
			return;
		}
		if (mainCamera == null)
		{
			mainCamera = GameManager.MainCamera.GetComponent<Camera>();
		}
		if (mainCamera == null || CursorManager.instance.cursorHidden)
		{
			return;
		}
		bool flag = false;
		if (PointerInside)
		{
			if (lastMousePosition == Input.mousePosition)
			{
				hoverTimer += Time.deltaTime;
				flag = false;
			}
			else
			{
				EndHover();
				flag = true;
			}
			lastMousePosition = Input.mousePosition;
			RaycastHit[] array = Physics.RaycastAll(mainCamera.ScreenPointToRay(Input.mousePosition));
			foreach (RaycastHit raycastHit in array)
			{
				TileBehavior component = raycastHit.collider.gameObject.GetComponent<TileBehavior>();
				if ((component != null) & flag)
				{
					GameManager.Instance.OnTileOver(component.x, component.y);
				}
				if (!(component != null) || skipRelease > 0)
				{
					continue;
				}
				if (GameManager.Instance.CurrentGameView == "Stage")
				{
					if (ControlManager.GetButtonReleasedThisFrame("AdventureMouseContextAction"))
					{
						GameManager.Instance.OnTileClicked("AdventureMouseContextAction", component.x, component.y);
					}
					if (ControlManager.GetButtonReleasedThisFrame("AdventureMouseInteract"))
					{
						GameManager.Instance.OnTileClicked("AdventureMouseInteract", component.x, component.y);
					}
					if (ControlManager.GetButtonReleasedThisFrame("AdventureMouseInteractAll"))
					{
						GameManager.Instance.OnTileClicked("AdventureMouseInteractAll", component.x, component.y);
					}
					if (ControlManager.GetButtonReleasedThisFrame("AdventureMouseLook"))
					{
						GameManager.Instance.OnTileClicked("AdventureMouseLook", component.x, component.y);
					}
					if (ControlManager.GetButtonReleasedThisFrame("AdventureMouseForceAttack"))
					{
						GameManager.Instance.OnTileClicked("AdventureMouseForceAttack", component.x, component.y);
					}
					if (ControlManager.GetButtonReleasedThisFrame("AdventureMouseForceMove"))
					{
						GameManager.Instance.OnTileClicked("AdventureMouseForceMove", component.x, component.y);
					}
					if (ControlManager.GetButtonReleasedThisFrame("AdventureMouseQuickLook"))
					{
						GameManager.Instance.ShowTooltipForTile(component.x, component.y);
						return;
					}
				}
				else
				{
					if (ControlManager.GetButtonReleasedThisFrame("AdventureNavMouseLeftClick"))
					{
						Keyboard.PushMouseEvent("LeftClick", component.x, component.y);
					}
					if (ControlManager.GetButtonReleasedThisFrame("AdventureNavMouseRightClick"))
					{
						Keyboard.PushMouseEvent("RightClick", component.x, component.y);
					}
				}
			}
			if (hoverTimer > hoverDelay)
			{
				StartHover(Input.mousePosition);
				hoverTimer = 0f;
			}
		}
		if (!dragging && skipRelease > 0)
		{
			skipRelease--;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		skipRelease = 1;
		dragging = true;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		GameManager.Instance.OnEndDrag(eventData);
		dragging = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
		GameManager.Instance.OnDrag(eventData.delta);
	}
}
