using System.Collections.Generic;
using ConsoleLib.Console;
using Kobold;
using Qud.API;
using Qud.UI;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;

public class SaveManagementRow : MonoBehaviour, IFrameworkControl
{
	public ImageTinyFrame imageTinyFrame;

	public List<UITextSkin> TextSkins;

	public Image background;

	public GameObject modsDiffer;

	public FrameworkContext deleteButton;

	private FrameworkContext _context;

	private bool? wasSelected;

	public FrameworkContext context => _context ?? (_context = GetComponent<FrameworkContext>());

	public void setData(FrameworkDataElement data)
	{
		FrameworkContext frameworkContext = deleteButton;
		if (frameworkContext.context == null)
		{
			NavigationContext navigationContext = (frameworkContext.context = new NavigationContext());
		}
		deleteButton.context.parentContext = context.context;
		if (data is SaveInfoData { SaveGame: var saveGame } saveInfoData)
		{
			SaveGameJSON saveGameJSON = saveGame?.json;
			ImageTinyFrame imageTinyFrame = this.imageTinyFrame;
			if (saveGameJSON != null)
			{
				imageTinyFrame.sprite = SpriteManager.GetUnitySprite(saveGameJSON.CharIcon);
				imageTinyFrame.unselectedBorderColor = The.Color.Black;
				imageTinyFrame.selectedBorderColor = The.Color.Yellow;
				imageTinyFrame.unselectedForegroundColor = The.Color.Black;
				imageTinyFrame.unselectedDetailColor = The.Color.Black;
				imageTinyFrame.selectedForegroundColor = (ConsoleLib.Console.ColorUtility.ColorMap.TryGetValue(saveGameJSON.FColor, out var value) ? value : The.Color.Gray);
				imageTinyFrame.selectedDetailColor = (ConsoleLib.Console.ColorUtility.ColorMap.TryGetValue(saveGameJSON.DColor, out var value2) ? value2 : The.Color.DarkBlack);
			}
			else
			{
				imageTinyFrame.sprite = SpriteManager.GetUnitySprite("Text/32.bmp");
				imageTinyFrame.unselectedBorderColor = The.Color.Black;
				imageTinyFrame.selectedBorderColor = The.Color.Yellow;
				imageTinyFrame.unselectedForegroundColor = Color.clear;
				imageTinyFrame.unselectedDetailColor = Color.clear;
				imageTinyFrame.selectedForegroundColor = Color.clear;
				imageTinyFrame.selectedDetailColor = Color.clear;
			}
			if ((bool)imageTinyFrame.ThreeColor)
			{
				imageTinyFrame.ThreeColor.SetHFlip(Value: true);
			}
			imageTinyFrame.Sync(force: true);
			TextSkins[0].SetText("{{W|" + saveInfoData.SaveGame.Name + " :: " + saveInfoData.SaveGame.Description + " }}");
			TextSkins[1].SetText("{{C|Location:}} " + saveInfoData.SaveGame.Info);
			TextSkins[2].SetText("{{C|Last saved:}} " + saveInfoData.SaveGame.SaveTime);
			TextSkins[3].SetText("{{K|" + saveInfoData.SaveGame.Size + " {" + saveInfoData.SaveGame.ID + "} }}");
			modsDiffer.SetActive(saveInfoData.SaveGame.DifferentMods());
			wasSelected = null;
			Update();
		}
	}

	public void Update()
	{
		bool valueOrDefault = context?.context?.IsActive() == true;
		if (valueOrDefault != wasSelected)
		{
			wasSelected = valueOrDefault;
			deleteButton.gameObject.SetActive(valueOrDefault);
			Color darkCyan = The.Color.DarkCyan;
			darkCyan.a = (valueOrDefault ? 0.25f : 0f);
			background.color = darkCyan;
			bool flag = true;
			foreach (UITextSkin textSkin in TextSkins)
			{
				if (valueOrDefault)
				{
					textSkin.color = The.Color.Gray;
					textSkin.StripFormatting = false;
				}
				else
				{
					textSkin.color = (flag ? The.Color.DarkCyan : The.Color.Black);
					textSkin.StripFormatting = true;
				}
				textSkin.Apply();
				flag = false;
			}
		}
		if (valueOrDefault && ControlManager.GetButtonDown("CmdDelete"))
		{
			deleteButton?.context.commandHandlers["CmdDelete"]();
		}
	}

	public void handleDelete()
	{
		deleteButton?.context.buttonHandlers[InputButtonTypes.AcceptButton]();
	}

	public NavigationContext GetNavigationContext()
	{
		return null;
	}
}
