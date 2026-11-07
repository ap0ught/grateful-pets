using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using XRL;

namespace RedShadow.CommonDialogs;

public class FileDialog : DialogBase
{
	public string Extensions;

	public string DefaultFilename;

	public string DefaultDirectory;

	public string LastDirectory;

	public bool ShowPath = true;

	public bool ShowPlaces = true;

	public bool ShowPreview = true;

	public bool ShowFolders = true;

	public bool ShowHidden;

	public bool ShowInputField = true;

	public bool ShowExtensions = true;

	public bool ShowContextMenu = true;

	public bool OverwriteWarn = true;

	public GameObject ListItemPrefab;

	public PathButton PathButtonPrefab;

	public GameObject PlacePrefab;

	public Sprite UnknownIcon;

	public Sprite DriveIcon;

	public Sprite FolderIcon;

	public Sprite ReturnIcon;

	public Sprite NewFolderIcon;

	public Sprite RenameIcon;

	public Sprite CopyIcon;

	public Sprite PasteIcon;

	public Sprite DeleteIcon;

	public List<FileIcon> Icons = new List<FileIcon>();

	public List<Place> Places = new List<Place>();

	private Text _titleText;

	private ScrollRect _pathScrollRect;

	private GameObject _pathPanel;

	private ScrollRect _placesScrollRect;

	private GameObject _placesPanel;

	private ScrollRect _fileListScrollRect;

	private GameObject _fileListContent;

	private GameObject _previewPanel;

	private InputField _filenameInput;

	private Button _actionButton;

	private Text _actionText;

	private RawImage _previewImage;

	private Text _modifiedDateText;

	private Text _fileSizeText;

	private DragPanel _dragPanel;

	private ResizePanel _resizePanel;

	private Dropdown _extensionsDropDown;

	private Action<string> _callback;

	private FileDialogMode _mode;

	private string _path;

	private readonly List<string> _paths = new List<string>();

	private readonly List<FileListItem> _fileListItems = new List<FileListItem>();

	private FileListItem _selectedItem;

	private string _clipboardPath;

	private const string ROOT_PATH = "%%ROOT%%";

	private const string PERSISTENT_DATA_PATH = "%%PERSISTENT_DATA_PATH%%";

	private const string THUMBS_PATH = "thumbs";

	protected override void Awake()
	{
		base.Awake();
		_titleText = transform.Find("Window/TitleBar/TitleText").gameObject.GetComponent<Text>();
		_pathScrollRect = transform.Find("Window/PathPanelScroll").GetComponent<ScrollRect>();
		_pathPanel = transform.Find("Window/PathPanelScroll/Viewport/PathPanel").gameObject;
		_placesScrollRect = transform.Find("Window/CentralPanel/PlacesPanelScroll").GetComponent<ScrollRect>();
		_placesPanel = transform.Find("Window/CentralPanel/PlacesPanelScroll/Viewport/PlacesPanel").gameObject;
		_fileListContent = transform.Find("Window/CentralPanel/FileListScrollView/Viewport/FileListContent").gameObject;
		_fileListScrollRect = transform.Find("Window/CentralPanel/FileListScrollView").GetComponent<ScrollRect>();
		_previewPanel = transform.Find("Window/CentralPanel/PreviewPanel").gameObject;
		_filenameInput = transform.Find("Window/FilenamePanel/FilenameInput/").gameObject.GetComponent<InputField>();
		_actionButton = transform.Find("Window/FilenamePanel/ActionButton/").gameObject.GetComponent<Button>();
		_actionText = transform.Find("Window/FilenamePanel/ActionButton/Text").gameObject.GetComponent<Text>();
		_previewImage = transform.Find("Window/CentralPanel/PreviewPanel/PreviewParentPanel/RawImage").gameObject.GetComponent<RawImage>();
		_modifiedDateText = transform.Find("Window/CentralPanel/PreviewPanel/DateText").gameObject.GetComponent<Text>();
		_fileSizeText = transform.Find("Window/CentralPanel/PreviewPanel/SizeText").gameObject.GetComponent<Text>();
		_dragPanel = transform.Find("Window/TitleBar").GetComponent<DragPanel>();
		_resizePanel = transform.Find("Window/ResizeZone").GetComponent<ResizePanel>();
		_extensionsDropDown = transform.Find("Window/FilenamePanel/FilenameInput/ExtensionDropDown").GetComponent<Dropdown>();
	}

