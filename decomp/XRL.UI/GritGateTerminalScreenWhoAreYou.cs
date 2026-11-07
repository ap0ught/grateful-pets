namespace XRL.UI;

public class GritGateTerminalScreenWhoAreYou : GritGateTerminalScreen
{
	public GritGateTerminalScreenWhoAreYou()
	{
		MainText = "I am Ereshkigal, your liaison with the Thin World. Ask of me.";
		Options.Add("Are you sapient?");
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
			Terminal.CurrentScreen = new GritGateTerminalScreenSapient();
		}
		if (Terminal.Selected == 1)
		{
			Terminal.CurrentScreen = new GritGateTerminalScreenThinWorld();
		}
		if (Terminal.Selected == 2)
		{
			Terminal.CurrentScreen = new GritGateTerminalScreenKnowledge();
		}
		if (Terminal.Selected == 3)
		{
			Terminal.CurrentScreen = null;
		}
	}
}
