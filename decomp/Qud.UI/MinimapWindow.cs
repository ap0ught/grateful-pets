using UnityEngine;
using XRL;
using XRL.UI;

namespace Qud.UI;

[ExecuteAlways]
[HasGameBasedStaticCache]
[UIView("Minimap", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "Minimap", UICanvasHost = 1)]
public class MinimapWindow : MovableSceneFrameWindowBase<MinimapWindow>
{
	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public void TogglePreferredState()
	{
		Toggle();
		SaveOptions();
	}

	public void SaveOptions()
	{
		Options.SetOption("OptionOverlayMinimap", Visible);
	}

	public void ShowIfEnabled()
	{
		if (GameManager.Instance.DisplayMinimap)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public override void Update()
	{
		if (Application.isPlaying)
		{
			if (GameManager.Instance.DockMovable > 0 && (bool)GameManager.MainCameraLetterbox)
			{
				GameManager.MainCameraLetterbox.GetScale();
			}
			else
			{
				Docked = false;
			}
			base.Update();
		}
	}
}
