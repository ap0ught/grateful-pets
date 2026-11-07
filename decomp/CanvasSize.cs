using UnityEngine;

public class CanvasSize : MonoBehaviour
{
	public RectTransform canvasRect;

	public static float width = 1248f;

	public static float height = 768f;

	public static bool updated = true;

	private void Start()
	{
	}

	private void Update()
	{
		updated = width != canvasRect.rect.width || height != canvasRect.rect.height;
		width = canvasRect.rect.width;
		height = canvasRect.rect.height;
	}
}
