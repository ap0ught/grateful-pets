using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Qud.UI;
using QupKit;
using SFB;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL;
using XRL.UI;

[UIView("SteamWorkshopUploaderView", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "SteamWorkshopUploader", UICanvasHost = 1)]
public class SteamWorkshopUploaderView : SingletonWindowBase<SteamWorkshopUploaderView>
{
	private ModScrollerController modScrollerController;

	private ModInfo currentMod;

	private Action UpdateAction;

	protected CallResult<CreateItemResult_t> m_itemCreated;

	protected CallResult<SubmitItemUpdateResult_t> m_itemSubmitted;

	private UGCUpdateHandle_t currentHandle = UGCUpdateHandle_t.Invalid;

	private string _tempPath;

	public Toggle uploadHiddenFilesToggle;

	public bool bFilter;

	private GameObject rootObject => gameObject;

	public bool uploadHiddenFiles
	{
		get
		{
			return uploadHiddenFilesToggle.isOn;
		}
		set
		{
			uploadHiddenFilesToggle.isOn = value;
		}
	}

	public static void OpenInWinFileBrowser(string path)
	{
		bool flag = false;
		string text = path.Replace("/", "\\");
		if (Directory.Exists(text))
		{
			flag = true;
		}
		try
		{
			Process.Start("explorer.exe", (flag ? "/root," : "/select,") + text);
		}
		catch (Win32Exception ex)
		{
			ex.HelpLink = "";
		}
	}

	public void ShowProgress(string Text)
	{
		rootObject.transform.Find("ProgressPanel").gameObject.SetActive(value: true);
		rootObject.transform.Find("ProgressPanel/ProgressLabel").GetComponent<UnityEngine.UI.Text>().text = Text;
		rootObject.transform.Find("ProgressPanel/Button").gameObject.SetActive(value: false);
	}

	public void SetProgress(string Text, float Progress)
	{
		rootObject.transform.Find("ProgressPanel/ProgressLabel").GetComponent<UnityEngine.UI.Text>().text = Text;
		if (Progress >= 100f)
		{
			rootObject.transform.Find("ProgressPanel/Button").gameObject.SetActive(value: true);
		}
	}

	public void ClearProgress()
	{
		rootObject.transform.Find("ProgressPanel").gameObject.SetActive(value: false);
	}

	public void SetModInfo(ModInfo info)
	{
		currentMod = info;
		if (info.WorkshopInfo == null)
		{
			rootObject.transform.Find("CreatePanel").gameObject.SetActive(value: true);
			rootObject.transform.Find("DetailsPanel").gameObject.SetActive(value: false);
		}
		else
		{
			rootObject.transform.Find("CreatePanel").gameObject.SetActive(value: false);
			rootObject.transform.Find("DetailsPanel").gameObject.SetActive(value: true);
			if (currentMod.WorkshopInfo.Title != null)
			{
				rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/TitleField/Field").GetComponent<TMP_InputField>().text = currentMod.WorkshopInfo.Title;
			}
			if (currentMod.WorkshopInfo.Description != null)
			{
				rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/DescriptionField/Field").GetComponent<TMP_InputField>().text = currentMod.WorkshopInfo.Description;
			}
			if (currentMod.WorkshopInfo.Tags != null)
			{
				rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/TagsField/Field").GetComponent<TMP_InputField>().text = string.Join(',', currentMod.WorkshopInfo.Tags);
			}
			if (!string.IsNullOrEmpty(currentMod.WorkshopInfo.Visibility))
			{
				rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/Visibility").GetComponent<Dropdown>().value = Convert.ToInt32(currentMod.WorkshopInfo.Visibility);
			}
		}
		rootObject.transform.Find("SelectedModLabel").GetComponent<UITextSkin>().SetText("Managing - " + info.ID);
		UpdatePreview();
	}

