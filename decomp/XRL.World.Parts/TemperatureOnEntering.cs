using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: temperature changes are increased by a percentage
/// equal to ((power load - 100) / 10), i.e. 30% for the standard overload
/// power load of 400.
/// </remarks>
[Serializable]
public class TemperatureOnEntering : IPart
{
	public string Amount = "0";

	public bool Max;

	public int MaxTemp = 400;

	public bool OnWielderHit;

	public override bool SameAs(IPart Part)
	{
		TemperatureOnEntering temperatureOnEntering = Part as TemperatureOnEntering;
		if (temperatureOnEntering.Amount != Amount)
		{
			return false;
		}
		if (temperatureOnEntering.Max != Max)
		{
			return false;
		}
		if (temperatureOnEntering.MaxTemp != MaxTemp)
		{
			return false;
		}
		if (temperatureOnEntering.OnWielderHit != OnWielderHit)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileEntering");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileEntering")
		{
			Cell obj = E.GetParameter("Cell") as Cell;
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			int phase = ParentObject.GetPhase();
			int num = MyPowerLoadBonus(int.MinValue, 100, 10);
			foreach (GameObject item in obj.GetObjectsWithPart("Physics"))
			{
				if (!Max || (Stat.RollMax(Amount) > 0 && item.Physics.Temperature < MaxTemp) || (Stat.RollMax(Amount) < 0 && item.Physics.Temperature > MaxTemp))
				{
					int num2 = Amount.RollCached();
					if (num != 0)
					{
						num2 = num2 * (100 + num) / 100;
					}
					item.TemperatureChange(num2, gameObjectParameter, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, phase);
				}
			}
		}
		return base.FireEvent(E);
	}
}