	public override void Update()
	{
		base.Update();
		if (!isTop())
		{
			return;
		}
		string path = Path.Combine(_path, _filenameInput.text);
		switch (_mode)
		{
		case FileDialogMode.Load:
			_actionButton.interactable = File.Exists(path);
			break;
		case FileDialogMode.Save:
			_actionButton.interactable = !string.IsNullOrEmpty(_filenameInput.text);
			break;
		case FileDialogMode.Path:
			_actionButton.interactable = string.IsNullOrEmpty(_filenameInput.text) || Directory.Exists(path);
			break;
		}
		if (_selectedItem != null && Input.GetKeyDown(KeyCode.Return))
		{
			onDoubleClickFile(_selectedItem);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			int num = _fileListItems.IndexOf(_selectedItem);
			if (_selectedItem == null || num == -1)
			{
				onSingleClickFile(_fileListItems[0]);
			}
			else if (num < _fileListItems.Count - 1)
			{
				onSingleClickFile(_fileListItems[num + 1]);
			}
			DialogUtility.centerOnItem(_fileListContent, _selectedItem.GetComponent<RectTransform>());
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			int num2 = _fileListItems.IndexOf(_selectedItem);
			if (_selectedItem == null || num2 == -1)
			{
				onSingleClickFile(_fileListItems[0]);
			}
			else if (num2 > 0)
			{
				onSingleClickFile(_fileListItems[num2 - 1]);
			}
			DialogUtility.centerOnItem(_fileListContent, _selectedItem.GetComponent<RectTransform>());
		}
	}

	protected override void hide()
	{
		Vector2 position = _dragPanel.getPosition();
		Vector2 size = _resizePanel.getSize();
		PlayerPrefs.SetFloat("FileDialog_X", position.x);
		PlayerPrefs.SetFloat("FileDialog_Y", position.y);
		PlayerPrefs.SetFloat("FileDialog_W", size.x);
		PlayerPrefs.SetFloat("FileDialog_H", size.y);
		PlayerPrefs.Save();
		base.hide();
	}

	protected override void show()
	{
		base.show();
		float x = PlayerPrefs.GetFloat("FileDialog_X", -400f);
		float y = PlayerPrefs.GetFloat("FileDialog_Y", 275f);
		_dragPanel.setPosition(new Vector2(x, y));
		float x2 = PlayerPrefs.GetFloat("FileDialog_W", 680f);
		float y2 = PlayerPrefs.GetFloat("FileDialog_H", 400f);
		_resizePanel.setSize(new Vector2(x2, y2));
	}

	public override void cancel()
	{
		hide();
		_callback(null);
	}

	public void show(FileDialogMode mode, string title, string extensions, Action<string> callback, string defaultFilename = null, string defaultPath = null)
	{
		switch (mode)
		{
		case FileDialogMode.Load:
			StandaloneFileBrowser.OpenFilePanelAsync(title, (defaultPath == null) ? Directory.GetCurrentDirectory() : defaultPath, extensions.Replace("*.", ""), multiselect: false, (string[] s) =>
			{
				if (s.Length != 0)
				{
					callback(s[0]);
				}
				else
				{
					callback(null);
				}
			});
			break;
		case FileDialogMode.Save:
			StandaloneFileBrowser.SaveFilePanelAsync(title, (defaultPath == null) ? Directory.GetCurrentDirectory() : defaultPath, defaultFilename, extensions.Replace("*.", ""), (string s) =>
			{
				callback(s);
			});
			break;
		case FileDialogMode.Path:
			StandaloneFileBrowser.OpenFolderPanelAsync(title, (defaultPath == null) ? Directory.GetCurrentDirectory() : defaultPath, multiselect: false, (string[] s) =>
			{
				if (s.Length != 0)
				{
					callback(s[0]);
				}
				else
				{
					callback(null);
				}
			});
			break;
		}
	}

