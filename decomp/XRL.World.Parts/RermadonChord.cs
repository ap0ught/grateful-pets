using System.Text;

namespace XRL.World.Parts;

public class RermadonChord : INephalChord
{
	public override string Source => "Rermadon";

	public virtual int ResistBonus => 80;

	public override void Initialize()
	{
		StatShifter.SetStatShift("HeatResistance", ResistBonus);
		StatShifter.SetStatShift("ColdResistance", ResistBonus);
		StatShifter.SetStatShift("ElectricResistance", ResistBonus);
		StatShifter.SetStatShift("AcidResistance", ResistBonus);
	}

	public override void Remove()
	{
		StatShifter.RemoveStatShifts();
	}

	public override void AppendRules(StringBuilder Postfix)
	{
		Postfix.Append("\n• +").Append(ResistBonus).Append(" all resists");
	}
}
