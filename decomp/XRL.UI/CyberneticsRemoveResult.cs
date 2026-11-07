using XRL.World;

namespace XRL.UI;

public class CyberneticsRemoveResult : CyberneticsScreen
{
	public bool Destroyed;

	public CyberneticsRemoveResult(GameObject SelectedImplant)
	{
		MainText += "..................................................\n";
		MainText += "................................\n";
		MainText = MainText + "Uninstalling " + SelectedImplant.BaseKnownDisplayName + "...........\n";
		MainText += "Interfacing with nervous system...................\n";
		MainText += "..................................................\n";
		MainText += "..................................................\n";
		MainText += "..................................................\n";
		MainText += "..................................................\n";
		MainText += "..................................................\n";
		MainText += ".....Complete!\n\n";
		MainText += "Congratulations! ";
		MainText += "Your cybernetic implant was successfully uninstalled.";
		Options.Add("Return To Main Menu");
	}

	public override void Back()
	{
		Terminal.CheckSecurity(25, new CyberneticsScreenMainMenu());
	}

	public override void Activate()
	{
		Back();
	}

	public override void TextComplete()
	{
		SoundManager.PlayUISound("sfx_cybernetic_implant_remove");
		if (Destroyed)
		{
			SoundManager.PlayUISound("sfx_cybernetic_implant_destroyed");
		}
	}
}
