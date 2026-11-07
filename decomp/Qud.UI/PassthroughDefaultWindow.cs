using XRL.UI;

namespace Qud.UI;

[UIView("PassthroughDefault", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "PassthroughDefault", UICanvasHost = 1)]
public class PassthroughDefaultWindow : SingletonWindowBase<StageWindow>
{
	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public void Start()
	{
	}
}
