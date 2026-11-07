using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Pistol_WeakSpotter : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetCriticalThresholdEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCriticalThresholdEvent E)
	{
		if (E.Attacker == ParentObject && E.Skill == "Pistol")
		{
			E.Threshold--;
		}
		return base.HandleEvent(E);
	}
}
