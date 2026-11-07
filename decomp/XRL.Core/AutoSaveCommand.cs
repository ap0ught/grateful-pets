using XRL.World;

namespace XRL.Core;

public class AutoSaveCommand : ISystemSaveCommand, IActionCommand, IComposite
{
	private static AutoSaveCommand Instance = new AutoSaveCommand();

	public string Type => "AutoSave";

	public static void Issue()
	{
		ActionManager actionManager = The.ActionManager;
		if (!actionManager.HasActionDescendedFrom<ISystemSaveCommand>())
		{
			actionManager.EnqueueAction(Instance);
		}
	}

	public void Execute(XRLGame Game, ActionManager Manager)
	{
		Game.SaveGame("Primary");
	}
}
