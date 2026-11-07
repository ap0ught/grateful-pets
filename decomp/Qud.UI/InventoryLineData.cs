using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class InventoryLineData : PooledFrameworkDataElement<InventoryLineData>
{
	public bool category;

	public string categoryName;

	public bool categoryExpanded;

	public int categoryWeight;

	public int categoryAmount;

	public int categoryOffset;

	private string _sortString;

	private string _displayName;

	public GameObject go;

	public HotkeySpread spread;

	public InventoryAndEquipmentStatusScreen screen;

	public string sortString
	{
		get
		{
			if (_sortString == null)
			{
				_sortString = displayName?.Strip()?.ToLower();
			}
			return _sortString;
		}
		set
		{
			_sortString = value;
		}
	}

	public string displayName
	{
		get
		{
			if (_displayName == null)
			{
				_displayName = go?.DisplayName;
			}
			return _displayName;
		}
		set
		{
			_displayName = value;
		}
	}

	public InventoryLineData set(bool category, string categoryName, bool categoryExpanded, int categoryAmount, int categoryWeight, int categoryOffset, GameObject go, HotkeySpread spread, InventoryAndEquipmentStatusScreen screen)
	{
		this.categoryWeight = categoryWeight;
		this.categoryExpanded = categoryExpanded;
		this.categoryName = categoryName;
		this.categoryOffset = categoryOffset;
		this.category = category;
		this.go = go;
		this.spread = spread;
		this.screen = screen;
		displayName = null;
		_sortString = null;
		return this;
	}

	public override void free()
	{
		_sortString = null;
		category = false;
		categoryName = null;
		categoryExpanded = false;
		categoryOffset = 0;
		_displayName = null;
		screen = null;
		spread = null;
		go = null;
		base.free();
	}
}
