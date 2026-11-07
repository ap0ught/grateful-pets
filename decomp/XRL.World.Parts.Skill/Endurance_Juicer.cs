using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Endurance_Juicer : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<GetTonicCapacityEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTonicCapacityEvent E)
	{
		E.Capacity++;
		return base.HandleEvent(E);
	}
}
