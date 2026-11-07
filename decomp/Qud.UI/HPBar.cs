using System;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

namespace Qud.UI;

[ExecuteAlways]
public class HPBar : MonoBehaviour
{
	public RectTransform Bar;

	public Color BarColor;

	public int BarStart;

	public int BarEnd = 100;

	public int BarValue = 50;

	public bool WantsUpdate;

	public UITextSkin text;

	public void UpdateBar()
	{
		Bar.anchorMax = new Vector2(Math.Min(1f, (float)(BarValue - BarStart) / (float)(BarEnd - BarStart)), 1f);
		Bar.GetComponent<Image>().color = BarColor;
	}

	public void SetText(string t)
	{
		text.SetText(t);
	}

	public void Update()
	{
		if (WantsUpdate)
		{
			UpdateBar();
			WantsUpdate = false;
		}
	}
}
