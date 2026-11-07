using XRL.Rules;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetThrownWeaponPerformanceEvent : PooledEvent<GetThrownWeaponPerformanceEvent>
{
	public GameObject Object;

	public GameObject Attacker;

	public GameObject Defender;

	public string BaseDamage;

	public int DamageDieModifier;

	public int DamageModifier;

	public int Penetration;

	public int PenetrationBonus;

	public int PenetrationModifier;

	public bool Vorpal;

	public bool Prospective;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Attacker = null;
		Defender = null;
		BaseDamage = null;
		DamageDieModifier = 0;
		DamageModifier = 0;
		Penetration = 0;
		PenetrationBonus = 0;
		PenetrationModifier = 0;
		Vorpal = false;
		Prospective = false;
	}

	public static void GetFor(GameObject Object, ref string Damage, ref int Penetration, ref int PenetrationBonus, ref int PenetrationModifier, ref bool Vorpal, bool Prospective = false, GameObject Attacker = null, GameObject Defender = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetThrownWeaponPerformance"))
		{
			Event obj = Event.New("GetThrownWeaponPerformance");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Attacker", Attacker);
			obj.SetParameter("Defender", Defender);
			obj.SetParameter("Damage", Damage);
			obj.SetParameter("Penetration", Penetration);
			obj.SetParameter("PenetrationBonus", PenetrationBonus);
			obj.SetParameter("PenetrationModifier", PenetrationModifier);
			obj.SetFlag("Vorpal", Vorpal);
			obj.SetFlag("Prospective", Prospective);
			flag = Object.FireEvent(obj);
			Damage = obj.GetStringParameter("Damage");
			Penetration = obj.GetIntParameter("Penetration");
			PenetrationBonus = obj.GetIntParameter("PenetrationBonus");
			PenetrationModifier = obj.GetIntParameter("PenetrationModifier");
			Vorpal = obj.HasFlag("Vorpal");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetThrownWeaponPerformanceEvent>.ID, MinEvent.CascadeLevel))
		{
			GetThrownWeaponPerformanceEvent getThrownWeaponPerformanceEvent = PooledEvent<GetThrownWeaponPerformanceEvent>.FromPool();
			getThrownWeaponPerformanceEvent.Object = Object;
			getThrownWeaponPerformanceEvent.Attacker = Attacker;
			getThrownWeaponPerformanceEvent.Defender = Defender;
			getThrownWeaponPerformanceEvent.BaseDamage = Damage;
			getThrownWeaponPerformanceEvent.Penetration = Penetration;
			getThrownWeaponPerformanceEvent.PenetrationBonus = PenetrationBonus;
			getThrownWeaponPerformanceEvent.PenetrationModifier = PenetrationModifier;
			getThrownWeaponPerformanceEvent.Vorpal = Vorpal;
			getThrownWeaponPerformanceEvent.Prospective = Prospective;
			flag = Object.HandleEvent(getThrownWeaponPerformanceEvent);
			Damage = getThrownWeaponPerformanceEvent.BaseDamage;
			Penetration = getThrownWeaponPerformanceEvent.Penetration;
			PenetrationBonus = getThrownWeaponPerformanceEvent.PenetrationBonus;
			PenetrationModifier = getThrownWeaponPerformanceEvent.PenetrationModifier;
			Vorpal = getThrownWeaponPerformanceEvent.Vorpal;
			if (getThrownWeaponPerformanceEvent.DamageDieModifier != 0)
			{
				Damage = DieRoll.AdjustDieSize(Damage, getThrownWeaponPerformanceEvent.DamageDieModifier);
			}
			if (getThrownWeaponPerformanceEvent.DamageModifier != 0)
			{
				Damage = DieRoll.AdjustResult(Damage, getThrownWeaponPerformanceEvent.DamageModifier);
			}
		}
	}
}
