using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using XRL;
using XRL.UI;
using XRL.World.Text;

namespace Qud.UI;

[ExecuteAlways]
[UIView("ModToolkit", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "ModToolkit", UICanvasHost = 1)]
public class ModToolkit : SingletonWindowBase<ModToolkit>
{
	public QudTextMenuController menuController;

	private TaskCompletionSource<bool> menuclosed = new TaskCompletionSource<bool>();

	public override void Hide()
	{
		base.Hide();
		gameObject.SetActive(value: false);
	}

	public override void Show()
	{
		gameObject.SetActive(value: true);
		base.Show();
		menuController.UpdateElements(evenIfNotCurrent: true);
		menuController.Reselect(0);
	}

	public override void Init()
	{
		base.Init();
		if (menuController != null)
		{
			menuController.cancelHandlers.RemoveAllListeners();
			menuController.cancelHandlers.AddListener(OnCancel);
			menuController.activateHandlers.RemoveAllListeners();
			menuController.activateHandlers.AddListener(OnActivate);
			menuController.isCurrentWindow = isCurrentWindow;
		}
	}

	public void OnCancel()
	{
		menuclosed.TrySetResult(result: true);
		Hide();
	}

	public async Task<bool> ShowMenuAsync()
	{
		menuclosed.TrySetCanceled();
		menuclosed = new TaskCompletionSource<bool>();
		await The.UiContext;
		UIManager.showWindow("ModToolkit");
		return await menuclosed.Task;
	}

	public void OnActivate(QudMenuItem data)
	{
		if (data.command == "ModManager")
		{
			ControlManager.ConsumeCurrentInput();
			UIManager.pushWindow("ModManager");
		}
		if (data.command == "MapEditor")
		{
			ControlManager.ConsumeCurrentInput();
			GameManager.Instance.PushGameView("MapEditor");
			Hide();
		}
		if (data.command == "Workshop")
		{
			ControlManager.ConsumeCurrentInput();
			UIManager.getWindow("SteamWorkshopUploader").Show();
			Hide();
		}
		if (data.command == "Wiki")
		{
			ControlManager.ConsumeCurrentInput();
			Application.OpenURL("https://wiki.cavesofqud.com/Modding:Overview");
		}
		if (data.command == "History")
		{
			ControlManager.ConsumeCurrentInput();
			UIManager.getWindow("HistoryTest").Show();
			Hide();
		}
		if (data.command == "Waveform")
		{
			ControlManager.ConsumeCurrentInput();
			UIManager.getWindow("WaveformTest").Show();
			Hide();
		}
		if (data.command == "Blueprint")
		{
			ControlManager.ConsumeCurrentInput();
			UIManager.getWindow("BrowseBlueprintsView").Show();
			Hide();
		}
		if (data.command == "OpenSave")
		{
			ControlManager.ConsumeCurrentInput();
			OpenFolder();
		}
		if (data.command == "csproj")
		{
			ControlManager.ConsumeCurrentInput();
			WriteCSProj();
		}
	}

	public void WriteCSProj()
	{
		string path = DataManager.SavePath("Mods.csproj");
		string path2 = DataManager.FilePath("Mods.csproj.template.txt");
		string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "Managed") + "/");
		string text = File.ReadAllText(path2);
		text = text.Replace("$MANAGED_PATH$", fullPath);
		text = text.Replace("$AUTHORS$", Environment.UserName.Coalesce("Unknown"));
		using (TextBuilder textBuilder = TextBuilder.Get())
		{
			foreach (Assembly item in from x in AppDomain.CurrentDomain.GetAssemblies().Where(ModManager.MainAssemblyPredicate)
				orderby x.GetName().Name
				select x)
			{
				string text2 = Path.Join("$(QudLibPath)", Path.GetRelativePath(fullPath, item.Location));
				textBuilder.Append("\n\t\t<Reference Include=\"" + text2 + "\" />");
			}
			textBuilder.Compound("\n\t", "");
			text = text.Replace("$REFERENCES", textBuilder.ToString());
		}
		File.WriteAllText(path, text);
	}

	public void OpenFolder()
	{
		string text = DataManager.SavePath("").TrimEnd('\\', '/');
		try
		{
			if (Process.Start("open", "\"" + text + "\"") != null)
			{
				return;
			}
		}
		catch (Exception)
		{
		}
		try
		{
			if (Process.Start("xdg-open", "\"" + text + "\"") != null)
			{
				return;
			}
		}
		catch (Exception)
		{
		}
		try
		{
			Process.Start("explorer.exe", "\"" + text.Replace("/", "\\") + "\"");
		}
		catch (Exception)
		{
		}
	}
}
