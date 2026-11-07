using System;
using System.IO;
using UnityEngine;

namespace QupKit;

public class PickFileView : BaseView
{
	private static BaseView PreviousView;

	private new static Action<string> After;

	protected Texture2D m_directoryImage;

	protected Texture2D m_fileImage;

	protected FileBrowser m_fileBrowser;

	protected string m_textPath;

	public static bool _showing;

	public static bool IsShowing()
	{
		return _showing;
	}

	public static void Show(Action<string> _After = null, FileBrowser.PickFileModes _Mode = FileBrowser.PickFileModes.Select, string pattern = "*.rpm")
	{
		PreviousView = LegacyViewManager.Instance.ActiveView;
		LegacyViewManager.Instance.ActiveView = LegacyViewManager.Instance.Views["PickFile"];
		LegacyViewManager.Instance.Views["PickFile"].Enter();
		After = _After;
		FileBrowser.SaveFileName = "";
		FileBrowser.Mode = _Mode;
		FileBrowser.Pattern = pattern;
	}

	public override void OnGUI()
	{
		m_fileBrowser.OnGUI();
	}

	public void Selected(string path)
	{
		PreviousView = null;
		m_fileBrowser = null;
		if (After != null)
		{
			After(path);
			After = null;
		}
		_showing = false;
	}

	public override void Enter()
	{
		_showing = true;
		base.Enter();
		m_fileBrowser = new FileBrowser(new Rect(100f, 100f, 1000f, 500f), "Choose " + FileBrowser.Pattern + " File", Selected);
		m_fileBrowser.SelectionPattern = FileBrowser.Pattern;
		m_fileBrowser.CurrentDirectory = Path.Combine(Application.streamingAssetsPath, "Base").Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
	}

	protected void OnGUIMain()
	{
		m_fileBrowser.OnGUI();
	}
}
