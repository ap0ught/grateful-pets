namespace XRL.UI;

public class GritGateTerminalScreenThinWorld : GritGateTerminalScreen
{
	public GritGateTerminalScreenThinWorld()
	{
		MainText = "The Thin World is where I traverse, as the Thick World is where you do. I've no domain in your world, and but for me, you've none in mine.";
		Options.Add("Who are you?");
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
			Terminal.CurrentScreen = new GritGateTerminalScreenWhoAreYou();
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
