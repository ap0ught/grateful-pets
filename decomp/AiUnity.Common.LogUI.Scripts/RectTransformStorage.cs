using UnityEngine;

namespace AiUnity.Common.LogUI.Scripts;

public struct RectTransformStorage
{
	public Vector3 anchoredPosition;

	public Vector2 anchorMax;

	public Vector2 anchorMin;

	public Vector3 localScale;

	public Vector3 position;

	public Quaternion rotation;

	public Vector2 sizeDelta;

	public void Restore(RectTransform t)
	{
		t.anchorMin = anchorMin;
		t.anchorMax = anchorMax;
		t.sizeDelta = sizeDelta;
		t.position = position;
		t.rotation = rotation;
		t.localScale = localScale;
		t.anchoredPosition = anchoredPosition;
	}

	public void Store(RectTransform t)
	{
		anchorMin = t.anchorMin;
		anchorMax = t.anchorMax;
		sizeDelta = t.sizeDelta;
		position = t.position;
		rotation = t.rotation;
		localScale = t.localScale;
		anchoredPosition = t.anchoredPosition;
	}
}