	public static bool saveThumbnail(string filepath, Texture2D image)
	{
		try
		{
			string text = Path.Combine(Path.GetDirectoryName(filepath), "thumbs");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
				File.SetAttributes(text, File.GetAttributes(text) | FileAttributes.Hidden);
			}
			string fileName = Path.GetFileName(filepath);
			File.WriteAllBytes(Path.Combine(text, fileName + ".png"), image.EncodeToPNG());
			return true;
		}
		catch
		{
			return false;
		}
	}

	public void onModsFolder()
	{
		setPath(DataManager.SavePath("Mods"));
	}

	public void onAction()
	{
		string text = Path.Combine(_path, _filenameInput.text);
		if (_mode == FileDialogMode.Save && File.Exists(text) && OverwriteWarn)
		{
			DialogManager.confirm("Overwrite file?", onOverwriteConfirm);
			return;
		}
		hide();
		_callback(text);
	}

	public void setPath(string path)
	{
		if (path == "%%ROOT%%")
		{
			_path = "%%ROOT%%";
		}
		else if (path == "%%PERSISTENT_DATA_PATH%%")
		{
			_path = Application.persistentDataPath.Replace('/', Path.DirectorySeparatorChar);
		}
		else
		{
			_path = Path.GetFullPath(path);
		}
		LastDirectory = path;
		PlayerPrefs.SetString("FileDialog_LastDirectory", LastDirectory);
		_paths.Clear();
		foreach (Transform item in _pathPanel.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (_path == "%%ROOT%%")
		{
			addPathButton("Drives", "%%ROOT%%", enabled: false);
		}
		else
		{
			string[] array = _path.Split(new char[1] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
			string text = "";
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i];
				string text3 = text;
				string text4 = text2;
				char directorySeparatorChar = Path.DirectorySeparatorChar;
				text = text3 + text4 + directorySeparatorChar;
				string text5 = text2;
				directorySeparatorChar = Path.DirectorySeparatorChar;
				text2 = text5 + directorySeparatorChar;
				addPathButton(text2, text, i < array.Length - 1);
			}
		}
		StartCoroutine(repositionPathScroll_co());
		updateFileList();
	}

	public void updatePlaces()
	{
		foreach (Transform item in _placesPanel.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Place place in Places)
		{
			addPlaceButton(place);
		}
	}

	public void updateFileList()
	{
		_previewImage.texture = null;
		_selectedItem = null;
		_fileListItems.Clear();
		foreach (Transform item in _fileListContent.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (_path == "%%ROOT%%")
		{
			string[] logicalDrives = Environment.GetLogicalDrives();
			Array.Sort(logicalDrives);
			string[] array = logicalDrives;
			foreach (string text in array)
			{
				addFileButton(DriveIcon, text);
			}
		}
		else
		{
			if (ShowFolders || _mode == FileDialogMode.Path)
			{
				string[] array2;
				string[] array;
				try
				{
					string[] directories = Directory.GetDirectories(_path);
					List<string> list = new List<string>();
					array = directories;
					foreach (string text2 in array)
					{
						bool flag = (File.GetAttributes(text2) & FileAttributes.Hidden) == FileAttributes.Hidden;
						if (!(!ShowHidden & flag))
						{
							list.Add(text2);
						}
					}
					array2 = list.ToArray();
				}
				catch (IOException)
				{
					Debug.LogWarning("Unable to access drive " + _path);
					return;
				}
				catch (UnauthorizedAccessException)
				{
					Debug.LogWarning("Access denied to directory " + _path);
					return;
				}
				if (_paths.Count > 1)
				{
					addFileButton(ReturnIcon, "..");
				}
				Array.Sort(array2);
				array = array2;
				foreach (string text3 in array)
				{
					addFileButton(FolderIcon, text3);
				}
			}
			if (_mode != FileDialogMode.Path)
			{
				List<string> list2 = new List<string>();
				string text4 = _extensionsDropDown.options[_extensionsDropDown.value].text;
				string[] array;
				if (text4 == "Known")
				{
					array = Extensions.Split(';');
					foreach (string searchPattern in array)
					{
						list2.AddRange(Directory.GetFiles(_path, searchPattern));
					}
				}
				else if (text4 == "All")
				{
					list2.AddRange(Directory.GetFiles(_path));
				}
				else
				{
					list2.AddRange(Directory.GetFiles(_path, text4));
				}
				string[] array3 = list2.ToArray();
				for (int j = 0; j < array3.Length; j++)
				{
					array3[j] = Path.GetFileName(list2[j]);
				}
				Array.Sort(array3);
				array = array3;
				foreach (string text5 in array)
				{
					bool flag2 = (File.GetAttributes(Path.Combine(_path, text5)) & FileAttributes.Hidden) == FileAttributes.Hidden;
					if (!(!ShowHidden & flag2))
					{
						addFileButton(getSprite(text5), text5);
					}
				}
			}
		}
		StartCoroutine(repositionFileListScroll_co());
	}

	public Sprite getSprite(string filename)
	{
		string text = Path.GetExtension(filename).ToLower();
		foreach (FileIcon icon in Icons)
		{
			string[] array = icon.Extension.Split(new char[2] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == text)
				{
					return icon.Sprite;
				}
			}
		}
		return UnknownIcon;
	}

	private void addPlaceButton(Place place)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(PlacePrefab);
		gameObject.transform.SetParent(_placesPanel.transform, worldPositionStays: false);
		gameObject.transform.Find("Button/Image").GetComponent<Image>().sprite = place.Sprite;
		gameObject.transform.Find("Button/Text").GetComponent<Text>().text = place.Name;
		gameObject.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
		{
			onPlaceClick(place);
		});
	}

	private void addPathButton(string text, string path, bool enabled)
	{
		_paths.Add(path);
		PathButton pathButton = UnityEngine.Object.Instantiate(PathButtonPrefab);
		pathButton.transform.SetParent(_pathPanel.transform, worldPositionStays: false);
		pathButton.Path = path;
		pathButton.transform.Find("Text").GetComponent<Text>().text = text;
		pathButton.GetComponent<Button>().onClick.AddListener(() =>
		{
			setPath(path);
		});
		pathButton.GetComponent<Button>().interactable = enabled;
	}

	private IEnumerator repositionPathScroll_co()
	{
		yield return null;
		LayoutRebuilder.MarkLayoutForRebuild(_pathPanel.GetComponent<RectTransform>());
		yield return new WaitForSeconds(0.05f);
		_pathScrollRect.horizontalNormalizedPosition = 1f;
	}

	private void addFileButton(Sprite icon, string text)
	{
		if (text.StartsWith(_path))
		{
			text = text.Substring(_path.Length);
		}
		text = text.TrimStart(Path.DirectorySeparatorChar);
		text = text.TrimEnd(Path.DirectorySeparatorChar);
		GameObject gameObject = UnityEngine.Object.Instantiate(ListItemPrefab);
		gameObject.transform.SetParent(_fileListContent.transform, worldPositionStays: false);
		gameObject.transform.Find("Image").GetComponent<Image>().sprite = icon;
		if (ShowExtensions)
		{
			gameObject.transform.Find("Text").GetComponent<Text>().text = text;
		}
		else
		{
			gameObject.transform.Find("Text").GetComponent<Text>().text = Path.GetFileNameWithoutExtension(text);
		}
		FileListItem fileListItem = gameObject.GetComponent<FileListItem>();
		fileListItem.Filename = text;
		fileListItem.onDoubleClick.AddListener(() =>
		{
			onDoubleClickFile(fileListItem);
		});
		gameObject.GetComponent<Button>().onClick.AddListener(() =>
		{
			onSingleClickFile(fileListItem);
		});
		if (ShowContextMenu && text != "..")
		{
			fileListItem.onRightClick.AddListener(() =>
			{
				onRightClickFile(fileListItem);
			});
		}
		_fileListItems.Add(fileListItem);
	}

	private IEnumerator repositionFileListScroll_co()
	{
		yield return null;
		LayoutRebuilder.MarkLayoutForRebuild(_fileListContent.GetComponent<RectTransform>());
		yield return new WaitForSeconds(0.05f);
		_fileListScrollRect.normalizedPosition = new Vector2(0f, 1f);
	}

	private void onPlaceClick(Place place)
	{
		if (string.IsNullOrEmpty(place.Path))
		{
			setPath(Environment.GetFolderPath(place.SpecialFolder));
		}
		else
		{
			setPath(place.Path);
		}
	}

	private void onSingleClickFile(FileListItem item)
	{
		_selectedItem = item;
		item.setHighlighted(highlighted: true);
		string fullFileName = getFullFileName(item);
		FileInfo fileInfo = new FileInfo(fullFileName);
		_modifiedDateText.text = "";
		_fileSizeText.text = "";
		_previewImage.texture = null;
		_previewImage.color = new Color(0f, 0f, 0f, 0f);
		if (!fileInfo.Exists)
		{
			return;
		}
		_modifiedDateText.text = fileInfo.LastWriteTime.ToShortDateString() + " " + fileInfo.LastWriteTime.ToShortTimeString();
		if (Directory.Exists(fullFileName))
		{
			if (_mode == FileDialogMode.Path)
			{
				_filenameInput.text = item.Filename;
			}
			return;
		}
		_fileSizeText.text = DialogUtility.prettyPrintSize(fileInfo.Length);
		_filenameInput.text = item.Filename;
		if (ShowPreview)
		{
			StartCoroutine(loadPreview_co(fullFileName));
		}
	}

	private void onDoubleClickFile(FileListItem item)
	{
		string fullFileName = getFullFileName(item);
		FileInfo fileInfo = new FileInfo(fullFileName);
		if (Directory.Exists(fullFileName))
		{
			setPath(fullFileName);
			if (_mode == FileDialogMode.Path)
			{
				_filenameInput.text = "";
			}
		}
		else if (fileInfo.Exists)
		{
			onAction();
		}
	}

	private void onRightClickFile(FileListItem item)
	{
		if (_path == "%%ROOT%%")
		{
			return;
		}
		Menu menu = DialogManager.createMenu();
		menu.transform.SetParent(transform, worldPositionStays: false);
		menu.onClick.AddListener(onMenuItemClick);
		menu.addItem(NewFolderIcon, "New Folder");
		if (item != null)
		{
			_selectedItem = item;
			item.setHighlighted(highlighted: true);
			menu.addSeparator();
			menu.addItem(RenameIcon, "Rename");
			if (!Directory.Exists(getFullFileName(item)))
			{
				menu.addSeparator();
				menu.addItem(CopyIcon, "Copy");
				menu.addItem(PasteIcon, "Paste");
			}
			menu.addSeparator();
			menu.addItem(DeleteIcon, "Delete");
		}
		menu.show(Input.mousePosition);
	}

	private void onMenuItemClick(string text)
	{
		switch (text)
		{
		case "New Folder":
			DialogManager.getString("New Folder Name:", "", onNewFolder);
			return;
		case "Rename":
			DialogManager.getString(Directory.Exists(getFullFileName(_selectedItem)) ? "New Folder Name:" : "New File Name:", _selectedItem.Filename, onRenameFile);
			return;
		case "Copy":
			_clipboardPath = getFullFileName(_selectedItem);
			return;
		case "Paste":
			if (!string.IsNullOrEmpty(_clipboardPath))
			{
				string text2 = Path.Combine(_path, Path.GetFileName(_clipboardPath));
				int num = 0;
				while (File.Exists(text2))
				{
					num++;
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_clipboardPath);
					string extension = Path.GetExtension(_clipboardPath);
					string path = $"{fileNameWithoutExtension} Copy({num}){extension}";
					text2 = Path.Combine(_path, path);
				}
				File.Copy(_clipboardPath, text2);
				updateFileList();
				return;
			}
			break;
		}
		if (text == "Delete")
		{
			DialogManager.confirm(Directory.Exists(getFullFileName(_selectedItem)) ? "Delete Folder?" : "Delete file?", onConfirmDelete);
		}
	}

	public void onNewFolder(string name)
	{
		Directory.CreateDirectory(Path.Combine(_path, name));
		updateFileList();
	}

	public void onRenameFile(string name)
	{
		string fullFileName = getFullFileName(_selectedItem);
		string text = Path.Combine(_path, name);
		if (Directory.Exists(fullFileName))
		{
			Directory.Move(fullFileName, text);
		}
		else
		{
			File.Move(fullFileName, text);
		}
		updateFileList();
	}

	public void onFileListClick(BaseEventData eventData)
	{
		if (((PointerEventData)eventData).button == PointerEventData.InputButton.Right && ShowContextMenu)
		{
			onRightClickFile(null);
		}
	}

	private void onConfirmDelete(Buttons result)
	{
		if (result == Buttons.Yes)
		{
			string fullFileName = getFullFileName(_selectedItem);
			if (Directory.Exists(fullFileName))
			{
				Directory.Delete(fullFileName);
			}
			else
			{
				File.Delete(fullFileName);
			}
			updateFileList();
		}
	}

	private IEnumerator loadPreview_co(string filepath)
	{
		string text = Path.GetExtension(filepath).ToLower();
		bool num = text == ".jpg" || text == ".jpeg" || text == ".png";
		_previewImage.texture = null;
		string text2;
		if (num)
		{
			text2 = filepath;
		}
		else
		{
			string path = Path.Combine(Path.GetDirectoryName(filepath), "thumbs");
			string fileName = Path.GetFileName(filepath);
			text2 = Path.Combine(path, fileName + ".png");
		}
		if (File.Exists(text2))
		{
			using (UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture("file://" + text2))
			{
				unityWebRequest.SendWebRequest();
				_previewImage.texture = DownloadHandlerTexture.GetContent(unityWebRequest);
				_previewImage.color = Color.white;
				DialogUtility.sizeToParent(_previewImage, 2f);
			}
		}
		yield break;
	}

	private void onOverwriteConfirm(Buttons result)
	{
		if (result == Buttons.Yes)
		{
			hide();
			_callback(Path.Combine(_path, _filenameInput.text));
		}
	}

	private string getFullFileName(FileListItem item)
	{
		if (_path == "%%ROOT%%")
		{
			string filename = item.Filename;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			return filename + directorySeparatorChar;
		}
		return Path.Combine(_path, item.Filename);
	}
}
