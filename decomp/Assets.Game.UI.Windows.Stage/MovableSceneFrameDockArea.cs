using System;
using UnityEngine;
using XRL;

namespace Assets.Game.UI.Windows.Stage;

public class MovableSceneFrameDockArea : MonoBehaviour
{
	[Serializable]
	public class RectTransformData
	{
		public Vector3 LocalPosition;

		public Vector2 AnchoredPosition;

		public Vector2 SizeDelta;

		public Vector2 AnchorMin;

		public Vector2 AnchorMax;

		public Vector2 Pivot;

		public Vector3 Scale;

		public Quaternion Rotation;

		public static RectTransformData PullFromTransform(RectTransform transform)
		{
			return new RectTransformData
			{
				LocalPosition = transform.localPosition,
				AnchorMin = transform.anchorMin,
				AnchorMax = transform.anchorMax,
				Pivot = transform.pivot,
				AnchoredPosition = transform.anchoredPosition,
				SizeDelta = transform.sizeDelta,
				Rotation = transform.localRotation,
				Scale = transform.localScale
			};
		}

		public void PushToTransform(RectTransform transform)
		{
			transform.localPosition = LocalPosition;
			transform.anchorMin = AnchorMin;
			transform.anchorMax = AnchorMax;
			transform.pivot = Pivot;
			transform.anchoredPosition = AnchoredPosition;
			transform.sizeDelta = SizeDelta;
			transform.localRotation = Rotation;
			transform.localScale = Scale;
		}
	}

	public GameObject contentFrame;

	public RectTransform content;

	public RectTransform ours;

	public RectTransform oldParent;

	public RectTransformData oldTransform;

	public void Update()
	{
		if (ours == null)
		{
			ours = GetComponent<RectTransform>();
		}
		if (content == null)
		{
			return;
		}
		if (GameManager.Instance.DockMovable > 0 && (bool)GameManager.MainCameraLetterbox)
		{
			if (content.parent != ours.parent)
			{
				oldParent = content.parent as RectTransform;
				oldTransform = RectTransformData.PullFromTransform(content);
				content.SetParent(ours.parent, worldPositionStays: false);
				contentFrame.SetActive(value: false);
			}
			content.CopyFrom(ours);
			if (content.pivot.x != 0f || content.pivot.y != 1f)
			{
				content.pivot = new Vector2(0f, 1f);
			}
		}
		else if (oldParent != null)
		{
			if (content.GetComponent<Canvas>() != null)
			{
				content.GetComponent<Canvas>().enabled = true;
			}
			content.transform.SetParent(oldParent, worldPositionStays: false);
			oldTransform.PushToTransform(content);
			contentFrame.SetActive(value: true);
			oldParent = null;
		}
	}

	public void LateUpdate()
	{
	}
}
