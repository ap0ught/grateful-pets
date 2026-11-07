using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class TimeCubed : Effect, ITierInitialized
{
	public int Amount;

	public TimeCubed()
	{
		DisplayName = "time cubed";
		Duration = 1;
	}

	public TimeCubed(int Amount)
		: this()
	{
		this.Amount = Amount;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override int GetEffectType()
	{
		return 67112960;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == PooledEvent<RealityStabilizeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Object.Stat("Energy") < 2000)
		{
			Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check())
		{
			if (Object.Energy != null && Object.Energy.Value > 1000)
			{
				Object.Energy.BaseValue = 1000;
			}
			Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.Energy != null)
		{
			if (Amount == 0)
			{
				Amount = 20000 + 3000 * Stat.Random(1, 10);
			}
			Object.Energy.BaseValue += Amount;
		}
		if (Object.IsPlayer())
		{
			Popup.ShowBlock("{{G|You are filled with the true vision! The {{B|Cubic Form}} is {{M|Infinite}}, {{W|Harmonic}} and transcends the {{R|1 Day rotation}}!}}");
		}
		if (Object.HasEffect<TimeCubed>())
		{
			return false;
		}
		return true;
	}
}
