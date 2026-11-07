using System;
using UnityEngine;
using UnityEngine.UI;

public class TotalWeightPanelBehavior : MonoBehaviour
{
	public Text text;

	public RectTransform progress;

	public RectTransform progressFrame;

	public void SetWeight(int current, int max)
	{
		float num = Math.Min((float)current / (float)max, 1f);
		progress.sizeDelta = new Vector2(progressFrame.sizeDelta.x * num, progressFrame.sizeDelta.y);
		text.text = current + " / " + max + " lbs.";
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
