using System;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Units;

[Serializable]
public class GameObjectSaveModifierUnit : GameObjectUnit
{
	public string Vs;

	public int Value;

	public override void Apply(GameObject Object)
	{
		SaveModifiers saveModifiers = Object.RequirePart<SaveModifiers>();
		saveModifiers.WorksOnSelf = true;
		saveModifiers.WorksOnEquipper = false;
		saveModifiers.AddModifier(Vs, Value);
	}

	public override void Remove(GameObject Object)
	{
		SaveModifiers part = Object.GetPart<SaveModifiers>();
		if (part != null)
		{
			part.AddModifier(Vs, -Value);
			if (part.Tracking.Count == 0)
			{
				Object.RemovePart(part);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		Vs = null;
		Value = 0;
	}

	public override string GetDescription(bool Inscription = false)
	{
		return SavingThrows.GetSaveBonusDescription(Value, Vs);
	}
}
