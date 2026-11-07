using System;
using UnityEngine;
using UnityEngine.UI;

public class FlashImage : MonoBehaviour
{
	private Image i;

	private void Start()
	{
	}

	private void Update()
	{
		float f = Time.timeSinceLevelLoad % 0.5f * 2f * MathF.PI * 2f;
		if (i == null)
		{
			i = gameObject.GetComponent<Image>();
		}
		i.color = new Color(i.color.r, i.color.g, i.color.b, (Mathf.Sin(f) + 1f) / 2f);
	}
}
