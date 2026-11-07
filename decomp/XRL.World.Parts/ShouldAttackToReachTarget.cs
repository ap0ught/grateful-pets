using System;

namespace XRL.World.Parts;

[Serializable]
public class ShouldAttackToReachTarget : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<ShouldAttackToReachTargetEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ShouldAttackToReachTargetEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.ShouldAttack = true;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
