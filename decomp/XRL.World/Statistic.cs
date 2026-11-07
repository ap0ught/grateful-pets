using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class Statistic
{
	public class IntBox
	{
		public int i;

		public IntBox(int _i)
		{
			i = _i;
		}
	}

	[Serializable]
	public struct StatShift
	{
		public Guid ID;

		public int Amount;

		public string DisplayName;

		public bool BaseValue;

		public void Write(SerializationWriter Writer)
		{
			Writer.Write(ID);
			Writer.WriteOptimized(Amount);
			Writer.Write(DisplayName);
			Writer.Write(BaseValue);
		}

		public void Read(SerializationReader Reader)
		{
			ID = Reader.ReadGuid();
			Amount = Reader.ReadOptimizedInt32();
			DisplayName = Reader.ReadString();
			BaseValue = Reader.ReadBoolean();
		}
	}

	[NonSerialized]
	public static List<string> Attributes = new List<string>(6) { "Strength", "Agility", "Toughness", "Intelligence", "Willpower", "Ego" };

	[NonSerialized]
	public static List<string> MentalStats = new List<string>(4) { "Ego", "Intelligence", "Willpower", "MA" };

	[NonSerialized]
	public static List<string> InverseBenefitStats = new List<string>(1) { "MoveSpeed" };

	[NonSerialized]
	public static Dictionary<string, string> StatDisplayNames = new Dictionary<string, string>
	{
		{ "AcidResistance", "acid resistance" },
		{ "ColdResistance", "cold resistance" },
		{ "ElectricResistance", "electric resistance" },
		{ "HeatResistance", "heat resistance" },
		{ "Hitpoints", "hit points" },
		{ "MoveSpeed", "move speed" },
		{ "Speed", "quickness" },
		{ "AP", "attribute points" },
		{ "MP", "mutation points" },
		{ "SP", "skill points" },
		{ "XP", "experience points" }
	};

	[NonSerialized]
	public static Dictionary<string, string> StatCapitalizedDisplayNames = new Dictionary<string, string>
	{
		{ "AcidResistance", "Acid Resistance" },
		{ "ColdResistance", "Cold Resistance" },
		{ "ElectricResistance", "Electric Resistance" },
		{ "HeatResistance", "Heat Resistance" },
		{ "Hitpoints", "Hit Points" },
		{ "MoveSpeed", "Move Speed" },
		{ "Speed", "Quickness" },
		{ "AP", "Attribute Points" },
		{ "MP", "Mutation Points" },
		{ "SP", "Skill Points" },
		{ "XP", "Experience Points" }
	};

	[NonSerialized]
	public static Dictionary<string, string> StatShortNames = new Dictionary<string, string>
	{
		{ "AcidResistance", "AR" },
		{ "ColdResistance", "CR" },
		{ "ElectricResistance", "ER" },
		{ "HeatResistance", "HR" },
		{ "MoveSpeed", "MS" },
		{ "Speed", "QN" },
		{ "Hitpoints", "HP" },
		{ "Strength", "STR" },
		{ "Intelligence", "INT" },
		{ "Agility", "AGI" },
		{ "Toughness", "TOU" },
		{ "Willpower", "WIL" },
		{ "Ego", "EGO" }
	};

	[NonSerialized]
	public static Dictionary<string, string> StatTitleCaseShortNames = new Dictionary<string, string>
	{
		{ "AcidResistance", "AR" },
		{ "ColdResistance", "CR" },
		{ "ElectricResistance", "ER" },
		{ "HeatResistance", "HR" },
		{ "MoveSpeed", "MS" },
		{ "Speed", "QN" },
		{ "Hitpoints", "HP" },
		{ "Strength", "Str" },
		{ "Intelligence", "Int" },
		{ "Agility", "Agi" },
		{ "Toughness", "Tou" },
		{ "Willpower", "Wil" },
		{ "Ego", "Ego" }
	};

	[NonSerialized]
	public static List<string> StatPlurality = new List<string> { "Hitpoints", "AP", "MP", "SP", "XP" };

	[NonSerialized]
	public static Dictionary<string, string> StatNarration = new Dictionary<string, string> { { "Hitpoints", "life" } };

	public GameObject Owner;

	public string Name = "";

	public string sValue = "";

	public int Boost;

	public static Dictionary<string, IntBox> _Max = new Dictionary<string, IntBox>();

	public static Dictionary<string, IntBox> _Min = new Dictionary<string, IntBox>();

	private IntBox _MaxValue;

	private IntBox _MinValue;

	public int _Value;

	[NonSerialized]
	private string StatChangeID;

	[NonSerialized]
	private Event eStatChange;

	public int _Bonus;

	public int _Penalty;

	public List<StatShift> Shifts;

	public string ValueOrSValue
	{
		get
		{
			if (sValue != "")
			{
				return sValue;
			}
			return Value.ToString();
		}
	}

	public int Max
	{
		get
		{
			if (_MaxValue != null || _Max.TryGetValue(Name, out _MaxValue))
			{
				return _MaxValue.i;
			}
			return 30;
		}
		set
		{
			if (_MaxValue == null && !_Max.TryGetValue(Name, out _MaxValue))
			{
				_Max[Name] = (_MaxValue = new IntBox(value));
			}
			else
			{
				_MaxValue.i = value;
			}
		}
	}

	public int Min
	{
		get
		{
			if (_MinValue != null || _Min.TryGetValue(Name, out _MinValue))
			{
				return _MinValue.i;
			}
			return 0;
		}
		set
		{
			if (_MinValue == null && !_Min.TryGetValue(Name, out _MinValue))
			{
				_Min[Name] = (_MinValue = new IntBox(value));
			}
			else
			{
				_MinValue.i = value;
			}
		}
	}

	public int BaseValue
	{
		get
		{
			return _Value;
		}
		set
		{
			if (_Value != value)
			{
				int value2 = Value;
				int baseValue = BaseValue;
				_Value = value;
				NotifyChange(value2, baseValue, "BaseValue");
			}
		}
	}

	public int Modifier => Stat.GetScoreModifier(Value);

	public int Value
	{
		get
		{
			int num = _Value + _Bonus - _Penalty;
			if (num < Min)
			{
				return Min;
			}
			if (num > Max)
			{
				return Max;
			}
			return num;
		}
	}

	public int Bonus
	{
		get
		{
			return _Bonus;
		}
		set
		{
			if (_Bonus != Math.Max(0, value))
			{
				int value2 = Value;
				int baseValue = BaseValue;
				_Bonus = value;
				if (_Bonus < 0)
				{
					_Bonus = 0;
				}
				NotifyChange(value2, baseValue, "Bonus");
			}
		}
	}

	public int Penalty
	{
		get
		{
			return _Penalty;
		}
		set
		{
			if (_Penalty != Math.Max(0, value))
			{
				int value2 = Value;
				int baseValue = BaseValue;
				_Penalty = value;
				if (_Penalty < 0)
				{
					_Penalty = 0;
				}
				NotifyChange(value2, baseValue, "Penalty");
			}
		}
	}

	public static bool IsMental(string Stat)
	{
		return MentalStats.Contains(Stat);
	}

	public static bool IsInverseBenefit(string Stat)
	{
		return InverseBenefitStats.Contains(Stat);
	}

	public static string GetStatTitleCaseShortNames(string Stat)
	{
		if (!StatTitleCaseShortNames.TryGetValue(Stat, out var value))
		{
			return Stat;
		}
		return value;
	}

	public static string GetStatShortName(string Stat)
	{
		if (!StatShortNames.TryGetValue(Stat, out var value))
		{
			return Stat.Substring(0, 2).ToUpper();
		}
		return value;
	}

	public static string GetStatDisplayName(string Stat)
	{
		if (!StatDisplayNames.TryGetValue(Stat, out var value))
		{
			return Stat;
		}
		return value;
	}

	public static string GetStatCapitalizedDisplayName(string Stat)
	{
		if (!StatCapitalizedDisplayNames.TryGetValue(Stat, out var value))
		{
			return Stat;
		}
		return value;
	}

	public static bool IsStatPlural(string Stat)
	{
		return StatPlurality.Contains(Stat);
	}

	public static string GetStatNarration(string Stat)
	{
		if (!StatNarration.TryGetValue(Stat, out var value))
		{
			return GetStatDisplayName(Stat);
		}
		return value;
	}

	public string GetDisplayValue()
	{
		if (InverseBenefitStats.Contains(Name))
		{
			return (200 - Value).ToStringCached();
		}
		return Value.ToStringCached();
	}

	public string GetHelpText()
	{
		if (Name == "Strength")
		{
			return "Your {{W|Strength}} determines how much melee damage you do (by improving your armor penetration), your ability to resist forced movement, and your carry capacity.";
		}
		if (Name == "Agility")
		{
			return "Your {{W|Agility}} determines your accuracy with both melee and ranged weapons and your chance to dodge attacks.";
		}
		if (Name == "Toughness")
		{
			return "Your {{W|Toughness}} determines your number of hit points, your hit point regeneration rate, and your ability to resist poison and disease.";
		}
		if (Name == "Intelligence")
		{
			return "Your {{W|Intelligence}} determines your number of skill points and your ability to examine artifacts.";
		}
		if (Name == "Willpower")
		{
			return "Your {{W|Willpower}} modifies the cooldowns of your activated abilities, determines your ability to resist mental attacks, and modifies your hit point regeneration rate.";
		}
		if (Name == "Ego")
		{
			return "Your {{W|Ego}} determines the potency of your mental mutations, your ability to haggle with merchants, and your ability to dominate the wills of other living creatures.";
		}
		if (Name == "AV")
		{
			return "Your {{W|Armor Value (AV)}} measures how well protected you are against physical attacks that hit you. The higher your score, the fewer times an opponent's attack will penetrate your armor and do damage to you. Your base AV is 0.";
		}
		if (Name == "DV")
		{
			return "Your {{W|Dodge Value (DV)}} measures how likely you are to be hit by physical attacks. The higher your score, the less likely an opponent's attack will hit you. Your DV is modified by your Agility modifier. Your base DV is 6.";
		}
		if (Name == "MA")
		{
			return "Your {{W|mental armor (MA)}} measures how well protected you are against mental attacks. The higher your score, the less likely an opponent's mental attack will penetrate your defenses and harm you. Your MA is modified by your Willpower modifier. Your base MA is 4.";
		}
		if (Name == "Speed")
		{
			return "Your {{W|Quickness}} measures the speed you perform every action you take. Your base Quickness score is 100.";
		}
		if (Name == "MoveSpeed")
		{
			return "Your {{W|Move Speed}} measures how fast you walk or fly. Your base move speed is 100.";
		}
		if (Name == "T")
		{
			return "Your {{W|Temperature}} measures how hot or cold you are. Being too cold can reduce your Quickness and prevent you from taking physical actions. Being too hot can set you aflame and cause damage every round.";
		}
		if (Name == "AcidResistance")
		{
			return "Your {{W|Acid Resist}} determines how much acid damage you ablate. Your base acid resist score is 0. At 100, you are immune to acid damage.";
		}
		if (Name == "ColdResistance")
		{
			return "Your {{W|Cold Resist}} determines how much cold damage you ablate and how insulated you are from effects that reduce your temperature. Your base Cold Resist score is 0. At 100, you are immune to cold damage and your temperature cannot be reduced.";
		}
		if (Name == "ElectricResistance")
		{
			return "Your {{W|Electrical Resist}} determines how much electrical damage you ablate and how resistive you are to electric current. Your base Electrical Resist score is 0. At 100, you are immune to electrical damage and you do not conduct electricity.";
		}
		if (Name == "HeatResistance")
		{
			return "Your {{W|Heat Resist}} determines how much heat damage you ablate and how insulated you are from effects that increase your temperature. Your base Heat Resist score is 0. At 100, you are immune to heat damage and your temperature cannot be increased.";
		}
		return "";
	}

	[Obsolete("Use AppendStatAdjustDescription")]
	public static void GetStatAdjustDescription(StringBuilder SB, string Stat, int Adjust, bool Activated = false, bool Percent = false)
	{
		AppendStatAdjustDescription(SB, Stat, Adjust, Activated, Percent);
	}

	public static void AppendStatAdjustDescription(StringBuilder SB, string Stat, int Adjust, bool Activated = false, bool Percent = false)
	{
		if (Activated)
		{
			SB.Append("When activated, ");
		}
		if (IsInverseBenefit(Stat))
		{
			Adjust = -Adjust;
		}
		if (Adjust > 0)
		{
			SB.Append('+');
		}
		SB.Append(Adjust);
		if (Percent)
		{
			SB.Append('%');
		}
		SB.Append(' ').Append(GetStatDisplayName(Stat));
	}

	public static void CompoundStatAdjustDescription(StringBuilder SB, string Stat, int Adjust, char With = ' ', bool Activated = false, bool Percent = false)
	{
		if (SB.Length > 0)
		{
			SB.Append(With);
		}
		AppendStatAdjustDescription(SB, Stat, Adjust, Activated, Percent);
	}

	public static string GetStatAdjustDescription(string Stat, int Adjust, bool Activated = false)
	{
		StringBuilder sB = Event.NewStringBuilder();
		AppendStatAdjustDescription(sB, Stat, Adjust, Activated);
		return Event.FinalizeString(sB);
	}

	private static int StatisticComparer(string a, string b)
	{
		if (a == b)
		{
			return 0;
		}
		int num = Attributes.IndexOf(a);
		int num2 = Attributes.IndexOf(b);
		if (num != -1)
		{
			if (num2 != -1)
			{
				return num.CompareTo(num2);
			}
			return -1;
		}
		if (num2 != -1)
		{
			return 1;
		}
		return a.CompareTo(b);
	}

	public static void SortStatistics(List<string> list)
	{
		list.Sort(StatisticComparer);
	}

	public Statistic()
	{
	}

	public Statistic(Statistic Source)
		: this()
	{
		Name = Source.Name;
		Min = Source.Min;
		Max = Source.Max;
		Penalty = Source.Penalty;
		Bonus = Source.Bonus;
		BaseValue = Source.BaseValue;
		Owner = Source.Owner;
		sValue = Source.sValue;
		Boost = Source.Boost;
		if (Source.Shifts != null)
		{
			Shifts = new List<StatShift>();
			{
				foreach (StatShift shift in Source.Shifts)
				{
					Shifts.Add(new StatShift
					{
						ID = shift.ID,
						Amount = shift.Amount,
						DisplayName = shift.DisplayName,
						BaseValue = shift.BaseValue
					});
				}
				return;
			}
		}
		Shifts = null;
	}

	public Statistic(string name, int min, int max, int val, GameObject parent)
	{
		Name = name;
		Min = min;
		Max = max;
		Owner = parent;
		Penalty = 0;
		Bonus = 0;
		BaseValue = val;
		Boost = 0;
	}

	public void BoostStat(int Amount)
	{
		if (Amount > 0)
		{
			BaseValue += (int)Math.Ceiling((double)BaseValue * 0.25 * (double)Amount);
		}
		else if (Amount < 0)
		{
			BaseValue += (int)Math.Ceiling((double)BaseValue * 0.2 * (double)Amount);
		}
	}

	public void BoostStat(double Amount)
	{
		if (Amount > 0.0)
		{
			BaseValue += (int)Math.Ceiling((double)BaseValue * 0.25 * Amount);
		}
		else if (Amount < 0.0)
		{
			BaseValue += (int)Math.Ceiling((double)BaseValue * 0.2 * Amount);
		}
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Name);
		Writer.WriteOptimized(_Bonus);
		Writer.WriteOptimized(_Penalty);
		Writer.WriteOptimized(_Value);
		int num = Shifts?.Count ?? 0;
		Writer.WriteOptimized(num);
		for (int i = 0; i < num; i++)
		{
			Shifts[i].Write(Writer);
		}
	}

	public static Statistic Load(SerializationReader Reader, GameObject Owner)
	{
		Statistic statistic = new Statistic();
		statistic.Owner = Owner;
		statistic.Name = Reader.ReadOptimizedString();
		statistic._Bonus = Reader.ReadOptimizedInt32();
		statistic._Penalty = Reader.ReadOptimizedInt32();
		statistic._Value = Reader.ReadOptimizedInt32();
		int num = Reader.ReadOptimizedInt32();
		if (num > 0)
		{
			statistic.Shifts = new List<StatShift>(num);
			for (int i = 0; i < num; i++)
			{
				StatShift item = default;
				item.Read(Reader);
				statistic.Shifts.Add(item);
			}
		}
		return statistic;
	}

	public bool SameAs(Statistic S)
	{
		if (Name != S.Name)
		{
			return false;
		}
		if (Min != S.Min)
		{
			return false;
		}
		if (Max != S.Max)
		{
			return false;
		}
		if (Penalty != S.Penalty)
		{
			return false;
		}
		if (Bonus != S.Bonus)
		{
			return false;
		}
		if (Value != S.Value)
		{
			return false;
		}
		return true;
	}

	public void NotifyChange(int OldValue, int OldBaseValue, string Type)
	{
		if (Owner == null)
		{
			return;
		}
		if (StatChangeID == null)
		{
			StatChangeID = "StatChange_" + Name;
		}
		if (Owner.HasRegisteredEvent(StatChangeID))
		{
			if (eStatChange == null)
			{
				eStatChange = new Event(StatChangeID, 0, 2, 4);
			}
			eStatChange.SetParameter("Stat", Name);
			eStatChange.SetParameter("OldValue", OldValue);
			eStatChange.SetParameter("NewValue", Value);
			eStatChange.SetParameter("OldBaseValue", OldBaseValue);
			eStatChange.SetParameter("NewBaseValue", BaseValue);
			eStatChange.SetParameter("Type", Type);
			Owner.FireEvent(eStatChange);
		}
		if (Owner.WantEvent(PooledEvent<StatChangeEvent>.ID, MinEvent.CascadeLevel))
		{
			StatChangeEvent e = StatChangeEvent.FromPool(Owner, Name, Type, OldValue, Value, OldBaseValue, BaseValue, this);
			Owner.HandleEvent(e);
		}
	}

	public override string ToString()
	{
		return Name + ": " + Value;
	}

	public string GetDisplayName()
	{
		return GetStatDisplayName(Name);
	}

	public string GetShortDisplayName()
	{
		return GetStatShortName(Name);
	}

	public StatShift GetShift(Guid shiftId)
	{
		return Shifts.Find((StatShift s) => s.ID == shiftId);
	}

	public Guid AddShift(int amount, string DisplayName, bool baseValue = false)
	{
		Guid guid = Guid.NewGuid();
		StatShift item = new StatShift
		{
			ID = guid,
			Amount = amount,
			DisplayName = DisplayName,
			BaseValue = baseValue
		};
		if (Shifts == null)
		{
			Shifts = new List<StatShift>(1);
		}
		if (Options.DebugStatShift && The.Game != null)
		{
			The.Game.Player.Messages.Add($"DEBUG: NEW STAT SHIFT - {Owner.DisplayNameOnly}'s {Name} was shifted {item.Amount} by the {item.DisplayName}.");
		}
		Shifts.Add(item);
		if (baseValue)
		{
			BaseValue += amount;
		}
		else if (amount > 0)
		{
			Bonus += amount;
		}
		else
		{
			Penalty += -amount;
		}
		return guid;
	}

	public void RemoveShift(Guid id)
	{
		if (Shifts == null)
		{
			return;
		}
		int num = Shifts.FindIndex((StatShift s) => s.ID == id);
		if (num != -1)
		{
			StatShift statShift = Shifts[num];
			if (Options.DebugStatShift && The.Game != null)
			{
				The.Game.Player.Messages.Add($"DEBUG: REMOVED STAT SHIFT - {Owner.DisplayNameOnlyDirect}'s {Name} was shifted {statShift.Amount} by the {statShift.DisplayName}.");
			}
			if (statShift.BaseValue)
			{
				BaseValue -= statShift.Amount;
			}
			if (statShift.Amount > 0)
			{
				Bonus -= statShift.Amount;
			}
			else
			{
				Penalty -= -statShift.Amount;
			}
			Shifts.RemoveAt(num);
			if (Shifts.Count == 0 && !Owner.IsPlayerControlled())
			{
				Shifts = null;
			}
		}
	}

	public bool UpdateShift(Guid id, int newAmount)
	{
		if (Shifts == null)
		{
			return false;
		}
		int num = Shifts.FindIndex((StatShift s) => s.ID == id);
		if (num == -1)
		{
			return false;
		}
		StatShift item = Shifts[num];
		int amount = item.Amount;
		int num2 = newAmount - amount;
		if (Options.DebugStatShift)
		{
			XRLCore.Core.Game.Player.Messages.Add($"DEBUG: UPDATED STAT SHIFT - {Owner.DebugName}'s {Name} shift was {amount}, now {newAmount}, change {num2}, from {item.DisplayName}.");
		}
		if (num2 == 0)
		{
			return true;
		}
		if (item.BaseValue)
		{
			BaseValue += num2;
		}
		else if (newAmount == 0)
		{
			if (amount > 0)
			{
				Bonus -= amount;
			}
			else
			{
				Penalty -= -amount;
			}
		}
		else if (amount == 0 || amount >= 0 == newAmount >= 0)
		{
			if (newAmount >= 0)
			{
				Bonus += num2;
			}
			else
			{
				Penalty += -num2;
			}
		}
		else
		{
			if (newAmount > 0)
			{
				Bonus += newAmount;
			}
			if (amount > 0)
			{
				Bonus -= amount;
			}
			else
			{
				Penalty -= -amount;
			}
			if (newAmount <= 0)
			{
				Penalty += -newAmount;
			}
		}
		item.Amount = newAmount;
		Shifts.RemoveAt(num);
		Shifts.Add(item);
		return true;
	}
}
