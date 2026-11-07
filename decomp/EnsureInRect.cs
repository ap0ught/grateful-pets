using UnityEngine;

public class EnsureInRect : MonoBehaviour
{
	public Canvas canvas;

	public RectTransform enclosingRect;

	private RectTransform _rectTransform;

	private Vector3[] enclosingCorners = new Vector3[4];

	private Vector3[] corners = new Vector3[4];

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

	private void LateUpdate()
	{
		enclosingRect.GetWorldCorners(enclosingCorners);
		rectTransform.GetWorldCorners(corners);
		if (corners[2].x > enclosingCorners[2].x)
		{
			rectTransform.position += new Vector3(enclosingCorners[2].x - corners[2].x, 0f, 0f);
		}
		if (corners[3].y < enclosingCorners[3].y)
		{
			rectTransform.position -= new Vector3(0f, corners[3].y - enclosingCorners[3].y, 0f);
		}
		if (corners[1].x < enclosingCorners[1].x)
		{
			rectTransform.position += new Vector3(enclosingCorners[0].x - corners[0].x, 0f, 0f);
		}
		if (corners[1].y > enclosingCorners[1].y)
		{
			rectTransform.position -= new Vector3(0f, corners[1].y - enclosingCorners[1].y, 0f);
		}
	}
}
