using UnityEngine;
using UnityEngine.UI;

public class BackdropWindowManager : MonoBehaviour
{
	public Image backdropImage;

	public RectTransform backdropTransform;

	public void SetBackdrop(string backdrop)
	{
	}

	public void SetTile(int x, int y)
	{
		backdropTransform.anchoredPosition = new Vector3((float)(-x) * 3f * 16f, (float)y * 3f * 24f);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
