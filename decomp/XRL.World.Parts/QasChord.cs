using System.Text;

namespace XRL.World.Parts;

public class QasChord : INephalChord
{
	public override string Source => "Qas";

	public virtual int SpeedBonus => 25;

	public override void Initialize()
	{
		StatShifter.SetStatShift("Speed", SpeedBonus);
	}

	public override void Remove()
	{
		StatShifter.RemoveStatShifts();
	}

	public override void AppendRules(StringBuilder Postfix)
	{
		Postfix.Append("\n• ");
		Statistic.AppendStatAdjustDescription(Postfix, "Speed", SpeedBonus);
	}
}
