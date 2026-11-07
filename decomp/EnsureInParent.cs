using UnityEngine;

public class EnsureInParent : MonoBehaviour
{
	public RectTransform _parentTransform;

	private RectTransform _rectTransform;

	public RectTransform parentTransform
	{
		get
		{
			if (_parentTransform == null)
			{
				_parentTransform = transform.parent.GetComponent<RectTransform>();
			}
			return _parentTransform;
		}
	}

	public RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (rectTransform.rect.width + rectTransform.anchoredPosition.x > parentTransform.rect.width)
		{
			rectTransform.anchoredPosition = new Vector2(parentTransform.rect.width - rectTransform.rect.width, rectTransform.anchoredPosition.y);
		}
		if (rectTransform.rect.height + (0f - rectTransform.anchoredPosition.y) > parentTransform.rect.height)
		{
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f - parentTransform.rect.height + rectTransform.rect.height);
		}
	}
}
