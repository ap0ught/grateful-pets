using System;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainStability_MoveResistUnit : ProceduralCookingEffectUnit
{
	public string Vs = "Move,Knockdown,Restraint";

	public int Amount = 6;

	public override void Init(GameObject target)
	{
	}

	public override string GetDescription()
	{
		return SavingThrows.GetSaveBonusDescription(Amount, Vs);
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "ModifyDefendingSave");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "ModifyDefendingSave");
	}

	public override void FireEvent(Event E)
	{
		if (!(E.ID == "ModifyDefendingSave"))
		{
			return;
		}
		try
		{
			if (parent != null && parent.Object != null && SavingThrows.Applicable(Vs, E))
			{
				E.SetParameter("Roll", E.GetIntParameter("Roll") + Amount);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("CookingDomainStability_MoveResistUnit::EndTurn", x);
		}
	}
}
