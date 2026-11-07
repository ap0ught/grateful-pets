using UnityEngine;

public class UIArrowMove : MonoBehaviour
{
	public RectTransform arrowRect;

	public float moveSpeed = 2f;

	public float moveDistance = 50f;

	public bool moveHorizontally = true;

	private Vector2 initialPosition;

	private void Start()
	{
		if (arrowRect == null)
		{
			arrowRect = GetComponent<RectTransform>();
		}
		initialPosition = arrowRect.anchoredPosition;
	}

	private void Update()
	{
		float num = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
		if (moveHorizontally)
		{
			arrowRect.anchoredPosition = new Vector2(initialPosition.x + num, initialPosition.y);
		}
		else
		{
			arrowRect.anchoredPosition = new Vector2(initialPosition.x, initialPosition.y + num);
		}
	}
}
