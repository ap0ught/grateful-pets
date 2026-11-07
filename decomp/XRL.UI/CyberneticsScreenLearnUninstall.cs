namespace XRL.UI;

public class CyberneticsScreenLearnUninstall : CyberneticsScreen
{
	protected override void OnUpdate()
	{
		MainText = "Aristocrat, your question was most finely uttered.\n\nYou may freely install and uninstall implants at a nook, though some implants are destroyed when uninstalled or can't be uninstalled at all.";
		ClearOptions();
		Options.Add("<back>");
		Options.Add("How many implants can I install?");
		Options.Add("Return To Main Menu");
	}

	public override void Back()
	{
		Terminal.CheckSecurity(1, new CyberneticsScreenMainMenu());
	}

	public override void Activate()
	{
		switch (Terminal.Selected)
		{
		case 0:
			Terminal.CheckSecurity(1, new CyberneticsScreenLearn());
			break;
		case 1:
			Terminal.CheckSecurity(1, new CyberneticsScreenLearnHowMany());
			break;
		case 2:
			Back();
			break;
		}
	}
}
