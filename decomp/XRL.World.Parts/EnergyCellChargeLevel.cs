using System;
using System.Text;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class EnergyCellChargeLevel : IPart
{
	public string Full;

	public string Fresh;

	public string Used;

	public string Low;

	public string VeryLow;

	public string Drained;

	public int ChargeMinimum;

	public bool Applied;

	[NonSerialized]
	private EnergyCellSocket Socket;

	public override bool WantTurnTick()
	{
		return true;
	}

	public void ApplyEffect(int Level)
	{
		string text = Level switch
		{
			0 => Drained, 
			1 => VeryLow, 
			2 => Low, 
			3 => Used, 
			4 => Fresh, 
			5 => Full, 
			_ => null, 
		};
		if (text.IsNullOrEmpty())
		{
			if (Applied)
			{
				ParentObject.RemoveEffect(typeof(EnergyCellChargeLevelEffect));
				Applied = false;
			}
			return;
		}
		EnergyCellChargeLevelEffect energyCellChargeLevelEffect = (Applied ? ParentObject.GetEffect<EnergyCellChargeLevelEffect>() : new EnergyCellChargeLevelEffect());
		if (energyCellChargeLevelEffect.Level != Level)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder("{{").Append(EnergyStorage.GetChargeLevelColor(Level)).Append('|');
			energyCellChargeLevelEffect.Level = Level;
			text.AsDelimitedSpans(':', out var First, out var Second);
			stringBuilder.Append(First).Append("}}");
			energyCellChargeLevelEffect.DisplayName = Event.FinalizeString(stringBuilder);
			energyCellChargeLevelEffect.Details = ((Second.Length == 0) ? "" : new string(Second));
			if (!Applied)
			{
				ParentObject.ApplyEffect(energyCellChargeLevelEffect);
				Applied = true;
			}
		}
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (Socket != null || ParentObject.TryGetPart<EnergyCellSocket>(out Socket))
		{
			int level = -1;
			if (GameObject.Validate(Socket.Cell) && Socket.Cell.TryGetPartDescendedFrom<EnergyCell>(out var Part) && Part.Charge >= ChargeMinimum)
			{
				level = Part.GetChargeLevel();
			}
			ApplyEffect(level);
		}
		else
		{
			ParentObject.RemovePart(this);
			ParentObject.RemoveEffect(typeof(EnergyCellChargeLevelEffect));
		}
	}
}
