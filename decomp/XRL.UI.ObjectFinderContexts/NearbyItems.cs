using System.Collections.Generic;
using System.Linq;
using Qud.UI;
using XRL.Core;
using XRL.World;

namespace XRL.UI.ObjectFinderContexts;

public class NearbyItems : ObjectFinder.Context
{
	public NearbyItems()
	{
		GameManager.Instance.gameQueue.queueSingletonTask("NearbyItemsInit", () =>
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
		Cell cell = The.Player?.CurrentCell;
		IEnumerable<GameObject> enumerable = null;
		enumerable = ((!Options.OverlayNearbyObjectsLocal) ? cell?.GetLocalAdjacentCells()?.Prepend(cell).SelectMany(GetObjectsFromCell) : GetObjectsFromCell(cell));
		finder?.UpdateContext(this, enumerable);
		SingletonWindowBase<NearbyItemsWindow>.instance.UpdateGameContext();
	}

	private IEnumerable<GameObject> GetObjectsFromCell(Cell c)
	{
		c.IsSolid();
		return c.Objects.Where((GameObject o) => o.ShouldShowInNearbyItemsList());
	}
}
