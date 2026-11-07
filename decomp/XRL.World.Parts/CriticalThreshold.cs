using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class CriticalThreshold : IPart
{
	public int Value;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCriticalThresholdEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(AppendEffect);
		return base.HandleEvent(E);
	}

	public void AppendEffect(StringBuilder SB)
	{
		SB.AppendSigned(Value * 5).Append("% chance to score critical hits");
	}

	public override bool HandleEvent(GetCriticalThresholdEvent E)
	{
		if (E.Weapon == ParentObject || E.Projectile == ParentObject)
		{
			E.Threshold -= Value;
		}
		return base.HandleEvent(E);
	}
}
