using Qud.UI;
using XRL.UI;

[UIView("BrowseBlueprintsView", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "BrowseBlueprintsView", UICanvasHost = 1)]
public class BrowseBlueprintsView : SingletonWindowBase<BrowseBlueprintsView>
{
	public string mode = "old";

	public override void Show()
	{
		mode = "old";
		base.Show();
	}

	public override void Hide()
	{
		_ = mode == "new";
		base.Hide();
	}

	public void OnCommand(string Command)
	{
		if (Command == "Back")
		{
			UIManager.getWindow("BrowseBlueprintsView").Hide();
			UIManager.showWindow("MainMenu");
		}
	}
}
