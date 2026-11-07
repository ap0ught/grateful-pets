using XRL.Core;

namespace XRL.UI;

public class InventoryCategory
{
	public string Name = "";

	public int Weight;

	public int Items;

	private string StateKey;

	private bool _Expanded = true;

	public bool Expanded
	{
		get
		{
			return _Expanded;
		}
		set
		{
			_Expanded = value;
			if (StateKey != null)
			{
				XRLCore.Core.Game.SetBooleanGameState(StateKey, value);
			}
		}
	}

	public InventoryCategory(string Name, bool Persist = false)
	{
		this.Name = Name;
		if (Persist)
		{
			StateKey = "ExpandState" + Name;
			_Expanded = XRLCore.Core.Game.GetBooleanGameState(StateKey, Default: true);
		}
	}
}
