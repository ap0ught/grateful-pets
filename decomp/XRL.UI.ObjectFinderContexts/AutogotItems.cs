using System.Collections.Generic;
using System.Linq;
using XRL.Core;
using XRL.World;

namespace XRL.UI.ObjectFinderContexts;

public class AutogotItems : ObjectFinder.Context
{
	public AutogotItems()
	{
		GameManager.Instance.gameQueue.queueSingletonTask("AutogotItemsInit", () =>
		{
			UpdateItems(The.Core);
		});
	}

	public override string GetDisplayName()
	{
		return "Nearby Items";
	}

	public override void Enable()
	{
		XRLCore.RegisterOnBeginPlayerTurnCallback(UpdateItems);
		XRLCore.RegisterOnEndPlayerTurnCallback(UpdateItems, Single: true);
	}

	public override void Disable()
	{
		XRLCore.RemoveOnBeginPlayerTurnCallback(UpdateItems);
		XRLCore.RemoveOnEndPlayerTurnCallback(UpdateItems, Single: true);
	}

	public void UpdateItems(XRLCore core)
	{
		finder?.UpdateContext(this, Sidebar.AutogotItems.Keys);
	}

	private IEnumerable<GameObject> GetObjectsFromCell(Cell c)
	{
		return c.Objects.Where((GameObject o) => o.IsVisible());
	}
}
