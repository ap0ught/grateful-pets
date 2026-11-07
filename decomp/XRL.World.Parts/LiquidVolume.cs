#define NLOG_ALL
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Newtonsoft.Json;
using Occult.Engine.CodeGeneration;
using XRL.Collections;
using XRL.Language;
using XRL.Liquids;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
[GeneratePoolingPartial]
[GenerateSerializationPartial]
[HasModSensitiveStaticCache]
public class LiquidVolume : IPart
{
	private class ComponentSorter : IComparer<KeyValuePair<string, int>>
	{
		public static readonly ComponentSorter Instance = new ComponentSorter();

		public int Compare(KeyValuePair<string, int> A, KeyValuePair<string, int> B)
		{
			return B.Value.CompareTo(A.Value);
		}
	}

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	private static readonly IPartPool LiquidVolumePool = new IPartPool();

	public const int SWIM_THRESHOLD = 2000;

	public const int WADE_THRESHOLD = 200;

	public const int CLEANING_MIXTURE_THRESHOLD = 2;

	public const int LIQUID_FLOWING = 1;

	public const int LIQUID_COLLECTOR = 2;

	public const int LIQUID_SEALED = 4;

	public const int LIQUID_MANUAL_SEAL = 8;

	public const int LIQUID_VISIBLE_WHEN_SEALED = 16;

	public const int LIQUID_SHOW_SEAL = 32;

	public const int LIQUID_HAS_DRAIN = 64;

	public int Flags = 32;

	[NonSerialized]
	public int FrameOffset;

	public int MaxVolume = -1;

	public int Volume;

	public string StartVolume = "";

	public string NamePreposition;

	public string UnknownNamePreposition;

	public string AutoCollectLiquidType;

	public string Primary;

	public string Secondary;

	public string _SmearedName;

	public string _SmearedColor;

	public string _StainedName;

	public string _StainedColor;

	public Dictionary<string, int> ComponentLiquids = new Dictionary<string, int>();

	[NonSerialized]
	[ModSensitiveStaticCache(false)]
	private static StringMap<BaseLiquid> _Liquids;

	private static List<KeyValuePair<string, int>> SortedComponents = new List<KeyValuePair<string, int>>(4);

	[NonSerialized]
	private static List<string> CombinedLiquidTypes = new List<string>();

	[NonSerialized]
	private static List<string> IncomingLiquidTypes = new List<string>();

	private static LiquidVolume tempVolume = new LiquidVolume();

	[NonSerialized]
	private static List<BodyPart> BodyParts = new List<BodyPart>();

	[NonSerialized]
	private static List<BodyPart> MobilityBodyParts = new List<BodyPart>();

	[NonSerialized]
	private static Dictionary<BodyPart, int> BodyPartCapacity = new Dictionary<BodyPart, int>();

	[NonSerialized]
	private static Dictionary<BodyPart, int> BodyPartExposure = new Dictionary<BodyPart, int>();

	[NonSerialized]
	private static string[] PuddleRenderStrings = new string[4] { ",", "`", "'", "\a" };

	[NonSerialized]
	private static LiquidVolume consumeVolume;

	[NonSerialized]
	private static Dictionary<string, int> EvaporativityProportions = new Dictionary<string, int>();

	[NonSerialized]
	private static Dictionary<string, int> EvaporativityAmounts = new Dictionary<string, int>();

	[NonSerialized]
	private static Dictionary<string, int> StainingProportions = new Dictionary<string, int>();

	[NonSerialized]
	private static Dictionary<string, int> StainingAmounts = new Dictionary<string, int>();

	[NonSerialized]
	private static Queue<List<string>> keylistPool = new Queue<List<string>>();

	[NonSerialized]
	private static StringBuilder PaintBuilder = new StringBuilder();

	[NonSerialized]
	public int LastPaintMask = -1;

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override IPartPool Pool => LiquidVolumePool;

	[JsonIgnore]
	public bool Flowing
	{
		get
		{
			return (Flags & 1) == 1;
		}
		set
		{
			Flags = (value ? (Flags | 1) : (Flags & -2));
		}
	}

	[JsonIgnore]
	public bool Collector
	{
		get
		{
			return (Flags & 2) == 2;
		}
		set
		{
			Flags = (value ? (Flags | 2) : (Flags & -3));
		}
	}

	[JsonIgnore]
	public bool Sealed
	{
		get
		{
			return (Flags & 4) == 4;
		}
		set
		{
			Flags = (value ? (Flags | 4) : (Flags & -5));
		}
	}

	[JsonIgnore]
	public bool ManualSeal
	{
		get
		{
			return (Flags & 8) == 8;
		}
		set
		{
			Flags = (value ? (Flags | 8) : (Flags & -9));
		}
	}

	[JsonIgnore]
	public bool LiquidVisibleWhenSealed
	{
		get
		{
			return (Flags & 0x10) == 16;
		}
		set
		{
			Flags = (value ? (Flags | 0x10) : (Flags & -17));
		}
	}

	[JsonIgnore]
	public bool ShowSeal
	{
		get
		{
			return (Flags & 0x20) == 32;
		}
		set
		{
			Flags = (value ? (Flags | 0x20) : (Flags & -33));
		}
	}

	[JsonIgnore]
	public bool HasDrain
	{
		get
		{
			return (Flags & 0x40) == 64;
		}
		set
		{
			Flags = (value ? (Flags | 0x40) : (Flags & -65));
		}
	}

	[JsonIgnore]
	public override int Priority => 90000;

	public static StringMap<BaseLiquid> Liquids
	{
		get
		{
			if (_Liquids == null)
			{
				Loading.LoadTask("Initialize liquids", Init);
			}
			return _Liquids;
		}
	}

	public string SmearedName
	{
		get
		{
			if (_SmearedName == null)
			{
				FindSmear();
			}
			return _SmearedName;
		}
	}

	public string SmearedColor
	{
		get
		{
			if (_SmearedColor == null && _SmearedName == null)
			{
				FindSmear();
			}
			return _SmearedColor;
		}
	}

	public string StainedName
	{
		get
		{
			if (_StainedName == null)
			{
				FindStain();
			}
			return _StainedName;
		}
	}

	public string StainedColor
	{
		get
		{
			if (_StainedColor == null && _StainedName == null)
			{
				FindStain();
			}
			return _StainedColor;
		}
	}

	public string InitialLiquid
	{
		set
		{
			if (value == "")
			{
				return;
			}
			ComponentLiquids.Clear();
			string text = (value.Contains(";") ? value.Split(';').GetRandomElement() : value);
			if (text.Contains(","))
			{
				string[] array = text.Split(',');
				foreach (string spec in array)
				{
					try
					{
						ProcessInitialLiquid(spec);
					}
					catch (Exception x)
					{
						MetricsManager.LogError("InitialLiquid " + text, x);
					}
				}
			}
			else
			{
				ProcessInitialLiquid(text);
			}
			NormalizeProportions();
			RecalculatePrimary();
			if (ParentObject?.CurrentCell != null)
			{
				CheckImage();
			}
		}
	}

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override void Reset()
	{
		base.Reset();
		Flags = 32;
		FrameOffset = 0;
		MaxVolume = -1;
		Volume = 0;
		StartVolume = "";
		NamePreposition = null;
		UnknownNamePreposition = null;
		AutoCollectLiquidType = null;
		Primary = null;
		Secondary = null;
		_SmearedName = null;
		_SmearedColor = null;
		_StainedName = null;
		_StainedColor = null;
		ComponentLiquids.Clear();
		LastPaintMask = -1;
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(Flags);
		Writer.WriteOptimized(MaxVolume);
		Writer.WriteOptimized(Volume);
		Writer.WriteOptimized(StartVolume);
		Writer.WriteOptimized(NamePreposition);
		Writer.WriteOptimized(UnknownNamePreposition);
		Writer.WriteOptimized(AutoCollectLiquidType);
		Writer.WriteOptimized(Primary);
		Writer.WriteOptimized(Secondary);
		Writer.WriteOptimized(_SmearedName);
		Writer.WriteOptimized(_SmearedColor);
		Writer.WriteOptimized(_StainedName);
		Writer.WriteOptimized(_StainedColor);
		Writer.Write(ComponentLiquids);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Flags = Reader.ReadOptimizedInt32();
		MaxVolume = Reader.ReadOptimizedInt32();
		Volume = Reader.ReadOptimizedInt32();
		StartVolume = Reader.ReadOptimizedString();
		NamePreposition = Reader.ReadOptimizedString();
		UnknownNamePreposition = Reader.ReadOptimizedString();
		AutoCollectLiquidType = Reader.ReadOptimizedString();
		Primary = Reader.ReadOptimizedString();
		Secondary = Reader.ReadOptimizedString();
		_SmearedName = Reader.ReadOptimizedString();
		_SmearedColor = Reader.ReadOptimizedString();
		_StainedName = Reader.ReadOptimizedString();
		_StainedColor = Reader.ReadOptimizedString();
		ComponentLiquids = Reader.ReadDictionary<string, int>();
	}

	public static bool isValidLiquid(string id)
	{
		if (Liquids == null || id == null)
		{
			return false;
		}
		return Liquids.ContainsKey(id);
	}

	public static IEnumerable<BaseLiquid> getAllLiquids()
	{
		foreach (BaseLiquid value in Liquids.Values)
		{
			yield return value;
		}
	}

	[Obsolete("Use GetLiquid(ReadOnlySpan<char>) - Will not be removed before Q1 2025")]
	public static BaseLiquid getLiquid(string id)
	{
		return GetLiquid(id);
	}

	/// <summary>
	///     Gets the corresponding liquid from the <see cref="P:XRL.World.Parts.LiquidVolume.Liquids" /> dictionary.
	/// </summary>
	/// <param name="ID">The liquid string ID</param>
	/// <returns>The BaseLiquid instance</returns>
	public static BaseLiquid GetLiquid(ReadOnlySpan<char> ID)
	{
		if (Liquids.TryGetValue(ID, out var Value))
		{
			return Value;
		}
		return null;
	}

	public LiquidVolume()
	{
	}

	public LiquidVolume(string Liquid, int Volume)
		: this()
	{
		InitialLiquid = Liquid;
		this.Volume = Volume;
	}

	public LiquidVolume(string Liquid, int Volume, int MaxVolume)
		: this(Liquid, Volume)
	{
		this.MaxVolume = MaxVolume;
	}

	public LiquidVolume(Dictionary<string, int> Amounts)
	{
		foreach (KeyValuePair<string, int> Amount in Amounts)
		{
			ComponentLiquids.Add(Amount.Key, Amount.Value);
			Volume += Amount.Value;
		}
		NormalizeProportions();
	}

