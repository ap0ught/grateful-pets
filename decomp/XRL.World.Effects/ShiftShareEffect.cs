using System;
using System.Collections.Generic;

namespace XRL.World.Effects;

[Serializable]
public class ShiftShareEffect : Effect
{
	public GameObject Source;

	public override string GetDescription()
	{
		return null;
	}

	public void TakeShifts(GameObject Item)
	{
		GameObject equipped = Item.Equipped;
		string iD = equipped.ID;
		foreach (IPart parts in Item.PartsList)
		{
			if (!parts.HasStatShifts() || !parts.StatShifter.ActiveShifts.TryGetValue(iD, out var value))
			{
				continue;
			}
			foreach (KeyValuePair<string, Guid> item in value)
			{
				if (equipped.Statistics.TryGetValue(item.Key, out var value2))
				{
					Statistic.StatShift shift = value2.GetShift(item.Value);
					StatShifter.SetStatShift(value2.Name, shift.Amount);
				}
			}
		}
	}

	public override bool Apply(GameObject Object)
	{
		TakeShifts(Source);
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Duration != 9999)
		{
			Duration--;
		}
		if (!GameObject.Validate(ref Source) || !GameObject.Validate(Source.Equipped) || Source.Equipped.PartyLeader != Object)
		{
			Duration = 0;
		}
		return base.HandleEvent(E);
	}
}
