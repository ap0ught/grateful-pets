using UnityEngine;
using UnityEngine.UI;

public class PositionResponsiveElement : MonoBehaviour
{
	public Camera mainCamera;

	public Canvas keyCanvas;

	public Image targetImage;

	private RectTransform rectTransform;

	private Vector3[] worldCorners = new Vector3[4];

	private Vector3[] screenCorners = new Vector3[4];

	private void LateUpdate()
	{
		if (!keyCanvas.enabled)
		{
			return;
		}
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		rectTransform.GetWorldCorners(worldCorners);
		bool flag = true;
		for (int i = 0; i < 4; i++)
		{
			screenCorners[i] = mainCamera.WorldToScreenPoint(worldCorners[i]);
			if (screenCorners[i].x < 0f)
			{
				flag = false;
			}
			if (screenCorners[i].y < 0f)
			{
				flag = false;
			}
			if (screenCorners[i].x >= (float)Screen.width)
			{
				flag = false;
			}
			if (screenCorners[i].y >= (float)Screen.height)
			{
				flag = false;
			}
		}
		if (!flag)
		{
			if (targetImage != null && targetImage.enabled)
			{
				targetImage.enabled = false;
			}
		}
		else if (targetImage != null && !targetImage.enabled)
		{
			targetImage.enabled = true;
		}
	}
}
