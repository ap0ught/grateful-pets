using UnityEngine;

namespace Battlehub.UIControls;

[RequireComponent(typeof(RectTransform))]
public class ItemDropMarker : MonoBehaviour
{
	private Canvas m_parentCanvas;

	private ItemsControl m_itemsControl;

	public GameObject SiblingGraphics;

	private ItemDropAction m_action;

	private RectTransform m_rectTransform;

	private ItemContainer m_item;

	protected Canvas ParentCanvas => m_parentCanvas;

	public virtual ItemDropAction Action
	{
		get
		{
			return m_action;
		}
		set
		{
			m_action = value;
		}
	}

	public RectTransform RectTransform => m_rectTransform;

	protected ItemContainer Item => m_item;

	private void Awake()
	{
		m_rectTransform = GetComponent<RectTransform>();
		SiblingGraphics.SetActive(value: true);
		m_parentCanvas = GetComponentInParent<Canvas>();
		m_itemsControl = GetComponentInParent<ItemsControl>();
		AwakeOverride();
	}

	protected virtual void AwakeOverride()
	{
	}

	public virtual void SetTraget(ItemContainer item)
	{
		gameObject.SetActive(item != null);
		m_item = item;
		if (m_item == null)
		{
			Action = ItemDropAction.None;
		}
	}

	public virtual void SetPosition(Vector2 position)
	{
		if (m_item == null || !m_itemsControl.CanReorder)
		{
			return;
		}
		RectTransform rectTransform = Item.RectTransform;
		Camera cam = null;
		if (ParentCanvas.renderMode == RenderMode.WorldSpace || ParentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
		{
			cam = m_itemsControl.Camera;
		}
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, cam, out var localPoint))
		{
			if (localPoint.y > (0f - rectTransform.rect.height) / 2f)
			{
				Action = ItemDropAction.SetPrevSibling;
				RectTransform.position = rectTransform.position;
			}
			else
			{
				Action = ItemDropAction.SetNextSibling;
				RectTransform.position = rectTransform.position;
				RectTransform.localPosition -= new Vector3(0f, rectTransform.rect.height * ParentCanvas.scaleFactor, 0f);
			}
		}
	}
}
