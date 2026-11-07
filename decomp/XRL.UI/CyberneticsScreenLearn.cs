namespace XRL.UI;

public class CyberneticsScreenLearn : CyberneticsScreen
{
	public CyberneticsScreenLearn()
	{
		MainText = "Your curiosity is admirable, aristocrat.\n\nCybernetics are bionic augmentations implanted in your body to assist in your self-actualization. You can have implants installed at becoming nooks such as this one. Either load them in the rack or carry them on your person.";
		Options.Add("How many implants can I install?");
		Options.Add("Can I uninstall implants?");
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
			Terminal.CheckSecurity(1, new CyberneticsScreenLearnHowMany());
			break;
		case 1:
			Terminal.CheckSecurity(1, new CyberneticsScreenLearnUninstall());
			break;
		case 2:
			Back();
			break;
		}
	}
}
