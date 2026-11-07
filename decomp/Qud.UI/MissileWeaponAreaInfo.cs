using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

namespace Qud.UI;

public class MissileWeaponAreaInfo : MonoBehaviour
{
	public UIThreeColorProperties image;

	public GameObject fullBarPrefab;

	public GameObject barContainer;

	public UITextSkin text;

	public Color fullColor;

	public Color emptyColor;

	public List<Image> bars = new List<Image>();

	public int totalBars;

	private string Tile;

	private string RenderString;

	private ColorChars Colors;

	private bool HFlip;

	private bool VFlip;

	private void UpdateImage(IRenderable Source)
	{
		if (Source == null)
		{
			if (Tile != null)
			{
				Tile = null;
				image.FromRenderable(null);
			}
			return;
		}
		string text = Source.getTile() ?? Source.getRenderString();
		bool hFlip = Source.getHFlip();
		bool vFlip = Source.getVFlip();
		ColorChars colorChars = Source.getColorChars();
		if (!(Tile == text) || !Colors.Equals(colorChars) || HFlip != hFlip || VFlip != vFlip)
		{
			Tile = text;
			Colors = colorChars;
			HFlip = hFlip;
			VFlip = vFlip;
			image.FromRenderable(Source);
		}
	}

	public void UpdateFrom(MissileWeaponArea.MissileWeaponAreaWeaponStatus status)
	{
		bool flag = false;
		UpdateImage(status.renderable);
		if (totalBars != status.displayAmmoTotalBars)
		{
			if (totalBars > status.displayAmmoTotalBars)
			{
				int num = totalBars - status.displayAmmoTotalBars;
				for (int i = 0; i < num; i++)
				{
					Object.Destroy(bars[0].gameObject);
					bars.RemoveAt(0);
				}
			}
			else
			{
				int num2 = status.displayAmmoTotalBars - totalBars;
				for (int j = 0; j < num2; j++)
				{
					Image component = Object.Instantiate(fullBarPrefab).GetComponent<Image>();
					component.gameObject.SetActive(value: true);
					component.transform.SetParent(barContainer.transform, worldPositionStays: false);
					component.transform.SetAsLastSibling();
					bars.Add(component);
				}
			}
			totalBars = status.displayAmmoTotalBars;
			flag = false;
		}
		for (int k = 0; k < totalBars; k++)
		{
			if (k < status.displayAmmoRemainingBars)
			{
				bars[k].color = fullColor;
			}
			else
			{
				bars[k].color = emptyColor;
			}
		}
		if ((status.text == null) ? text.SetText("") : text.SetText(status.text))
		{
			LayoutRebuilder.MarkLayoutForRebuild(gameObject.transform as RectTransform);
			Canvas.ForceUpdateCanvases();
		}
	}
}
