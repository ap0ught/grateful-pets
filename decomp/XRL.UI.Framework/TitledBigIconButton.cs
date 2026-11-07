using System;
using Kobold;
using Qud.UI;
using UnityEngine;

namespace XRL.UI.Framework;

public class TitledBigIconButton : MonoBehaviour, IFrameworkControl
{
	public void setData(FrameworkDataElement data)
	{
		if (!(data is ChoiceWithColorIcon choiceWithColorIcon))
		{
			throw new ArgumentException("TitledBigIconButton expected ChoiceWithColorIcon data");
		}
		ImageTinyFrame imageTinyFrame = GetComponent<ImageTinyFrame>() ?? GetComponent<TitledIconButton>().ImageTinyFrame;
		GetComponent<TitledIconButton>().SetTitle(choiceWithColorIcon.Title + (string.IsNullOrEmpty(choiceWithColorIcon.Hotkey) ? "" : ("\n" + choiceWithColorIcon.Hotkey)));
		imageTinyFrame.sprite = SpriteManager.GetUnitySprite(choiceWithColorIcon.IconPath);
		if ((bool)imageTinyFrame.ThreeColor)
		{
			imageTinyFrame.ThreeColor.SetHFlip(choiceWithColorIcon.HFlip);
			imageTinyFrame.ThreeColor.SetVFlip(choiceWithColorIcon.VFlip);
		}
		imageTinyFrame.unselectedBorderColor = (choiceWithColorIcon.IsChosen() ? The.Color.Cyan : The.Color.Black);
		imageTinyFrame.selectedBorderColor = (choiceWithColorIcon.IsChosen() ? The.Color.Yellow : The.Color.Yellow);
		imageTinyFrame.selectedForegroundColor = choiceWithColorIcon.IconForegroundColor;
		imageTinyFrame.selectedDetailColor = choiceWithColorIcon.IconDetailColor;
		imageTinyFrame.Sync(force: true);
	}

	public NavigationContext GetNavigationContext()
	{
		return GetComponent<FrameworkContext>()?.context;
	}
}
