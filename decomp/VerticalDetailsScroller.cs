using UnityEngine;

public class VerticalDetailsScroller : MonoBehaviour
{
	public Canvas keyCanvas;

	public RectTransform contentTransform;

	private void Update()
	{
		if ((!(keyCanvas != null) || keyCanvas.enabled) && contentTransform != null)
		{
			float num = 512f;
			string text = ControlManager.ResolveAxisDirection("UI:DetailsNavigate");
			if (text != null && text.Contains("N"))
			{
				contentTransform.anchoredPosition += new Vector2(0f, (0f - num) * Time.deltaTime);
			}
			if (text != null && text.Contains("S"))
			{
				contentTransform.anchoredPosition += new Vector2(0f, num * Time.deltaTime);
			}
		}
	}
}
