using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class SaveModifiers : IActivePart
{
	public Dictionary<string, int> Tracking = new Dictionary<string, int>();

	public string Modifiers
	{
		set
		{
			string[] array = value.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(':');
				if (array2.Length == 2)
				{
					int result;
					if (array2[1] == "*")
					{
						Tracking[array2[0]] = SavingThrows.IMMUNITY;
					}
					else if (int.TryParse(array2[1], out result))
					{
						Tracking[array2[0]] = result;
					}
				}
			}
		}
	}

	public SaveModifiers()
	{
		WorksOnEquipper = true;
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		SaveModifiers obj = base.DeepCopy(Parent) as SaveModifiers;
		obj.Tracking = new Dictionary<string, int>(Tracking);
		return obj;
	}

	public void AddModifier(string Vs, int Level)
	{
		if (Tracking.TryGetValue(Vs, out var value))
		{
			if (value + Level == 0)
			{
				Tracking.Remove(Vs);
			}
			else
			{
				Tracking[Vs] = value + Level;
			}
		}
		else
		{
			Tracking.Add(Vs, Level);
		}
	}

	public override bool SameAs(IPart p)
	{
		if (!(p as SaveModifiers).Tracking.SameAs(Tracking))
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
		if (!OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnCarrier)
		{
			List<string> list = new List<string>(Tracking.Keys);
			list.Sort();
			foreach (string Vs in list)
			{
				E.Postfix.AppendRules((StringBuilder sb) =>
				{
					SavingThrows.AppendSaveBonusDescription(sb, Tracking[Vs], Vs, HighlightNumber: false, Highlight: false, LeadingNewline: false);
				}, GetEventSensitiveAddStatusSummary(E));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (IsObjectActivePartSubject(E.Defender) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			foreach (KeyValuePair<string, int> item in Tracking)
			{
				if (SavingThrows.Applicable(item.Key, E))
				{
					E.Roll += item.Value;
				}
			}
		}
		return base.HandleEvent(E);
	}
}
