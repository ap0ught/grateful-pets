using System;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class TradeLineData : PooledFrameworkDataElement<TradeLineData>
{
	public TradeLineDataType type;

	public TradeLineDataStyle style;

	public GameObject go;

	public bool traderInventory;

	public TradeEntry entry;

	public int numberSelected;

	public string category;

	public int numberInCategory;

	public bool collapsed;

	public bool indent;

	public char quickKey;

	public string hotkeyDescription;

	public int side;

	public Action update;

	public TradeLineData set(int side = 0, TradeLineDataType type = TradeLineDataType.Item, TradeLineDataStyle style = TradeLineDataStyle.Interact, GameObject go = null, TradeEntry entry = null, bool traderInventory = false, int numberSelected = 0, string category = null, bool collapsed = false, bool indent = false, char quickKey = '\0', string hotkeyDescription = null)
	{
		this.side = side;
		this.type = type;
		this.style = style;
		this.go = go;
		this.category = category;
		this.collapsed = collapsed;
		this.indent = indent;
		this.quickKey = quickKey;
		this.hotkeyDescription = hotkeyDescription;
		this.traderInventory = traderInventory;
		this.numberSelected = numberSelected;
		this.entry = entry;
		return this;
	}

	public override void free()
	{
		update = null;
		entry = null;
		go = null;
		base.free();
	}
}
