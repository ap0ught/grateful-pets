using System;

namespace XRL.World;

public class ModEntry
{
	public string Part;

	public string Tables;

	public int Rarity;

	public int MinTier = 1;

	public int MaxTier = 99;

	public int NativeTier;

	public int TinkerTier = 1;

	public double Value = 1.0;

	public string TinkerDisplayName = "";

	public string Description = "";

	public string TinkerIngredient = "";

	public string TinkerCategory = "utility";

	public bool TinkerAllowed = true;

	public bool BonusAllowed = true;

	public bool CanAutoTinker = true;

	public bool NoSparkingQuest;

	[NonSerialized]
	private string[] _TableList;

	public string[] TableList
	{
		get
		{
			if (_TableList == null)
			{
				_TableList = (Tables.IsNullOrEmpty() ? Array.Empty<string>() : Tables.Split(','));
			}
			return _TableList;
		}
	}
}
