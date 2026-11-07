using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class AttackerElementalAmplifier : IPart
{
	public string Attribute = "Heat";

	public int Amplification;

	public AttackerElementalAmplifier()
	{
	}

	public AttackerElementalAmplifier(string Attribute, int Amplification)
	{
		this.Attribute = Attribute;
		this.Amplification = Amplification;
	}

	public override bool SameAs(IPart Part)
	{
		if (Part is AttackerElementalAmplifier attackerElementalAmplifier && attackerElementalAmplifier.Attribute == Attribute)
		{
			return attackerElementalAmplifier.Amplification == Amplification;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AttackerDealingDamageEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(AppendAmplification);
		return base.HandleEvent(E);
	}

	public void AppendAmplification(StringBuilder SB)
	{
		SB.AppendSigned(Amplification).Append("% ").Append(Attribute.ToLowerInvariant())
			.Append(" damage dealt");
	}

	public override bool HandleEvent(AttackerDealingDamageEvent E)
	{
		if (E.Damage.HasAttribute(Attribute))
		{
			E.Damage.Amount = (int)((float)E.Damage.Amount * (1f + (float)Amplification * 0.01f));
		}
		return base.HandleEvent(E);
	}
}
