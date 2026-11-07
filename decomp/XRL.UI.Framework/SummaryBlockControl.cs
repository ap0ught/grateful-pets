using System;
using Kobold;
using UnityEngine;

namespace XRL.UI.Framework;

public class SummaryBlockControl : MonoBehaviour, IFrameworkControl
{
	public UIThreeColorProperties image;

	public GameObject titleArea;

	public UITextSkin title;

	public UITextSkin text;

	public void setData(FrameworkDataElement data)
	{
		if (!(data is SummaryBlockData summaryBlockData))
		{
			throw new ArgumentException("StartingLocationData expected StartingLocationControl data");
		}
		if (string.IsNullOrEmpty(summaryBlockData.Description))
		{
			text.gameObject.SetActive(value: false);
		}
		else
		{
			text.gameObject.SetActive(value: true);
			text.SetText(summaryBlockData.Description);
		}
		if (string.IsNullOrEmpty(summaryBlockData.Title))
		{
			titleArea.SetActive(value: false);
		}
		else
		{
			titleArea.SetActive(value: true);
			title.SetText("{{W|" + summaryBlockData.Title + "}}");
		}
		if (string.IsNullOrEmpty(summaryBlockData.IconPath))
		{
			image.gameObject.SetActive(value: false);
		}
		else
		{
			image.gameObject.SetActive(value: true);
			image.image.sprite = SpriteManager.GetUnitySprite(summaryBlockData.IconPath);
			image.SetHFlip(Value: true);
			image.SetColors(summaryBlockData.IconForegroundColor, summaryBlockData.IconDetailColor, Color.clear);
		}
		if (GetNavigationContext() != null)
		{
			GetNavigationContext().disabled = true;
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return GetComponent<FrameworkContext>()?.context;
	}
}
