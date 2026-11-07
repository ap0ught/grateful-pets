using System.Collections.Generic;
using ConsoleLib.Console;
using XRL;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace Qud.UI;

public class TinkeringLineData : PooledFrameworkDataElement<TinkeringLineData>
{
	public bool category;

	public string categoryName;

	public bool categoryExpanded;

	public int categoryCount;

	public int categoryOffset;

	public TinkeringStatusScreen screen;

	public TinkerData data;

	public GameObject modObject;

	public List<GameObject> applicableObjects = new List<GameObject>(8);

	public List<string> applicableKeys = new List<string>(8);

	public int mode;

	private BitCost _cost;

	private string _costString;

	public string _uiCategory;

	public string _sortString;

	public virtual string uiCategory
	{
		get
		{
			if (_uiCategory == null)
			{
				_uiCategory = data.UICategory;
			}
			return _uiCategory;
		}
	}

	public virtual string sortString
	{
		get
		{
			if (_sortString == null)
			{
				_sortString = ColorUtility.StripFormatting(data?.DisplayName).ToLower() ?? modObject?.DisplayNameOnlyStripped?.ToLower();
			}
			return _sortString;
		}
	}

	public string costString
	{
		get
		{
			if (_costString == null)
			{
				_ = cost;
			}
			return _costString;
		}
	}

	public BitCost cost
	{
		get
		{
			if (_cost == null)
			{
				if (mode == 0)
				{
					_cost = new BitCost();
					_cost.Import(TinkerItem.GetBitCostFor(data.Blueprint));
					ModifyBitCostEvent.Process(The.Player, _cost, data.Type);
					_costString = _cost.ToString();
				}
				else if (mode == 1)
				{
					_cost = new BitCost();
					if (modObject == null)
					{
						MetricsManager.LogError("no modobject on mod line " + data?.DisplayName);
						return _cost;
					}
					int key = Tier.Constrain(data.Tier);
					int key2 = Tier.Constrain(modObject.GetModificationSlotsUsed() - modObject.GetIntProperty("NoCostMods") + modObject.GetTechTier());
					_cost.Clear();
					_cost.Increment(BitType.TierBits[key]);
					_cost.Increment(BitType.TierBits[key2]);
					ModifyBitCostEvent.Process(modObject, _cost, "Mod");
					_costString = _cost.ToString();
				}
			}
			return _cost;
		}
	}

	public TinkeringLineData set(TinkeringStatusScreen screen, int mode, bool category, string categoryName, int categoryCount, bool categoryExpanded, int categoryOffset, TinkerData data, GameObject modObject)
	{
		this.screen = screen;
		this.mode = mode;
		this.modObject = modObject;
		this.category = category;
		this.categoryName = categoryName;
		this.categoryCount = categoryCount;
		this.categoryExpanded = categoryExpanded;
		this.categoryOffset = categoryOffset;
		this.data = data;
		applicableObjects.Clear();
		applicableKeys.Clear();
		_uiCategory = null;
		_cost = null;
		_costString = null;
		_sortString = null;
		return this;
	}

	public override void free()
	{
		modObject = null;
		mode = 0;
		category = false;
		categoryName = null;
		categoryExpanded = true;
		categoryOffset = 0;
		_cost = null;
		_costString = null;
		_sortString = null;
		_uiCategory = null;
		data = null;
		applicableObjects.Clear();
		applicableKeys.Clear();
		base.free();
	}
}