	public override void Attach()
	{
		ParentObject.LiquidVolume = this;
		FrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override void Remove()
	{
		if (ParentObject?.LiquidVolume == this)
		{
			ParentObject.LiquidVolume = null;
		}
	}

	public static GameObject create(List<string> components, int vol = 1000)
	{
		GameObject gameObject = GameObjectFactory.create("WaterPool");
		LiquidVolume liquidVolume = gameObject.LiquidVolume;
		liquidVolume.Empty(WillCheckImage: true);
		for (int i = 0; i < components.Count; i++)
		{
			string key = components[i];
			if (liquidVolume.ComponentLiquids.ContainsKey(key))
			{
				liquidVolume.ComponentLiquids[key] += 1000 / components.Count;
			}
			else
			{
				liquidVolume.ComponentLiquids.Add(key, 1000 / components.Count);
			}
		}
		liquidVolume.Volume = vol;
		liquidVolume.Update();
		return gameObject;
	}

	public bool EffectivelySealed()
	{
		if (Sealed)
		{
			return !IsBroken();
		}
		return false;
	}

	public int GetAdsorbableDrams(GameObject obj)
	{
		return obj.GetMaximumLiquidExposure() * GetLiquidAdsorbence() / 100;
	}

	/// <summary>
	/// If the specified object is in contact with this liquid volume,
	/// calculates how much of a specified liquid the object is exposed to,
	/// in millidrams.
	/// </summary>
	/// <param Name="obj">
	/// the object whose exposure we're checking
	/// </param>
	/// <param Name="Liquid">
	/// the ID of the liquid we're checking exposure to
	/// </param>
	/// <returns>
	/// amount of the liquid the object is exposed to, in millidrams (divide
	/// by 1000 to get drams)
	/// </returns>
	public int GetLiquidExposureMillidrams(GameObject obj, string Liquid)
	{
		if (!ComponentLiquids.TryGetValue(Liquid, out var value) || value <= 0)
		{
			return 0;
		}
		double num = Math.Min(obj.GetMaximumLiquidExposureAsDouble(), Volume);
		if (num <= 0.0)
		{
			return 0;
		}
		return (int)Math.Round(num * (double)value);
	}

	public string GetPreparedCookingIngredientLiquidDomainPairs()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		foreach (BaseLiquid componentLiquid in GetComponentLiquids())
		{
			string preparedCookingIngredient = componentLiquid.GetPreparedCookingIngredient();
			if (preparedCookingIngredient.IsNullOrEmpty())
			{
				continue;
			}
			foreach (string item in preparedCookingIngredient.CachedCommaExpansion())
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(componentLiquid.ID).Append(':').Append(item);
			}
		}
		return stringBuilder.ToString();
	}

	public bool HasPreparedCookingIngredient()
	{
		if (Volume == 0)
		{
			return false;
		}
		foreach (BaseLiquid componentLiquid in GetComponentLiquids())
		{
			if (!componentLiquid.GetPreparedCookingIngredient().IsNullOrEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public string GetPreparedCookingIngredient()
	{
		if (Volume == 0)
		{
			return "";
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		foreach (BaseLiquid componentLiquid in GetComponentLiquids())
		{
			string preparedCookingIngredient = componentLiquid.GetPreparedCookingIngredient();
			if (!preparedCookingIngredient.IsNullOrEmpty())
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(preparedCookingIngredient);
			}
		}
		return stringBuilder.ToString();
	}

	public List<BaseLiquid> GetComponentLiquids()
	{
		List<BaseLiquid> list = new List<BaseLiquid>(2);
		BaseLiquid primaryLiquid = GetPrimaryLiquid();
		if (primaryLiquid != null)
		{
			list.Add(primaryLiquid);
		}
		BaseLiquid secondaryLiquid = GetSecondaryLiquid();
		if (secondaryLiquid != null)
		{
			list.Add(secondaryLiquid);
		}
		return list;
	}

	public bool HasLiquid(string ID)
	{
		return ComponentLiquids?.Any((KeyValuePair<string, int> k) => k.Key == ID && k.Value > 0) ?? false;
	}

	public bool HasPrimaryOrSecondaryLiquid(string ID)
	{
		if (GetPrimaryLiquidID() == ID)
		{
			return true;
		}
		if (GetSecondaryLiquidID() == ID)
		{
			return true;
		}
		return false;
	}

	public BaseLiquid GetPrimaryLiquid()
	{
		if (Volume <= 0)
		{
			return null;
		}
		if (ComponentLiquids == null)
		{
			return null;
		}
		if (ComponentLiquids.Count <= 0)
		{
			return null;
		}
		if (Primary == null)
		{
			RecalculatePrimary();
			if (Primary == null)
			{
				return null;
			}
		}
		if (!Liquids.TryGetValue(Primary, out var Value))
		{
			return null;
		}
		return Value;
	}

	public string GetPrimaryLiquidID()
	{
		return GetPrimaryLiquid()?.ID;
	}

	public BaseLiquid RequirePrimaryLiquid()
	{
		if (Primary == null)
		{
			if (Volume <= 0)
			{
				throw new Exception("no liquid");
			}
			if (ComponentLiquids == null)
			{
				throw new Exception("no component liquid list");
			}
			if (ComponentLiquids.Count <= 0)
			{
				throw new Exception("empty component liquid");
			}
			RecalculatePrimary();
			if (Primary == null)
			{
				throw new Exception("primary liquid cannot be determined");
			}
		}
		if (!Liquids.TryGetValue(Primary, out var Value))
		{
			throw new Exception("primary liquid \"" + Primary + "\" unknown");
		}
		return Value;
	}

	public BaseLiquid GetSecondaryLiquid()
	{
		if (Volume <= 0)
		{
			return null;
		}
		if (ComponentLiquids == null)
		{
			return null;
		}
		if (ComponentLiquids.Count <= 1)
		{
			return null;
		}
		if (Secondary == null)
		{
			RecalculatePrimary();
			if (Secondary == null)
			{
				return null;
			}
		}
		if (!Liquids.TryGetValue(Secondary, out var Value))
		{
			return null;
		}
		return Value;
	}

	public string GetSecondaryLiquidID()
	{
		return GetSecondaryLiquid()?.ID;
	}

	public BaseLiquid RequireSecondaryLiquid()
	{
		if (Secondary == null)
		{
			if (Volume <= 0)
			{
				throw new Exception("no liquid");
			}
			if (ComponentLiquids == null)
			{
				throw new Exception("no component liquid list");
			}
			if (ComponentLiquids.Count <= 0)
			{
				throw new Exception("empty component liquid");
			}
			RecalculatePrimary();
			if (Secondary == null)
			{
				throw new Exception("secondary liquid cannot be determined");
			}
		}
		if (!Liquids.TryGetValue(Secondary, out var Value))
		{
			throw new Exception("secondary liquid \"" + Secondary + "\" unknown");
		}
		return Value;
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		LiquidVolume liquidVolume = base.DeepCopy(Parent, MapInv) as LiquidVolume;
		liquidVolume.ComponentLiquids = new Dictionary<string, int>(ComponentLiquids.Count);
		foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
		{
			liquidVolume.ComponentLiquids.Add(componentLiquid.Key, componentLiquid.Value);
		}
		return liquidVolume;
	}

	private void FindSmear()
	{
		if (ComponentLiquids.Count == 1)
		{
			string text = null;
			if (Primary == null)
			{
				foreach (string tertiary in GetTertiaries())
				{
					BaseLiquid liquid = GetLiquid(tertiary);
					text = liquid.GetSmearedName(this);
					if (!text.IsNullOrEmpty())
					{
						_SmearedColor = liquid.GetColor();
						break;
					}
				}
			}
			else
			{
				BaseLiquid baseLiquid = RequirePrimaryLiquid();
				text = baseLiquid.GetSmearedName(this);
				_SmearedColor = baseLiquid.GetColor();
			}
			_SmearedName = text ?? "liquid-covered";
			return;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (Secondary != null)
		{
			BaseLiquid baseLiquid2 = RequireSecondaryLiquid();
			string smearedAdjective = baseLiquid2.GetSmearedAdjective(this);
			if (!smearedAdjective.IsNullOrEmpty())
			{
				stringBuilder.Compound(smearedAdjective);
				_SmearedColor = baseLiquid2.GetColor();
			}
		}
		List<string> tertiaries = GetTertiaries();
		if (tertiaries != null)
		{
			foreach (string item in tertiaries)
			{
				BaseLiquid liquid2 = GetLiquid(item);
				string smearedAdjective2 = liquid2.GetSmearedAdjective(this);
				if (!smearedAdjective2.IsNullOrEmpty())
				{
					stringBuilder.Compound(smearedAdjective2);
					if (_SmearedColor == null)
					{
						_SmearedColor = liquid2.GetColor();
					}
				}
			}
		}
		if (Primary != null)
		{
			BaseLiquid baseLiquid3 = RequirePrimaryLiquid();
			string smearedName = RequirePrimaryLiquid().GetSmearedName(this);
			if (!smearedName.IsNullOrEmpty())
			{
				stringBuilder.Compound(smearedName);
				_SmearedColor = baseLiquid3.GetColor() ?? _SmearedColor;
			}
		}
		_SmearedName = ((stringBuilder.Length > 0) ? stringBuilder.ToString() : "liquid-covered");
	}

	public void ProcessSmear(GetDisplayNameEvent E)
	{
		if (E.Visible && !E.Reference)
		{
			E.AddAdjective(SmearedName, -25);
			if (!_SmearedColor.IsNullOrEmpty())
			{
				E.AddColor(_SmearedColor, 22);
			}
		}
	}

	private void FindStain()
	{
		if (ComponentLiquids.Count == 1)
		{
			string text = null;
			if (Primary == null)
			{
				foreach (string tertiary in GetTertiaries())
				{
					BaseLiquid liquid = GetLiquid(tertiary);
					text = liquid.GetStainedName(this);
					if (!text.IsNullOrEmpty())
					{
						_StainedColor = liquid.GetColor();
						break;
					}
				}
			}
			else
			{
				BaseLiquid baseLiquid = RequirePrimaryLiquid();
				text = baseLiquid.GetStainedName(this);
				_StainedColor = baseLiquid.GetColor();
			}
			if (text != null)
			{
				_StainedName = text + "-stained";
			}
		}
		else
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (Primary != null)
			{
				BaseLiquid baseLiquid2 = RequirePrimaryLiquid();
				BaseLiquid baseLiquid3 = ((Secondary != null) ? RequireSecondaryLiquid() : null);
				_StainedColor = baseLiquid2.GetColor() ?? baseLiquid3?.GetColor();
				string stainedName = baseLiquid2.GetStainedName(this);
				if (!stainedName.IsNullOrEmpty())
				{
					stringBuilder.Append(stainedName);
				}
				if (baseLiquid3 != null)
				{
					string stainedName2 = baseLiquid3.GetStainedName(this);
					if (!stainedName2.IsNullOrEmpty())
					{
						if (!stainedName.IsNullOrEmpty())
						{
							stringBuilder.Append("-and-");
						}
						stringBuilder.Append(stainedName2);
					}
				}
			}
			if (stringBuilder.Length <= 0)
			{
				foreach (string tertiary2 in GetTertiaries())
				{
					BaseLiquid liquid2 = GetLiquid(tertiary2);
					string stainedName3 = liquid2.GetStainedName(this);
					if (stainedName3 != null)
					{
						_StainedColor = liquid2.GetColor();
						stringBuilder.Append(stainedName3);
						break;
					}
				}
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append("-stained");
				_StainedName = stringBuilder.ToString();
			}
		}
		if (_StainedName == null)
		{
			_StainedName = "stained";
		}
	}

	public void ProcessStain(GetDisplayNameEvent E)
	{
		if (E.Visible && !E.Reference)
		{
			E.AddAdjective(StainedName, -20);
			if (!_StainedColor.IsNullOrEmpty())
			{
				E.AddColor(_StainedColor, 20);
			}
		}
	}

	public int Proportion(string Liquid)
	{
		if (ComponentLiquids.TryGetValue(Liquid, out var value))
		{
			return value;
		}
		return 0;
	}

	public int Proportion(string Liquid1, string Liquid2)
	{
		return Proportion(Liquid1) + Proportion(Liquid2);
	}

	public int Proportion(string Liquid1, string Liquid2, string Liquid3)
	{
		return Proportion(Liquid1) + Proportion(Liquid2) + Proportion(Liquid3);
	}

	public int Proportion(string Liquid1, string Liquid2, string Liquid3, string Liquid4)
	{
		return Proportion(Liquid1) + Proportion(Liquid2) + Proportion(Liquid3) + Proportion(Liquid4);
	}

	public int Proportion(string Liquid1, string Liquid2, string Liquid3, string Liquid4, string Liquid5)
	{
		return Proportion(Liquid1) + Proportion(Liquid2) + Proportion(Liquid3) + Proportion(Liquid4) + Proportion(Liquid5);
	}

	public int Amount(string Liquid)
	{
		return Volume * Proportion(Liquid) / 1000;
	}

	public int Amount(string Liquid1, string Liquid2)
	{
		return Volume * Proportion(Liquid1, Liquid2) / 1000;
	}

	public int Amount(string Liquid1, string Liquid2, string Liquid3)
	{
		return Volume * Proportion(Liquid1, Liquid2, Liquid3) / 1000;
	}

	public int Amount(string Liquid1, string Liquid2, string Liquid3, string Liquid4)
	{
		return Volume * Proportion(Liquid1, Liquid2, Liquid3, Liquid4) / 1000;
	}

	public int Amount(string Liquid1, string Liquid2, string Liquid3, string Liquid4, string Liquid5)
	{
		return Volume * Proportion(Liquid1, Liquid2, Liquid3, Liquid4, Liquid5) / 1000;
	}

	public int UpperAmount(string Liquid)
	{
		return (int)Math.Ceiling((double)(Volume * Proportion(Liquid)) / 1000.0);
	}

	public int UpperAmount(string Liquid1, string Liquid2)
	{
		return (int)Math.Ceiling((double)(Volume * Proportion(Liquid1, Liquid2)) / 1000.0);
	}

	public int UpperAmount(string Liquid1, string Liquid2, string Liquid3)
	{
		return (int)Math.Ceiling((double)(Volume * Proportion(Liquid1, Liquid2, Liquid3)) / 1000.0);
	}

	public int UpperAmount(string Liquid1, string Liquid2, string Liquid3, string Liquid4)
	{
		return (int)Math.Ceiling((double)(Volume * Proportion(Liquid1, Liquid2, Liquid3, Liquid4)) / 1000.0);
	}

	public int UpperAmount(string Liquid1, string Liquid2, string Liquid3, string Liquid4, string Liquid5)
	{
		return (int)Math.Ceiling((double)(Volume * Proportion(Liquid1, Liquid2, Liquid3, Liquid4, Liquid5)) / 1000.0);
	}

	public int MilliAmount(string Liquid)
	{
		return Volume * Proportion(Liquid);
	}

	public int MilliAmount(string Liquid1, string Liquid2)
	{
		return Volume * Proportion(Liquid1, Liquid2);
	}

	public int MilliAmount(string Liquid1, string Liquid2, string Liquid3)
	{
		return Volume * Proportion(Liquid1, Liquid2, Liquid3);
	}

	public int MilliAmount(string Liquid1, string Liquid2, string Liquid3, string Liquid4)
	{
		return Volume * Proportion(Liquid1, Liquid2, Liquid3, Liquid4);
	}

	public int MilliAmount(string Liquid1, string Liquid2, string Liquid3, string Liquid4, string Liquid5)
	{
		return Volume * Proportion(Liquid1, Liquid2, Liquid3, Liquid4, Liquid5);
	}

	public bool IsMixed()
	{
		return ComponentLiquids.Count > 1;
	}

	public bool IsPure()
	{
		return ComponentLiquids.Count <= 1;
	}

	public bool RecalculatePrimary()
	{
		_SmearedName = null;
		_SmearedColor = null;
		_StainedName = null;
		_StainedColor = null;
		string primary = Primary;
		string secondary = Secondary;
		if (ComponentLiquids.Count == 0)
		{
			Primary = null;
			Secondary = null;
		}
		else if (ComponentLiquids.Count == 1)
		{
			if (Primary != null && ComponentLiquids.ContainsKey(Primary))
			{
				return false;
			}
			Primary = ComponentLiquids.First().Key;
			Secondary = null;
		}
		else
		{
			SortedComponents.Clear();
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				int num = SortedComponents.BinarySearch(componentLiquid, ComponentSorter.Instance);
				SortedComponents.Insert((num < 0) ? (~num) : num, componentLiquid);
			}
			Primary = SortedComponents[0].Key;
			Secondary = SortedComponents[1].Key;
			if (Primary != "blood" && ComponentLiquids.ContainsKey("blood"))
			{
				Secondary = "blood";
			}
			else if (Primary == "warmstatic" && Secondary == "water")
			{
				Secondary = ((ComponentLiquids.Count == 2) ? null : SortedComponents[2].Key);
			}
		}
		if (Primary != primary || Secondary != secondary)
		{
			if (IsOpenVolume())
			{
				BaseRender();
			}
			return true;
		}
		return false;
	}

	public static void Init()
	{
		_Liquids = new StringMap<BaseLiquid>();
		foreach (Type item in ModManager.GetTypesWithAttribute(typeof(IsLiquid)))
		{
			try
			{
				if (!(Activator.CreateInstance(item) is BaseLiquid baseLiquid))
				{
					Logger.gameLog.Error("couldn't instantiate " + item?.ToString() + " or it is not derived from BaseLiquid.");
				}
				else
				{
					_Liquids[baseLiquid.ID] = baseLiquid;
				}
			}
			catch (Exception ex)
			{
				Logger.Exception("Initializing liquid type " + item.Name, ex);
			}
		}
	}

	public static List<string> GetLiquidColors(string liquid)
	{
		return Liquids[liquid].GetColors();
	}

	private void ProcessInitialLiquid(string Spec)
	{
		try
		{
			if (Spec.Contains('-'))
			{
				string[] array = Spec.Split('-');
				string key = array[0];
				int value = Convert.ToInt32(array[1]);
				ComponentLiquids.Add(key, value);
			}
			else
			{
				string iD = Liquids[Spec].ID;
				ComponentLiquids.Add(iD, 1000);
			}
		}
		catch
		{
			MetricsManager.LogError("invalid initial liquid specification " + (Spec ?? "NULL"));
		}
	}

	public bool IsEmpty()
	{
		if (ComponentLiquids.Count != 0)
		{
			return Volume == 0;
		}
		return true;
	}

	public int GetNavigationWeight(GameObject Object, bool Smart, bool Slimewalking, bool FilthAffinity, ref bool Uncacheable)
	{
		int num = 0;
		foreach (string key in ComponentLiquids.Keys)
		{
			BaseLiquid baseLiquid = Liquids[key];
			int num2 = baseLiquid.GetNavigationWeight(this, Object, Smart, Slimewalking, FilthAffinity, ref Uncacheable);
			if (num2 < 2 && baseLiquid.InterruptAutowalk)
			{
				num2 = 2;
			}
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	private void TrackAsLiquid(string Liquid)
	{
		if (Liquid.IndexOf(',') == -1)
		{
			if (ComponentLiquids.Count > 1 || !ComponentLiquids.ContainsKey(Liquid))
			{
				ComponentLiquids.Clear();
				ComponentLiquids.Add(Liquid, 1000);
			}
			else
			{
				ComponentLiquids[Liquid] = 1000;
			}
			return;
		}
		ComponentLiquids.Clear();
		int num = 0;
		foreach (string item in Liquid.CachedCommaExpansion())
		{
			string[] array = item.Split('-');
			if (array.Length != 2)
			{
				MetricsManager.LogWarning("Invalid liquid specification: " + item);
				return;
			}
			string text = array[0];
			string value = array[1];
			int num2;
			try
			{
				num2 = Convert.ToInt32(value);
			}
			catch
			{
				return;
			}
			if (!Liquids.ContainsKey(text))
			{
				MetricsManager.LogWarning("Unknown liquid type: " + text);
				return;
			}
			string iD = Liquids[text].ID;
			ComponentLiquids.Add(iD, num2);
			num += num2;
		}
		NormalizeProportions();
	}

	public bool IsPureLiquid(bool AllowEmpty = false)
	{
		if (Volume == 0 || ComponentLiquids == null)
		{
			return AllowEmpty;
		}
		return ComponentLiquids.Count == 1;
	}

	public bool IsPureLiquid(string LiquidType, bool AllowEmpty = false)
	{
		if (LiquidType.IsNullOrEmpty())
		{
			return false;
		}
		if (Volume == 0 || ComponentLiquids == null)
		{
			return AllowEmpty;
		}
		if (LiquidType.IndexOf(',') == -1)
		{
			if (ComponentLiquids.Count != 1)
			{
				return false;
			}
			if (LiquidType.EndsWith("-1000"))
			{
				LiquidType = LiquidType.Substring(0, LiquidType.LastIndexOf('-'));
			}
			if (!ComponentLiquids.ContainsKey(LiquidType))
			{
				return false;
			}
			return true;
		}
		if (ComponentLiquids.Count < 2)
		{
			return false;
		}
		string[] array = LiquidType.Split(',');
		if (ComponentLiquids.Count != array.Length)
		{
			return false;
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split('-');
			if (array3.Length != 2)
			{
				MetricsManager.LogWarning("Invalid liquid specification: " + text);
				return false;
			}
			string text2 = array3[0];
			string value = array3[1];
			int num;
			try
			{
				num = Convert.ToInt32(value);
			}
			catch
			{
				return false;
			}
			if (!isValidLiquid(text2))
			{
				MetricsManager.LogWarning("Unknown liquid type: " + text2);
				return false;
			}
			if (!ComponentLiquids.TryGetValue(text2, out var value2))
			{
				return false;
			}
			if (value2 != num)
			{
				return false;
			}
		}
		return true;
	}

	public bool ContainsLiquid(string LiquidID)
	{
		return ComponentLiquids.ContainsKey(LiquidID);
	}

	public bool ContainsSignificantLiquid(string LiquidID)
	{
		if (Volume < 1)
		{
			return false;
		}
		if (!ComponentLiquids.ContainsKey(LiquidID))
		{
			return false;
		}
		if (ComponentLiquids.Count == 1)
		{
			return true;
		}
		int num = ComponentLiquids[LiquidID];
		return Volume * num / 1000 >= 1;
	}

	public bool IsWaste()
	{
		if (!ContainsSignificantLiquid("ooze") && !ContainsSignificantLiquid("sludge"))
		{
			return ContainsSignificantLiquid("goo");
		}
		return true;
	}

	public bool IsWater(bool AllowEmpty = false)
	{
		return ContainsSignificantLiquid("water");
	}

	public bool IsFreshWater(bool AllowEmpty = false)
	{
		return IsPureLiquid("water", AllowEmpty);
	}

	public bool IsFreshWaterFast()
	{
		if (Volume > 0 && ComponentLiquids.Count == 1)
		{
			return Primary == "water";
		}
		return false;
	}

	public bool MixWith(LiquidVolume Liquid, ref bool RequestInterfaceExit, GameObject PouredFrom = null, GameObject PouredBy = null, int? Amount = null, bool UseTempSplit = false)
	{
		int volume = Volume;
		int num = Amount ?? Liquid.Volume;
		if (ParentObject != null && ParentObject.HasRegisteredEvent("LiquidMixing"))
		{
			Event obj = Event.New("LiquidMixing");
			obj.SetParameter("TargetVolume", this);
			obj.SetParameter("MixingVolume", Liquid);
			obj.SetParameter("Amount", num);
			obj.SetParameter("PouredFrom", PouredFrom);
			obj.SetParameter("PouredTo", ParentObject);
			obj.SetParameter("PouredBy", PouredBy);
			if (!ParentObject.FireEvent(obj))
			{
				return false;
			}
		}
		IncomingLiquidTypes.Clear();
		IncomingLiquidTypes.AddRange(Liquid.ComponentLiquids.Keys);
		CombinedLiquidTypes.Clear();
		foreach (string key in Liquid.ComponentLiquids.Keys)
		{
			if (!Liquids[key].MixingWith(Liquid, this, num, PouredFrom, ParentObject, PouredBy, ref RequestInterfaceExit))
			{
				return false;
			}
			if (!CombinedLiquidTypes.Contains(key))
			{
				CombinedLiquidTypes.Add(key);
			}
		}
		foreach (string key2 in ComponentLiquids.Keys)
		{
			if (!Liquids[key2].MixingWith(Liquid, this, num, PouredFrom, ParentObject, PouredBy, ref RequestInterfaceExit))
			{
				return false;
			}
			if (!CombinedLiquidTypes.Contains(key2))
			{
				CombinedLiquidTypes.Add(key2);
			}
		}
		if (Amount.HasValue)
		{
			Liquid = (UseTempSplit ? Liquid.TempSplit(Amount.Value) : Liquid.Split(Amount.Value));
		}
		int num2 = int.MinValue;
		string text = null;
		foreach (string combinedLiquidType in CombinedLiquidTypes)
		{
			int num3 = (ComponentLiquids.ContainsKey(combinedLiquidType) ? ComponentLiquids[combinedLiquidType] : 0);
			int num4 = (Liquid.ComponentLiquids.ContainsKey(combinedLiquidType) ? Liquid.ComponentLiquids[combinedLiquidType] : 0);
			int num5 = (int)Math.Floor((double)(num3 * volume + num4 * num) / (double)(volume + num));
			if (num5 > num2 || (num5 == num2 && (text == null || text.CompareTo(combinedLiquidType) < 0)))
			{
				num2 = num5;
				text = combinedLiquidType;
			}
			if (num5 < 1 && num > 0 && num4 > 0)
			{
				num5 = 1;
			}
			if (num5 > 0)
			{
				ComponentLiquids[combinedLiquidType] = num5;
			}
			else
			{
				ComponentLiquids.Remove(combinedLiquidType);
			}
		}
		Volume += num;
		if (MaxVolume >= 0 && Volume > MaxVolume)
		{
			Volume = MaxVolume;
		}
		if (Volume > 0 && ComponentLiquids.Count == 0 && text != null)
		{
			ComponentLiquids.Add(text, 1000);
		}
		if (num > 0 && ParentObject != null && !IsOpenVolume())
		{
			foreach (string incomingLiquidType in IncomingLiquidTypes)
			{
				Liquids[incomingLiquidType].FillingContainer(ParentObject, this);
			}
		}
		foreach (string item in new List<string>(CombinedLiquidTypes))
		{
			Liquids[item].MixedWith(Liquid, this, num, PouredFrom, ParentObject, PouredBy, ref RequestInterfaceExit);
		}
		Update();
		if (ParentObject != null)
		{
			LiquidMixedEvent.Send(this);
		}
		return true;
	}

	public bool MixWith(LiquidVolume Liquid, GameObject PouredFrom = null, GameObject PouredBy = null, int? Amount = null, bool UseTempSplit = false)
	{
		bool RequestInterfaceExit = false;
		return MixWith(Liquid, ref RequestInterfaceExit, PouredFrom, PouredBy, Amount, UseTempSplit);
	}

	public void NormalizeProportions()
	{
		if (ComponentLiquids.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (int value in ComponentLiquids.Values)
		{
			num += value;
		}
		if (num == 1000)
		{
			return;
		}
		if (ComponentLiquids.Count > 1)
		{
			int num2 = 1000 - num;
			if (num2 > 1 || num2 < -1)
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>(ComponentLiquids.Count);
				foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
				{
					int num3 = num2 * componentLiquid.Value / num;
					if (num3 != 0)
					{
						dictionary[componentLiquid.Key] = num3;
					}
				}
				foreach (KeyValuePair<string, int> item in dictionary)
				{
					ComponentLiquids[item.Key] += item.Value;
					num2 -= item.Value;
				}
			}
			switch (num2)
			{
			case -1:
			case 1:
			{
				int num4 = int.MinValue;
				string text = null;
				foreach (KeyValuePair<string, int> componentLiquid2 in ComponentLiquids)
				{
					if (componentLiquid2.Value > num4)
					{
						text = componentLiquid2.Key;
						num4 = componentLiquid2.Value;
					}
				}
				if (text != null)
				{
					if (num2 > 0)
					{
						ComponentLiquids[text]++;
						num2--;
					}
					else
					{
						ComponentLiquids[text]--;
						num2++;
					}
				}
				break;
			}
			default:
			{
				List<KeyValuePair<string, int>> list = ComponentLiquids.ToList();
				list.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => b.Value.CompareTo(a.Value));
				while (num2 != 0)
				{
					foreach (KeyValuePair<string, int> item2 in list)
					{
						if (num2 > 0)
						{
							ComponentLiquids[item2.Key]++;
							num2--;
						}
						else
						{
							ComponentLiquids[item2.Key]--;
							num2++;
						}
						if (num2 == 0)
						{
							break;
						}
					}
				}
				break;
			}
			case 0:
				break;
			}
		}
		else
		{
			string text2 = null;
			using (Dictionary<string, int>.KeyCollection.Enumerator enumerator4 = ComponentLiquids.Keys.GetEnumerator())
			{
				if (enumerator4.MoveNext())
				{
					text2 = enumerator4.Current;
				}
			}
			if (text2 != null)
			{
				ComponentLiquids[text2] = 1000;
			}
		}
		FlushWeightCaches();
	}

	public string GetLiquidDesignation(bool hyphenateSoloLiquids = false)
	{
		if (ComponentLiquids.Count == 0)
		{
			return null;
		}
		if (ComponentLiquids.Count == 1)
		{
			using Dictionary<string, int>.KeyCollection.Enumerator enumerator = ComponentLiquids.Keys.GetEnumerator();
			if (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				if (hyphenateSoloLiquids)
				{
					return current + "-1000";
				}
				return current;
			}
		}
		string[] array = new string[ComponentLiquids.Count];
		int num = 0;
		foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
		{
			array[num++] = componentLiquid.Key + "-" + componentLiquid.Value;
		}
		return string.Join(",", array);
	}

	public string GetLiquidDebugDesignation()
	{
		string text = GetLiquidDesignation() ?? "nothing";
		return Volume + "/" + MaxVolume + "(" + text + ")";
	}

	public void SetComponent(string b, int value)
	{
		string key;
		BaseLiquid value2;
		if (value == 0)
		{
			if (ComponentLiquids.ContainsKey(b))
			{
				int num = ComponentLiquids[b];
				ComponentLiquids.Remove(b);
				foreach (KeyValuePair<string, BaseLiquid> liquid in Liquids)
				{
					liquid.Deconstruct(out key, out value2);
					string text = key;
					if (text != b && ComponentLiquids.ContainsKey(text))
					{
						Dictionary<string, int> componentLiquids = ComponentLiquids;
						key = text;
						componentLiquids[key] += (int)((float)num * ((float)ComponentLiquids[text] / 1000f));
					}
				}
			}
		}
		else
		{
			int num2 = ((!ComponentLiquids.ContainsKey(b)) ? (1000 - value) : (1000 - (value - ComponentLiquids[b])));
			ComponentLiquids[b] = value;
			foreach (KeyValuePair<string, BaseLiquid> liquid2 in Liquids)
			{
				liquid2.Deconstruct(out key, out value2);
				string text2 = key;
				if (text2 != b && ComponentLiquids.ContainsKey(text2))
				{
					ComponentLiquids[text2] = (int)((float)num2 * ((float)ComponentLiquids[text2] / 1000f));
				}
			}
		}
		Update();
	}

	public void Update()
	{
		if (Volume <= 0)
		{
			Empty();
			return;
		}
		NormalizeProportions();
		RecalculatePrimary();
		RecalculateProperties();
	}

	public void RecalculateProperties()
	{
		if (ParentObject != null)
		{
			SyncPhysicalProperties();
			CheckImage();
			FlushWeightCaches();
		}
	}

	public void SyncPhysicalProperties()
	{
		if (ParentObject?.Physics != null && IsOpenVolume())
		{
			GetLiquidPhysicalProperties(out ParentObject.Physics.FlameTemperature, out ParentObject.Physics.VaporTemperature, out ParentObject.Physics.FreezeTemperature, out ParentObject.Physics.BrittleTemperature, out var ElectricalConductivity);
			ParentObject.BaseElectricalConductivity = ElectricalConductivity;
		}
	}

	public LiquidVolume TempSplit(int SplitVolume)
	{
		LiquidVolume liquidVolume = tempVolume;
		liquidVolume.Volume = ((SplitVolume == 0) ? 1 : SplitVolume);
		liquidVolume.ComponentLiquids.Clear();
		foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
		{
			liquidVolume.ComponentLiquids.Add(componentLiquid.Key, componentLiquid.Value);
		}
		Volume -= SplitVolume;
		if (Volume <= 0)
		{
			Empty();
		}
		else
		{
			FlushWeightCaches();
		}
		return liquidVolume;
	}

	public LiquidVolume Split(int SplitVolume)
	{
		LiquidVolume liquidVolume = new LiquidVolume();
		liquidVolume.Volume = ((SplitVolume == 0) ? 1 : SplitVolume);
		foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
		{
			liquidVolume.ComponentLiquids.Add(componentLiquid.Key, componentLiquid.Value);
		}
		Volume -= SplitVolume;
		if (Volume <= 0)
		{
			Empty();
		}
		else
		{
			FlushWeightCaches();
		}
		return liquidVolume;
	}

	public bool LiquidSameAs(LiquidVolume V)
	{
		if (V == null)
		{
			return false;
		}
		if (V.ComponentLiquids.Count != ComponentLiquids.Count)
		{
			return false;
		}
		foreach (KeyValuePair<string, int> componentLiquid in V.ComponentLiquids)
		{
			if (!ComponentLiquids.TryGetValue(componentLiquid.Key, out var value))
			{
				return false;
			}
			if (value != componentLiquid.Value)
			{
				return false;
			}
		}
		return true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AllowLiquidCollectionEvent.ID && ID != AnyAutoCollectDramsEvent.ID && ID != AutoexploreObjectEvent.ID && ID != PooledEvent<CheckAnythingToCleanWithEvent>.ID && ID != PooledEvent<CheckAnythingToCleanWithNearbyEvent>.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EnteredCellEvent.ID && ID != BeforeRenderEvent.ID && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != FellDownEvent.ID && ID != PooledEvent<FrozeEvent>.ID && ID != GetAutoCollectDramsEvent.ID && ID != PooledEvent<GetCleaningItemsEvent>.ID && ID != PooledEvent<GetCleaningItemsNearbyEvent>.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetExtrinsicValueEvent.ID && ID != GetExtrinsicWeightEvent.ID && ID != GetFreeDramsEvent.ID && ID != PooledEvent<GetGameObjectSortEvent>.ID && ID != GetInventoryActionsAlwaysEvent.ID && ID != PooledEvent<GetMatterPhaseEvent>.ID && ID != GetNavigationWeightEvent.ID && ID != GetShortDescriptionEvent.ID && ID != GetSlottedInventoryActionsEvent.ID && ID != PooledEvent<GetSpringinessEvent>.ID && ID != GetStorableDramsEvent.ID && ID != GiveDramsEvent.ID && ID != GravitationEvent.ID && ID != PooledEvent<InterruptAutowalkEvent>.ID && ID != InventoryActionEvent.ID && ID != ObjectCreatedEvent.ID && ID != ObjectEnteredCellEvent.ID && ID != ObjectGoingProneEvent.ID && ID != ObjectStoppedFlyingEvent.ID && ID != OnDestroyObjectEvent.ID && ID != PooledEvent<PollForHealingLocationEvent>.ID && ID != SingletonEvent<RadiatesHeatEvent>.ID && ID != PooledEvent<StripContentsEvent>.ID && ID != PooledEvent<ThawedEvent>.ID && ID != UseDramsEvent.ID)
		{
			return ID == PooledEvent<VaporizedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (E.Command == null && !IsMixed())
		{
			string primaryLiquidID = GetPrimaryLiquidID();
			if (primaryLiquidID != null && (Options.AutogetDroppedLiquid || ParentObject.GetIntProperty("DroppedByPlayer") <= 0) && !ParentObject.IsOwned() && !EffectivelySealed() && !ParentObject.IsInStasis() && !ParentObject.IsTemporary && E.Actor.AnyAutoCollectDrams(primaryLiquidID))
			{
				E.Command = "CollectLiquid";
				E.AllowRetry = true;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSpringinessEvent E)
	{
		if (IsOpenVolume() && !ParentObject.IsFrozen())
		{
			E.LinearIncrease += ParentObject.GetKineticResistance() * 95 / 100;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatEvent E)
	{
		if (IsOpenVolume() && GetLiquidTemperature() > 25)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMatterPhaseEvent E)
	{
		if (IsOpenVolume())
		{
			E.MinMatterPhase(2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!E.Flying && IsOpenVolume() && E.PhaseMatches(ParentObject) && CanInteractWithAnything(E.Cell))
		{
			int num = GetNavigationWeight(E.Actor, E.Smart, E.Slimewalking, E.FilthAffinity, ref E.Uncacheable);
			if (num < 60 && IsSwimmingDepth())
			{
				if (E.Smart)
				{
					num = ((!E.Swimming) ? (num + 5) : (num + 2));
					num = Math.Min(num, 60);
				}
				else
				{
					num = Math.Min(num + (E.Swimming ? 2 : 5), 60);
				}
			}
			else if (num < 30 && IsWadingDepth())
			{
				num = Math.Min(num + 1, 30);
			}
			if (E.Reefer && num < 25 && ContainsLiquid("algae") && IsWadingDepth())
			{
				num = 25;
			}
			E.MinWeight(num);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (Primary != null)
		{
			RequirePrimaryLiquid().BeforeRender(this);
		}
		if (Secondary != null)
		{
			RequireSecondaryLiquid().BeforeRenderSecondary(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetFreeDramsEvent E)
	{
		if (E.ApplyTo(ParentObject) && !EffectivelySealed())
		{
			if (IsPureLiquid(E.Liquid))
			{
				E.Drams += Volume;
			}
			else if (E.ImpureOkay && ComponentLiquids.ContainsKey(E.Liquid))
			{
				E.Drams += Math.Max(Volume * ComponentLiquids[E.Liquid] / 1000, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetStorableDramsEvent E)
	{
		if (Volume < MaxVolume && E.ApplyTo(ParentObject) && (IsPureLiquid(E.Liquid) || (IsEmpty() && (AutoCollectLiquidType == null || AutoCollectLiquidType == E.Liquid))) && !EffectivelySealed() && ParentObject.AllowLiquidCollection(E.Liquid, E.Actor))
		{
			E.Drams += MaxVolume - Volume;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GiveDramsEvent E)
	{
		if (Volume < MaxVolume && E.Drams > 0 && E.ApplyTo(ParentObject) && ((E.Pass == 1 && AutoCollectLiquidType == E.Liquid) || (E.Pass >= 2 && ParentObject.WantsLiquidCollection(E.Liquid)) || (E.Pass >= 3 && IsPureLiquid(E.Liquid) && !ProducesLiquidEvent.Check(ParentObject, E.Liquid)) || (E.Pass >= 4 && IsEmpty() && !ProducesLiquidEvent.Check(ParentObject, E.Liquid)) || E.Pass >= 5))
		{
			int Drams = E.Drams;
			GiveDrams(E.Liquid, ref Drams, E.Auto, E.StoredIn, E.Actor);
			if (E.Drams != Drams)
			{
				E.Drams = Drams;
				if (E.Drams <= 0)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseDramsEvent E)
	{
		if (Volume > 0 && E.Drams > 0 && E.ApplyTo(ParentObject) && ((E.Pass == 1 && ProducesLiquidEvent.Check(ParentObject, E.Liquid)) || (E.Pass == 2 && !ParentObject.WantsLiquidCollection(E.Liquid)) || E.Pass >= 3) && !EffectivelySealed())
		{
			if (IsPureLiquid(E.Liquid))
			{
				if (Volume >= E.Drams)
				{
					Volume -= E.Drams;
					E.Drams = 0;
				}
				else
				{
					E.Drams -= Volume;
					Volume = 0;
				}
				if (E.TrackContainers != null && !E.TrackContainers.Contains(ParentObject))
				{
					E.TrackContainers.Add(ParentObject);
				}
				if (Volume <= 0)
				{
					Empty();
				}
				else
				{
					FlushWeightCaches();
					CheckImage();
				}
				if (E.Drams <= 0)
				{
					return false;
				}
			}
			else if (E.ImpureOkay && ComponentLiquids.ContainsKey(E.Liquid))
			{
				int num = Math.Max(Volume * ComponentLiquids[E.Liquid] / 1000, 1);
				int amount;
				if (num >= E.Drams)
				{
					amount = E.Drams;
					E.Drams = 0;
				}
				else
				{
					amount = num;
					E.Drams -= num;
				}
				UseDrams(E.Liquid, amount);
				if (E.TrackContainers != null && !E.TrackContainers.Contains(ParentObject))
				{
					E.TrackContainers.Add(ParentObject);
				}
				if (E.Drams <= 0)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AnyAutoCollectDramsEvent E)
	{
		if (Volume < MaxVolume && E.ApplyTo(ParentObject))
		{
			string activeAutogetLiquid = GetActiveAutogetLiquid();
			if (activeAutogetLiquid != null && E.Liquid == activeAutogetLiquid && (IsPureLiquid(E.Liquid) || IsEmpty()) && !EffectivelySealed() && ParentObject.AllowLiquidCollection(E.Liquid, E.Actor))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAutoCollectDramsEvent E)
	{
		if (Volume < MaxVolume && E.ApplyTo(ParentObject))
		{
			string activeAutogetLiquid = GetActiveAutogetLiquid();
			if (activeAutogetLiquid != null && E.Liquid == activeAutogetLiquid && (IsPureLiquid(E.Liquid) || IsEmpty()) && !EffectivelySealed() && ParentObject.AllowLiquidCollection(E.Liquid, E.Actor))
			{
				E.Drams += MaxVolume - Volume;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		if (AutoCollectLiquidType != null && E.Liquid != AutoCollectLiquidType)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (E.Cell != null)
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				keylist.AddRange(ComponentLiquids.Keys);
				foreach (string item in keylist)
				{
					if (!Liquids[item].EnteredCell(this, E))
					{
						return false;
					}
				}
				if (IsOpenVolume())
				{
					int i = 0;
					for (int count = E.Cell.Objects.Count; i < count; i++)
					{
						GameObject gameObject = E.Cell.Objects[i];
						if (gameObject == ParentObject)
						{
							continue;
						}
						LiquidVolume liquidVolume = gameObject.LiquidVolume;
						if (liquidVolume == null || !liquidVolume.IsOpenVolume())
						{
							continue;
						}
						Temporary part = ParentObject.GetPart<Temporary>();
						ExistenceSupport part2 = ParentObject.GetPart<ExistenceSupport>();
						if (part != null)
						{
							part.Expire(Silent: true);
							if (GameObject.Validate(ParentObject))
							{
								Empty();
							}
						}
						else if (part2 != null)
						{
							part2.Unsupported(Silent: true);
							if (GameObject.Validate(ParentObject))
							{
								Empty();
							}
						}
						else
						{
							ParentObject.RemoveFromContext();
							liquidVolume.MixWith(this, PouredFrom: ParentObject, RequestInterfaceExit: ref E.InterfaceExit);
							ParentObject.Obliterate();
						}
						return false;
					}
					if (E.Type != "Pour" && E.Type != "Flow" && E.Cell.Objects.Count > 1 && E.Cell.ParentZone.IsActive() && CanInteractWithAnything(E.Cell))
					{
						int j = 0;
						for (int count2 = E.Cell.Objects.Count; j < count2; j++)
						{
							GameObject gameObject2 = E.Cell.Objects[j];
							if (CanInteractWith(gameObject2, E.Cell, AnythingChecked: true))
							{
								foreach (string item2 in keylist)
								{
									Liquids[item2].ObjectInCell(this, gameObject2);
								}
								ProcessContact(gameObject2, Initial: true, gameObject2.HasEffect<Prone>(), Poured: false, null, FromCell: true);
							}
							if (count2 != E.Cell.Objects.Count)
							{
								count2 = E.Cell.Objects.Count;
								if (j < count2 && E.Cell.Objects[j] != gameObject2)
								{
									j--;
								}
							}
						}
					}
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
			}
		}
		return base.HandleEvent(E);
	}

	public void Splash(Cell C = null)
	{
		if (C == null)
		{
			if (ParentObject != null)
			{
				C = ParentObject.CurrentCell;
			}
			if (C == null)
			{
				return;
			}
		}
		if (C.IsVisible())
		{
			if (Secondary != null && 10.in100())
			{
				C.LiquidSplash(RequireSecondaryLiquid());
			}
			else if (Primary != null)
			{
				C.LiquidSplash(RequirePrimaryLiquid());
			}
		}
		string text = null;
		if (Primary != null)
		{
			string text2 = RequirePrimaryLiquid().SplashSound(this);
			if (text2 != null)
			{
				text = text2;
			}
		}
		if (Secondary != null && text == null)
		{
			string text3 = RequireSecondaryLiquid().SplashSound(this);
			if (text3 != null)
			{
				text = text3;
			}
		}
		if (text == null)
		{
			List<string> tertiaries = GetTertiaries();
			if (tertiaries != null)
			{
				foreach (string item in tertiaries)
				{
					if (item != Primary && item != Secondary)
					{
						string text4 = Liquids[item].SplashSound(this);
						if (text4 != null)
						{
							text = text4;
							break;
						}
					}
				}
			}
		}
		PlayWorldSound(text, 0.5f, 0f, Combat: false, C);
	}

	private int GetCurrentCoverIfSame(GameObject obj)
	{
		LiquidCovered effect = obj.GetEffect<LiquidCovered>();
		if (effect != null && LiquidSameAs(effect.Liquid))
		{
			return effect.Liquid.Volume;
		}
		return 0;
	}

	private void LiquidInContact(GameObject obj, int Amount, ref bool TemporaryKnown, ref bool Temporary, bool Poured, GameObject PouredBy, bool FromCell)
	{
		if (!TemporaryKnown)
		{
			TemporaryKnown = true;
			Temporary = ParentObject.IsTemporary;
		}
		if (Temporary)
		{
			int volume = Volume;
			Volume = Amount;
			try
			{
				SmearOn(obj, PouredBy, FromCell);
				return;
			}
			finally
			{
				Volume = volume;
			}
		}
		obj.ApplyEffect(new LiquidCovered(this, Amount, 1, Poured, PouredBy, FromCell));
	}

	public int ProcessContact(GameObject obj, bool Initial = false, bool Prone = false, bool Poured = false, GameObject PouredBy = null, bool FromCell = false, int ContactVolume = -1)
	{
		if (ParentObject != null)
		{
			if (ParentObject.IsFrozen())
			{
				return 0;
			}
			if (ParentObject.IsInStasis())
			{
				return 0;
			}
		}
		Physics physics = obj.Physics;
		if (physics == null || !physics.IsReal)
		{
			return 0;
		}
		if (obj.IsScenery)
		{
			return 0;
		}
		if (FromCell)
		{
			LiquidCovered effect = obj.GetEffect<LiquidCovered>();
			if (effect != null && effect.FromCell)
			{
				if (LiquidSameAs(effect.Liquid))
				{
					return 0;
				}
				effect.FromCell = false;
			}
		}
		int volume = Volume;
		if (ContactVolume == -1)
		{
			ContactVolume = Volume;
		}
		bool flag = false;
		int maximumLiquidExposure = obj.GetMaximumLiquidExposure();
		if (maximumLiquidExposure <= 0)
		{
			return 0;
		}
		Body body = obj.Body;
		List<GameObject> list = null;
		if (body != null)
		{
			list = Event.NewGameObjectList();
			body.GetEquippedObjectsExceptNatural(list);
		}
		bool TemporaryKnown = false;
		bool Temporary = false;
		if (!Poured && IsSwimmableFor(obj))
		{
			flag = true;
			if (Initial && obj.IsPlayer())
			{
				IComponent<GameObject>.XDidYToZ(obj, "swim", "through", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
			}
			if (obj.IsCreature && !obj.HasEffect<Swimming>() && obj.IsPotentiallyMobile())
			{
				obj.ApplyEffect(new Swimming());
			}
			int num = volume / 2;
			int num2 = Math.Min(Math.Min(maximumLiquidExposure, num), ContactVolume);
			if (num2 > 0)
			{
				int num3 = num2 - GetCurrentCoverIfSame(obj);
				if (num3 > 0)
				{
					LiquidInContact(obj, num3, ref TemporaryKnown, ref Temporary, Poured, PouredBy, FromCell);
				}
				flag = true;
				ContactVolume -= num2;
			}
			if (list != null && list.Count > 0 && Volume >= num && ContactVolume > 0)
			{
				if (list.Count > 1)
				{
					list.ShuffleInPlace();
				}
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					num2 = Math.Min(Math.Min(GetAdsorbableDrams(list[i]), Volume / 2), ContactVolume);
					if (num2 > 0)
					{
						int num4 = num2 - GetCurrentCoverIfSame(list[i]);
						if (num4 > 0)
						{
							LiquidInContact(list[i], num4, ref TemporaryKnown, ref Temporary, Poured, PouredBy, FromCell);
						}
						flag = true;
						ContactVolume -= num2;
					}
					if (Volume < num || ContactVolume <= 0)
					{
						break;
					}
				}
			}
			if (Volume >= num && ContactVolume > 0)
			{
				Inventory inventory = obj.Inventory;
				List<GameObject> list2 = null;
				if (inventory != null)
				{
					list2 = Event.NewGameObjectList();
					list2.AddRange(inventory.Objects);
				}
				if (list2 != null && list2.Count > 0)
				{
					if (list2.Count > 1)
					{
						list2.ShuffleInPlace();
					}
					int j = 0;
					for (int count2 = list2.Count; j < count2; j++)
					{
						if (list2[j].Weight == 0)
						{
							continue;
						}
						num2 = Math.Min(Math.Min(GetAdsorbableDrams(list2[j]), Volume / 2), ContactVolume);
						if (num2 > 0)
						{
							int num5 = num2 - GetCurrentCoverIfSame(list2[j]);
							if (num5 > 0)
							{
								LiquidInContact(list2[j], num5, ref TemporaryKnown, ref Temporary, Poured, PouredBy, FromCell);
							}
							flag = true;
							ContactVolume -= num2;
						}
						if (Volume < num || ContactVolume <= 0)
						{
							break;
						}
					}
				}
			}
		}
		else
		{
			if (!Poured && Volume > 0)
			{
				flag = true;
			}
			if (!Poured && IsWadingDepth())
			{
				if (Initial && obj.IsPlayer())
				{
					IComponent<GameObject>.XDidYToZ(obj, "wade", "through", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
				}
				if (obj.IsCreature && !obj.HasEffect<Wading>() && obj.IsPotentiallyMobile())
				{
					obj.ApplyEffect(new Wading());
				}
			}
			int num6 = (Poured ? Math.Max(ContactVolume / 2, 1) : 0);
			int num8;
			if (Poured)
			{
				double num7 = 1.0;
				num8 = ContactVolume;
			}
			else
			{
				double num7;
				if (body == null)
				{
					num7 = 0.2;
				}
				else if (Prone)
				{
					num7 = 0.4;
				}
				else
				{
					num7 = ((body.GetBodyMobility() <= 0 && body.GetTotalMobility() > 0) ? 0.1 : 0.3);
				}
				num7 *= 1.0 + (double)ContactVolume / 300.0;
				if (num7 >= 1.0)
				{
					num7 = 1.0;
					num8 = ContactVolume;
				}
				else
				{
					num8 = (int)((double)ContactVolume * num7);
				}
			}
			int num9 = (Poured ? 100 : (ContactVolume + num8));
			int num10;
			if (num9 >= 100)
			{
				num10 = Math.Min(ContactVolume, num8);
			}
			else
			{
				num10 = 0;
				for (int k = 0; k < ContactVolume; k++)
				{
					if (num9.in100())
					{
						num10++;
						if (num10 >= num8)
						{
							break;
						}
					}
				}
			}
			if (num10 > 0)
			{
				int adsorbableDrams = GetAdsorbableDrams(obj);
				num10 -= num6;
				if (body != null)
				{
					BodyParts.Clear();
					body.GetConcreteParts(BodyParts);
					BodyPartExposure.Clear();
					BodyPartCapacity.Clear();
					BodyPartExposure.Clear();
					int num11 = 0;
					int num12 = 0;
					if (!Poured)
					{
						MobilityBodyParts.Clear();
						body.GetConcreteMobilityProvidingParts(MobilityBodyParts);
						if (MobilityBodyParts.Count > 0)
						{
							if (MobilityBodyParts.Count > 1)
							{
								MobilityBodyParts.ShuffleInPlace();
							}
							int l = 0;
							for (int count3 = MobilityBodyParts.Count; l < count3; l++)
							{
								BodyPart bodyPart = MobilityBodyParts[l];
								if (bodyPart.Equipped != null)
								{
									int adsorbableDrams2 = GetAdsorbableDrams(bodyPart.Equipped);
									if (adsorbableDrams2 > 0)
									{
										BodyPartCapacity.Add(bodyPart, adsorbableDrams2);
									}
								}
								else
								{
									BodyPartCapacity.Add(bodyPart, Math.Max(maximumLiquidExposure / BodyParts.Count, 1));
								}
							}
							int total = 0;
							for (int m = 0; m < num10; m++)
							{
								BodyPart randomElement = BodyPartCapacity.GetRandomElement(ref total);
								if (randomElement == null)
								{
									break;
								}
								if (BodyPartExposure.TryGetValue(randomElement, out var value))
								{
									if (value < BodyPartCapacity[randomElement])
									{
										BodyPartExposure[randomElement]++;
										num12++;
									}
								}
								else
								{
									BodyPartExposure.Add(randomElement, 1);
									num12++;
								}
							}
							if (num10 > num12 && BodyParts.Count > MobilityBodyParts.Count)
							{
								num11 = (Volume - 100) / 2;
							}
						}
						else
						{
							num11 = 100;
						}
					}
					else
					{
						num11 = 100;
					}
					if (num11 > 0)
					{
						int num13 = 0;
						if (num11 >= 100)
						{
							num13 = num10;
						}
						else
						{
							for (int n = num12; n < num10; n++)
							{
								if (num11.in100())
								{
									num13++;
								}
							}
						}
						if (num13 > 0)
						{
							int num14 = 0;
							for (int count4 = BodyParts.Count; num14 < count4; num14++)
							{
								BodyPart bodyPart2 = BodyParts[num14];
								if (BodyPartCapacity.ContainsKey(bodyPart2))
								{
									continue;
								}
								if (bodyPart2.Equipped != null)
								{
									int adsorbableDrams3 = GetAdsorbableDrams(bodyPart2.Equipped);
									if (adsorbableDrams3 > 0)
									{
										BodyPartCapacity.Add(bodyPart2, adsorbableDrams3);
									}
								}
								else
								{
									BodyPartCapacity.Add(bodyPart2, Math.Max(maximumLiquidExposure / BodyParts.Count, 1));
								}
							}
							int total2 = 0;
							for (int num15 = 0; num15 < num13; num15++)
							{
								BodyPart randomElement2 = BodyPartCapacity.GetRandomElement(ref total2);
								if (randomElement2 == null)
								{
									break;
								}
								if (BodyPartExposure.TryGetValue(randomElement2, out var value2))
								{
									if (value2 < BodyPartCapacity[randomElement2])
									{
										BodyPartExposure[randomElement2]++;
										num12++;
									}
								}
								else
								{
									BodyPartExposure.Add(randomElement2, 1);
									num12++;
								}
							}
						}
					}
					foreach (KeyValuePair<BodyPart, int> item in BodyPartExposure)
					{
						if (item.Key.Equipped != null && !item.Key.Equipped.HasTag("NaturalGear"))
						{
							num10 -= item.Value;
							int num16 = item.Value - GetCurrentCoverIfSame(item.Key.Equipped);
							if (num16 > 0)
							{
								LiquidInContact(item.Key.Equipped, num16, ref TemporaryKnown, ref Temporary, Poured, PouredBy, FromCell);
							}
						}
					}
				}
				num10 += num6;
				if (num10 > 0)
				{
					int num17 = Math.Min(num10 - GetCurrentCoverIfSame(obj), adsorbableDrams);
					if (num17 > 0)
					{
						LiquidInContact(obj, num17, ref TemporaryKnown, ref Temporary, Poured, PouredBy, FromCell);
					}
				}
				flag = true;
			}
		}
		if (flag && obj.IsPlayer() && !AutoAct.IsActive(IgnoreAutoget: true))
		{
			Splash(obj.CurrentCell);
		}
		return volume - Volume;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		GameObject gameObject = E.Object;
		if (!E.IgnoreGravity && CanInteractWith(gameObject, E.Cell))
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
				{
					keylist.Add(componentLiquid.Key);
				}
				foreach (string item in keylist)
				{
					BaseLiquid baseLiquid = Liquids[item];
					baseLiquid.ObjectEnteredCell(this, E);
					baseLiquid.ObjectEnteredCell(this, gameObject);
				}
				if (IsOpenVolume())
				{
					ProcessContact(gameObject, Initial: true, Prone: false, Poured: false, null, FromCell: true);
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GravitationEvent E)
	{
		GameObject gameObject = E.Object;
		if (CanInteractWith(gameObject, E.Cell))
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
				{
					keylist.Add(componentLiquid.Key);
				}
				foreach (string item in keylist)
				{
					BaseLiquid baseLiquid = Liquids[item];
					baseLiquid.ObjectEnteredCell(this, E);
					baseLiquid.ObjectEnteredCell(this, gameObject);
				}
				if (IsOpenVolume())
				{
					ProcessContact(gameObject, Initial: true, Prone: false, Poured: false, null, FromCell: true);
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectGoingProneEvent E)
	{
		GameObject obj = E.Object;
		if (CanInteractWith(obj, E.Cell))
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				keylist.AddRange(ComponentLiquids.Keys);
				foreach (string item in keylist)
				{
					Liquids[item].ObjectGoingProne(this, E.Object, E.UsePopups);
				}
				if (IsOpenVolume())
				{
					ProcessContact(obj, Initial: false, Prone: true, Poured: false, null, FromCell: true);
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectStoppedFlyingEvent E)
	{
		GameObject obj = E.Object;
		if (CanInteractWith(obj, E.Cell))
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				keylist.AddRange(ComponentLiquids.Keys);
				foreach (string item in keylist)
				{
					Liquids[item].ObjectInCell(this, E.Object);
				}
				if (IsOpenVolume())
				{
					ProcessContact(obj, Initial: true, Prone: false, Poured: false, null, FromCell: true);
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
			}
		}
		return base.HandleEvent(E);
	}

	public void ProcessExposure(GameObject GO, GameObject By = null, bool FromCell = false)
	{
		if (!GameObject.Validate(ref GO))
		{
			return;
		}
		double num = Math.Min(Volume, GO.GetMaximumLiquidExposureAsDouble());
		if (num <= 0.0)
		{
			return;
		}
		int liquidThermalConductivity = GetLiquidThermalConductivity();
		if (liquidThermalConductivity <= 0)
		{
			return;
		}
		int liquidTemperature = GetLiquidTemperature();
		int temperature = GO.Physics.Temperature;
		int num2 = liquidTemperature - temperature;
		if (num2 == 0 || (ParentObject != null && !GO.PhaseMatches(ParentObject)))
		{
			return;
		}
		int liquidCombustibility = GetLiquidCombustibility();
		bool flag = GO.IsAflame();
		if (flag && liquidCombustibility >= 50)
		{
			GO.TemperatureChange((int)(num.DiminishingReturns(8.0) * (double)liquidCombustibility / 50.0), By);
			return;
		}
		double num3 = (double)num2 * num.DiminishingReturns(8.0) / 4.0;
		if (num2 > 0)
		{
			if (num3 > (double)num2)
			{
				num3 = (float)num2;
			}
		}
		else if (num3 < (double)num2)
		{
			num3 = (float)num2;
		}
		if (flag && liquidCombustibility != 0)
		{
			num3 = ((!(num3 > 0.0)) ? (num3 * (double)(100 - liquidCombustibility) / 100.0) : (num3 * (double)(100 + liquidCombustibility) / 100.0));
		}
		if (liquidThermalConductivity != 100)
		{
			num3 = num3 * (double)liquidThermalConductivity / 100.0;
		}
		if (num3 > 0.0)
		{
			GameObject gameObject = GO;
			int amount = (int)num3;
			int? max = liquidTemperature;
			gameObject.TemperatureChange(amount, By, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, 0, null, max);
		}
		else
		{
			GO.TemperatureChange((int)num3, By, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, 0, liquidTemperature);
		}
	}

	public override bool HandleEvent(GetGameObjectSortEvent E)
	{
		if (E.Category1 == "Water Containers" && E.Category2 == "Water Containers")
		{
			LiquidVolume liquidVolume = E.Object1?.LiquidVolume;
			LiquidVolume liquidVolume2 = E.Object2?.LiquidVolume;
			int num = (liquidVolume2 != null).CompareTo(liquidVolume != null);
			if (num != 0)
			{
				E.Sort = num;
				return false;
			}
			if (liquidVolume != null && liquidVolume2 != null)
			{
				int num2 = liquidVolume.IsEmpty().CompareTo(liquidVolume2.IsEmpty());
				if (num2 != 0)
				{
					E.Sort = num2;
					return false;
				}
				int num3 = liquidVolume2.GetLiquidExtrinsicValuePerDram().CompareTo(liquidVolume.GetLiquidExtrinsicValuePerDram());
				if (num3 != 0)
				{
					E.Sort = num3;
					return false;
				}
				int num4 = liquidVolume.GetLiquidName().CompareTo(liquidVolume2.GetLiquidName());
				if (num4 != 0)
				{
					E.Sort = num4;
					return false;
				}
				int num5 = liquidVolume2.Volume.CompareTo(liquidVolume.Volume);
				if (num5 != 0)
				{
					E.Sort = num5;
					return false;
				}
			}
			int num6 = string.Compare(E.Object1?.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true), E.Object2?.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true), ignoreCase: true);
			if (num6 != 0)
			{
				E.Sort = num6;
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.ForSort && ParentObject.GetInventoryCategory() == "Water Containers")
		{
			E.ReplaceEntirety("Water Containers");
			return false;
		}
		if (E.GetPrimaryBase() == "pool")
		{
			if (IsSwimmingDepth())
			{
				E.AddAdjective("deep");
			}
			else if (!IsWadingDepth())
			{
				E.ReplacePrimaryBase("puddle");
			}
		}
		if (!E.Reference && (E.AsIfKnown || GetEpistemicStatus() != 0))
		{
			if (IsEmpty())
			{
				if (!UsesNamePreposition())
				{
					if (EffectivelySealed())
					{
						if (LiquidVisibleWhenSealed)
						{
							if (ShowSeal)
							{
								if (E.NoColor)
								{
									E.AddTag("[empty, sealed]", -5);
								}
								else
								{
									E.AddTag("{{y|[{{K|empty, {{c|sealed}}}}]}}", -5);
								}
							}
							else if (E.NoColor)
							{
								E.AddTag("[empty]", -5);
							}
							else
							{
								E.AddTag("{{y|[{{K|empty}}]}}", -5);
							}
						}
						else if (ShowSeal)
						{
							if (E.NoColor)
							{
								E.AddTag("[sealed]", -5);
							}
							else
							{
								E.AddTag("{{y|[{{c|sealed}}]}}", -5);
							}
						}
					}
					else if (E.NoColor)
					{
						E.AddTag("[empty]", -5);
					}
					else
					{
						E.AddTag("{{y|[{{K|empty}}]}}", -5);
					}
				}
			}
			else
			{
				string liquidDescription = GetLiquidDescription(E.Cutoff > 1040, IgnoreSeal: false, E.NoColor, Syntactic: false, E.ForSort);
				if (!liquidDescription.IsNullOrEmpty())
				{
					if (UsesNamePreposition())
					{
						E.AddClause(liquidDescription);
					}
					else
					{
						E.AddTag(liquidDescription, -5);
					}
				}
			}
			if (AutoCollectLiquidType != null)
			{
				if (Volume == 0 || !IsPureLiquid(AutoCollectLiquidType))
				{
					if (Liquids.ContainsKey(AutoCollectLiquidType) && GetLiquid(AutoCollectLiquidType) != null)
					{
						if (E.NoColor)
						{
							E.AddTag("[auto-collecting " + Liquids[AutoCollectLiquidType].GetName(this).Strip() + "]", 20);
						}
						else
						{
							E.AddTag("{{y|[{{c|auto-collecting " + Liquids[AutoCollectLiquidType].GetName(this) + "}}]}}", 20);
						}
					}
				}
				else if (E.NoColor)
				{
					E.AddTag("[auto-collecting]", 20);
				}
				else
				{
					E.AddTag("{{y|[{{c|auto-collecting}}]}}", 20);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ParentObject.HasTagOrProperty("Pool") && Volume > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (IsSwimmingDepth())
			{
				stringBuilder.Append("Light scatters off the surface of the wide and yawning pool and gets lost in its depths.");
			}
			else if (IsWadingDepth())
			{
				stringBuilder.Append("Liquid pools into a cistern of air and ground.");
			}
			else
			{
				stringBuilder.Append("Liquid dots the ground in shallow pools.");
			}
			E.Base = stringBuilder;
		}
		if (Volume > 0 && (!EffectivelySealed() || LiquidVisibleWhenSealed))
		{
			List<string> list = null;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				if (componentLiquid.Value <= 0)
				{
					continue;
				}
				List<string> list2 = Liquids[componentLiquid.Key].GetPreparedCookingIngredient().CachedCommaExpansion();
				if (list == null)
				{
					list = list2.ToList();
					continue;
				}
				int i = 0;
				for (int count = list2.Count; i < count; i++)
				{
					if (!list.Contains(list2[i]))
					{
						list.Add(list2[i]);
					}
				}
			}
			if (list != null && (IsOpenVolume() || (ParentObject != null && ParentObject.HasTagOrProperty("WaterContainer"))))
			{
				List<string> list3 = null;
				foreach (string item in list)
				{
					string key = "ProceduralCookingIngredient_" + item;
					if (!GameObjectFactory.Factory.Blueprints.ContainsKey(key))
					{
						continue;
					}
					string tag = GameObjectFactory.Factory.Blueprints[key].GetTag("Description");
					if (!tag.IsNullOrEmpty())
					{
						if (list3 == null)
						{
							list3 = new List<string>();
						}
						list3.Add(tag);
					}
				}
				if (list3 != null)
				{
					E.Postfix.AppendRules("Adds " + Grammar.MakeOrList(list3) + " effects to cooked meals.");
				}
			}
		}
		if (Sealed)
		{
			E.Postfix.AppendRules("Sealed: The liquid contained inside can't be accessed.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
		{
			stringBuilder.Compound(componentLiquid.Key, '\n').Append('-').Append(componentLiquid.Value);
		}
		E.AddEntry(this, "ComponentLiquids", stringBuilder.ToString());
		E.AddEntry(this, "Primary", Primary);
		E.AddEntry(this, "Secondary", Secondary);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSlottedInventoryActionsEvent E)
	{
		if (GetEpistemicStatus() != 0 && !EffectivelySealed())
		{
			E.AddAction("FillSlotted", "fill " + ParentObject.BaseDisplayNameStripped, "FillFrom", null, 'f', FireOnActor: false, 50 - Volume * 50 / MaxVolume, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: true, ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		int epistemicStatus = GetEpistemicStatus();
		if (epistemicStatus != 0)
		{
			if (ManualSeal)
			{
				if (Sealed)
				{
					E.AddAction("Unseal", "unseal", "Unseal", null, 'u', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				}
				else
				{
					E.AddAction("Seal", "seal", "Seal", null, 's', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				}
			}
			if (!EffectivelySealed())
			{
				if (!IsOpenVolume())
				{
					E.AddAction("Fill", "fill", "FillFrom", null, 'f');
					if (HasDrain)
					{
						E.AddAction("Drain", "drain", "Drain", null, 'r', FireOnActor: false, -1);
					}
				}
				if (Volume > 0)
				{
					int num = 0;
					if ((ParentObject.Equipped != null || ParentObject.InInventory != null) && !ParentObject.HasPart<MissileWeapon>())
					{
						num = 20;
					}
					E.AddAction("Drink", "drink", "Drink", null, 'k');
					E.AddAction("Pour", "pour", "Pour", null, 'p', FireOnActor: false, num);
					bool flag = false;
					bool flag2 = false;
					int num2 = 10;
					if (ParentObject.InInventory == E.Actor)
					{
						flag = true;
					}
					else
					{
						flag2 = true;
						if (ParentObject.Equipped == E.Actor)
						{
							flag = true;
							num2 = -1;
						}
					}
					if (flag && epistemicStatus == 2)
					{
						if (AutoCollectLiquidType != null)
						{
							E.AddAction("AutoCollect", "stop auto-collecting liquid", "AutoCollectLiquid", null, 'a', FireOnActor: false, -1);
						}
						else
						{
							E.AddAction("AutoCollect", "auto-collect liquid", "AutoCollectLiquid", null, 'a', FireOnActor: false, -1);
						}
					}
					if (flag2)
					{
						E.AddAction("Collect", "collect liquid", "CollectLiquid", null, 'c', FireOnActor: false, num2);
					}
					if (UsableForCleaning() && CheckAnythingToCleanEvent.Check(IComponent<GameObject>.ThePlayer, ParentObject))
					{
						E.AddAction("CleanAll", "clean all your items [1 dram]", "CleanWithLiquid", null, 'n', FireOnActor: false, 20);
					}
				}
				else if (AutoCollectLiquidType != null)
				{
					if (epistemicStatus == 2)
					{
						E.AddAction("AutoCollect", "stop auto-collecting liquid", "AutoCollectLiquid", null, 'a', FireOnActor: false, -1);
					}
				}
				else if ((ParentObject.InInventory == E.Actor || ParentObject.Equipped == E.Actor) && epistemicStatus == 2 && !GetPreferredLiquidEvent.GetFor(ParentObject, E.Actor).IsNullOrEmpty())
				{
					E.AddAction("AutoCollect", "auto-collect liquid", "AutoCollectLiquid", null, 'a', FireOnActor: false, -1);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		if (IsFreshWater())
		{
			E.Value += Volume;
		}
		else
		{
			bool pure = IsPure();
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				float num = (float)(Volume * componentLiquid.Value) * Liquids[componentLiquid.Key].GetExtrinsicValuePerDram(pure) / 1000f;
				E.Value += num;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		E.Weight += GetLiquidWeight();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Drink")
		{
			if (!EffectivelySealed())
			{
				CanDrinkEvent.GetFor(E.Actor, this, out var CanDrinkThis, out var CouldDrinkOther);
				if (!CanDrinkThis)
				{
					if (CouldDrinkOther)
					{
						E.Actor.Fail("You are unable to drink that.");
					}
					else
					{
						E.Actor.Fail("You are unable to drink liquids.");
					}
					return false;
				}
				if (ParentObject.IsInStasis())
				{
					E.Actor.Fail("You cannot seem to interact with " + ParentObject.t() + " in any way.");
					return false;
				}
				if (E.Actor.IsPlayer() && ConsiderLiquidDangerousToDrink() && Options.ConfirmDangerousLiquid && Popup.WarnYesNoCancel("Are you sure you want to drink that?") != DialogResult.Yes)
				{
					return false;
				}
				if (ParentObject.IsTemporary)
				{
					PlayWorldSound("Sounds/Interact/sfx_interact_liquidContainer_drink");
					if (E.Actor.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("It's fizzy.");
					}
					Empty();
					return false;
				}
				if (ParentObject.FireEvent(Event.New("BeforeDrink", "Drinker", E.Actor), E))
				{
					int value = 0;
					if (E.Actor.IsPlayer() && ParentObject.Owner != null)
					{
						if (Popup.ShowYesNoCancel(ParentObject.IndicativeDistal + ParentObject.Is + " not owned by you. Are you sure you want to drink from " + ParentObject.them + "?") != DialogResult.Yes)
						{
							return false;
						}
						ParentObject.Physics.BroadcastForHelp(E.Actor);
					}
					if (!E.Actor.FireEvent(Event.New("DrinkingFrom", "Container", ParentObject)))
					{
						return false;
					}
					bool flag = false;
					if (Volume > 0)
					{
						StringBuilder stringBuilder = Event.NewStringBuilder();
						int num = 0;
						int num2 = 0;
						Stomach part = E.Actor.GetPart<Stomach>();
						if (part != null)
						{
							num = part.Water;
							num2 = part.HungerLevel;
						}
						List<string> keylist = getKeylist();
						try
						{
							keylist.Clear();
							keylist.AddRange(ComponentLiquids.Keys);
							foreach (string item in keylist)
							{
								if (!Liquids[item].Drank(this, 1, E.Actor, stringBuilder, E))
								{
									if (stringBuilder.Length > 0)
									{
										Popup.ShowBlock(stringBuilder.ToString());
									}
									return false;
								}
							}
						}
						finally
						{
							keylistPool.Enqueue(keylist);
						}
						if (IsFreshWater())
						{
							value = 1;
						}
						UseDram();
						if (part != null && (part.Water != num || part.HungerLevel != num2))
						{
							stringBuilder.Compound("You are now ");
							if (part.Water != num)
							{
								stringBuilder.Append(part.WaterStatus());
								if (part.HungerLevel != num2)
								{
									stringBuilder.Append(" and ");
								}
							}
							if (part.HungerLevel != num2)
							{
								stringBuilder.Append(part.FoodStatus());
							}
							stringBuilder.Append('.');
						}
						PlayWorldSound("sfx_interact_liquidContainer_drink");
						if (E.Actor.IsPlayer() && stringBuilder.Length > 0)
						{
							Popup.ShowBlock(stringBuilder.ToString());
						}
						if (Volume <= 0)
						{
							Empty(WillCheckImage: true);
						}
						flag = true;
					}
					else
					{
						E.Actor.Fail("It's empty, there's nothing left to drink!");
						Empty(WillCheckImage: true);
					}
					CheckImage();
					if (!flag)
					{
						return false;
					}
					if (E.Actor != null)
					{
						if (E.Actor.HasRegisteredEvent("Drank"))
						{
							E.Actor.FireEvent(Event.New("Drank", "Object", ParentObject, "WasFreshWater", value));
						}
						E.Actor.UseEnergy(1000, "Item Drink");
						E.RequestInterfaceExit();
					}
				}
			}
		}
		else if (E.Command == "Pour")
		{
			Pour(Actor: E.Actor, RequestInterfaceExit: ref E.InterfaceExit);
		}
		else if (E.Command == "Fill")
		{
			PerformFill(E.Actor, ref E.InterfaceExit, E.OwnershipHandled);
		}
		else if (E.Command == "Drain")
		{
			if (!HasDrain)
			{
				E.Actor.Fail(ParentObject.Does("have") + " no drain.");
				return false;
			}
			if (EffectivelySealed())
			{
				E.Actor.Fail(ParentObject.Does("are") + " sealed.");
				return false;
			}
			if (ParentObject.IsInStasis())
			{
				E.Actor.Fail("You cannot seem to interact with " + ParentObject.t() + " in any way.");
				return false;
			}
			if (Volume <= 0)
			{
				E.Actor.Fail(ParentObject.Does("are") + " empty.");
				return false;
			}
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			if (E.Actor.IsPlayer())
			{
				if (ParentObject.Owner != null && !E.OwnershipHandled)
				{
					if (Popup.ShowYesNoCancel(ParentObject.IndicativeDistal + ParentObject.Is + " not owned by you. Are you sure you want to drain " + ParentObject.them + "?") != DialogResult.Yes)
					{
						return false;
					}
					ParentObject.Physics.BroadcastForHelp(E.Actor);
				}
				else if (Popup.ShowYesNoCancel("Are you sure you want to drain " + ParentObject.t() + "?") != DialogResult.Yes)
				{
					return false;
				}
			}
			EmptyIntoCell(null, E.Actor);
		}
		else if (E.Command == "FillFrom")
		{
			if (EffectivelySealed())
			{
				E.Actor.Fail(ParentObject.Does("are") + " sealed.");
				return false;
			}
			if (ParentObject.IsInStasis())
			{
				E.Actor.Fail("You cannot seem to interact with " + ParentObject.t() + " in any way.");
				return false;
			}
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			if (E.Actor.IsPlayer() && ParentObject.Owner != null && !E.OwnershipHandled)
			{
				if (Popup.ShowYesNoCancel(ParentObject.IndicativeDistal + ParentObject.Is + " not owned by you. Are you sure you want to fill " + ParentObject.them + "?") != DialogResult.Yes)
				{
					return false;
				}
				ParentObject.Physics.BroadcastForHelp(E.Actor);
			}
			List<GameObject> list = Event.NewGameObjectList();
			List<GameObject> list2 = Event.NewGameObjectList();
			List<GameObject> list3 = Event.NewGameObjectList();
			List<GameObject> list4 = Event.NewGameObjectList();
			E.Actor.GetInventoryAndEquipment(list4);
			int i = 0;
			for (int count = list4.Count; i < count; i++)
			{
				GameObject gameObject = list4[i];
				if (gameObject == ParentObject)
				{
					continue;
				}
				LiquidVolume liquidVolume = gameObject.LiquidVolume;
				if (liquidVolume != null && !liquidVolume.EffectivelySealed() && liquidVolume.Volume > 0)
				{
					if (liquidVolume.LiquidSameAs(this))
					{
						list.Add(gameObject);
					}
					else
					{
						list2.Add(gameObject);
					}
				}
			}
			list3.AddRange(list);
			list3.AddRange(list2);
			if (list3.Count == 0)
			{
				E.Actor.Fail("You have no containers to pour from.");
				return true;
			}
			GameObject gameObject2 = PickItem.ShowPicker(list3, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor, null, null, "[Select a container to fill from]", PreserveOrder: true);
			if (gameObject2 != null)
			{
				try
				{
					ParentObject.SplitFromStack();
					if (gameObject2 == ParentObject)
					{
						E.Actor.Fail("You can't pour from a container into " + ParentObject.itself + ".");
						return false;
					}
					if (gameObject2.IsTemporary)
					{
						E.Actor.Fail("It's fizzy.");
						return false;
					}
					LiquidVolume liquidVolume2 = gameObject2.LiquidVolume;
					bool flag2 = false;
					if (Volume > 0 && !liquidVolume2.LiquidSameAs(this))
					{
						switch (Popup.ShowYesNoCancel("Do you want to empty " + ParentObject.t() + " first?"))
						{
						case DialogResult.Cancel:
							return true;
						case DialogResult.Yes:
							flag2 = true;
							break;
						}
					}
					int num3 = Math.Min(flag2 ? MaxVolume : (MaxVolume - Volume), liquidVolume2.Volume);
					int? num4 = Popup.AskNumber("How many drams? (max=" + num3 + ")", "Sounds/UI/ui_notification", "", num3, 0, num3);
					int num5 = 0;
					try
					{
						num5 = Convert.ToInt32(num4);
					}
					catch
					{
						return true;
					}
					if (num5 > liquidVolume2.Volume)
					{
						num5 = liquidVolume2.Volume;
					}
					if (num5 <= 0)
					{
						return true;
					}
					if (flag2)
					{
						EmptyIntoCell(null, E.Actor);
					}
					int num6 = MaxVolume - Volume;
					int num7 = 0;
					num7 = ((num6 >= liquidVolume2.Volume) ? liquidVolume2.Volume : num6);
					if (num7 > num5)
					{
						num7 = num5;
					}
					MixWith(liquidVolume2, Amount: num7, PouredFrom: gameObject2, PouredBy: E.Actor, RequestInterfaceExit: ref E.InterfaceExit);
					CheckImage();
					liquidVolume2.CheckImage();
				}
				finally
				{
					ParentObject.CheckStack();
				}
			}
		}
		else if (E.Command == "CollectLiquid")
		{
			if (Volume <= 0 || EffectivelySealed())
			{
				return false;
			}
			if (!E.Actor.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			if (ParentObject.IsInStasis())
			{
				E.Actor.Fail("You cannot seem to interact with " + ParentObject.t() + " in any way.");
				return false;
			}
			if (ParentObject.IsTemporary)
			{
				E.Actor.Fail("It's fizzy.");
				Empty();
				return false;
			}
			string liquidDesignation = GetLiquidDesignation();
			int num8 = (E.Auto ? E.Actor.GetAutoCollectDrams(liquidDesignation, ParentObject) : E.Actor.GetStorableDrams(liquidDesignation, ParentObject));
			if (num8 <= 0)
			{
				if (!E.Auto)
				{
					E.Actor.Fail("You have nowhere available to collect that.");
				}
			}
			else
			{
				if (E.Actor.IsPlayer() && ParentObject.Owner != null && !E.OwnershipHandled)
				{
					if (E.Auto || Popup.ShowYesNoCancel(ParentObject.IndicativeDistal + ParentObject.Is + " not owned by you. Are you sure you want to collect from " + ParentObject.them + "?") != DialogResult.Yes)
					{
						goto IL_1614;
					}
					ParentObject.Physics.BroadcastForHelp(E.Actor);
				}
				int num9 = Math.Min(num8, Volume);
				if (!E.Actor.IsPlayer() || E.Auto || num9 <= 128 || Popup.ShowYesNoCancel("You are able to collect " + num9.Things("dram") + " of " + GetLiquidName() + ". Are you sure you want to?") == DialogResult.Yes)
				{
					List<GameObject> list5 = Event.NewGameObjectList();
					E.Actor.GiveDrams(num9, liquidDesignation, skip: ParentObject, auto: E.Auto, skipList: null, filter: null, storedIn: list5, safeOnly: true, liquidVolume: this);
					StringBuilder stringBuilder2 = Event.NewStringBuilder();
					stringBuilder2.Append(E.Actor.Does("collect")).Append(' ').Append(num9.Things("dram"))
						.Append(" of ")
						.Append(GetLiquidName());
					if (IsOpenVolume())
					{
						stringBuilder2.Append(' ').Append(E.Actor.DescribeDirectionFrom(ParentObject));
					}
					else
					{
						stringBuilder2.Append(" from ").Append(ParentObject.t()).Append(' ')
							.Append(E.Actor.DescribeDirectionToward(ParentObject));
					}
					if (list5.Count > 0)
					{
						List<string> list6 = new List<string>();
						List<bool> list7 = new List<bool>();
						List<string> list8 = new List<string>();
						Dictionary<string, int> dictionary = new Dictionary<string, int>();
						foreach (GameObject item2 in list5)
						{
							string displayNameOnly = item2.DisplayNameOnly;
							if (item2.HasProperName)
							{
								list8.Add(displayNameOnly);
								continue;
							}
							if (list6.Contains(displayNameOnly))
							{
								dictionary[displayNameOnly]++;
								continue;
							}
							list6.Add(displayNameOnly);
							list7.Add(item2.IsPlural);
							dictionary.Add(displayNameOnly, 1);
						}
						stringBuilder2.Append(" in ");
						if (list8.Count > 0)
						{
							stringBuilder2.Append(Grammar.MakeAndList(list8));
							if (list6.Count > 0)
							{
								stringBuilder2.Append(" and ");
							}
						}
						if (list6.Count > 0)
						{
							int j = 0;
							for (int count2 = list6.Count; j < count2; j++)
							{
								if (!list7[j] && dictionary[list6[j]] > 1)
								{
									list6[j] = Grammar.Pluralize(list6[j]);
								}
							}
							stringBuilder2.Append(E.Actor.its).Append(' ').Append(Grammar.MakeAndList(list6));
						}
					}
					stringBuilder2.Append('.');
					EmitMessage(E.Actor, stringBuilder2, ' ', !E.Auto);
					Volume -= num9;
					Update();
					E.Actor.UseEnergy(1000, "Item Liquid Collect");
				}
			}
		}
		else if (E.Command == "CleanWithLiquid")
		{
			bool flag3 = true;
			if (ParentObject.IsInStasis())
			{
				E.Actor.Fail("You cannot seem to interact with " + ParentObject.t() + " in any way.");
				return false;
			}
			if (!E.Actor.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			if (E.Actor.IsPlayer() && ParentObject.Owner != null && !E.OwnershipHandled)
			{
				if (Popup.ShowYesNoCancel(ParentObject.IndicativeDistal + ParentObject.Is + " not owned by you. Are you sure you want to use " + GetLiquidName() + " from " + ParentObject.them + "?") != DialogResult.Yes)
				{
					flag3 = false;
				}
				else
				{
					ParentObject.Physics.BroadcastForHelp(E.Actor);
				}
			}
			if (flag3)
			{
				List<GameObject> Objects = null;
				List<string> Types = null;
				if (UsableForCleaning())
				{
					CleanItemsEvent.PerformFor(E.Actor, E.Actor, ParentObject, out Objects, out Types);
				}
				if (Objects != null && Objects.Count > 0)
				{
					CleaningMessage(E.Actor, Objects, Types, ParentObject, this, UseDram: true);
					E.Actor.UseEnergy(1000, "Cleaning");
					E.RequestInterfaceExit();
				}
				else
				{
					E.Actor.Fail("You cannot do that for some reason.");
				}
			}
		}
		else if (E.Command == "AutoCollectLiquid")
		{
			if (AutoCollectLiquidType != null)
			{
				AutoCollectLiquidType = null;
			}
			else if (Sealed || ComponentLiquids.Count > 1)
			{
				E.Actor.Fail("Auto collection only works on unsealed containers with pure liquids.");
			}
			else if (ComponentLiquids.Count == 0)
			{
				AutoCollectLiquidType = GetPreferredLiquidEvent.GetFor(ParentObject, E.Actor);
				if (AutoCollectLiquidType == null)
				{
					E.Actor.Fail("It isn't clear what kind of liquid would be appropriate for " + ParentObject.t() + " to collect. Pour a pure liquid into it, and then enable auto-collect.");
				}
			}
			else
			{
				using Dictionary<string, int>.KeyCollection.Enumerator enumerator3 = ComponentLiquids.Keys.GetEnumerator();
				if (enumerator3.MoveNext())
				{
					string current3 = enumerator3.Current;
					AutoCollectLiquidType = current3;
				}
			}
		}
		else if (E.Command == "Seal")
		{
			if (!Sealed)
			{
				if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
				{
					return false;
				}
				if (ParentObject.IsInStasis())
				{
					E.Actor.Fail("You cannot seem to interact with " + ParentObject.t() + " in any way.");
					return false;
				}
				Sealed = true;
				PlayWorldSound("Sounds/Interact/sfx_interact_liquidContainer_seal");
				if (!EffectivelySealed())
				{
					IComponent<GameObject>.XDidYToZ(E.Actor, "seal", ParentObject, "as best " + E.Actor.it + " can, but in " + ParentObject.its + " condition this does little", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
				}
				else
				{
					IComponent<GameObject>.XDidYToZ(E.Actor, "seal", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
				}
				E.Actor.UseEnergy(1000, "Item Seal");
			}
		}
		else if (E.Command == "Unseal" && Sealed)
		{
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			if (ParentObject.IsInStasis())
			{
				E.Actor.Fail("You cannot seem to interact with " + ParentObject.t() + " in any way.");
				return false;
			}
			PlayWorldSound("Sounds/Interact/sfx_interact_liquidContainer_unseal");
			Sealed = false;
			IComponent<GameObject>.XDidYToZ(E.Actor, "unseal", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			E.Actor.UseEnergy(1000, "Item Unseal");
		}
		goto IL_1614;
		IL_1614:
		return base.HandleEvent(E);
	}

	private static string GetNameForCleaningMessage(GameObject Object)
	{
		return Object.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: true, Reflexive: false, null, AsPossessed: true) ?? "something";
	}

	public static void CleaningMessage(GameObject Actor, List<GameObject> Objects, List<string> Types = null, GameObject Source = null, LiquidVolume LiquidVolume = null, bool UseDram = false)
	{
		List<string> list = new List<string>(Objects.Count);
		if (Objects.Contains(Actor))
		{
			list.Add(Actor.itself);
		}
		foreach (GameObject Object in Objects)
		{
			if (Object != Actor && Object.HasProperName)
			{
				list.Add(GetNameForCleaningMessage(Object));
			}
		}
		foreach (GameObject Object2 in Objects)
		{
			if (Object2 != Actor && !Object2.HasProperName && Object2.Equipped == Actor)
			{
				list.Add(GetNameForCleaningMessage(Object2));
			}
		}
		foreach (GameObject Object3 in Objects)
		{
			if (Object3 != Actor && !Object3.HasProperName && Object3.Equipped != Actor)
			{
				list.Add(GetNameForCleaningMessage(Object3));
			}
		}
		foreach (GameObject Object4 in Objects)
		{
			Object4.CheckStack();
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("the ");
		if (Types.IsNullOrEmpty())
		{
			stringBuilder.Append("mess");
		}
		else
		{
			stringBuilder.Append(Grammar.MakeAndList(Types));
		}
		stringBuilder.Append(" from ").Append(Grammar.MakeAndList(list));
		if (LiquidVolume == null)
		{
			LiquidVolume = Source?.LiquidVolume;
		}
		if (LiquidVolume != null)
		{
			stringBuilder.Append(" with a dram of ");
			LiquidVolume.AppendLiquidName(stringBuilder);
			if (Source != null)
			{
				if (LiquidVolume.IsOpenVolume())
				{
					if (Source.CurrentCell != null)
					{
						stringBuilder.Append(' ').Append(Actor.DescribeDirectionFrom(Source));
					}
				}
				else
				{
					stringBuilder.Append(" from ").Append(Source.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true, null, IndicateHidden: false, Capitalize: false, SecondPerson: true, Reflexive: false, null, AsPossessed: true));
					if (Source.CurrentCell != null)
					{
						stringBuilder.Append(' ').Append(Actor.DescribeDirectionToward(Source));
					}
				}
			}
		}
		else if (Source != null)
		{
			stringBuilder.Append(" with ").Append(GetNameForCleaningMessage(Source));
		}
		IComponent<GameObject>.XDidY(Actor, "clean", stringBuilder.ToString());
		if (UseDram)
		{
			LiquidVolume?.FlowIntoCell(1, Actor.GetCurrentCell(), Actor);
		}
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (IsOpenVolume() || (!ParentObject.IsCreature && !EffectivelySealed()))
		{
			if (!IsFreshWater() && E.MinPriority > 0)
			{
				return true;
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (IsOpenVolume() || (!ParentObject.IsCreature && !EffectivelySealed()))
		{
			ParentObject.Twiddle();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (!StartVolume.IsNullOrEmpty())
		{
			Volume = StartVolume.RollCached();
		}
		if (Volume == 0)
		{
			Empty(WillCheckImage: false, NoDestroy: true);
		}
		else
		{
			CheckImage();
		}
		SyncPhysicalProperties();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckImage();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckImage();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StripContentsEvent E)
	{
		if (!IsEmpty() && (!E.KeepNatural || ParentObject == null || GetLiquidDesignation(hyphenateSoloLiquids: true) != ParentObject.GetBlueprint().GetPartParameter<string>("LiquidVolume", "InitialLiquid")))
		{
			Empty();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		if (!IsOpenVolume() && !E.Obliterate)
		{
			MaxVolume = -1;
			GameObject holder = ParentObject.Holder;
			Cell cell = holder?.GetCellContext();
			Cell cell2 = ParentObject.GetCellContext() ?? ParentObject.GetObjectContext()?.GetCellContext() ?? cell;
			if (cell2 != null)
			{
				GameObject actor = holder;
				Cell targetCell = cell2;
				bool douse = cell == cell2;
				Pour(ref E.InterfaceExit, actor, targetCell, Forced: true, douse);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(FellDownEvent E)
	{
		if (IsOpenVolume())
		{
			EmptyIntoCell(E.Cell);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(VaporizedEvent E)
	{
		if (Volume > 0)
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				keylist.AddRange(ComponentLiquids.Keys);
				foreach (string item in keylist)
				{
					if (!Liquids[item].Vaporized(this, E.By))
					{
						return false;
					}
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
				CheckImage();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(FrozeEvent E)
	{
		if (Volume > 0)
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				keylist.AddRange(ComponentLiquids.Keys);
				foreach (string item in keylist)
				{
					if (Liquids.ContainsKey(item) && !Liquids[item].Froze(this, E.By))
					{
						return false;
					}
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
				CheckImage();
			}
			Cell cell = ParentObject.CurrentCell;
			string liquidFreezeObject = GetLiquidFreezeObject(out var FreezeVerb);
			if (!string.IsNullOrEmpty(liquidFreezeObject) && ParentObject.IsValid())
			{
				int density = Volume * ParentObject.Count / 20;
				PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_frozen");
				ParentObject.Physics.LastDamagedByType = "Frozen";
				ParentObject.Physics.LastDamageAccidental = true;
				ParentObject.Die(E.By, null, "You froze.", ParentObject.It + " @@froze.", Accidental: true, null, null, Force: false, AlwaysUsePopups: false, FreezeVerb.IsNullOrEmpty() ? "" : null, FreezeVerb.IsNullOrEmpty() ? null : FreezeVerb);
				if (cell != null)
				{
					GameObject gameObject = GameObject.Create(liquidFreezeObject, 0, 0, null, null, null, "Freeze");
					if (gameObject == null)
					{
						MetricsManager.LogError("Unknown freeze object " + liquidFreezeObject);
						return true;
					}
					Gas part = gameObject.GetPart<Gas>();
					if (part != null)
					{
						part.Density = density;
						if (GameObject.Validate(E.By))
						{
							part.Creator = E.By;
						}
					}
					cell.AddObject(gameObject);
					int i = 0;
					for (int count = cell.Objects.Count; i < count; i++)
					{
						GameObject gameObject2 = cell.Objects[i];
						if (gameObject2 == null || gameObject2 == gameObject || !gameObject2.IsCombatObject() || !gameObject.ConsiderSolidFor(gameObject2))
						{
							continue;
						}
						Stuck e = new Stuck(30, 25, "Frozen Stuck Restraint", null, "stuck", "in", gameObject.ID, "Move", 0.25f, 1f, 1f, DependsOnMustBeFrozen: false, DependsOnMustBeSolid: true);
						if (!gameObject2.ApplyEffect(e))
						{
							continue;
						}
						gameObject2.RemoveAllEffects<LiquidCovered>();
						foreach (GameObject item2 in gameObject2.GetInventoryAndEquipment())
						{
							item2.RemoveAllEffects<LiquidCovered>();
						}
					}
				}
				return false;
			}
			if (CanInteractWithAnything(cell) && ParentObject.CurrentCell != null)
			{
				foreach (GameObject item3 in ParentObject.CurrentCell.GetObjectsWithPartReadonly("Combat"))
				{
					if (CanInteractWith(item3, cell, AnythingChecked: true) && Volume >= item3.GetMaximumLiquidExposure() / 8)
					{
						Stuck e2 = new Stuck(5, 15, "Frozen Stuck Restraint", null, "stuck", "in", ParentObject.ID, "Move", 0.25f, 1f, 1f, DependsOnMustBeFrozen: true);
						item3.ApplyEffect(e2);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ThawedEvent E)
	{
		if (Volume > 0)
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				keylist.AddRange(ComponentLiquids.Keys);
				foreach (string item in keylist)
				{
					if (!Liquids[item].Thawed(this, E.By))
					{
						return false;
					}
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
				CheckImage();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PollForHealingLocationEvent E)
	{
		if (Volume > 0)
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				keylist.AddRange(ComponentLiquids.Keys);
				foreach (string item in keylist)
				{
					int healingLocationValue = Liquids[item].GetHealingLocationValue(this, E.Actor);
					if (healingLocationValue > E.Value)
					{
						E.Value = healingLocationValue;
						if (E.First)
						{
							return false;
						}
					}
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		if (IsSwimmingDepth() && Options.ConfirmSwimming && !E.Actor.HasEffect<Swimming>() && CanInteractWith(E.Actor, E.Cell))
		{
			E.IndicateObject = ParentObject;
			return false;
		}
		if (Options.ConfirmDangerousLiquid)
		{
			foreach (string key in ComponentLiquids.Keys)
			{
				if (Liquids[key].InterruptAutowalk)
				{
					if (CanInteractWith(E.Actor, E.Cell) && !EffectivelySealed() && IsOpenVolume())
					{
						E.IndicateObject = ParentObject;
						return false;
					}
					break;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckAnythingToCleanWithEvent E)
	{
		if (UsableForCleaning())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckAnythingToCleanWithNearbyEvent E)
	{
		if (UsableForCleaning())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCleaningItemsEvent E)
	{
		if (UsableForCleaning())
		{
			E.Objects.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCleaningItemsNearbyEvent E)
	{
		if (UsableForCleaning())
		{
			E.Objects.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ProcessTurns(Amount);
	}

	public void ProcessTurns(int Turns)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && cell.ParentZone != null && cell.ParentZone.IsActive())
		{
			if (Primary == "wax" && IsOpenVolume() && ParentObject.Physics.Temperature < 25 && !cell.IsOccluding())
			{
				if (Volume >= 200)
				{
					cell.AddObject("Wax Block");
				}
				else
				{
					cell.AddObject("Wax Nodule");
				}
				ParentObject.Destroy(null, Silent: true);
			}
			List<string> list = null;
			try
			{
				if (ComponentLiquids.Count == 1 && Primary != null)
				{
					RequirePrimaryLiquid().ProcessTurns(this, ParentObject, Turns);
				}
				else if (ComponentLiquids.Count == 2 && Primary != null && Secondary != null)
				{
					RequirePrimaryLiquid().ProcessTurns(this, ParentObject, Turns);
					if (Secondary != null)
					{
						RequireSecondaryLiquid().ProcessTurns(this, ParentObject, Turns);
					}
				}
				else
				{
					list = getKeylist();
					list.Clear();
					list.AddRange(ComponentLiquids.Keys);
					foreach (string item in list)
					{
						Liquids[item].ProcessTurns(this, ParentObject, Turns);
					}
				}
				if (Volume > 0 && CanInteractWithAnything(cell))
				{
					int i = 0;
					for (int count = cell.Objects.Count; i < count; i++)
					{
						GameObject gameObject = cell.Objects[i];
						if (CanInteractWith(gameObject, cell, AnythingChecked: true))
						{
							if (ComponentLiquids.Count == 1 && Primary != null)
							{
								RequirePrimaryLiquid().ObjectInCell(this, gameObject);
							}
							else if (ComponentLiquids.Count == 2 && Primary != null && Secondary != null)
							{
								RequirePrimaryLiquid().ObjectInCell(this, gameObject);
								if (Secondary != null)
								{
									RequireSecondaryLiquid().ObjectInCell(this, gameObject);
								}
							}
							else
							{
								if (list == null)
								{
									list = getKeylist();
								}
								foreach (string item2 in list)
								{
									if (ComponentLiquids.ContainsKey(item2))
									{
										Liquids[item2].ObjectInCell(this, gameObject);
									}
								}
							}
							if (IsOpenVolume())
							{
								ProcessContact(gameObject, Initial: false, gameObject.HasEffect<Prone>(), Poured: false, null, FromCell: true);
							}
						}
						if (count != cell.Objects.Count)
						{
							count = cell.Objects.Count;
							if (i < count && cell.Objects[i] != gameObject)
							{
								i--;
							}
						}
					}
				}
				if (Volume <= 0 || !CanEvaporate())
				{
					return;
				}
				int liquidEvaporativity = GetLiquidEvaporativity();
				if (liquidEvaporativity <= 0)
				{
					return;
				}
				int num = 0;
				for (int j = 0; j < Volume; j++)
				{
					if (liquidEvaporativity.in100())
					{
						num++;
					}
				}
				if (num > 0)
				{
					UseDramsByEvaporativity(num);
				}
				return;
			}
			finally
			{
				if (list != null)
				{
					keylistPool.Enqueue(list);
				}
			}
		}
		if (ParentObject == null)
		{
			return;
		}
		if (ComponentLiquids.Count == 1 && Primary != null)
		{
			RequirePrimaryLiquid().ProcessTurns(this, ParentObject, Turns);
			return;
		}
		if (ComponentLiquids.Count == 2 && Primary != null && Secondary != null)
		{
			RequirePrimaryLiquid().ProcessTurns(this, ParentObject, Turns);
			if (Secondary != null)
			{
				RequireSecondaryLiquid().ProcessTurns(this, ParentObject, Turns);
			}
			return;
		}
		List<string> keylist = getKeylist();
		try
		{
			keylist.Clear();
			keylist.AddRange(ComponentLiquids.Keys);
			foreach (string item3 in keylist)
			{
				Liquids[item3].ProcessTurns(this, ParentObject, Turns);
			}
		}
		finally
		{
			keylistPool.Enqueue(keylist);
		}
	}

	public void RenderSmearWithNativeSupplied(RenderEvent E, GameObject obj, string native)
	{
		if (Primary != null || Secondary != null)
		{
			if (Primary != null && native != Primary)
			{
				RequirePrimaryLiquid().RenderSmearPrimary(this, E, obj);
			}
			if (Secondary != null && native != Secondary)
			{
				RequireSecondaryLiquid().RenderSmearSecondary(this, E, obj);
			}
		}
	}

	public void RenderSmear(RenderEvent E, GameObject obj)
	{
		if (Primary != null || Secondary != null)
		{
			string text = obj?.GetPropertyOrTag("LiquidNative");
			if (Primary != null && text != Primary)
			{
				RequirePrimaryLiquid().RenderSmearPrimary(this, E, obj);
			}
			if (Secondary != null && text != Secondary)
			{
				RequireSecondaryLiquid().RenderSmearSecondary(this, E, obj);
			}
		}
	}

	public void RenderStain(RenderEvent E, GameObject obj)
	{
		if (Primary != null)
		{
			string color = RequirePrimaryLiquid().GetColor();
			if (color != null)
			{
				E.ColorString = "&" + color;
			}
		}
		if (Secondary != null)
		{
			string color2 = RequireSecondaryLiquid().GetColor();
			if (color2 != null)
			{
				E.DetailColor = color2;
			}
		}
	}

	public void Empty(bool WillCheckImage = false, bool NoDestroy = false)
	{
		if (!NoDestroy && IsOpenVolume() && ParentObject?.CurrentCell != null)
		{
			ParentObject.Destroy(null, Silent: true, Obliterate: true);
			return;
		}
		if (Volume != 0)
		{
			Volume = 0;
			FlushWeightCaches();
		}
		ComponentLiquids.Clear();
		if (!WillCheckImage)
		{
			CheckImage();
		}
	}

	public void EmptyIntoCell(Cell C = null, GameObject Pourer = null)
	{
		PourIntoCell(Pourer, C ?? ParentObject.GetCurrentCell(), Volume);
		Empty();
	}

	public int GetLiquidCount()
	{
		return ComponentLiquids.Count;
	}

	public string GetLiquidName()
	{
		if (ComponentLiquids.Count == 1)
		{
			if (Primary != null)
			{
				return RequirePrimaryLiquid().GetName(this);
			}
			using Dictionary<string, int>.KeyCollection.Enumerator enumerator = ComponentLiquids.Keys.GetEnumerator();
			if (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				return Liquids[current].GetName(this);
			}
		}
		else if (ComponentLiquids.Count > 1)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			int num = 0;
			string text = null;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				if (componentLiquid.Value > num || (componentLiquid.Value == num && componentLiquid.Key.CompareTo(text) < 0))
				{
					text = componentLiquid.Key;
					num = componentLiquid.Value;
				}
			}
			foreach (string key in ComponentLiquids.Keys)
			{
				if (key != text)
				{
					stringBuilder.Append(Liquids[key].GetAdjective(this)).Append(' ');
				}
			}
			stringBuilder.Append(Liquids[text].GetName(this));
			return stringBuilder.ToString();
		}
		return "";
	}

	private int CompareProportions(string a, string b)
	{
		return -ComponentLiquids[a].CompareTo(ComponentLiquids[b]);
	}

	public List<string> GetTertiaries()
	{
		int num = ComponentLiquids.Count;
		if (Primary != null)
		{
			num--;
		}
		if (Secondary != null)
		{
			num--;
		}
		if (num <= 0)
		{
			return null;
		}
		List<string> list = new List<string>(num);
		foreach (string key in ComponentLiquids.Keys)
		{
			if (key != Primary && key != Secondary)
			{
				list.Add(key);
			}
		}
		if (num > 1)
		{
			list.Sort(CompareProportions);
		}
		return list;
	}

	public void AppendLiquidName(StringBuilder SB, bool Stripped = false)
	{
		if (ComponentLiquids.Count == 0)
		{
			return;
		}
		if (ComponentLiquids.Count == 1)
		{
			if (Primary != null)
			{
				SB.Append(Liquids[Primary].GetName(this));
			}
			else
			{
				foreach (string key in ComponentLiquids.Keys)
				{
					SB.Append(Liquids[key].GetName(this));
				}
			}
		}
		else
		{
			bool flag = false;
			if (Secondary != null)
			{
				string adjective = RequireSecondaryLiquid().GetAdjective(this);
				if (!adjective.IsNullOrEmpty())
				{
					SB.Append(adjective);
					flag = true;
				}
			}
			List<string> tertiaries = GetTertiaries();
			if (tertiaries != null)
			{
				foreach (string item in tertiaries)
				{
					string adjective2 = Liquids[item].GetAdjective(this);
					if (!adjective2.IsNullOrEmpty())
					{
						if (flag)
						{
							SB.Append(' ');
						}
						else
						{
							flag = true;
						}
						SB.Append(adjective2);
					}
				}
			}
			if (Primary != null)
			{
				string name = RequirePrimaryLiquid().GetName(this);
				if (!name.IsNullOrEmpty())
				{
					if (flag)
					{
						SB.Append(' ');
					}
					SB.Append(name);
				}
			}
		}
		if (Stripped)
		{
			ColorUtility.StripFormatting(SB);
		}
	}

	public string GetDescriptionPreposition()
	{
		if (ParentObject != null && !ParentObject.Understood())
		{
			return UnknownNamePreposition;
		}
		return NamePreposition;
	}

	public bool UsesNamePreposition()
	{
		return !GetDescriptionPreposition().IsNullOrEmpty();
	}

	public void AppendLiquidDescription(StringBuilder SB, bool IncludeAmount = true, bool IgnoreSeal = false, bool Stripped = false, bool Syntactic = false, bool ForSort = false, string UsePreposition = null)
	{
		if (UsePreposition == null)
		{
			UsePreposition = GetDescriptionPreposition();
		}
		bool flag = !IgnoreSeal && EffectivelySealed();
		if (flag && !LiquidVisibleWhenSealed && !ShowSeal)
		{
			return;
		}
		if (!Syntactic && UsePreposition.IsNullOrEmpty())
		{
			SB.Append(Stripped ? "[" : "{{y|[");
		}
		else if ((!flag || LiquidVisibleWhenSealed) && !UsePreposition.IsNullOrEmpty())
		{
			SB.Append(UsePreposition).Append(' ');
		}
		if (!flag || LiquidVisibleWhenSealed)
		{
			if (IncludeAmount)
			{
				if (ForSort)
				{
					if (Stripped)
					{
						SB.Append(Volume.ToString("D6")).Append((Volume == 1) ? " dram of " : " drams of ");
					}
					else
					{
						SB.Append("{{rules|").Append(Volume.ToString("D6")).Append("}}")
							.Append((Volume == 1) ? " dram of " : " drams of ");
					}
				}
				else if (Stripped)
				{
					SB.Append(Volume).Append((Volume == 1) ? " dram of " : " drams of ");
				}
				else
				{
					SB.Append("{{rules|").Append(Volume).Append("}}")
						.Append((Volume == 1) ? " dram of " : " drams of ");
				}
			}
			AppendLiquidName(SB, Stripped);
		}
		if (flag && ShowSeal)
		{
			if (LiquidVisibleWhenSealed)
			{
				SB.Append(", ");
			}
			SB.Append(Stripped ? "sealed" : "{{c|sealed}}");
		}
		if (!Syntactic && UsePreposition.IsNullOrEmpty())
		{
			SB.Append(Stripped ? "]" : "]}}");
		}
	}

	public string GetLiquidDescription(bool IncludeAmount = true, bool IgnoreSeal = false, bool Stripped = false, bool Syntactic = false, bool ForSort = false, string UsePreposition = null)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		AppendLiquidDescription(stringBuilder, IncludeAmount, IgnoreSeal, Stripped, Syntactic, ForSort, UsePreposition);
		return stringBuilder.ToString();
	}

	public bool IsOpenVolume()
	{
		return MaxVolume == -1;
	}

	public bool IsWadingDepth()
	{
		if (IsOpenVolume())
		{
			return Volume >= 200;
		}
		return false;
	}

	public bool IsSwimmingDepth()
	{
		if (IsOpenVolume())
		{
			return Volume >= 2000;
		}
		return false;
	}

	public bool IsSwimmableFor(GameObject who)
	{
		if (!IsOpenVolume())
		{
			return false;
		}
		if (!IsWadingDepth())
		{
			return false;
		}
		if (!IsSwimmingDepth() && Swimming.GetTargetMoveSpeedPenalty(who) > 0)
		{
			return false;
		}
		return true;
	}

	public void BaseRender()
	{
		if (ParentObject != null && IsOpenVolume())
		{
			if (Primary != null)
			{
				RequirePrimaryLiquid().BaseRenderPrimary(this);
			}
			if (Secondary != null)
			{
				RequireSecondaryLiquid().BaseRenderSecondary(this);
			}
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (ParentObject != null && IsOpenVolume())
		{
			if (Primary != null)
			{
				RequirePrimaryLiquid().RenderPrimary(this, E);
			}
			if (Secondary != null)
			{
				RequireSecondaryLiquid().RenderSecondary(this, E);
			}
		}
		return true;
	}

	public string GetPrimaryLiquidColor()
	{
		if (Primary != null)
		{
			return RequirePrimaryLiquid().GetColor();
		}
		return null;
	}

	public bool CheckImage()
	{
		if (ParentObject == null || ParentObject.Render == null)
		{
			return false;
		}
		bool result = false;
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			LastPaintMask = -1;
			cell.CheckPaintLiquidsAround(ParentObject);
		}
		if (IsWadingDepth())
		{
			if (!ParentObject.Render.RenderIfDark || ParentObject.Render.RenderString != "~")
			{
				ParentObject.Render.RenderIfDark = true;
				ParentObject.Render.RenderString = "~";
				result = true;
			}
		}
		else if (IsOpenVolume())
		{
			if (ParentObject.Render.RenderIfDark)
			{
				ParentObject.Render.RenderIfDark = false;
				result = true;
			}
			string randomElement = PuddleRenderStrings.GetRandomElement();
			if (ParentObject.Render.RenderString != randomElement)
			{
				ParentObject.Render.RenderString = PuddleRenderStrings.GetRandomElement();
				result = true;
			}
		}
		string propertyOrTag = ParentObject.GetPropertyOrTag("DetailColorByLiquid");
		string propertyOrTag2 = ParentObject.GetPropertyOrTag("ColorStringByLiquid");
		if (!propertyOrTag.IsNullOrEmpty() || !propertyOrTag2.IsNullOrEmpty())
		{
			if (Volume <= 0 || (!LiquidVisibleWhenSealed && EffectivelySealed()) || GetPrimaryLiquid() == null)
			{
				if (!propertyOrTag.IsNullOrEmpty() && ParentObject.Render.DetailColor != propertyOrTag)
				{
					ParentObject.Render.DetailColor = propertyOrTag;
					result = true;
				}
				if (!propertyOrTag2.IsNullOrEmpty() && ParentObject.Render.ColorString != propertyOrTag2)
				{
					ParentObject.Render.ColorString = propertyOrTag2;
					result = true;
				}
			}
			else
			{
				string text = GetPrimaryLiquid()?.GetColor();
				if (!propertyOrTag.IsNullOrEmpty())
				{
					string text2 = text ?? propertyOrTag;
					string propertyOrTag3 = ParentObject.GetPropertyOrTag("DetailColorByLiquidContrastShiftFrom");
					if (propertyOrTag3 != null)
					{
						string propertyOrTag4 = ParentObject.GetPropertyOrTag("DetailColorByLiquidContrastShiftTo");
						if (text2 == propertyOrTag3)
						{
							text2 = propertyOrTag4;
						}
					}
					if (ParentObject.Render.DetailColor != text2)
					{
						ParentObject.Render.DetailColor = text2;
						result = true;
					}
				}
				if (!propertyOrTag2.IsNullOrEmpty())
				{
					string text3 = (text.IsNullOrEmpty() ? propertyOrTag2 : ("&" + text));
					string propertyOrTag5 = ParentObject.GetPropertyOrTag("ColorStringByLiquidContrastShiftFrom");
					if (propertyOrTag5 != null)
					{
						string propertyOrTag6 = ParentObject.GetPropertyOrTag("ColorStringByLiquidContrastShiftTo");
						if (text3 == propertyOrTag5)
						{
							text3 = propertyOrTag6;
						}
					}
					if (ParentObject.Render.ColorString != text3)
					{
						ParentObject.Render.ColorString = text3;
						result = true;
					}
				}
			}
		}
		return result;
	}

	public bool UseDrams(int Num)
	{
		if (Volume > Num)
		{
			Volume -= Num;
			FlushWeightCaches();
			return true;
		}
		Empty();
		CheckImage();
		return false;
	}

	public bool UseDram()
	{
		return UseDrams(1);
	}

	public bool UseDrams(Dictionary<string, int> Amounts)
	{
		if (consumeVolume == null)
		{
			consumeVolume = new LiquidVolume();
		}
		consumeVolume.ComponentLiquids.Clear();
		bool result = true;
		int num = 0;
		foreach (KeyValuePair<string, int> Amount in Amounts)
		{
			if (ComponentLiquids.TryGetValue(Amount.Key, out var value))
			{
				int num2 = Math.Max(Volume * value / 1000, 1);
				int num3 = Math.Min(Amount.Value, num2);
				num += num3;
				consumeVolume.ComponentLiquids.Add(Amount.Key, num3);
				if (num2 < Amount.Value)
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
		}
		if (num > 0)
		{
			consumeVolume.MaxVolume = num;
			consumeVolume.Volume = -num;
			consumeVolume.NormalizeProportions();
			MixWith(consumeVolume);
			FlushNavigationCaches();
			CheckGroundLiquidMerge();
		}
		return result;
	}

	public bool UseDrams(string Liquid, int Amount)
	{
		bool result = true;
		int num = 0;
		if (ComponentLiquids.TryGetValue(Liquid, out var value))
		{
			int num2 = Math.Max(Volume * value / 1000, 1);
			if (num2 < Amount)
			{
				num = num2;
				result = false;
			}
			else
			{
				num = Amount;
			}
		}
		else
		{
			result = false;
		}
		if (num > 0)
		{
			if (consumeVolume == null)
			{
				consumeVolume = new LiquidVolume();
			}
			consumeVolume.ComponentLiquids.Clear();
			if (!consumeVolume.ComponentLiquids.ContainsKey(Liquid) || consumeVolume.ComponentLiquids.Count > 1)
			{
				if (consumeVolume.ComponentLiquids.Count > 0)
				{
					consumeVolume.ComponentLiquids.Clear();
				}
				consumeVolume.ComponentLiquids.Add(Liquid, 1000);
			}
			consumeVolume.MaxVolume = num;
			consumeVolume.Volume = -num;
			MixWith(consumeVolume);
			FlushNavigationCaches();
			CheckGroundLiquidMerge();
		}
		return result;
	}

	public bool UseDramsByEvaporativity(int Num)
	{
		if (ComponentLiquids.Count < 2)
		{
			foreach (string key in ComponentLiquids.Keys)
			{
				if (GetLiquid(key).Evaporativity > 0)
				{
					return UseDrams(Num);
				}
			}
			return false;
		}
		EvaporativityProportions.Clear();
		EvaporativityAmounts.Clear();
		int num = 0;
		foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
		{
			int num2 = GetLiquid(componentLiquid.Key).Evaporativity * componentLiquid.Value;
			EvaporativityProportions.Add(componentLiquid.Key, num2);
			num += num2;
		}
		int num3 = 0;
		foreach (KeyValuePair<string, int> evaporativityProportion in EvaporativityProportions)
		{
			int num4 = Num * evaporativityProportion.Value / num;
			EvaporativityAmounts.Add(evaporativityProportion.Key, num4);
			num3 += num4;
		}
		while (num3 < Num)
		{
			string text = null;
			int num5 = 0;
			foreach (KeyValuePair<string, int> evaporativityProportion2 in EvaporativityProportions)
			{
				if (evaporativityProportion2.Value > num5)
				{
					text = evaporativityProportion2.Key;
					num5 = evaporativityProportion2.Value;
				}
			}
			if (text == null)
			{
				break;
			}
			EvaporativityAmounts[text]++;
			num3++;
			EvaporativityProportions.Remove(text);
		}
		UseDrams(EvaporativityAmounts);
		return true;
	}

	public bool Stain(GameObject obj, int Num)
	{
		StainingProportions.Clear();
		StainingAmounts.Clear();
		int total = 0;
		foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
		{
			BaseLiquid liquid = GetLiquid(componentLiquid.Key);
			if (!liquid.StainOnlyWhenPure || ComponentLiquids.Count == 1)
			{
				int num = liquid.Staining * componentLiquid.Value;
				StainingProportions.Add(componentLiquid.Key, num);
				total += num;
			}
		}
		for (int i = 0; i < Num; i++)
		{
			string randomElement = StainingProportions.GetRandomElement(ref total);
			if (randomElement == null)
			{
				return false;
			}
			if (StainingAmounts.ContainsKey(randomElement))
			{
				StainingAmounts[randomElement]++;
			}
			else
			{
				StainingAmounts.Add(randomElement, 1);
			}
		}
		UseDrams(StainingAmounts);
		return obj.ForceApplyEffect(new LiquidStained(new LiquidVolume(StainingAmounts)));
	}

	public double GetLiquidExtrinsicValuePerDram()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].GetExtrinsicValuePerDram(Pure: true);
					}
				}
				break;
			}
			return RequirePrimaryLiquid().GetExtrinsicValuePerDram(Pure: true);
		default:
		{
			double num = 0.0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += (double)(Liquids[componentLiquid.Key].GetExtrinsicValuePerDram(Pure: false) * (float)componentLiquid.Value);
			}
			return num / 1000.0;
		}
		case 0:
			break;
		}
		return 0.0;
	}

	public int GetLiquidTemperature()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].Temperature;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().Temperature;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].Temperature * componentLiquid.Value;
			}
			return num / 1000;
		}
		case 0:
			break;
		}
		return 25;
	}

	public int GetLiquidFlameTemperature()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].FlameTemperature;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().FlameTemperature;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].FlameTemperature * componentLiquid.Value;
			}
			return num / 1000;
		}
		case 0:
			break;
		}
		return 0;
	}

	public int GetLiquidVaporTemperature()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].VaporTemperature;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().VaporTemperature;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].VaporTemperature * componentLiquid.Value;
			}
			return num / 1000;
		}
		case 0:
			break;
		}
		return 0;
	}

	public int GetLiquidFreezeTemperature()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].FreezeTemperature;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().FreezeTemperature;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].FreezeTemperature * componentLiquid.Value;
			}
			return num / 1000;
		}
		case 0:
			break;
		}
		return 0;
	}

	public int GetLiquidBrittleTemperature()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].BrittleTemperature;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().BrittleTemperature;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].BrittleTemperature * componentLiquid.Value;
			}
			return num / 1000;
		}
		case 0:
			break;
		}
		return 0;
	}

	public int GetLiquidElectricalConductivity()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].PureElectricalConductivity;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().PureElectricalConductivity;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].MixedElectricalConductivity * componentLiquid.Value;
			}
			return num / 1000;
		}
		case 0:
			break;
		}
		return 80;
	}

	public string GetLiquidFreezeObject()
	{
		return GetPrimaryLiquid()?.GetFreezeObjectForVolume(Amount(Primary));
	}

	public string GetLiquidFreezeObject(out string FreezeVerb)
	{
		FreezeVerb = null;
		return GetPrimaryLiquid()?.GetFreezeObjectForVolume(Amount(Primary), out FreezeVerb);
	}

	public void GetLiquidPhysicalProperties(out int FlameTemperature, out int VaporTemperature, out int FreezeTemperature, out int BrittleTemperature, out int ElectricalConductivity)
	{
		FlameTemperature = 99999;
		VaporTemperature = 350;
		FreezeTemperature = 0;
		BrittleTemperature = -100;
		ElectricalConductivity = 80;
		switch (ComponentLiquids.Count)
		{
		case 1:
		{
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						BaseLiquid baseLiquid = Liquids[current];
						FlameTemperature = baseLiquid.FlameTemperature;
						VaporTemperature = baseLiquid.VaporTemperature;
						FreezeTemperature = baseLiquid.FreezeTemperature;
						BrittleTemperature = baseLiquid.BrittleTemperature;
						ElectricalConductivity = baseLiquid.PureElectricalConductivity;
					}
					return;
				}
			}
			BaseLiquid baseLiquid2 = RequirePrimaryLiquid();
			FlameTemperature = baseLiquid2.FlameTemperature;
			VaporTemperature = baseLiquid2.VaporTemperature;
			FreezeTemperature = baseLiquid2.FreezeTemperature;
			BrittleTemperature = baseLiquid2.BrittleTemperature;
			ElectricalConductivity = baseLiquid2.PureElectricalConductivity;
			return;
		}
		case 0:
			return;
		}
		FlameTemperature = 0;
		VaporTemperature = 0;
		FreezeTemperature = 0;
		BrittleTemperature = 0;
		ElectricalConductivity = 0;
		foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
		{
			BaseLiquid baseLiquid3 = Liquids[componentLiquid.Key];
			FlameTemperature += baseLiquid3.FlameTemperature * componentLiquid.Value;
			VaporTemperature += baseLiquid3.VaporTemperature * componentLiquid.Value;
			FreezeTemperature += baseLiquid3.FreezeTemperature * componentLiquid.Value;
			BrittleTemperature += baseLiquid3.BrittleTemperature * componentLiquid.Value;
			ElectricalConductivity += baseLiquid3.MixedElectricalConductivity * componentLiquid.Value;
		}
		FlameTemperature /= 1000;
		VaporTemperature /= 1000;
		FreezeTemperature /= 1000;
		BrittleTemperature /= 1000;
		ElectricalConductivity /= 1000;
	}

	public int GetLiquidThermalConductivity()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].ThermalConductivity;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().ThermalConductivity;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].ThermalConductivity * componentLiquid.Value;
			}
			return num / 1000;
		}
		case 0:
			break;
		}
		return 50;
	}

	public int GetLiquidCombustibility()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].Combustibility;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().Combustibility;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].Combustibility * componentLiquid.Value;
			}
			return num / 1000;
		}
		case 0:
			break;
		}
		return 0;
	}

	public float GetLiquidCooling()
	{
		int liquidTemperature = GetLiquidTemperature();
		int liquidThermalConductivity = GetLiquidThermalConductivity();
		int liquidCombustibility = GetLiquidCombustibility();
		float num = 0.125f + (1000f - (float)liquidTemperature) / 1000f;
		if (liquidCombustibility > 0)
		{
			num = num * (float)(100 - liquidCombustibility) / 100f;
		}
		if (liquidThermalConductivity != 50)
		{
			num = num * (float)liquidThermalConductivity / 50f;
		}
		return num;
	}

	public int GetLiquidAdsorbence()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].Adsorbence;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().Adsorbence;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].Adsorbence * componentLiquid.Value;
			}
			if (num == 0)
			{
				return 0;
			}
			return Math.Max(num / 1000, 1);
		}
		case 0:
			break;
		}
		return 0;
	}

	public int GetLiquidFluidity()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].Fluidity;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().Fluidity;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].Fluidity * componentLiquid.Value;
			}
			if (num == 0)
			{
				return 0;
			}
			return Math.Max(num / 1000, 1);
		}
		case 0:
			break;
		}
		return 0;
	}

	public int GetLiquidViscosity()
	{
		return 100 - GetLiquidFluidity();
	}

	public int GetLiquidEvaporativity()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].Evaporativity;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().Evaporativity;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].Evaporativity * componentLiquid.Value;
			}
			if (num == 0)
			{
				return 0;
			}
			return Math.Max(num / 1000, 1);
		}
		case 0:
			break;
		}
		return 0;
	}

	public int GetLiquidStaining()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].Staining;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().Staining;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				BaseLiquid baseLiquid = Liquids[componentLiquid.Key];
				if (!baseLiquid.StainOnlyWhenPure)
				{
					num += baseLiquid.Staining * componentLiquid.Value;
				}
			}
			if (num == 0)
			{
				return 0;
			}
			return Math.Max(num / 1000, 1);
		}
		case 0:
			break;
		}
		return 0;
	}

	public int GetLiquidCleansing()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].Cleansing;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().Cleansing;
		default:
		{
			int num = 0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].Cleansing * componentLiquid.Value;
			}
			if (num == 0)
			{
				return 0;
			}
			return Math.Max(num / 1000, 1);
		}
		case 0:
			break;
		}
		return 0;
	}

	public bool IsLiquidUsableForCleaning()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				foreach (string key in ComponentLiquids.Keys)
				{
					if (Liquids[key].EnableCleaning)
					{
						return true;
					}
				}
			}
			else if (RequirePrimaryLiquid().EnableCleaning)
			{
				return true;
			}
			break;
		default:
		{
			int num = 0;
			bool flag = false;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				BaseLiquid baseLiquid = Liquids[componentLiquid.Key];
				num += baseLiquid.Cleansing * componentLiquid.Value;
				if (baseLiquid.EnableCleaning)
				{
					flag = true;
				}
			}
			if (flag && num > 0 && Math.Max(num / 1000, 1) >= 2)
			{
				return true;
			}
			break;
		}
		case 0:
			break;
		}
		return false;
	}

	public bool UsableForCleaning()
	{
		if (ParentObject == null)
		{
			return false;
		}
		if (Volume <= 0)
		{
			return false;
		}
		if (!IsLiquidUsableForCleaning())
		{
			return false;
		}
		if (EffectivelySealed())
		{
			return false;
		}
		return true;
	}

	public bool SafeContainer(GameObject obj)
	{
		if (ComponentLiquids.Count == 1 && Primary != null)
		{
			return RequirePrimaryLiquid().SafeContainer(obj);
		}
		foreach (string key in ComponentLiquids.Keys)
		{
			if (!Liquids[key].SafeContainer(obj))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsGameObjectSafeContainerForLiquid(GameObject obj, string liquid)
	{
		try
		{
			if (liquid.Contains(','))
			{
				string[] array = liquid.Split(',');
				foreach (string text in array)
				{
					if (text.Contains('-'))
					{
						if (!GetLiquid(Liquids[text.Split('-')[0]].ID).SafeContainer(obj))
						{
							return false;
						}
					}
					else if (!GetLiquid(Liquids[text].ID).SafeContainer(obj))
					{
						return false;
					}
				}
			}
			else if (liquid.Contains('-'))
			{
				if (!GetLiquid(Liquids[liquid.Split('-')[0]].ID).SafeContainer(obj))
				{
					return false;
				}
			}
			else if (!GetLiquid(Liquids[liquid].ID).SafeContainer(obj))
			{
				return false;
			}
		}
		catch
		{
			MetricsManager.LogError("invalid liquid specification " + (liquid ?? "NULL"));
		}
		return true;
	}

	public bool ConsiderLiquidDangerousToContact()
	{
		if (ComponentLiquids.Count == 1 && Primary != null)
		{
			return RequirePrimaryLiquid().ConsiderDangerousToContact;
		}
		foreach (string key in ComponentLiquids.Keys)
		{
			if (Liquids[key].ConsiderDangerousToContact)
			{
				return true;
			}
		}
		return false;
	}

	public bool ConsiderLiquidDangerousToDrink()
	{
		if (ComponentLiquids.Count == 1 && Primary != null)
		{
			return RequirePrimaryLiquid().ConsiderDangerousToDrink;
		}
		foreach (string key in ComponentLiquids.Keys)
		{
			if (Liquids[key].ConsiderDangerousToDrink)
			{
				return true;
			}
		}
		return false;
	}

	public double GetLiquidWeightPerDram()
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = ComponentLiquids.Keys.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string current2 = enumerator2.Current;
						return Liquids[current2].Weight;
					}
				}
				break;
			}
			return RequirePrimaryLiquid().Weight;
		default:
		{
			double num = 0.0;
			foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
			{
				num += Liquids[componentLiquid.Key].Weight * (double)componentLiquid.Value;
			}
			return num / 1000.0;
		}
		case 0:
			break;
		}
		return 0.0;
	}

	public string GetCirculatoryLossTerm()
	{
		return RequirePrimaryLiquid().CirculatoryLossTerm;
	}

	public string GetColoredCirculatoryLossTerm()
	{
		return RequirePrimaryLiquid().ColoredCirculatoryLossTerm;
	}

	public string GetCirculatoryLossNoun()
	{
		return RequirePrimaryLiquid().CirculatoryLossNoun;
	}

	public string GetColoredCirculatoryLossNoun()
	{
		return RequirePrimaryLiquid().ColoredCirculatoryLossNoun;
	}

	private bool PourIntoCellInternal(GameObject Pourer, Cell TargetCell, int PourAmount, bool CanPourOn, bool Pure, bool CanMergeWithGroundLiquid, ref bool RequestInterfaceExit, bool UsePerformancePath = false)
	{
		if (ParentObject != null && ParentObject.IsTemporary)
		{
			Volume -= PourAmount;
			return false;
		}
		if (TargetCell == null || TargetCell.OnWorldMap())
		{
			return false;
		}
		foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
		{
			if (!Liquids[componentLiquid.Key].PourIntoCell(this, Pourer, TargetCell, ref PourAmount, CanPourOn, ref RequestInterfaceExit))
			{
				return false;
			}
		}
		bool flag = TargetCell.IsSolidFor(Pourer);
		while (true)
		{
			if (CanPourOn)
			{
				GameObject combatTarget = TargetCell.GetCombatTarget(Pourer);
				if (combatTarget != null)
				{
					LiquidVolume liquidVolume = combatTarget.LiquidVolume;
					if (liquidVolume == null || (!liquidVolume.IsOpenVolume() && !liquidVolume.Collector))
					{
						PourAmount -= ProcessContact(combatTarget, Initial: true, Prone: false, Poured: true, Pourer, FromCell: false, PourAmount);
						if (PourAmount <= 0)
						{
							break;
						}
					}
				}
				int i = 0;
				for (int count = TargetCell.Objects.Count; i < count; i++)
				{
					GameObject gameObject = TargetCell.Objects[i];
					if (gameObject == ParentObject || gameObject == Pourer || gameObject == combatTarget || (Pourer != null && !Pourer.PhaseMatches(gameObject)) || ((Pourer == null || !Pourer.IsFlying) && gameObject.IsFlying))
					{
						continue;
					}
					LiquidVolume liquidVolume2 = gameObject.LiquidVolume;
					if (liquidVolume2 == null || (!liquidVolume2.IsOpenVolume() && !liquidVolume2.Collector))
					{
						PourAmount -= ProcessContact(gameObject, Initial: true, Prone: false, Poured: true, Pourer, FromCell: false, PourAmount);
						if (PourAmount <= 0)
						{
							return true;
						}
					}
				}
			}
			int num = 0;
			while (true)
			{
				if (num < TargetCell.Objects.Count)
				{
					GameObject gameObject2 = TargetCell.Objects[num];
					if (gameObject2 != ParentObject && (!flag || gameObject2.CanInteractInCellWithSolid(Pourer)))
					{
						LiquidVolume liquidVolume3 = gameObject2.LiquidVolume;
						if (liquidVolume3 != null && liquidVolume3.IsOpenVolume())
						{
							Temporary part = gameObject2.GetPart<Temporary>();
							ExistenceSupport part2 = gameObject2.GetPart<ExistenceSupport>();
							if (part != null)
							{
								part.Expire(Silent: true);
								if (!GameObject.Validate(gameObject2))
								{
									break;
								}
							}
							else
							{
								if (part2 == null)
								{
									MixInto(liquidVolume3, PourAmount, ref RequestInterfaceExit, Pourer);
									return true;
								}
								part2.Unsupported(Silent: true);
								if (!GameObject.Validate(gameObject2))
								{
									break;
								}
							}
						}
					}
					num++;
					continue;
				}
				for (int j = 0; j < TargetCell.Objects.Count; j++)
				{
					GameObject gameObject3 = TargetCell.Objects[j];
					if (gameObject3 == ParentObject || (flag && !gameObject3.CanInteractInCellWithSolid(Pourer)))
					{
						continue;
					}
					LiquidVolume liquidVolume4 = gameObject3.LiquidVolume;
					if (liquidVolume4 != null && liquidVolume4 != this && !liquidVolume4.IsOpenVolume() && liquidVolume4.Collector && liquidVolume4.Volume < liquidVolume4.MaxVolume)
					{
						int num2 = liquidVolume4.MaxVolume - liquidVolume4.Volume;
						if (num2 >= PourAmount)
						{
							MixInto(liquidVolume4, PourAmount, ref RequestInterfaceExit, Pourer);
							return true;
						}
						MixInto(liquidVolume4, num2, ref RequestInterfaceExit, Pourer);
						PourAmount -= num2;
					}
				}
				int num3 = 0;
				GameObject gameObject4;
				LiquidVolume liquidVolume5;
				while (true)
				{
					if (num3 < TargetCell.Objects.Count)
					{
						gameObject4 = TargetCell.Objects[num3];
						if (gameObject4 != ParentObject && (!flag || gameObject4.CanInteractInCellWithSolid(Pourer)))
						{
							liquidVolume5 = gameObject4.LiquidVolume;
							if (liquidVolume5 != null && liquidVolume5.IsOpenVolume())
							{
								break;
							}
						}
						num3++;
						continue;
					}
					string groundLiquid = TargetCell.GroundLiquid;
					if (CanMergeWithGroundLiquid && !groundLiquid.IsNullOrEmpty() && IsPureLiquid(groundLiquid))
					{
						Volume -= PourAmount;
						if (Volume <= 0)
						{
							Empty();
							CheckImage();
						}
						return true;
					}
					if (UsePerformancePath && TargetCell.HasOpenLiquidVolume())
					{
						TargetCell.GetOpenLiquidVolume().GetPart<LiquidVolume>().MixWith(this, null, null, PourAmount, UseTempSplit: true);
						Volume -= PourAmount;
						if (Volume <= 0)
						{
							Empty();
							CheckImage();
						}
						return true;
					}
					GameObject gameObject5 = GameObject.Create("Water");
					LiquidVolume liquidVolume6 = gameObject5.LiquidVolume;
					if (Pure || groundLiquid.IsNullOrEmpty())
					{
						liquidVolume6.Volume = 0;
						MixInto(liquidVolume6, PourAmount, ref RequestInterfaceExit, Pourer);
					}
					else
					{
						liquidVolume6.InitialLiquid = groundLiquid;
						liquidVolume6.Volume = PourAmount;
						MixInto(liquidVolume6, PourAmount, ref RequestInterfaceExit, Pourer);
						liquidVolume6.Volume = PourAmount;
					}
					if (Pourer != null)
					{
						Phase.carryOver(Pourer, gameObject5, 1);
					}
					TargetCell.AddObject(gameObject5, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Silent: false, Repaint: true, FlushTransient: true, null, CanPourOn ? "Pour" : "Flow", null, Pourer);
					return true;
				}
				if (liquidVolume5 != this)
				{
					Temporary part3 = gameObject4.GetPart<Temporary>();
					ExistenceSupport part4 = gameObject4.GetPart<ExistenceSupport>();
					if (part3 != null)
					{
						part3.Expire(Silent: true);
						if (!GameObject.Validate(gameObject4))
						{
							break;
						}
					}
					else if (part4 != null)
					{
						part4.Unsupported(Silent: true);
						if (!GameObject.Validate(gameObject4))
						{
							break;
						}
					}
					else
					{
						MixInto(liquidVolume5, PourAmount, ref RequestInterfaceExit, Pourer);
					}
				}
				return true;
			}
		}
		return true;
	}

	public bool PourIntoCell(GameObject Pourer, Cell TargetCell, int PourAmount, ref bool RequestInterfaceExit, bool CanPourOn = true, bool Pure = false, bool CanMergeWithGroundLiquid = false, bool UsePerformancePath = false)
	{
		bool result = PourIntoCellInternal(Pourer, TargetCell, PourAmount, CanPourOn, Pure, CanMergeWithGroundLiquid, ref RequestInterfaceExit, UsePerformancePath);
		if (Volume <= 0)
		{
			Empty();
		}
		CheckImage();
		return result;
	}

	public bool PourIntoCell(GameObject Pourer, Cell TargetCell, int PourAmount, bool CanPourOn = true, bool Pure = false, bool CanMergeWithGroundLiquid = false, bool UsePerformancePath = false)
	{
		bool RequestInterfaceExit = false;
		return PourIntoCell(Pourer, TargetCell, PourAmount, ref RequestInterfaceExit, CanPourOn, Pure, CanMergeWithGroundLiquid, UsePerformancePath);
	}

	public bool PourIntoCell(Cell TargetCell, int PourAmount, bool CanPourOn = true, bool Pure = false, bool CanMergeWithGroundLiquid = false)
	{
		return PourIntoCell(null, TargetCell, PourAmount, CanPourOn, Pure, CanMergeWithGroundLiquid);
	}

	public bool FlowIntoCell(int Amount = -1, Cell TargetCell = null, GameObject Pourer = null, bool CanPourOn = false, bool Pure = false, bool CanMergeWithGroundLiquid = true)
	{
		if (Amount == -1)
		{
			Amount = Volume;
		}
		if (TargetCell == null && ParentObject != null)
		{
			TargetCell = ParentObject.GetCurrentCell();
		}
		return PourIntoCell(Pourer, TargetCell, Amount, CanPourOn, Pure, CanMergeWithGroundLiquid, UsePerformancePath: true);
	}

	private bool MixInto(LiquidVolume V, int Amount, ref bool RequestInterfaceExit, GameObject PouredBy = null)
	{
		bool flag = false;
		if (V != this)
		{
			if (Amount < Volume)
			{
				V.MixWith(this, ref RequestInterfaceExit, Amount: Amount, PouredFrom: ParentObject, PouredBy: PouredBy, UseTempSplit: true);
			}
			else
			{
				flag = V.MixWith(this, ref RequestInterfaceExit, ParentObject, PouredBy);
				if (flag)
				{
					Empty();
				}
			}
		}
		return flag;
	}

	private bool MixInto(LiquidVolume V, int Amount, GameObject PouredBy = null)
	{
		bool RequestInterfaceExit = false;
		return MixInto(V, Amount, ref RequestInterfaceExit, PouredBy);
	}

	/// <summary>
	/// Mingles this liquid volume with another as appropriate to them being
	/// adjacent and free to intermix, as if you had a divider between two
	/// halves of a bottle then removed it.
	/// </summary>
	/// <param Name="other">
	/// The other liquid volume to mingle with.
	/// </param>
	public void MingleAdjacent(LiquidVolume other)
	{
		int num;
		int num2;
		if (Volume == other.Volume && MaxVolume == other.MaxVolume)
		{
			num = Volume;
			num2 = other.Volume;
		}
		else if (Volume == MaxVolume && other.Volume == other.MaxVolume)
		{
			num = Volume;
			num2 = other.Volume;
		}
		else
		{
			int num3 = MaxVolume + other.MaxVolume;
			int num4 = Volume + other.Volume;
			if (num4 <= 1)
			{
				return;
			}
			if (IsOpenVolume() || other.IsOpenVolume())
			{
				num = (num2 = num4 / 2);
			}
			else
			{
				num = num4 * MaxVolume / num3;
				num2 = num4 * other.MaxVolume / num3;
			}
			if (num + num2 < num4)
			{
				if (string.Compare(ParentObject.ID, other.ParentObject.ID) > 0)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		if (Volume < num)
		{
			MixWith(other, Amount: num - Volume, PouredFrom: other.ParentObject);
		}
		else if (other.Volume < num2)
		{
			other.MixWith(this, Amount: num2 - other.Volume, PouredFrom: other.ParentObject);
		}
		else if (!ComponentLiquids.SameAs(other.ComponentLiquids))
		{
			int value = Stat.Random(1, Math.Max(Math.Min(Volume, other.Volume) / 4, 1));
			MixWith(other, Amount: value, PouredFrom: other.ParentObject);
			other.MixWith(this, Amount: value, PouredFrom: ParentObject);
		}
	}

	private List<string> getKeylist()
	{
		if (keylistPool.Count > 0)
		{
			return keylistPool.Dequeue();
		}
		return new List<string>(256);
	}

	public string GetActiveAutogetLiquid()
	{
		if (AutoCollectLiquidType != null)
		{
			return AutoCollectLiquidType;
		}
		if (Options.AutogetFreshWater)
		{
			return "water";
		}
		return null;
	}

	public void Fill(string Liquid, int Drams)
	{
		Empty(WillCheckImage: true, NoDestroy: true);
		if (Drams <= 0)
		{
			CheckImage();
			return;
		}
		TrackAsLiquid(Liquid);
		Volume = Drams;
		Liquids[Liquid].FillingContainer(ParentObject, this);
		RecalculatePrimary();
		RecalculateProperties();
	}

	public bool AddDrams(string Liquid, int Drams)
	{
		if (Drams > 0)
		{
			int num = (IsOpenVolume() ? int.MaxValue : MaxVolume) - Volume;
			if (num > 0)
			{
				bool flag = IsEmpty();
				if (flag || IsPureLiquid(Liquid))
				{
					if (flag)
					{
						TrackAsLiquid(Liquid);
					}
					if (Drams < num)
					{
						Volume += Drams;
					}
					else
					{
						Volume += num;
					}
					if (ParentObject != null && !IsOpenVolume())
					{
						string[] array = new string[ComponentLiquids.Keys.Count];
						ComponentLiquids.Keys.CopyTo(array, 0);
						string[] array2 = array;
						foreach (string key in array2)
						{
							Liquids[key].FillingContainer(ParentObject, this);
						}
					}
					RecalculatePrimary();
					RecalculateProperties();
				}
				else
				{
					MixWith(new LiquidVolume(Liquid, Math.Min(Drams, num)));
				}
				return true;
			}
		}
		return false;
	}

	public bool GiveDrams(string Liquid, ref int Drams, bool Auto = false, List<GameObject> StoredIn = null, GameObject Actor = null)
	{
		if (Drams > 0 && (IsPureLiquid(Liquid) || IsEmpty()) && (!Auto || Liquid == GetActiveAutogetLiquid()) && !EffectivelySealed() && ParentObject.AllowLiquidCollection(Liquid, Actor))
		{
			int num = MaxVolume - Volume;
			if (num > 0)
			{
				if (IsEmpty())
				{
					TrackAsLiquid(Liquid);
				}
				if (StoredIn != null && ParentObject != null)
				{
					StoredIn.Add(ParentObject);
				}
				if (Drams < num)
				{
					Volume += Drams;
					Drams = 0;
				}
				else
				{
					Drams -= num;
					Volume += num;
				}
				if (ParentObject != null && !IsOpenVolume())
				{
					string[] array = new string[ComponentLiquids.Keys.Count];
					ComponentLiquids.Keys.CopyTo(array, 0);
					string[] array2 = array;
					foreach (string key in array2)
					{
						Liquids[key].FillingContainer(ParentObject, this);
					}
				}
				RecalculatePrimary();
				RecalculateProperties();
				return true;
			}
		}
		return false;
	}

	public bool GiveDrams(string Liquid, int Drams, bool Auto = false, List<GameObject> StoredIn = null, GameObject Actor = null)
	{
		return GiveDrams(Liquid, ref Drams, Auto, StoredIn, Actor);
	}

	public bool Pour(ref bool RequestInterfaceExit, GameObject Actor = null, Cell TargetCell = null, bool Forced = false, bool Douse = false, int PourAmount = -1, bool OwnershipHandled = false)
	{
		if (Forced || !EffectivelySealed())
		{
			if (ParentObject.IsInStasis())
			{
				Actor?.Fail("You cannot seem to interact with " + ParentObject.t() + " in any way.");
				return false;
			}
			if (!Forced && !OwnershipHandled && Actor != null && Actor.IsPlayer() && ParentObject.Owner != null)
			{
				if (Popup.ShowYesNoCancel(ParentObject.IndicativeDistal + ParentObject.Is + " not owned by you. Are you sure you want to pour from " + ParentObject.them + "?") != DialogResult.Yes)
				{
					return false;
				}
				ParentObject.Physics.BroadcastForHelp(Actor);
			}
			Temporary part = ParentObject.GetPart<Temporary>();
			if (part != null)
			{
				if (IsOpenVolume())
				{
					part.Expire();
					if (GameObject.Validate(ParentObject))
					{
						Empty();
					}
				}
				else
				{
					Empty();
				}
				return false;
			}
			ExistenceSupport part2 = ParentObject.GetPart<ExistenceSupport>();
			if (part2 != null)
			{
				if (IsOpenVolume())
				{
					part2.Unsupported();
					if (GameObject.Validate(ParentObject))
					{
						Empty();
					}
				}
				else
				{
					Empty();
				}
				return false;
			}
			if (TargetCell == null)
			{
				if (!Forced && Actor != null && Actor.IsPlayer())
				{
					if (IsOpenVolume() || Actor.OnWorldMap())
					{
						return InventoryActionEvent.Check(ParentObject, Actor, ParentObject, "Fill", Auto: false, OwnershipHandled: true);
					}
					int num = Popup.PickOption("", "Where do you want to pour " + ParentObject.t() + "?", "", "Sounds/UI/ui_notification", new string[3]
					{
						"Pour it into another container.",
						"Pour it nearby.",
						"Pour it on " + Actor.itself + "."
					}, new char[3] { 'a', 'b', 'c' }, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
					if (num < 0)
					{
						return false;
					}
					switch (num)
					{
					case 0:
						return InventoryActionEvent.Check(ParentObject, Actor, ParentObject, "Fill", Auto: false, OwnershipHandled: true);
					case 1:
					{
						Douse = false;
						string text = XRL.UI.PickDirection.ShowPicker("Pour");
						if (text == null)
						{
							return false;
						}
						TargetCell = Actor.GetCurrentCell().GetCellFromDirection(text, BuiltOnly: false);
						break;
					}
					}
					if (num == 2)
					{
						Douse = true;
						TargetCell = Actor.GetCurrentCell();
					}
				}
				else
				{
					if (IsOpenVolume())
					{
						return false;
					}
					Douse = true;
					if (Actor != null)
					{
						TargetCell = Actor.GetCurrentCell();
					}
				}
			}
			if (PourAmount == -1)
			{
				if (Forced)
				{
					PourAmount = Volume;
				}
				else if (Actor != null && Actor.IsPlayer())
				{
					int? num2 = Popup.AskNumber("How many drams? (max=" + Volume + ")", "Sounds/UI/ui_notification", "", Volume, 0, Volume);
					try
					{
						PourAmount = Convert.ToInt32(num2);
					}
					catch
					{
						return false;
					}
				}
			}
			if (PourAmount > Volume)
			{
				PourAmount = Volume;
			}
			if (PourAmount <= 0)
			{
				return false;
			}
			if (Volume > 0)
			{
				if (Actor != null && Douse)
				{
					foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
					{
						if (!Liquids[componentLiquid.Key].Douse(this, Actor, ref PourAmount, ref RequestInterfaceExit))
						{
							return false;
						}
					}
					PlayWorldSound("Sounds/Interact/sfx_interact_liquidContainer_pourout");
					if (Actor.IsPlayer())
					{
						Popup.Show(PourAmount.Things("dram") + " of " + GetLiquidName() + " pours out all over you!");
					}
					else if (Actor.IsVisible() && Actor.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(PourAmount.Things("dram") + " of " + GetLiquidName() + " pours out all over " + Actor.t() + "!");
					}
					PourAmount -= ProcessContact(Actor, Initial: true, Prone: false, Poured: true, Actor, FromCell: false, PourAmount);
					if (PourAmount <= 0)
					{
						return true;
					}
				}
				if (TargetCell != null)
				{
					if (!Douse && Actor != null)
					{
						if (Actor != null && Actor.IsPlayer())
						{
							IComponent<GameObject>.PlayUISound("Sounds/Abilities/sfx_ability_generic_waterPour");
						}
						else
						{
							TargetCell?.PlayWorldSound("Sounds/Abilities/sfx_ability_generic_waterPour");
						}
						IComponent<GameObject>.XDidY(Actor, "pour", PourAmount.Things("dram") + " of " + GetLiquidName() + " out", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
					}
					if (!PourIntoCell(Actor, TargetCell, PourAmount, ref RequestInterfaceExit, !Forced))
					{
						return false;
					}
				}
			}
			else
			{
				Actor?.Fail("There's nothing in " + ParentObject.t() + " to pour.");
			}
		}
		return true;
	}

	public bool Pour(GameObject Actor = null, Cell TargetCell = null, bool Forced = false, bool Douse = false, int PourAmount = -1, bool OwnershipHandled = false)
	{
		bool RequestInterfaceExit = false;
		return Pour(ref RequestInterfaceExit, Actor, TargetCell, Forced, Douse, PourAmount, OwnershipHandled);
	}

	public double GetLiquidWeight()
	{
		return (double)Volume * GetLiquidWeightPerDram();
	}

	public void SmearOn(GameObject Object, GameObject By, bool FromCell)
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				string text = null;
				foreach (string key in ComponentLiquids.Keys)
				{
					text = key;
				}
				if (text != null)
				{
					GetLiquid(text).SmearOn(this, Object, By, FromCell);
				}
			}
			else
			{
				RequirePrimaryLiquid().SmearOn(this, Object, By, FromCell);
			}
			break;
		default:
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
				{
					keylist.Add(componentLiquid.Key);
				}
				foreach (string item in keylist)
				{
					GetLiquid(item).SmearOn(this, Object, By, FromCell);
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
			}
			break;
		}
		case 0:
			break;
		}
		ProcessExposure(Object, By, FromCell);
	}

	public void SmearOnTick(GameObject Object, GameObject By, bool FromCell)
	{
		switch (ComponentLiquids.Count)
		{
		case 1:
			if (Primary == null)
			{
				string text = null;
				foreach (string key in ComponentLiquids.Keys)
				{
					text = key;
				}
				if (text != null)
				{
					GetLiquid(text).SmearOnTick(this, Object, By, FromCell);
				}
			}
			else
			{
				RequirePrimaryLiquid().SmearOnTick(this, Object, By, FromCell);
			}
			break;
		default:
		{
			List<string> keylist = getKeylist();
			try
			{
				keylist.Clear();
				foreach (KeyValuePair<string, int> componentLiquid in ComponentLiquids)
				{
					keylist.Add(componentLiquid.Key);
				}
				foreach (string item in keylist)
				{
					GetLiquid(item).SmearOnTick(this, Object, By, FromCell);
				}
			}
			finally
			{
				keylistPool.Enqueue(keylist);
			}
			break;
		}
		case 0:
			break;
		}
		ProcessExposure(Object, By, FromCell);
	}

	public bool CanEvaporate()
	{
		if (!IsOpenVolume())
		{
			return false;
		}
		if (IsWadingDepth())
		{
			return false;
		}
		if (ComponentLiquids.Count == 1)
		{
			return false;
		}
		return true;
	}

	public bool CheckGroundLiquidMerge()
	{
		if (Volume <= 0)
		{
			return false;
		}
		if (!IsOpenVolume())
		{
			return false;
		}
		if (IsWadingDepth())
		{
			return false;
		}
		if (ParentObject == null)
		{
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		string groundLiquid = cell.GroundLiquid;
		if (groundLiquid.IsNullOrEmpty())
		{
			return false;
		}
		if (!IsPureLiquid(groundLiquid))
		{
			return false;
		}
		ParentObject.Obliterate();
		return true;
	}

	public bool PerformFill(GameObject Actor, ref bool RequestInterfaceExit, bool ownershipHandled = false)
	{
		if (EffectivelySealed())
		{
			return false;
		}
		if (ParentObject.IsInStasis())
		{
			Actor?.Fail("You cannot seem to interact with " + ParentObject.t() + " in any way.");
			return false;
		}
		if (Actor.IsPlayer() && ParentObject.Owner != null && !ownershipHandled)
		{
			if (Popup.ShowYesNoCancel(ParentObject.IndicativeDistal + ParentObject.Is + " not owned by you. Are you sure you want to take from " + ParentObject.them + "?") != DialogResult.Yes)
			{
				return false;
			}
			ParentObject.Physics.BroadcastForHelp(Actor);
		}
		if (!Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		List<GameObject> list = Event.NewGameObjectList();
		List<GameObject> list2 = Event.NewGameObjectList();
		List<GameObject> list3 = Event.NewGameObjectList();
		List<GameObject> list4 = Event.NewGameObjectList();
		Actor.GetInventoryAndEquipment(list4);
		int i = 0;
		for (int count = list4.Count; i < count; i++)
		{
			GameObject gameObject = list4[i];
			if (gameObject == ParentObject)
			{
				continue;
			}
			LiquidVolume liquidVolume = gameObject.LiquidVolume;
			if (liquidVolume != null && !liquidVolume.EffectivelySealed())
			{
				if (liquidVolume.LiquidSameAs(this))
				{
					list.Add(gameObject);
				}
				else
				{
					list2.Add(gameObject);
				}
			}
		}
		list3.AddRange(list);
		list3.AddRange(list2);
		if (list3.Count == 0)
		{
			Actor.Fail("You have no containers to fill.");
			return false;
		}
		GameObject gameObject2 = PickItem.ShowPicker(list3, null, PickItem.PickItemDialogStyle.SelectItemDialog, Actor, null, null, "[Select a container to fill]", PreserveOrder: true);
		if (gameObject2 != null)
		{
			if (gameObject2 == ParentObject)
			{
				Actor.Fail("You can't pour from a container into " + ParentObject.itself + ".");
				return false;
			}
			try
			{
				gameObject2 = gameObject2.RemoveOne();
				LiquidVolume liquidVolume2 = gameObject2.LiquidVolume;
				bool flag = false;
				if (liquidVolume2.Volume > 0 && !liquidVolume2.LiquidSameAs(this))
				{
					if (Actor.IsPlayer())
					{
						switch (Popup.ShowYesNoCancel("Do you want to empty " + gameObject2.t() + " first?"))
						{
						case DialogResult.Cancel:
							return false;
						case DialogResult.Yes:
							flag = true;
							break;
						}
					}
					else
					{
						flag = true;
					}
				}
				int num = Math.Min(flag ? liquidVolume2.MaxVolume : (liquidVolume2.MaxVolume - liquidVolume2.Volume), Volume);
				int num2 = 0;
				if (Actor.IsPlayer())
				{
					int? num3 = Popup.AskNumber("How many drams? (max=" + num + ")", "Sounds/UI/ui_notification", "", num, 0, num);
					try
					{
						num2 = Convert.ToInt32(num3);
					}
					catch
					{
						return false;
					}
				}
				else
				{
					num2 = num;
				}
				if (num2 > Volume)
				{
					num2 = Volume;
				}
				if (num2 <= 0)
				{
					return false;
				}
				PlayWorldSound("Sounds/Interact/sfx_interact_liquidContainer_fill");
				if (flag)
				{
					liquidVolume2.EmptyIntoCell(null, Actor);
				}
				int num4 = liquidVolume2.MaxVolume - liquidVolume2.Volume;
				int num5 = 0;
				num5 = ((num4 >= Volume) ? Volume : num4);
				if (num5 > num2)
				{
					num5 = num2;
				}
				liquidVolume2.MixWith(this, ref RequestInterfaceExit, Amount: num5, PouredFrom: ParentObject, PouredBy: Actor);
				CheckImage();
				liquidVolume2.CheckImage();
			}
			finally
			{
				gameObject2.CheckStack();
			}
		}
		return true;
	}

	public bool PerformFill(GameObject Actor, bool ownershipHandled = false)
	{
		bool RequestInterfaceExit = false;
		return PerformFill(Actor, ref RequestInterfaceExit, ownershipHandled);
	}

	private bool CanInteractWithAnything(Cell inCell = null)
	{
		if (inCell != null && IsOpenVolume() && inCell.HasBridge())
		{
			return false;
		}
		if (EffectivelySealed())
		{
			return false;
		}
		if (ParentObject.HasPart<FungalVision>() && FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		return true;
	}

	private bool CanInteractWith(GameObject obj, Cell inCell = null, bool AnythingChecked = false)
	{
		if (!GameObject.Validate(ref obj))
		{
			return false;
		}
		if (obj == ParentObject)
		{
			return false;
		}
		if (!obj.IsReal)
		{
			return false;
		}
		if (obj.IsScenery)
		{
			return false;
		}
		if (obj.IsBridge)
		{
			return false;
		}
		if (!AnythingChecked && !CanInteractWithAnything(inCell))
		{
			return false;
		}
		if (!ParentObject.PhaseAndFlightMatches(obj))
		{
			return false;
		}
		if (obj.GetMatterPhase() >= 3)
		{
			return false;
		}
		return true;
	}

	public bool CanPaintWith(int Group)
	{
		BaseLiquid primaryLiquid = GetPrimaryLiquid();
		if (primaryLiquid == null)
		{
			return false;
		}
		return primaryLiquid.GetPaintGroup(this) == Group;
	}

	public void Paint(int Mask)
	{
		if (LastPaintMask == Mask)
		{
			return;
		}
		BaseLiquid primaryLiquid = GetPrimaryLiquid();
		if (primaryLiquid != null)
		{
			if (Mask == 0 && !IsWadingDepth())
			{
				ParentObject.Render.Tile = primaryLiquid.GetPuddle(this);
			}
			else
			{
				ParentObject.Render.Tile = PaintBuilder.Clear().Append(primaryLiquid.GetPaintAtlas(this)).Append(primaryLiquid.GetPaint(this))
					.Append('-')
					.AppendMask(Mask, 8)
					.Append(primaryLiquid.GetPaintExtension(this))
					.ToString();
			}
			LastPaintMask = Mask;
		}
	}

	public static string GetCleaningLiquidGeneralization()
	{
		string text = null;
		foreach (BaseLiquid value in Liquids.Values)
		{
			if (!value.EnableCleaning)
			{
				continue;
			}
			if (text == null)
			{
				text = value.GetName().Strip();
				continue;
			}
			if (text.Contains(" or "))
			{
				text = null;
				break;
			}
			string text2 = value.GetName().Strip();
			text = ((text.CompareTo(text2) <= 0) ? (text + " or " + text2) : (text2 + " or " + text));
		}
		return text ?? "liquid";
	}
}
