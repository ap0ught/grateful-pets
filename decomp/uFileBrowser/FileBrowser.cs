using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace uFileBrowser;

public class FileBrowser : MonoBehaviour
{
	public string defaultPath = "";

	public bool selectDirectory;

	public bool showFiles;

	public bool canCancel = true;

	public string fileFormat = "";

	public GameObject overlay;

	public GameObject window;

	public GameObject fileButtonPrefab;

	public GameObject directoryButtonPrefab;

	public RectTransform fileContent;

	public RectTransform dirContent;

	public InputField currentPathField;

	public InputField searchField;

	public Button searchCancelButton;

	public Button cancelButton;

	public Sprite folderIcon;

	public Sprite defaultIcon;

	public List<FileIcon> fileIcons = new List<FileIcon>();

	[SerializeField]
	[HideInInspector]
	private string currentPath;

	[SerializeField]
	[HideInInspector]
	private string search;

	[SerializeField]
	[HideInInspector]
	private string slash;

	[SerializeField]
	[HideInInspector]
	private List<string> drives;

	private List<FileButton> fileButtons;

	private List<DirectoryButton> dirButtons;

	private int selected = -1;

	private FileBrowserCallback callback;

	public string SelectedPath
	{
		get
		{
			if (selected > -1)
			{
				return fileButtons[selected].fullPath;
			}
			return null;
		}
	}

	private void Awake()
	{
		char directorySeparatorChar = Path.DirectorySeparatorChar;
		slash = directorySeparatorChar.ToString();
		if (Application.platform == RuntimePlatform.Android)
		{
			if (string.IsNullOrEmpty(defaultPath))
			{
				defaultPath = "/storage";
			}
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			drives = new List<string>(Directory.GetLogicalDrives());
		}
		else if ((Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) && string.IsNullOrEmpty(defaultPath))
		{
			defaultPath = "/home/";
		}
	}

	public void Show(FileBrowserCallback callback)
	{
		GotoDirectory(defaultPath);
		UpdateUI();
		this.callback = callback;
		if ((bool)overlay)
		{
			overlay.SetActive(value: true);
		}
		window.SetActive(value: true);
	}

	public void Hide()
	{
		if (selected > -1)
		{
			fileButtons[selected].Unselect();
		}
		currentPath = "";
		selected = -1;
		search = "";
		if ((bool)overlay)
		{
			overlay.SetActive(value: false);
		}
		window.SetActive(value: false);
	}

	public void UpdateUI()
	{
		if ((bool)cancelButton)
		{
			cancelButton.gameObject.SetActive(canCancel);
		}
		currentPathField.text = currentPath;
		searchField.text = search;
	}

	public void OnFileClick(int i)
	{
		if (i >= fileButtons.Count)
		{
			Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
		}
		else if (fileButtons[i].isDir)
		{
			if (!selectDirectory)
			{
				GotoDirectory(fileButtons[i].fullPath);
			}
			else
			{
				SelectFile(i);
			}
		}
		else
		{
			SelectFile(i);
		}
	}

	public void OnDirectoryClick(int i)
	{
		if (i >= dirButtons.Count)
		{
			Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
		}
		else
		{
			GotoDirectory(dirButtons[i].fullPath);
		}
	}

