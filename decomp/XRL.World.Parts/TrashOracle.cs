using System;
using System.Text;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class TrashOracle : IActivePart
{
	public int Bonus;

	public int Magnitude;

	public TrashOracle()
	{
		WorksOnSelf = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetSkillEffectChanceEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetSkillEffectChanceEvent E)
	{
		if (E.Skill is Customs_TrashDivining && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (Bonus != 0)
			{
				E.Chance += Bonus;
			}
			if (Magnitude != 0)
			{
				if (Magnitude > 0)
				{
					E.Chance *= Magnitude;
				}
				else
				{
					E.Chance /= Magnitude;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(AppendEffect);
		return base.HandleEvent(E);
	}

	public void AppendEffect(StringBuilder SB)
	{
		SB.Append("Chance to reveal secrets via Trash Divining ");
		bool flag = false;
		if (Bonus != 0)
		{
			SB.Append((Bonus > 0) ? "increased " : "decreased ").Append("by ").Append(Bonus)
				.Append('%');
			if (Magnitude != 0)
			{
				flag = Math.Sign(Magnitude) != Math.Sign(Bonus);
				SB.Append(flag ? " but " : " and ");
			}
		}
		if (Magnitude != 0)
		{
			if ((Bonus == 0) | flag)
			{
				SB.Append((Magnitude > 0) ? "increased " : "decreased ");
			}
			SB.Append("by ").Append(Magnitude).Append('x');
		}
	}
}
