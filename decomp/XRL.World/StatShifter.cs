using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.Language;

namespace XRL.World;

public class StatShifter
{
	public string DefaultDisplayName;

	public GameObject Owner;

	public Dictionary<string, Dictionary<string, Guid>> ActiveShifts;

	public static void Save(StatShifter Shifter, SerializationWriter Writer)
	{
		Dictionary<string, Dictionary<string, Guid>> dictionary = Shifter?.ActiveShifts;
		int num = dictionary?.Count ?? 0;
		Writer.WriteOptimized(num);
		if (num <= 0)
		{
			return;
		}
		Writer.WriteOptimized(Shifter.DefaultDisplayName);
		foreach (KeyValuePair<string, Dictionary<string, Guid>> item in dictionary)
		{
			Writer.WriteOptimized(item.Key);
			Writer.WriteOptimized(item.Value.Count);
			foreach (KeyValuePair<string, Guid> item2 in item.Value)
			{
				Writer.WriteOptimized(item2.Key);
				Writer.Write(item2.Value);
			}
		}
	}

	public static StatShifter Load(SerializationReader Reader, GameObject Owner)
	{
		int num = Reader.ReadOptimizedInt32();
		if (num <= 0)
		{
			return null;
		}
		StatShifter statShifter = new StatShifter(Owner);
		statShifter.DefaultDisplayName = Reader.ReadOptimizedString();
		statShifter.ActiveShifts = new Dictionary<string, Dictionary<string, Guid>>(num);
		for (int i = 0; i < num; i++)
		{
			string key = Reader.ReadOptimizedString();
			int num2 = Reader.ReadOptimizedInt32();
			Dictionary<string, Guid> dictionary = new Dictionary<string, Guid>(num2);
			for (int j = 0; j < num2; j++)
			{
				dictionary[Reader.ReadOptimizedString()] = Reader.ReadGuid();
			}
			statShifter.ActiveShifts[key] = dictionary;
		}
		return statShifter;
	}

	public StatShifter(GameObject Owner)
	{
		this.Owner = Owner;
	}

	public StatShifter(GameObject Owner, string DefaultDisplayName)
	{
		this.Owner = Owner;
		this.DefaultDisplayName = DefaultDisplayName;
	}

	public StatShifter(GameObject Owner, StatShifter Source)
	{
		this.Owner = Owner;
		if (Source == null || Source.ActiveShifts == null)
		{
			return;
		}
		ActiveShifts = new Dictionary<string, Dictionary<string, Guid>>();
		foreach (KeyValuePair<string, Dictionary<string, Guid>> activeShift in Source.ActiveShifts)
		{
			ActiveShifts.Add(activeShift.Key, activeShift.Value.ToDictionary((KeyValuePair<string, Guid> e) => e.Key, (KeyValuePair<string, Guid> e) => e.Value));
		}
	}

	/// <summary>Shift a stat on the owner by an amount, further calls to SetStatShift with the same stat will
	/// undo the previous shift, and set to the new amount.</summary>
	public bool SetStatShift(string statName, int amount, bool baseValue = false)
	{
		return SetStatShift(Owner, statName, amount, baseValue);
	}

	/// <summary>Get the value of the stat shift (optional base value) applied by this object</summary>
	public int GetStatShift(string statNAme, bool baseValue = false)
	{
		return GetStatShift(Owner, statNAme, baseValue);
	}

	/// <summary>Get the value of the stat shift (optional base value) applied by this object</summary>
	public int GetStatShift(GameObject target, string statName, bool baseValue = false)
	{
		if (target == null || !target.IsValid() || !target.HasStat(statName) || ActiveShifts == null)
		{
			return 0;
		}
		if (!ActiveShifts.TryGetValue(target.ID, out var value))
		{
			return 0;
		}
		string key = statName + (baseValue ? ":base" : "");
		if (!value.TryGetValue(key, out var value2))
		{
			return 0;
		}
		return target.Statistics[statName].GetShift(value2).Amount;
	}

