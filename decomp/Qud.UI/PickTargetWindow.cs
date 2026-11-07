using XRL.UI;

namespace Qud.UI;

[UIView("PickTargetFrame", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "PickTarget", UICanvasHost = 1)]
public class PickTargetWindow : SingletonWindowBase<PickTargetWindow>
{
	public enum TargetMode
	{
		PickDirection,
		PickField,
		PickCells
	}

	public UITextSkin text;

	public static string currentText;

	public static TargetMode currentMode;

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public void Update()
	{
		if (text.text != currentText)
		{
			text.SetText(currentText);
		}
	}

	public override void Show()
	{
		base.Show();
		if (Options.ModernUI)
		{
			UIManager.getWindow("Stage").Show();
			UIManager.getWindow("PlayerStatusBar").Show();
			UIManager.getWindow("AbilityBar").Show();
			UIManager.getWindow("MessageLog").Hide();
			UIManager.getWindow("NearbyItems").Hide();
			UIManager.getWindow("Minimap").Hide();
		}
		Update();
	}

	public override void Hide()
	{
		base.Hide();
		UIManager.getWindow("Stage").Hide();
		UIManager.getWindow("PlayerStatusBar").Hide();
		UIManager.getWindow("AbilityBar").Hide();
		UIManager.getWindow("MessageLog").Hide();
		UIManager.getWindow("NearbyItems").Hide();
		UIManager.getWindow("Minimap").Hide();
	}
}
