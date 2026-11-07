using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileBrowser
{
	public delegate void FinishedCallback(string path);

	public enum PickFileModes
	{
		Select,
		Save
	}

	public static string Pattern = "*.*";

	public static string SaveFileName = "";

	public static PickFileModes Mode = PickFileModes.Select;

	protected FileBrowserType m_browserType;

	public FinishedCallback m_callback;

	protected GUIStyle m_centredText;

	protected string m_currentDirectory;

	protected bool m_currentDirectoryMatches;

	protected string[] m_currentDirectoryParts;

	protected string[] m_directories;

	protected GUIContent[] m_directoriesWithImages;

	protected Texture2D m_directoryImage;

	protected Texture2D m_fileImage;

	protected string m_filePattern;

	protected string[] m_files;

	protected GUIContent[] m_filesWithImages;

	protected string m_name;

	protected string m_newDirectory;

	protected string[] m_nonMatchingDirectories;

	protected GUIContent[] m_nonMatchingDirectoriesWithImages;

	protected string[] m_nonMatchingFiles;

	protected GUIContent[] m_nonMatchingFilesWithImages;

	protected Rect m_screenRect;

	protected Vector2 m_scrollPosition;

	protected int m_selectedDirectory;

	protected int m_selectedFile;

	protected int m_selectedNonMatchingDirectory;

	public GUISkin skin;

	public string CurrentDirectory
	{
		get
		{
			return m_currentDirectory;
		}
		set
		{
			Debug.Log("Setdir->" + value);
			SetNewDirectory(value);
			SwitchDirectoryNow();
		}
	}

	public string SelectionPattern
	{
		get
		{
			return m_filePattern;
		}
		set
		{
			m_filePattern = value;
			ReadDirectoryContents();
		}
	}

	public Texture2D DirectoryImage
	{
		get
		{
			return m_directoryImage;
		}
		set
		{
			m_directoryImage = value;
			BuildContent();
		}
	}

	public Texture2D FileImage
	{
		get
		{
			return m_fileImage;
		}
		set
		{
			m_fileImage = value;
			BuildContent();
		}
	}

	public FileBrowserType BrowserType
	{
		get
		{
			return m_browserType;
		}
		set
		{
			m_browserType = value;
			ReadDirectoryContents();
		}
	}

	protected GUIStyle CentredText
	{
		get
		{
			if (m_centredText == null)
			{
				m_centredText = new GUIStyle(GUI.skin.label);
				m_centredText.alignment = TextAnchor.MiddleLeft;
				m_centredText.fixedHeight = GUI.skin.button.fixedHeight;
			}
			return m_centredText;
		}
	}

	public FileBrowser(Rect screenRect, string name, FinishedCallback callback)
	{
		m_name = name;
		m_screenRect = screenRect;
		m_browserType = FileBrowserType.File;
		m_callback = callback;
		SetNewDirectory(Directory.GetCurrentDirectory());
		SwitchDirectoryNow();
	}

	protected void SetNewDirectory(string directory)
	{
		m_newDirectory = directory;
	}

	protected void SwitchDirectoryNow()
	{
		if (m_newDirectory != null && !(m_currentDirectory == m_newDirectory))
		{
			m_currentDirectory = m_newDirectory;
			m_scrollPosition = Vector2.zero;
			m_selectedDirectory = (m_selectedNonMatchingDirectory = (m_selectedFile = -1));
			ReadDirectoryContents();
		}
	}

	protected void ReadDirectoryContents()
	{
		if (m_currentDirectory == "/")
		{
			m_currentDirectoryParts = new string[1] { "" };
			m_currentDirectoryMatches = false;
		}
		else
		{
			m_currentDirectoryParts = m_currentDirectory.Split(Path.DirectorySeparatorChar);
			if (SelectionPattern != null)
			{
				string directoryName = Path.GetDirectoryName(m_currentDirectory);
				string[] array = new string[0];
				if (directoryName != null)
				{
					array = Directory.GetDirectories(Path.GetDirectoryName(m_currentDirectory), SelectionPattern);
				}
				m_currentDirectoryMatches = Array.IndexOf(array, m_currentDirectory) >= 0;
			}
			else
			{
				m_currentDirectoryMatches = false;
			}
		}
		if (BrowserType == FileBrowserType.File || SelectionPattern == null)
		{
			m_directories = Directory.GetDirectories(m_currentDirectory);
			m_nonMatchingDirectories = new string[0];
		}
		else
		{
			m_directories = Directory.GetDirectories(m_currentDirectory, SelectionPattern);
			List<string> list = new List<string>();
			string[] directories = Directory.GetDirectories(m_currentDirectory);
			foreach (string text in directories)
			{
				if (Array.IndexOf(m_directories, text) < 0)
				{
					list.Add(text);
				}
			}
			m_nonMatchingDirectories = list.ToArray();
			for (int j = 0; j < m_nonMatchingDirectories.Length; j++)
			{
				int num = m_nonMatchingDirectories[j].LastIndexOf(Path.DirectorySeparatorChar);
				m_nonMatchingDirectories[j] = m_nonMatchingDirectories[j].Substring(num + 1);
			}
			Array.Sort(m_nonMatchingDirectories);
		}
		for (int k = 0; k < m_directories.Length; k++)
		{
			m_directories[k] = m_directories[k].Substring(m_directories[k].LastIndexOf(Path.DirectorySeparatorChar) + 1);
		}
		if (BrowserType == FileBrowserType.Directory || SelectionPattern == null)
		{
			m_files = Directory.GetFiles(m_currentDirectory);
			m_nonMatchingFiles = new string[0];
		}
		else
		{
			m_files = Directory.GetFiles(m_currentDirectory, SelectionPattern);
			List<string> list2 = new List<string>();
			string[] directories = Directory.GetFiles(m_currentDirectory);
			foreach (string text2 in directories)
			{
				if (Array.IndexOf(m_files, text2) < 0)
				{
					list2.Add(text2);
				}
			}
			m_nonMatchingFiles = list2.ToArray();
			for (int l = 0; l < m_nonMatchingFiles.Length; l++)
			{
				m_nonMatchingFiles[l] = Path.GetFileName(m_nonMatchingFiles[l]);
			}
			Array.Sort(m_nonMatchingFiles);
		}
		for (int m = 0; m < m_files.Length; m++)
		{
			m_files[m] = Path.GetFileName(m_files[m]);
		}
		Array.Sort(m_files);
		BuildContent();
		m_newDirectory = null;
	}

	protected void BuildContent()
	{
		m_directoriesWithImages = new GUIContent[m_directories.Length];
		for (int i = 0; i < m_directoriesWithImages.Length; i++)
		{
			m_directoriesWithImages[i] = new GUIContent(m_directories[i], DirectoryImage);
		}
		m_nonMatchingDirectoriesWithImages = new GUIContent[m_nonMatchingDirectories.Length];
		for (int j = 0; j < m_nonMatchingDirectoriesWithImages.Length; j++)
		{
			m_nonMatchingDirectoriesWithImages[j] = new GUIContent(m_nonMatchingDirectories[j], DirectoryImage);
		}
		m_filesWithImages = new GUIContent[m_files.Length];
		for (int k = 0; k < m_filesWithImages.Length; k++)
		{
			m_filesWithImages[k] = new GUIContent(m_files[k], FileImage);
		}
		m_nonMatchingFilesWithImages = new GUIContent[m_nonMatchingFiles.Length];
		for (int l = 0; l < m_nonMatchingFilesWithImages.Length; l++)
		{
			m_nonMatchingFilesWithImages[l] = new GUIContent(m_nonMatchingFiles[l], FileImage);
		}
	}

	public void OnGUI()
	{
		if (skin == null)
		{
			skin = Resources.Load("GUISKIN/QudSkin") as GUISkin;
			GUI.skin = skin;
		}
		GUILayout.BeginArea(m_screenRect, m_name, GUI.skin.window);
		GUILayout.BeginHorizontal();
		for (int i = 0; i < m_currentDirectoryParts.Length; i++)
		{
			if (i == m_currentDirectoryParts.Length - 1)
			{
				GUILayout.Label(m_currentDirectoryParts[i], CentredText);
			}
			else if (GUILayout.Button(m_currentDirectoryParts[i]))
			{
				string text = m_currentDirectory;
				for (int num = m_currentDirectoryParts.Length - 1; num > i; num--)
				{
					text = Path.GetDirectoryName(text);
				}
				SetNewDirectory(text);
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box);
		m_selectedDirectory = GUILayoutx.SelectionList(m_selectedDirectory, m_directoriesWithImages, DirectoryDoubleClickCallback);
		if (m_selectedDirectory > -1)
		{
			m_selectedFile = (m_selectedNonMatchingDirectory = -1);
		}
		m_selectedNonMatchingDirectory = GUILayoutx.SelectionList(m_selectedNonMatchingDirectory, m_nonMatchingDirectoriesWithImages, NonMatchingDirectoryDoubleClickCallback);
		if (m_selectedNonMatchingDirectory > -1)
		{
			m_selectedDirectory = (m_selectedFile = -1);
		}
		GUI.enabled = BrowserType == FileBrowserType.File;
		m_selectedFile = GUILayoutx.SelectionList(m_selectedFile, m_filesWithImages, FileDoubleClickCallback);
		GUI.enabled = true;
		if (m_selectedFile > -1)
		{
			m_selectedDirectory = (m_selectedNonMatchingDirectory = -1);
		}
		GUI.enabled = false;
		GUILayoutx.SelectionList(-1, m_nonMatchingFilesWithImages);
		GUI.enabled = true;
		GUILayout.EndScrollView();
		GUILayout.BeginHorizontal();
		string text2 = "Select";
		if (Mode == PickFileModes.Save)
		{
			text2 = "Save";
			SaveFileName = GUILayout.TextArea(SaveFileName, GUILayout.Width(500f));
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Cancel", GUILayout.Width(100f)))
		{
			m_callback(null);
		}
		if (Mode == PickFileModes.Save)
		{
			GUI.enabled = !string.IsNullOrEmpty(SaveFileName);
		}
		else if (BrowserType == FileBrowserType.File)
		{
			GUI.enabled = m_selectedFile > -1;
		}
		else if (SelectionPattern == null)
		{
			GUI.enabled = m_selectedDirectory > -1;
		}
		else
		{
			GUI.enabled = m_selectedDirectory > -1 || (m_currentDirectoryMatches && m_selectedNonMatchingDirectory == -1 && m_selectedFile == -1);
		}
		if (GUILayout.Button(text2, GUILayout.Width(100f)))
		{
			if (Mode == PickFileModes.Save && SaveFileName != null)
			{
				m_callback(Path.Combine(m_currentDirectory, SaveFileName));
			}
			else if (BrowserType == FileBrowserType.File)
			{
				m_callback(Path.Combine(m_currentDirectory, m_files[m_selectedFile]));
			}
			else if (m_selectedDirectory > -1)
			{
				m_callback(Path.Combine(m_currentDirectory, m_directories[m_selectedDirectory]));
			}
			else
			{
				m_callback(m_currentDirectory);
			}
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		if (Event.current.type == EventType.Repaint)
		{
			SwitchDirectoryNow();
		}
	}

	protected void FileDoubleClickCallback(int i)
	{
		if (BrowserType == FileBrowserType.File)
		{
			m_callback(Path.Combine(m_currentDirectory, m_files[i]));
		}
	}

	protected void DirectoryDoubleClickCallback(int i)
	{
		SetNewDirectory(Path.Combine(m_currentDirectory, m_directories[i]));
	}

	protected void NonMatchingDirectoryDoubleClickCallback(int i)
	{
		SetNewDirectory(Path.Combine(m_currentDirectory, m_nonMatchingDirectories[i]));
	}
}