	/// <summary>Shift a stat on target object by an amount, further calls to SetStatShift with the same stat will
	/// undo the previous shift, and set to the new amount.</summary>
	public bool SetStatShift(GameObject target, string statName, int amount, bool baseValue = false)
	{
		if (target == null || !target.IsValid() || !target.HasStat(statName))
		{
			return false;
		}
		if (ActiveShifts == null)
		{
			ActiveShifts = new Dictionary<string, Dictionary<string, Guid>>();
		}
		if (!ActiveShifts.TryGetValue(target.ID, out var value))
		{
			value = new Dictionary<string, Guid>();
			ActiveShifts.Add(target.ID, value);
		}
		Statistic statistic = target.Statistics[statName];
		string displayName = DefaultDisplayName;
		if (target != Owner)
		{
			displayName = ((!string.IsNullOrEmpty(DefaultDisplayName)) ? (Grammar.MakePossessive(Owner.ShortDisplayName) + " " + DefaultDisplayName) : Owner.ShortDisplayName);
		}
		string text = statName;
		if (baseValue)
		{
			text += ":base";
		}
		if (!value.TryGetValue(text, out var value2))
		{
			if (amount == 0)
			{
				return true;
			}
			value2 = statistic.AddShift(amount, displayName, baseValue);
			value.Add(text, value2);
		}
		else if (amount == 0)
		{
			statistic.RemoveShift(value2);
			value.Remove(text);
		}
		else if (!statistic.UpdateShift(value2, amount))
		{
			value[text] = statistic.AddShift(amount, displayName);
		}
		return true;
	}

	public bool HasStatShifts()
	{
		if (ActiveShifts != null)
		{
			return ActiveShifts.Count > 0;
		}
		return false;
	}

	public void RemoveStatShifts()
	{
		if (!HasStatShifts())
		{
			return;
		}
		foreach (var (text2, shifts) in ActiveShifts)
		{
			try
			{
				GameObject gameObject = ((Owner != null && Owner.ID == text2) ? Owner : GameObject.FindByID(text2));
				if (gameObject == null)
				{
					throw new Exception("Can't resolve object id " + text2);
				}
				RemoveTargetShifts(gameObject, shifts);
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Unresolved object trying to remove stat shifts", x);
			}
		}
		ActiveShifts.Clear();
	}

	public void RemoveStatShifts(GameObject target)
	{
		if (GameObject.Validate(ref target) && HasStatShifts() && ActiveShifts.TryGetValue(target.ID, out var value))
		{
			RemoveTargetShifts(target, value);
			ActiveShifts.Remove(target.ID);
		}
	}

	private void RemoveTargetShifts(GameObject Target, Dictionary<string, Guid> Shifts)
	{
		foreach (KeyValuePair<string, Guid> Shift in Shifts)
		{
			string key = (Shift.Key.EndsWith(":base") ? Shift.Key.Substring(0, Shift.Key.Length - 5) : Shift.Key);
			Target.Statistics[key].RemoveShift(Shift.Value);
		}
	}

	public void RemoveStatShift(GameObject target, string stat, bool baseValue = false)
	{
		if (GameObject.Validate(ref target) && HasStatShifts() && ActiveShifts.TryGetValue(target.ID, out var value))
		{
			string key = stat + (baseValue ? ":base" : "");
			if (value.TryGetValue(key, out var value2))
			{
				target.Statistics[stat].RemoveShift(value2);
				value.Remove(key);
			}
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("[StatShifter Owner:").Append(Owner.ID).Append(" Description: ")
			.Append(DefaultDisplayName);
		stringBuilder.Append(" ");
		if (HasStatShifts())
		{
			foreach (KeyValuePair<string, Dictionary<string, Guid>> activeShift in ActiveShifts)
			{
				stringBuilder.Append("[Object:").Append(activeShift.Key).Append(" ");
				foreach (KeyValuePair<string, Guid> item in activeShift.Value)
				{
					stringBuilder.Append(" Stat:").Append(item.Key).Append(":")
						.Append(item.Value);
				}
				stringBuilder.Append("] ");
			}
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}
}
