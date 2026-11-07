using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class TenfoldPath_Bin : BaseInitiatorySkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetTinkeringBonusEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if (E.Type == "Examine" || E.Type == "ReverseEngineerTurns" || E.Type == "Hacking")
		{
			E.Bonus++;
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject Object)
	{
		Object.ModIntProperty(Scanning.GetScanPropertyName(Scanning.Scan.Bio), 1);
		Object.ModIntProperty(Scanning.GetScanPropertyName(Scanning.Scan.Tech), 1);
		Object.ModIntProperty(Scanning.GetScanPropertyName(Scanning.Scan.Structure), 1);
		return base.AddSkill(Object);
	}

	public override bool RemoveSkill(GameObject Object)
	{
		Object.ModIntProperty(Scanning.GetScanPropertyName(Scanning.Scan.Bio), -1, RemoveIfZero: true);
		Object.ModIntProperty(Scanning.GetScanPropertyName(Scanning.Scan.Tech), -1, RemoveIfZero: true);
		Object.ModIntProperty(Scanning.GetScanPropertyName(Scanning.Scan.Structure), -1, RemoveIfZero: true);
		return base.RemoveSkill(Object);
	}
}
