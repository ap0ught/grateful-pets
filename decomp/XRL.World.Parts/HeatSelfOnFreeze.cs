using System;

namespace XRL.World.Parts;

[Serializable]
public class HeatSelfOnFreeze : IPart
{
	public int HeatCooldown;

	public string HeatFrequency = "1";

	public string HeatAmount = "60";

	public bool AmountIsPercentage = true;

	public string HeatVerb = "vibrate";

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (HeatCooldown > 0)
			{
				HeatCooldown--;
			}
			if (ParentObject.IsFrozen() && HeatCooldown <= 0)
			{
				HeatCooldown = HeatFrequency.RollCached();
				if (ParentObject.IsVisible())
				{
					DidXToY(HeatVerb, "to warm", ParentObject, null, "!", null, null, ParentObject);
				}
				int amount = ((!AmountIsPercentage) ? HeatAmount.RollCached() : (HeatAmount.RollCached() * (ParentObject.Physics.BrittleTemperature - ParentObject.Physics.Temperature) / 100));
				ParentObject.TemperatureChange(amount);
			}
		}
		return base.FireEvent(E);
	}
}
