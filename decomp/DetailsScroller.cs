using UnityEngine;

public class DetailsScroller : MonoBehaviour
{
	public Canvas keyCanvas;

	public RectTransform contentTransform;

	private void Update()
	{
		if (keyCanvas.enabled && contentTransform != null)
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
			if (text != null && text.Contains("E"))
			{
				contentTransform.anchoredPosition += new Vector2((0f - num) * Time.deltaTime, 0f);
			}
			if (text != null && text.Contains("W"))
			{
				contentTransform.anchoredPosition += new Vector2(num * Time.deltaTime, 0f);
			}
		}
	}
}
