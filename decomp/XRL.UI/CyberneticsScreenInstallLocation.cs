using System.Collections.Generic;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace XRL.UI;

public class CyberneticsScreenInstallLocation : CyberneticsScreen
{
	private GameObject Implant;

	private static List<BodyPart> Slots = new List<BodyPart>();

	public CyberneticsScreenInstallLocation(GameObject Implant)
	{
		this.Implant = Implant;
	}

	protected override void OnUpdate()
	{
		Slots.Clear();
		ClearOptions();
		MainText = "Please choose a target body part.";
		CyberneticsBaseItem part = Implant.GetPart<CyberneticsBaseItem>();
		Body body = Terminal.Subject.Body;
		List<string> list = part.Slots.CachedCommaExpansion();
		int num = 1;
		foreach (BodyPart part2 in body.GetParts())
		{
			if (!list.Contains(part2.Type) || !part2.CanReceiveCyberneticImplant())
			{
				continue;
			}
			if (part2.Cybernetics != null)
			{
				if (!part2.Cybernetics.HasTagOrProperty("CyberneticsNoRemove"))
				{
					string text = "";
					if (part2.Cybernetics.HasTag("CyberneticsDestroyOnRemoval"))
					{
						text = "{{R|[destroyed on uninstall]}}";
					}
					Slots.Add(part2);
					Options.Add(part2.Name + " [will replace " + part2.Cybernetics.BaseKnownDisplayName + "]" + text);
				}
			}
			else
			{
				Slots.Add(part2);
				Options.Add(part2.Name);
			}
			num++;
		}
		Options.Add("<cancel operation, return to main menu>");
	}

	public override void Back()
	{
		Terminal.CheckSecurity(1, new CyberneticsScreenMainMenu());
	}

	public override void Activate()
	{
		if (Terminal.Selected < Slots.Count)
		{
			Terminal.CurrentScreen = new CyberneticsInstallResult(Implant, Slots, Terminal.Selected);
		}
		else
		{
			Back();
		}
	}
}
