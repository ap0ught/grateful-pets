using UnityEngine;

[ExecuteAlways]
public class PopupScrollbarRightJustify : MonoBehaviour
{
	public RectTransform parent;

	public RectTransform target;

	public float offset;

	public float proposedOffset;

	private void Start()
	{
	}

	private void Update()
	{
		proposedOffset = target.rect.width - offset - parent.rect.width;
		RectTransform rectTransform = transform as RectTransform;
		if (rectTransform.anchoredPosition.x != proposedOffset)
		{
			rectTransform.anchoredPosition = new Vector2(proposedOffset, 0f);
		}
	}
}
