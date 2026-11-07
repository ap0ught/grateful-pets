using XRL.Language;

namespace XRL.UI;

public class CyberneticsScreenMainMenu : CyberneticsScreen
{
	private int SelectOption = -1;

	public CyberneticsScreenMainMenu()
	{
		MainText = "Welcome, Aristocrat, to a becoming nook. " + Terminal.Subject.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null, AsPossessed: true, null, Reference: true) + " one step closer to the Grand Unification. Please choose from the following options.";
		Options.Add("Learn About Cybernetics");
		Options.Add("Install Cybernetics");
		Options.Add("Uninstall Cybernetics");
		Options.Add("Upgrade Your License");
		if (!XRL.UI.Options.SifrahHacking)
		{
			return;
		}
		SelectOption = -1;
		HackOption = -1;
		int possibleSubjectCount = Terminal.GetPossibleSubjectCount();
		if (possibleSubjectCount > 1)
		{
			int authorizedSubjectCount = Terminal.GetAuthorizedSubjectCount();
			if (authorizedSubjectCount > 1)
			{
				SelectOption = Options.Count;
				Options.Add("Select Subject");
			}
			if (!Terminal.HackActive && possibleSubjectCount > authorizedSubjectCount)
			{
				HackOption = Options.Count;
				Options.Add(TextFilters.Leet("Attempt Hack to Select Subject").Color("R"));
			}
		}
	}

	public override void Activate()
	{
		switch (Terminal.Selected)
		{
		case 0:
			Terminal.CurrentScreen = new CyberneticsScreenLearn();
			return;
		case 1:
			Terminal.CurrentScreen = new CyberneticsScreenInstall();
			return;
		case 2:
			Terminal.CurrentScreen = new CyberneticsScreenRemove();
			return;
		case 3:
			Terminal.CurrentScreen = new CyberneticsScreenUpgrade();
			return;
		}
		if (Terminal.Selected == SelectOption)
		{
			Terminal.CurrentScreen = new CyberneticsScreenSelectSubject();
		}
		else if (Terminal.Selected == HackOption)
		{
			if (Terminal.HackActive || Terminal.AttemptHack())
			{
				Terminal.CurrentScreen = new CyberneticsScreenSelectSubject();
			}
			else
			{
				Terminal.CurrentScreen = new CyberneticsScreenGoodbye(AllowHack: false);
			}
		}
	}
}
