using XRL.Language;

namespace XRL.UI;

public class CyberneticsScreenGoodbye : CyberneticsScreen
{
	public CyberneticsScreenGoodbye(bool AllowHack = true)
	{
		MainText = "You are no aristocrat. Goodbye.";
		Options.Add("...");
		if (AllowHack && XRL.UI.Options.SifrahHacking)
		{
			HackOption = Options.Count;
			Options.Add(TextFilters.Leet("attempt hack"));
		}
	}

	public override void Activate()
	{
		Terminal.CurrentScreen = null;
		if (Terminal.Selected == HackOption && Terminal.AttemptHack())
		{
			Terminal.CurrentScreen = new CyberneticsScreenMainMenu();
		}
	}
}
