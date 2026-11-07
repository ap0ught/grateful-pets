namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetCriticalThresholdEvent : PooledEvent<GetCriticalThresholdEvent>
{
	public const int BASE_THRESHOLD = 20;

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Weapon;

	public GameObject Projectile;

	public string Skill;

	public int Threshold = 20;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Attacker = null;
		Defender = null;
		Weapon = null;
		Projectile = null;
		Skill = null;
		Threshold = 20;
	}

	public static GetCriticalThresholdEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Weapon, GameObject Projectile, string Skill, int Threshold)
	{
		GetCriticalThresholdEvent getCriticalThresholdEvent = PooledEvent<GetCriticalThresholdEvent>.FromPool();
		getCriticalThresholdEvent.Attacker = Attacker;
		getCriticalThresholdEvent.Defender = Defender;
		getCriticalThresholdEvent.Weapon = Weapon;
		getCriticalThresholdEvent.Projectile = Projectile;
		getCriticalThresholdEvent.Skill = Skill;
		getCriticalThresholdEvent.Threshold = Threshold;
		return getCriticalThresholdEvent;
	}

	public static int GetFor(GameObject Attacker, GameObject Defender, GameObject Weapon, GameObject Projectile = null, string Skill = null, int Threshold = 20)
	{
		bool flag = Attacker?.HasRegisteredEvent("GetCriticalThreshold") ?? false;
		bool flag2 = Defender?.HasRegisteredEvent("GetCriticalThreshold") ?? false;
		bool flag3 = Weapon?.HasRegisteredEvent("GetCriticalThreshold") ?? false;
		bool flag4 = Projectile?.HasRegisteredEvent("GetCriticalThreshold") ?? false;
		if (flag | flag2 | flag3 | flag4)
		{
			Event obj = Event.New("GetCriticalThreshold");
			obj.SetParameter("Attacker", Attacker);
			obj.SetParameter("Defender", Defender);
			obj.SetParameter("Weapon", Weapon);
			obj.SetParameter("Projectile", Projectile);
			obj.SetParameter("Skill", Skill);
			obj.SetParameter("Threshold", Threshold);
			if (flag)
			{
				Attacker.FireEvent(obj);
			}
			if (flag2)
			{
				Defender.FireEvent(obj);
			}
			if (flag3)
			{
				Weapon.FireEvent(obj);
			}
			if (flag4)
			{
				Projectile.FireEvent(obj);
			}
			Threshold = obj.GetIntParameter("Threshold");
		}
		bool flag5 = Attacker?.WantEvent(PooledEvent<GetCriticalThresholdEvent>.ID, MinEvent.CascadeLevel) ?? false;
		bool flag6 = Defender?.WantEvent(PooledEvent<GetCriticalThresholdEvent>.ID, MinEvent.CascadeLevel) ?? false;
		bool flag7 = Weapon?.WantEvent(PooledEvent<GetCriticalThresholdEvent>.ID, MinEvent.CascadeLevel) ?? false;
		bool flag8 = Projectile?.WantEvent(PooledEvent<GetCriticalThresholdEvent>.ID, MinEvent.CascadeLevel) ?? false;
		if (flag5 | flag6 | flag7 | flag8)
		{
			GetCriticalThresholdEvent getCriticalThresholdEvent = FromPool(Attacker, Defender, Weapon, Projectile, Skill, Threshold);
			if (flag5)
			{
				Attacker.HandleEvent(getCriticalThresholdEvent);
			}
			if (flag6)
			{
				Defender.HandleEvent(getCriticalThresholdEvent);
			}
			if (flag7)
			{
				Weapon.HandleEvent(getCriticalThresholdEvent);
			}
			if (flag8)
			{
				Projectile.HandleEvent(getCriticalThresholdEvent);
			}
			Threshold = getCriticalThresholdEvent.Threshold;
		}
		return Threshold;
	}
}
