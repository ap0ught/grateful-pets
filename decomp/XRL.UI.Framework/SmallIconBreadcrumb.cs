using System;
using Kobold;
using Qud.UI;
using UnityEngine;

namespace XRL.UI.Framework;

public class SmallIconBreadcrumb : MonoBehaviour, IFrameworkControl
{
	public void setData(FrameworkDataElement data)
	{
		if (!(data is ChoiceWithColorIcon choiceWithColorIcon))
		{
			throw new ArgumentException("TitledBigIconButton expected ChoiceWithColorIcon data");
		}
		ImageTinyFrame imageTinyFrame = GetComponent<ImageTinyFrame>() ?? GetComponent<TitledIconButton>().ImageTinyFrame;
		GetComponent<TitledIconButton>().SetTitle(choiceWithColorIcon.Title);
		if (string.IsNullOrEmpty(choiceWithColorIcon.IconPath))
		{
			imageTinyFrame.sprite = null;
		}
		else
		{
			imageTinyFrame.sprite = SpriteManager.GetUnitySprite(choiceWithColorIcon.IconPath);
		}
		if ((bool)imageTinyFrame.ThreeColor)
		{
			imageTinyFrame.ThreeColor.SetHFlip(choiceWithColorIcon.HFlip);
			imageTinyFrame.ThreeColor.SetVFlip(choiceWithColorIcon.VFlip);
		}
		imageTinyFrame.selectedForegroundColor = choiceWithColorIcon.IconForegroundColor;
		imageTinyFrame.selectedDetailColor = choiceWithColorIcon.IconDetailColor;
		imageTinyFrame.Sync();
	}

	public NavigationContext GetNavigationContext()
	{
		return GetComponent<FrameworkContext>()?.context;
	}
}
