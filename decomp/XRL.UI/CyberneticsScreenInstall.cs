using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class CyberneticsScreenInstall : CyberneticsScreen
{
	private static List<GameObject> Implants = new List<GameObject>();

	protected override void OnUpdate()
	{
		MainText = "You are becoming, aristocrat. Choose an implant to install.";
		Implants.Clear();
		ClearOptions();
		if (Terminal.Implants.Count == 0)
		{
			MainText += "\n\n{{R|<No implants available>}}";
		}
		foreach (GameObject implant in Terminal.Implants)
		{
			if (!Implants.Contains(implant))
			{
				Implants.Add(implant);
				string baseDisplayName = implant.BaseDisplayName;
				if (implant.HasTag("CyberneticsOneOnly") && Terminal.Subject.HasInstalledCybernetics(implant.Blueprint))
				{
					baseDisplayName += " {{R|[already installed]}}";
				}
				else
				{
					int cost = implant.GetPart<CyberneticsBaseItem>().Cost;
					baseDisplayName = baseDisplayName + " " + ("[" + cost.Things("license point") + "]").Color((cost > LicensesRemaining) ? "R" : "C");
				}
				Options.Add(baseDisplayName);
			}
		}
		Options.Add("Return to main menu");
	}

	public override void Back()
	{
		Terminal.CurrentScreen = new CyberneticsScreenMainMenu();
	}

	public override void Activate()
	{
		if (Terminal.Selected < Implants.Count)
		{
			GameObject gameObject = Implants[Terminal.Selected];
			if (gameObject.HasTag("CyberneticsOneOnly") && Terminal.Subject.HasInstalledCybernetics(gameObject.Blueprint))
			{
				Terminal.CurrentScreen = new CyberneticsScreenSimpleText("Cybernetics already present:\n  -" + (GameObjectFactory.Factory.GetBlueprintIfExists(gameObject.Blueprint)?.CachedDisplayNameStripped ?? gameObject.BaseDisplayName) + "\n\nPlease uninstall existing implant.", new CyberneticsScreenInstall(), 1);
			}
			else if (gameObject.GetPart<CyberneticsBaseItem>().Cost > LicensesRemaining)
			{
				Terminal.CurrentScreen = new CyberneticsScreenSimpleText("Insufficent license points to install:\n  -" + gameObject.BaseDisplayName + "\n\nPlease uninstall an implant or upgrade your license.", new CyberneticsScreenInstall(), 1);
			}
			else if (gameObject.IsBroken() || gameObject.IsRusted() || gameObject.IsEMPed() || gameObject.IsTemporary)
			{
				Terminal.CurrentScreen = new CyberneticsScreenSimpleText("Error: Condition inadequate for installation\n  -" + gameObject.BaseDisplayName + "\n\nPlease supply a replacement.", new CyberneticsScreenInstall(), 1);
			}
			else
			{
				Terminal.CurrentScreen = new CyberneticsScreenInstallLocation(gameObject);
			}
		}
		else
		{
			Terminal.CheckSecurity(1, new CyberneticsScreenMainMenu());
		}
	}
}
