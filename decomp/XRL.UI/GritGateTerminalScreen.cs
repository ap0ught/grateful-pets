using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class GritGateTerminalScreen : TerminalScreen
{
	public List<Action> optionActions = new List<Action>();

	private ElectricalPowerTransmission generator;

	public int available;

	protected override void ClearOptions()
	{
		base.ClearOptions();
		optionActions.Clear();
	}

	public override void BeforeRender(ScreenBuffer buffer, ref string footerText)
	{
		if (generator == null)
		{
			GameObject gameObject = Terminal.Terminal.CurrentCell.ParentZone.FindObject((GameObject o) => o.Blueprint == "GritGateFusionPowerStation" && o.Physics.CurrentCell.Y >= 10);
			if (gameObject != null)
			{
				generator = gameObject.GetPart<ElectricalPowerTransmission>();
			}
		}
		if (generator == null)
		{
			available = 0;
			footerText = "[{{R|!!! ERROR: POWER SYSTEMS HAVE FAILED !!!}}]";
		}
		else
		{
			int num = 4000;
			int totalDraw = generator.GetTotalDraw();
			available = num - totalDraw;
			if (available < 0)
			{
				footerText = "[{{W|!!! WARNING: INSUFFICIENT POWER !!!}}]";
			}
			else
			{
				footerText = $" Available power: {(float)available * 0.1f} amps ";
			}
			GritGateAmperageImposter.display = " Available power: " + (float)available * 0.1f + " amps ";
		}
		if (buffer != null)
		{
			int num2 = 3;
			buffer.Goto(num2 + 3, 24);
			buffer.Write(footerText);
		}
	}
}
