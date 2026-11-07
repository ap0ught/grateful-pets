using System.Collections.Generic;
using XRL.World;
using XRL.World.Anatomy;

namespace XRL.UI;

public class CyberneticsScreenRemove : CyberneticsScreen
{
	private List<GameObject> Cybernetics = new List<GameObject>();

	private List<BodyPart> BodyParts = new List<BodyPart>();

	protected override void OnUpdate()
	{
		ClearOptions();
		Cybernetics.Clear();
		BodyParts.Clear();
		foreach (BodyPart part in Terminal.Subject.Body.GetBody().GetParts())
		{
			if (part.Cybernetics != null && !Cybernetics.Contains(part.Cybernetics))
			{
				string text = "";
				if (part.Cybernetics.HasTagOrProperty("CyberneticsNoRemove"))
				{
					text += " {{R|[cannot be uninstalled]}}";
				}
				if (part.Cybernetics.HasTag("CyberneticsDestroyOnRemoval"))
				{
					text += " {{R|[destroyed on uninstall]}}";
				}
				Options.Add(part.Cybernetics.BaseKnownDisplayName + " [" + GetBodyPartNameForTerminal(part) + "]" + text);
				BodyParts.Add(part);
				Cybernetics.Add(part.Cybernetics);
			}
		}
		MainText = "You are given to whimsy, Aristocrat. Choose an implant to uninstall.";
		if (Options.Count == 0)
		{
			MainText += "\n\n{{R|<no implants installed>}}";
		}
		Options.Add("Return To Main Menu");
	}

	private string GetBodyPartNameForTerminal(BodyPart Part)
	{
		return Part.Name.Replace("Worn on ", "");
	}

	public override void Back()
	{
		Terminal.CurrentScreen = new CyberneticsScreenMainMenu();
	}

	public override void Activate()
	{
		if (Terminal.Selected < Cybernetics.Count)
		{
			if (Cybernetics[Terminal.Selected].HasTagOrProperty("CyberneticsNoRemove"))
			{
				Terminal.CurrentScreen = new CyberneticsScreenSimpleText("Whimsy must yield to necessity, Aristocrat.", new CyberneticsScreenRemove(), 1);
			}
			else if (Cybernetics[Terminal.Selected].HasTag("CyberneticsDestroyOnRemoval"))
			{
				Cybernetics[Terminal.Selected].Unimplant();
				Cybernetics[Terminal.Selected].Destroy();
				CyberneticsRemoveResult cyberneticsRemoveResult = new CyberneticsRemoveResult(Cybernetics[Terminal.Selected]);
				cyberneticsRemoveResult.Destroyed = true;
				Terminal.CurrentScreen = cyberneticsRemoveResult;
			}
			else
			{
				Cybernetics[Terminal.Selected].Unimplant(MoveToInventory: true);
				Terminal.CurrentScreen = new CyberneticsRemoveResult(Cybernetics[Terminal.Selected]);
			}
		}
		else
		{
			Terminal.CheckSecurity(1, new CyberneticsScreenMainMenu());
		}
	}
}
