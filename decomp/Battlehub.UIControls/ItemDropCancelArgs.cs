namespace Battlehub.UIControls;

public class ItemDropCancelArgs : ItemDropArgs
{
	public bool Cancel { get; set; }

	public ItemDropCancelArgs(object[] dragItems, object dropTarget, ItemDropAction action, bool isExternal)
		: base(dragItems, dropTarget, action, isExternal)
	{
	}
}
