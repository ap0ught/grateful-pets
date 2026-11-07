using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class DoubleEdge : IPart
{
	public int Dealt;

	public int Received;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AttackerDealingDamageEvent.ID && ID != BeforeApplyDamageEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AttackerDealingDamageEvent E)
	{
		E.Damage.Amount += (int)((float)E.Damage.Amount * ((float)Dealt / 100f));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		E.Damage.Amount += (int)((float)E.Damage.Amount * ((float)Received / 100f));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(AppendEffect);
		return base.HandleEvent(E);
	}

	public void AppendEffect(StringBuilder SB)
	{
		if (Received != 0)
		{
			SB.Append("Take ").Append(Received).Append((Received > 0) ? "% more" : "% less")
				.Append(" damage");
			if (Dealt != 0)
			{
				SB.Append((Math.Sign(Received) != Math.Sign(Dealt)) ? " but " : " and ");
			}
		}
		if (Dealt != 0)
		{
			SB.Append((Received != 0) ? "deal " : "Deal ").Append(Dealt).Append((Dealt > 0) ? "% more" : "% less")
				.Append(" damage");
		}
	}
}
