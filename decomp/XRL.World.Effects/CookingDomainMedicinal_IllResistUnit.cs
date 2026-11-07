using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainMedicinal_IllResistUnit : ProceduralCookingEffectUnit
{
	public override void Init(GameObject target)
	{
	}

	public override string GetDescription()
	{
		return "@thisCreature only get@s Ill for 1/10th the usual length of time.";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "EndTurn");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "EndTurn");
	}

	public override void FireEvent(Event E)
	{
		if (!(E.ID == "EndTurn"))
		{
			return;
		}
		try
		{
			if (parent != null && parent.Object != null && parent.Object.TryGetEffect<Ill>(out var Effect))
			{
				Effect.Duration -= 9;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("CookingDomainMedicinal_IllResistUnit::EndTurn", x);
		}
	}
}
