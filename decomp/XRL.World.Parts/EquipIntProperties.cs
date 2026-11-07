using System;
using System.Collections.Generic;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class EquipIntProperties : IPoweredPart
{
	public string Props = "";

	[NonSerialized]
	public Dictionary<string, int> _BonusList;

	public bool bApplied;

	public EquipIntProperties()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		WorksOnEquipper = true;
	}

	public EquipIntProperties(string Props)
		: this()
	{
		this.Props = Props;
	}

	public static void AppendIntPropertiesOnEquip(GameObject obj, string prop)
	{
		EquipIntProperties equipIntProperties = null;
		if (obj.HasPart<EquipIntProperties>())
		{
			equipIntProperties = obj.GetPart<EquipIntProperties>();
			equipIntProperties.AddProps(prop);
		}
		else
		{
			equipIntProperties = new EquipIntProperties(prop);
			equipIntProperties.DescribeStatusForProperty = null;
			obj.AddPart(equipIntProperties);
		}
	}

	public void AddProps(string Spec)
	{
		Dictionary<string, int> bonusList = GetBonusList();
		Dictionary<string, int> dictionary = DeterminePropsList(Spec);
		foreach (string key in dictionary.Keys)
		{
			if (bonusList.ContainsKey(key))
			{
				bonusList[key] += dictionary[key];
			}
			else
			{
				bonusList.Add(key, dictionary[key]);
			}
		}
		Props = PropsToString(bonusList);
	}

	private static Dictionary<string, int> DeterminePropsList(string Spec)
	{
		if (string.IsNullOrEmpty(Spec))
		{
			return new Dictionary<string, int>(1);
		}
		string[] array = Spec.Split(';');
		Dictionary<string, int> dictionary = new Dictionary<string, int>(array.Length);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!string.IsNullOrEmpty(text))
			{
				string[] array3 = text.Split(':');
				if (dictionary.ContainsKey(array3[0]))
				{
					dictionary[array3[0]] += Convert.ToInt32(array3[1]);
				}
				else
				{
					dictionary.Add(array3[0], Convert.ToInt32(array3[1]));
				}
			}
		}
		return dictionary;
	}

	private static string PropsToString(Dictionary<string, int> Bonuses)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int num = 0;
		foreach (KeyValuePair<string, int> Bonuse in Bonuses)
		{
			if (num++ > 0)
			{
				stringBuilder.Append(';');
			}
			stringBuilder.Append(Bonuse.Key).Append(':').Append(Bonuse.Value);
		}
		return stringBuilder.ToString();
	}

	private Dictionary<string, int> GetBonusList(bool bForceRebuild = false)
	{
		if ((_BonusList == null) | bForceRebuild)
		{
			_BonusList = DeterminePropsList(Props);
		}
		return _BonusList;
	}

	public void Apply(GameObject Object)
	{
		if (bApplied)
		{
			return;
		}
		Dictionary<string, int> bonusList = GetBonusList();
		foreach (string key in bonusList.Keys)
		{
			if (!string.IsNullOrEmpty(key))
			{
				int value = bonusList[key];
				Object.ModIntProperty(key, value);
			}
		}
		bApplied = true;
		Object.SyncMutationLevelAndGlimmer();
	}

	public void UnapplyEffects(GameObject Object)
	{
		if (!bApplied)
		{
			return;
		}
		Dictionary<string, int> bonusList = GetBonusList();
		foreach (string key in bonusList.Keys)
		{
			if (!string.IsNullOrEmpty(key))
			{
				int num = bonusList[key];
				Object.ModIntProperty(key, -num);
			}
		}
		bApplied = false;
		Object.SyncMutationLevelAndGlimmer();
	}

	public override bool SameAs(IPart p)
	{
		if ((p as EquipIntProperties).Props != Props)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (bApplied)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
		CheckApplyEffects();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckApplyEffects();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApplyEffects(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		UnapplyEffects(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		CheckApplyEffects();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void CheckApplyEffects(GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject.Physics.Equipped;
			if (obj == null)
			{
				return;
			}
		}
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			UnapplyEffects(obj);
		}
		else
		{
			Apply(obj);
		}
	}
}
