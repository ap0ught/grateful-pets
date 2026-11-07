using UnityEngine;

public class ScreenAwareSize : MonoBehaviour
{
	public float widthPercentage;

	public float heightPercentage;

	public float lastCanvasX = -1f;

	public float lastCanvasY = -1f;

	public void Adjust()
	{
		lastCanvasX = CanvasSize.width;
		lastCanvasY = CanvasSize.height;
		if (widthPercentage > 0f)
		{
			GetComponent<RectTransform>().sizeDelta = new Vector2(CanvasSize.width * widthPercentage, GetComponent<RectTransform>().sizeDelta.y);
		}
		if (heightPercentage > 0f)
		{
			GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, CanvasSize.height * heightPercentage);
		}
	}

	public void Awake()
	{
		Adjust();
	}

	public void Update()
	{
		if (lastCanvasX != CanvasSize.width || lastCanvasY != CanvasSize.height)
		{
			Adjust();
		}
	}
}
