using System;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: temperature changes are increased by a percentage
/// equal to ((power load - 100) / 10), i.e. 30% for the standard overload
/// power load of 400.
/// </remarks>
[Serializable]
public class TemperatureOnHit : IPart
{
	public string Amount = "0";

	public bool Max;

	public int MaxTemp = 400;

	public bool OnWielderHit;

	public bool RequiresLit;

	public TemperatureOnHit()
	{
	}

	public TemperatureOnHit(string Amount, int Max)
		: this()
	{
		this.Amount = Amount;
		MaxTemp = Max;
	}

	public override bool SameAs(IPart p)
	{
		TemperatureOnHit temperatureOnHit = p as TemperatureOnHit;
		if (temperatureOnHit.Amount != Amount)
		{
			return false;
		}
		if (temperatureOnHit.Max != Max)
		{
			return false;
		}
		if (temperatureOnHit.MaxTemp != MaxTemp)
		{
			return false;
		}
		if (temperatureOnHit.OnWielderHit != OnWielderHit)
		{
			return false;
		}
		if (temperatureOnHit.RequiresLit != RequiresLit)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeMeleeAttackEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeMeleeAttackEvent E)
	{
		if (E.Weapon == ParentObject && CheckRequirements())
		{
			int num = Amount.RollMaxCached();
			if (num > 0)
			{
				PlayWorldSound("Sounds/Enhancements/sfx_enhancement_fire_attack", 0.5f, 0f, Combat: true);
			}
			else if (num < 0)
			{
				PlayWorldSound("Sounds/Enhancements/sfx_enhancement_cold", 0.5f, 0f, Combat: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			if (Amount.RollMax() < 0)
			{
				E.Add("ice", 2);
			}
			else if (Amount.RollMin() > 0)
			{
				E.Add("salt", 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileHit");
		if (!OnWielderHit)
		{
			Registrar.Register("WeaponDealDamage");
		}
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	private bool CheckRequirements()
	{
		if (RequiresLit)
		{
			LightSource part = ParentObject.GetPart<LightSource>();
			if (part == null)
			{
				return false;
			}
			if (!part.Lit)
			{
				return false;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "WeaponHit" || E.ID == "WeaponDealDamage" || E.ID == "ProjectileHit") && CheckRequirements())
		{
			E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null && (!Max || ((Amount.RollMaxCached() >= 0) ? (gameObjectParameter.Physics.Temperature < MaxTemp) : (gameObjectParameter.Physics.Temperature > MaxTemp))))
			{
				int num = Amount.RollCached();
				int num2 = MyPowerLoadBonus(int.MinValue, 100, 10);
				if (num2 != 0)
				{
					num = num * (100 + num2) / 100;
				}
				gameObjectParameter.TemperatureChange(num, E.GetGameObjectParameter("Attacker"), Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, ParentObject.GetPhase());
			}
		}
		return base.FireEvent(E);
	}
}