	private void GotoDirectory(string path)
	{
		if (path == currentPath && path != string.Empty)
		{
			return;
		}
		if (string.IsNullOrEmpty(path))
		{
			if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				currentPath = "";
			}
			else if (Application.platform == RuntimePlatform.Android)
			{
				currentPath = "/storage";
			}
			else
			{
				currentPath = "/home/";
			}
		}
		else
		{
			if (!Directory.Exists(path))
			{
				Debug.LogError("uFileBrowser: Directory doesn't exist:\n" + path);
				return;
			}
			currentPath = path;
		}
		if ((bool)currentPathField)
		{
			currentPathField.text = currentPath;
		}
		selected = -1;
		UpdateFileList();
		UpdateDirectoryList();
	}

	private void SelectFile(int i)
	{
		if (i >= fileButtons.Count)
		{
			Debug.LogError("uFileBrowser: Selection index bigger than array.");
		}
		else if (i == selected && selectDirectory && fileButtons[i].isDir)
		{
			GotoDirectory(fileButtons[i].fullPath);
		}
		else if (fileButtons[i].isDir || !selectDirectory)
		{
			if (selected != -1)
			{
				fileButtons[selected].Unselect();
			}
			selected = i;
			fileButtons[i].Select();
		}
	}

	private void UpdateFileList()
	{
		if (fileButtons == null)
		{
			fileButtons = new List<FileButton>();
		}
		else
		{
			for (int i = 0; i < fileButtons.Count; i++)
			{
				UnityEngine.Object.Destroy(fileButtons[i].gameObject);
			}
			fileButtons.Clear();
		}
		if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && string.IsNullOrEmpty(currentPath))
		{
			for (int j = 0; j < drives.Count; j++)
			{
				CreateFileButton(drives[j], drives[j], dir: true, j);
			}
			return;
		}
		List<string> list = new List<string>();
		if ((selectDirectory && showFiles) || !selectDirectory)
		{
			try
			{
				list = new List<string>(Directory.GetFiles(currentPath));
			}
			catch (Exception ex)
			{
				Debug.LogError("uFileBrowser: " + ex.Message);
			}
			FilterFormat(list);
		}
		List<string> list2 = new List<string>();
		try
		{
			list2 = new List<string>(Directory.GetDirectories(currentPath));
		}
		catch (Exception ex2)
		{
			Debug.LogError("uFileBrowser: " + ex2.Message);
		}
		for (int k = 0; k < list2.Count; k++)
		{
			string text = list2[k].Substring(list2[k].LastIndexOf(slash) + 1);
			CreateFileButton(text, list2[k], dir: true, fileButtons.Count);
		}
		for (int l = 0; l < list.Count; l++)
		{
			string text2 = list[l].Substring(list[l].LastIndexOf(slash) + 1);
			CreateFileButton(text2, list[l], dir: false, fileButtons.Count);
		}
	}

	private void UpdateDirectoryList()
	{
		if (!directoryButtonPrefab)
		{
			return;
		}
		if (dirButtons == null)
		{
			dirButtons = new List<DirectoryButton>();
		}
		else
		{
			for (int i = 0; i < dirButtons.Count; i++)
			{
				UnityEngine.Object.Destroy(dirButtons[i].gameObject);
			}
			dirButtons.Clear();
		}
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			CreateDirectoryButton("My Computer", "", 0);
		}
		else
		{
			CreateDirectoryButton(slash, slash, 0);
		}
		if (string.IsNullOrEmpty(currentPath))
		{
			return;
		}
		string[] array = currentPath.Split(slash[0]);
		for (int j = 0; j < array.Length; j++)
		{
			if (!string.IsNullOrEmpty(array[j]))
			{
				string text = currentPath.Substring(0, currentPath.LastIndexOf(array[j]));
				CreateDirectoryButton(array[j] + slash, text + array[j] + slash, dirButtons.Count);
			}
		}
	}

	private void FilterFormat(List<string> files)
	{
		if (string.IsNullOrEmpty(fileFormat))
		{
			return;
		}
		string[] array = fileFormat.Split('|');
		for (int i = 0; i < files.Count; i++)
		{
			bool flag = true;
			string text = "";
			if (files[i].Contains("."))
			{
				text = files[i].Substring(files[i].LastIndexOf('.') + 1).ToLowerInvariant();
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (text == array[j].Trim().ToLowerInvariant())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				files.RemoveAt(i);
				i--;
			}
		}
	}

	private void FilterList()
	{
		if (string.IsNullOrEmpty(search))
		{
			for (int i = 0; i < fileButtons.Count; i++)
			{
				fileButtons[i].gameObject.SetActive(value: true);
			}
			return;
		}
		for (int j = 0; j < fileButtons.Count; j++)
		{
			if (!fileButtons[j].text.ToLowerInvariant().Contains(search))
			{
				fileButtons[j].gameObject.SetActive(value: false);
			}
			else
			{
				fileButtons[j].gameObject.SetActive(value: true);
			}
		}
	}

	private void CreateFileButton(string text, string path, bool dir, int i)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(fileButtonPrefab, Vector3.zero, Quaternion.identity);
		gameObject.GetComponent<RectTransform>().SetParent(fileContent, worldPositionStays: false);
		FileButton component = gameObject.GetComponent<FileButton>();
		component.Set(this, text, path, dir, i);
		fileButtons.Add(component);
	}

	private void CreateDirectoryButton(string text, string path, int i)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(directoryButtonPrefab, Vector3.zero, Quaternion.identity);
		gameObject.GetComponent<RectTransform>().SetParent(dirContent, worldPositionStays: false);
		DirectoryButton component = gameObject.GetComponent<DirectoryButton>();
		component.Set(this, text, path, i);
		dirButtons.Add(component);
	}

	public void PathFieldEndEdit()
	{
		if (Directory.Exists(currentPathField.text))
		{
			GotoDirectory(currentPathField.text);
		}
		else
		{
			currentPathField.text = currentPath;
		}
	}

	public void SearchChanged()
	{
		if ((bool)searchField)
		{
			search = searchField.text.Trim();
			FilterList();
		}
	}

	public void SearchCancelClick()
	{
		search = "";
		searchField.text = "";
		FilterList();
	}

	public void SelectButtonClicked()
	{
		if (selected > -1 && ((fileButtons[selected].isDir && selectDirectory) || (!fileButtons[selected].isDir && !selectDirectory)))
		{
			callback(fileButtons[selected].fullPath);
			Hide();
		}
	}

	public void CancelButtonClicked()
	{
		if (canCancel)
		{
			if (callback != null)
			{
				callback("");
			}
			Hide();
		}
	}

	public Sprite GetFileIcon(string path)
	{
		string text = "";
		if (path.Contains("."))
		{
			text = path.Substring(path.LastIndexOf('.') + 1);
			for (int i = 0; i < fileIcons.Count; i++)
			{
				if (fileIcons[i].extension == text)
				{
					return fileIcons[i].icon;
				}
			}
			return defaultIcon;
		}
		return defaultIcon;
	}
}
