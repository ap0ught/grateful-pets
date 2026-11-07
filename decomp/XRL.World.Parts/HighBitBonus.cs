using System;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class HighBitBonus : IActivePart
{
	public int Chance;

	public int Amount;

	public HighBitBonus()
	{
		WorksOnHolder = true;
		WorksOnCarrier = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID && ID != DroppedEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Chance > 0 && Amount > 0)
		{
			E.Postfix.Compound("{{rules|", '\n').Append(Chance).Append("% chance when disassembling an artifact that you receive ")
				.Append(Amount)
				.Append(" extra bit of the highest tier");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public void EvaluateAsSubject(GameObject Object)
	{
		if (IsObjectActivePartSubject(Object))
		{
			Object.RegisterPartEvent(this, "ModifyBitsReceived");
		}
		else
		{
			Object.UnregisterPartEvent(this, "ModifyBitsReceived");
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ModifyBitsReceived" && Chance.in100())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Item");
			if (gameObjectParameter == null || gameObjectParameter == ParentObject)
			{
				return true;
			}
			if (gameObjectParameter.HasIntProperty("TinkeredItem"))
			{
				return true;
			}
			string stringParameter = E.GetStringParameter("Bits");
			if (string.IsNullOrEmpty(stringParameter))
			{
				return true;
			}
			if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return true;
			}
			char c = '\0';
			int num = -1;
			string text = stringParameter;
			foreach (char c2 in text)
			{
				if (BitType.BitMap.TryGetValue(c2, out var value) && value.Level > num)
				{
					c = c2;
					num = value.Level;
				}
			}
			if (num >= 0)
			{
				E.SetParameter("Bits", stringParameter + new string(c, Amount));
				IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_artifact_examination_success_total");
			}
		}
		return base.FireEvent(E);
	}
}