	public override void Show()
	{
		base.Show();
		EventSystem.current.SetSelectedGameObject(rootObject.transform.Find("Mod Scroller").gameObject);
		modScrollerController = GameObject.Find("ModScrollerController").GetComponent<ModScrollerController>();
		modScrollerController.Refresh();
		if (m_itemCreated == null)
		{
			m_itemCreated = CallResult<CreateItemResult_t>.Create(OnItemCreated);
			m_itemSubmitted = CallResult<SubmitItemUpdateResult_t>.Create(OnItemSubmitted);
		}
	}

	public void Popup(string text)
	{
		rootObject.transform.Find("PopupPanel").gameObject.SetActive(value: true);
		rootObject.transform.Find("PopupPanel/Panel/PopupLabel").GetComponent<UnityEngine.UI.Text>().text = text;
	}

	private void OnItemSubmitted(SubmitItemUpdateResult_t callback, bool ioFailure)
	{
		if (ioFailure)
		{
			Popup("Error: I/O Failure! :(");
		}
		else if (callback.m_eResult == EResult.k_EResultOK)
		{
			Popup("SUCCESS! Item submitted!");
			ClearProgress();
		}
		else
		{
			Popup("Unknown result: " + callback.m_eResult);
		}
	}

	private void OnItemCreated(CreateItemResult_t callback, bool ioFailure)
	{
		ClearProgress();
		if (ioFailure)
		{
			Popup("Error: I/O Failure!");
			return;
		}
		switch (callback.m_eResult)
		{
		case EResult.k_EResultInsufficientPrivilege:
			SetProgress("Upload failed", 100f);
			Popup("Error: Unfortunately, you're banned by the community from uploading to the workshop! Bummer.");
			return;
		case EResult.k_EResultTimeout:
			SetProgress("Upload failed", 100f);
			Popup("Error: Timeout");
			return;
		case EResult.k_EResultNotLoggedOn:
			SetProgress("Upload failed", 100f);
			Popup("Error: You're not logged into Steam!");
			return;
		}
		if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			Application.OpenURL("https://steamcommunity.com/workshop/workshoplegalagreement/");
		}
		if (callback.m_eResult != EResult.k_EResultOK)
		{
			return;
		}
		SetProgress("Upload complete!", 100f);
		Popup("Item creation successful! Published Item ID: " + callback.m_nPublishedFileId.ToString());
		UnityEngine.Debug.Log("Item created: Id: " + callback.m_nPublishedFileId.ToString());
		currentMod.InitializeWorkshopInfo(callback.m_nPublishedFileId.m_PublishedFileId);
		SetModInfo(currentMod);
		if (_tempPath != null)
		{
			if (_tempPath.StartsWith(Path.GetTempPath()) && _tempPath != Path.GetTempPath())
			{
				Directory.Delete(_tempPath, recursive: true);
			}
			_tempPath = null;
		}
	}

	public void Update()
	{
		if (canvas.enabled && ControlManager.isCommandDown("Cancel"))
		{
			OnCommand("Back");
		}
		if (UpdateAction != null)
		{
			UpdateAction();
			UpdateAction = null;
		}
		if (bFilter && !PickFileView.IsShowing())
		{
			bFilter = false;
		}
		else if (currentHandle != UGCUpdateHandle_t.Invalid)
		{
			EItemUpdateStatus itemUpdateProgress = SteamUGC.GetItemUpdateProgress(currentHandle, out var punBytesProcessed, out var punBytesTotal);
			float progress = (float)punBytesProcessed / (float)punBytesTotal;
			switch (itemUpdateProgress)
			{
			case EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges:
				SetProgress("Committing changes...", progress);
				break;
			case EItemUpdateStatus.k_EItemUpdateStatusInvalid:
				SetProgress("Item invalid ... dunno why! :(", 100f);
				break;
			case EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile:
				SetProgress("Uploading preview image...", progress);
				break;
			case EItemUpdateStatus.k_EItemUpdateStatusUploadingContent:
				SetProgress("Uploading content...", progress);
				break;
			case EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig:
				SetProgress("Preparing configuration...", progress);
				break;
			case EItemUpdateStatus.k_EItemUpdateStatusPreparingContent:
				SetProgress("Preparing content...", progress);
				break;
			}
		}
	}

	public void SubmitCurrentMod()
	{
		try
		{
			ShowProgress("Submitting update... Please wait.");
			int value = rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/Visibility").GetComponent<Dropdown>().value;
			currentMod.WorkshopInfo.Title = rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/TitleField/Field").GetComponent<TMP_InputField>().text;
			currentMod.WorkshopInfo.Description = rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/DescriptionField/Field").GetComponent<TMP_InputField>().text;
			currentMod.WorkshopInfo.Tags = rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/TagsField/Field").GetComponent<TMP_InputField>().text?.Split(',');
			currentMod.WorkshopInfo.Visibility = value.ToString();
			currentMod.SaveWorkshopInfo();
			ulong workshopId = currentMod.WorkshopInfo.WorkshopId;
			UGCUpdateHandle_t uGCUpdateHandle_t = SteamUGC.StartItemUpdate(nPublishedFileID: new PublishedFileId_t(workshopId), nConsumerAppId: new AppId_t(333640u));
			SteamUGC.SetItemTitle(uGCUpdateHandle_t, currentMod.WorkshopInfo.Title);
			SteamUGC.SetItemDescription(uGCUpdateHandle_t, currentMod.WorkshopInfo.Description);
			SteamUGC.RemoveItemKeyValueTags(uGCUpdateHandle_t, "manifest_id");
			SteamUGC.AddItemKeyValueTag(uGCUpdateHandle_t, "manifest_id", currentMod.ID);
			SteamUGC.RemoveItemKeyValueTags(uGCUpdateHandle_t, "manifest_version");
			SteamUGC.AddItemKeyValueTag(uGCUpdateHandle_t, "manifest_version", currentMod.Manifest.Version.ToString());
			SteamUGC.SetItemVisibility(uGCUpdateHandle_t, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
			if (value == 0)
			{
				SteamUGC.SetItemVisibility(uGCUpdateHandle_t, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
			}
			if (value == 1)
			{
				SteamUGC.SetItemVisibility(uGCUpdateHandle_t, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly);
			}
			if (value == 2)
			{
				SteamUGC.SetItemVisibility(uGCUpdateHandle_t, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
			}
			if (value == 3)
			{
				SteamUGC.SetItemVisibility(uGCUpdateHandle_t, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityUnlisted);
			}
			if (uploadHiddenFiles)
			{
				SteamUGC.SetItemContent(uGCUpdateHandle_t, currentMod.Path);
			}
			else
			{
				ShowProgress("Copying files for deployment...");
				string path = currentMod.Path;
				string text = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Directory.CreateDirectory(text);
				_tempPath = text;
				string[] files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
				foreach (string text2 in files)
				{
					if (!new FileInfo(text2).Attributes.HasFlag(FileAttributes.Hidden) && !new FileInfo(text2).Attributes.HasFlag(FileAttributes.ReparsePoint))
					{
						File.Copy(text2, text2.Replace(path, text), overwrite: true);
					}
				}
				List<string> list = new List<string>();
				files = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
				foreach (string dirPath in files)
				{
					if (!new DirectoryInfo(dirPath).Attributes.HasFlag(FileAttributes.Hidden) && !new DirectoryInfo(dirPath).Attributes.HasFlag(FileAttributes.ReparsePoint) && !list.Any((string hd) => dirPath.StartsWith(hd)))
					{
						Directory.CreateDirectory(dirPath.Replace(path, text));
						string[] files2 = Directory.GetFiles(dirPath, "*.*", SearchOption.TopDirectoryOnly);
						foreach (string text3 in files2)
						{
							if (!new FileInfo(text3).Attributes.HasFlag(FileAttributes.Hidden) && !new FileInfo(text3).Attributes.HasFlag(FileAttributes.ReparsePoint))
							{
								File.Copy(text3, text3.Replace(path, text), overwrite: true);
							}
						}
					}
					else
					{
						list.Add(dirPath);
					}
				}
				SteamUGC.SetItemContent(uGCUpdateHandle_t, text);
			}
			if (!string.IsNullOrEmpty(currentMod.WorkshopInfo.ImagePath))
			{
				SteamUGC.SetItemPreview(uGCUpdateHandle_t, Path.Combine(currentMod.Path, currentMod.WorkshopInfo.ImagePath));
			}
			SteamUGC.SetItemTags(uGCUpdateHandle_t, currentMod.WorkshopInfo.Tags);
			currentHandle = uGCUpdateHandle_t;
			SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(uGCUpdateHandle_t, rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/ChangelistPanel/ChangelistField/Field").GetComponent<TMP_InputField>().text);
			m_itemSubmitted.Set(hAPICall);
		}
		catch (Exception ex)
		{
			ClearProgress();
			Popup(ex.ToString());
		}
	}

	public void UpdatePreview()
	{
		if (currentMod.WorkshopInfo != null && !string.IsNullOrEmpty(currentMod.WorkshopInfo.ImagePath))
		{
			byte[] data = File.ReadAllBytes(Path.Combine(currentMod.Path, currentMod.WorkshopInfo.ImagePath));
			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(data);
			rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/WorkshopImage").GetComponent<Image>().sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		}
		else
		{
			rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/WorkshopImage").GetComponent<Image>().sprite = null;
		}
	}

	public void SetImage(string Path)
	{
		try
		{
			if (Path != null)
			{
				File.Copy(Path, System.IO.Path.Combine(currentMod.Path, "preview.png"), overwrite: true);
				currentMod.WorkshopInfo.ImagePath = "preview.png";
			}
			UpdatePreview();
			currentMod.SaveWorkshopInfo();
		}
		catch (Exception ex)
		{
			Popup(ex.ToString());
		}
	}

	public void OnCommand(string Command)
	{
		if (PickFileView.IsShowing())
		{
			bFilter = true;
			return;
		}
		switch (Command)
		{
		case "SelectImage":
			if (currentMod == null || currentMod.WorkshopInfo == null)
			{
				return;
			}
			StandaloneFileBrowser.OpenFilePanelAsync("Select Workshop Image", "", "png", multiselect: false, (string[] s) =>
			{
				rootObject.transform.Find("BrowsePanel").gameObject.SetActive(value: false);
				if (s.Length != 0)
				{
					SetImage(s[0]);
				}
				else
				{
					SetImage(null);
				}
			});
			return;
		case "UploadContent":
			if (currentMod != null && currentMod.WorkshopInfo != null)
			{
				SubmitCurrentMod();
			}
			return;
		case "CreateWorkshopItem":
			if (currentMod != null && currentMod.WorkshopInfo == null)
			{
				try
				{
					ShowProgress("Requesting a new Steam Workshop item... Please wait.");
					SteamAPICall_t hAPICall = SteamUGC.CreateItem(new AppId_t(333640u), EWorkshopFileType.k_EWorkshopFileTypeFirst);
					m_itemCreated.Set(hAPICall);
					return;
				}
				catch
				{
					ClearProgress();
					return;
				}
			}
			return;
		case "Back":
			UIManager.getWindow("SteamWorkshopUploader").Hide();
			UIManager.showWindow("MainMenu");
			break;
		}
		if (Command == "RefreshMods")
		{
			modScrollerController.Refresh();
		}
		if (Command == "BrowseMods")
		{
			if (Directory.Exists(Path.Combine(Application.persistentDataPath, "Mods")))
			{
				OpenInWinFileBrowser(Path.Combine(Application.persistentDataPath, "Mods"));
			}
			else
			{
				OpenInWinFileBrowser(Application.persistentDataPath);
			}
		}
	}
}
