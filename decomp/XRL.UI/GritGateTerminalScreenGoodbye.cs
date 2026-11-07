namespace XRL.UI;

public class GritGateTerminalScreenGoodbye : GritGateTerminalScreen
{
	public GritGateTerminalScreenGoodbye()
	{
		MainText = "Access denied. Continue on your path through the Thick World.";
		Options.Add("...");
	}

	public override void Back()
	{
		Terminal.CurrentScreen = null;
	}

	public override void Activate()
	{
		Terminal.CurrentScreen = null;
	}
}
