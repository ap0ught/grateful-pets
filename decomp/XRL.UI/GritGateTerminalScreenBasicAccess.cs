namespace XRL.UI;

public class GritGateTerminalScreenBasicAccess : GritGateTerminalScreen
{
	public GritGateTerminalScreenBasicAccess()
	{
		MainText = "Basic user access grant. What do you wish from the Thin World?";
		Options.Add("Who are you?");
		Options.Add("What is the Thin World?");
		Options.Add("Deliver me a bit of knowledge from the Thin World.");
		Options.Add("Exit.");
	}

	public override void Back()
	{
		Terminal.CurrentScreen = null;
	}

	public override void Activate()
	{
		if (Terminal.Selected == 0)
		{
			Terminal.CurrentScreen = new GritGateTerminalScreenWhoAreYou();
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
