using System.Collections.Generic;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace XRL.UI;

public class CyberneticsInstallResult : CyberneticsScreen
{
	public GameObject implant;

	public List<BodyPart> Slots;

	public int Slot;

	public int licensePointsInstalled;

	public bool installed;

	public CyberneticsInstallResult(GameObject SelectedImplant, List<BodyPart> Slots, int Slot)
	{
		this.Slot = Slot;
		implant = SelectedImplant;
		this.Slots = Slots;
		MainText += "..................................................\n";
		MainText += "................................\n";
		MainText = MainText + "Installing " + SelectedImplant.BaseDisplayName + ".............\n";
		MainText += "Interfacing with nervous system...................\n";
		MainText += "..................................................\n";
		MainText += "..................................................\n";
		MainText += "..................................................\n";
		MainText += "..................................................\n";
		MainText += "..................................................\n";
		MainText += ".....Complete!\n\n";
		MainText += "Congratulations! Your cybernetic implant was successfully installed.\n";
		MainText += "You are becoming.";
		Options.Add("Return To Main Menu");
	}

	public override void TextComplete()
	{
		if (installed)
		{
			return;
		}
		installed = true;
		GameObject Object = Slots[Slot].Cybernetics;
		if (implant.HasTag("CyberneticsUsesEqSlot") && Slots[Slot].Equipped != null && !Slots[Slot].TryUnequip())
		{
			Terminal.CurrentScreen = new CyberneticsScreenSimpleText("!Error: cannot unequip that limb", this);
			return;
		}
		if (GameObject.Validate(ref Object))
		{
			if (Object.HasTag("CyberneticsDestroyOnRemoval"))
			{
				Object.Unimplant();
				Object.Destroy();
			}
			else
			{
				Object.Unimplant(MoveToInventory: true);
			}
		}
		implant.RemoveFromContext();
		implant.MakeUnderstood();
		Slots[Slot].Implant(implant);
		licensePointsInstalled = (implant?.GetPart<CyberneticsBaseItem>()?.Cost).GetValueOrDefault();
		Body parentBody = Slots[Slot].ParentBody;
		if (!parentBody.ParentObject.IsPlayer())
		{
			return;
		}
		SoundManager.PlayUISound("sfx_cybernetic_implant_install");
		Achievement.INSTALL_IMPLANT.Unlock();
		bool flag = true;
		foreach (string item in new List<string> { "Head", "Face", "Body", "Hands", "Feet", "Arm", "Back" })
		{
			List<BodyPart> part = parentBody.GetPart(item);
			if (part.IsNullOrEmpty())
			{
				flag = false;
				break;
			}
			foreach (BodyPart item2 in part)
			{
				if (item2.Cybernetics == null && item2.CanReceiveCyberneticImplant())
				{
					flag = false;
					goto end_IL_0237;
				}
			}
			continue;
			end_IL_0237:
			break;
		}
		if (flag)
		{
			Achievement.INSTALL_IMPLANT_EVERY_SLOT.Unlock();
		}
	}

	public override void Back()
	{
		Terminal.CheckSecurity(25, new CyberneticsScreenMainMenu(), licensePointsInstalled);
	}

	public override void Activate()
	{
		Back();
	}
}
