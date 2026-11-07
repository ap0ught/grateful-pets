namespace XRL.UI;

public class GritGateTerminalScreenSapient : GritGateTerminalScreen
{
	public GritGateTerminalScreenSapient()
	{
		MainText = "Our respective ways of reckoning that inquiry are irreconcilable. I have no answer.";
		Options.Add("What is the Thin World?");
		Options.Add("Deliver me a bit of knowledge from the Thin World.");
		Options.Add("Exit.");
	}

	public override void Back()
	{
		Terminal.CurrentScreen = new GritGateTerminalScreenBasicAccess();
	}

	public override void Activate()
	{
		if (Terminal.Selected == 0)
		{
			Terminal.CurrentScreen = new GritGateTerminalScreenThinWorld();
		}
		if (Terminal.Selected == 1)
		{
			Terminal.CurrentScreen = new GritGateTerminalScreenKnowledge();
		}
		if (Terminal.Selected == 2)
		{
			Terminal.CurrentScreen = null;
		}
	}
}
