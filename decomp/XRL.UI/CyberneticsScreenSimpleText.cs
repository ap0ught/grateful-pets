namespace XRL.UI;

public class CyberneticsScreenSimpleText : CyberneticsScreen
{
	private TerminalScreen BackTo;

	private int SecurityChance;

	public CyberneticsScreenSimpleText(string Text, TerminalScreen BackTo, int SecurityChance = 0)
	{
		this.BackTo = BackTo;
		this.SecurityChance = SecurityChance;
		MainText = Text;
		Options.Add("<back>");
	}

	public override void Back()
	{
		Terminal.CheckSecurity(SecurityChance, BackTo);
	}

	public override void Activate()
	{
		Back();
	}
}
