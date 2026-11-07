using System;
using System.Runtime.InteropServices;

namespace SFB;

public class StandaloneFileBrowserLinux : IStandaloneFileBrowser
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate void AsyncCallback(string path);

	private static Action<string[]> _openFileCb;

	private static Action<string[]> _openFolderCb;

	private static Action<string> _saveFileCb;

	[DllImport("StandaloneFileBrowser")]
	private static extern void DialogInit();

	[DllImport("StandaloneFileBrowser")]
	private static extern IntPtr DialogOpenFilePanel(string title, string directory, string extension, bool multiselect);

	[DllImport("StandaloneFileBrowser")]
	private static extern void DialogOpenFilePanelAsync(string title, string directory, string extension, bool multiselect, AsyncCallback callback);

	[DllImport("StandaloneFileBrowser")]
	private static extern IntPtr DialogOpenFolderPanel(string title, string directory, bool multiselect);

	[DllImport("StandaloneFileBrowser")]
	private static extern void DialogOpenFolderPanelAsync(string title, string directory, bool multiselect, AsyncCallback callback);

	[DllImport("StandaloneFileBrowser")]
	private static extern IntPtr DialogSaveFilePanel(string title, string directory, string defaultName, string extension);

	[DllImport("StandaloneFileBrowser")]
	private static extern void DialogSaveFilePanelAsync(string title, string directory, string defaultName, string extension, AsyncCallback callback);

	public StandaloneFileBrowserLinux()
	{
		DialogInit();
	}

	public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
	{
		return Marshal.PtrToStringAnsi(DialogOpenFilePanel(title, directory, GetFilterFromFileExtensionList(extensions), multiselect)).Split('\u001c');
	}

	public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb)
	{
		_openFileCb = cb;
		DialogOpenFilePanelAsync(title, directory, GetFilterFromFileExtensionList(extensions), multiselect, (string result) =>
		{
			_openFileCb(result.Split('\u001c'));
		});
	}

	public string[] OpenFolderPanel(string title, string directory, bool multiselect)
	{
		return Marshal.PtrToStringAnsi(DialogOpenFolderPanel(title, directory, multiselect)).Split('\u001c');
	}

	public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb)
	{
		_openFolderCb = cb;
		DialogOpenFolderPanelAsync(title, directory, multiselect, (string result) =>
		{
			_openFolderCb(result.Split('\u001c'));
		});
	}

	public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
	{
		return Marshal.PtrToStringAnsi(DialogSaveFilePanel(title, directory, defaultName, GetFilterFromFileExtensionList(extensions)));
	}

	public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb)
	{
		_saveFileCb = cb;
		DialogSaveFilePanelAsync(title, directory, defaultName, GetFilterFromFileExtensionList(extensions), (string result) =>
		{
			_saveFileCb(result);
		});
	}

	private static string GetFilterFromFileExtensionList(ExtensionFilter[] extensions)
	{
		if (extensions == null)
		{
			return "";
		}
		string text = "";
		for (int i = 0; i < extensions.Length; i++)
		{
			ExtensionFilter extensionFilter = extensions[i];
			text = text + extensionFilter.Name + ";";
			string[] extensions2 = extensionFilter.Extensions;
			foreach (string text2 in extensions2)
			{
				text = text + text2 + ",";
			}
			text = text.Remove(text.Length - 1);
			text += "|";
		}
		return text.Remove(text.Length - 1);
	}
}
