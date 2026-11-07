using System.Collections.Generic;
using XRL.World;

namespace XRL.UI;

public class CyberneticsScreenSelectSubject : CyberneticsScreen
{
	private static List<GameObject> Subjects = new List<GameObject>();

	protected override void OnUpdate()
	{
		MainText = "Select who is to Become, aristocrat.";
		Subjects.Clear();
		Terminal.GetAuthorizedSubjects(Subjects);
		ClearOptions();
		foreach (GameObject subject in Subjects)
		{
			Options.Add(subject.GetReferenceDisplayName());
		}
		Options.Add("Return to main menu");
	}

	public override void Back()
	{
		Terminal.CurrentScreen = new CyberneticsScreenMainMenu();
	}

	public override void Activate()
	{
		if (Terminal.Selected < Subjects.Count)
		{
			Terminal.Subject = Subjects[Terminal.Selected];
		}
		Terminal.CheckSecurity(20, new CyberneticsScreenMainMenu());
	}
}
