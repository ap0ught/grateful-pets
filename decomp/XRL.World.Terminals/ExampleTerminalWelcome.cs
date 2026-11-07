using XRL.UI;

namespace XRL.World.Terminals;

public class ExampleTerminalWelcome : GenericTerminalScreen
{
	public ExampleTerminalWelcome()
	{
		mainText = "<Welcome to this example terminal>";
		Options.Add("Goto 10");
		Options.Add("Exit.");
	}

	public override void Back()
	{
		terminal.currentScreen = null;
	}

	public override void Activate()
	{
		if (terminal.nSelected == 0)
		{
			terminal.currentScreen = new ExampleTerminalWelcome();
		}
		if (terminal.nSelected == 1)
		{
			terminal.currentScreen = null;
		}
	}
}
