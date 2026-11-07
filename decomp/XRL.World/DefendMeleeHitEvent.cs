namespace XRL.World;

[GameEvent(Cascade = 3, Cache = Cache.Pool)]
public class DefendMeleeHitEvent : PooledEvent<DefendMeleeHitEvent>
{
	public new static readonly int CascadeLevel = 3;

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Weapon;

	public Damage Damage;

	public int Result;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

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
		Damage = null;
		Result = 0;
	}

	public static DefendMeleeHitEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Weapon, Damage Damage, int Result)
	{
		DefendMeleeHitEvent defendMeleeHitEvent = PooledEvent<DefendMeleeHitEvent>.FromPool();
		defendMeleeHitEvent.Attacker = Attacker;
		defendMeleeHitEvent.Defender = Defender;
		defendMeleeHitEvent.Weapon = Weapon;
		defendMeleeHitEvent.Damage = Damage;
		defendMeleeHitEvent.Result = Result;
		return defendMeleeHitEvent;
	}

	public static void Send(GameObject Attacker, GameObject Defender, GameObject Weapon, Damage Damage, int Result)
	{
		if ((!Defender.WantEvent(PooledEvent<DefendMeleeHitEvent>.ID, CascadeLevel) || Defender.HandleEvent(FromPool(Attacker, Defender, Weapon, Damage, Result))) && Defender.HasRegisteredEvent("DefendMeleeHit"))
		{
			Event obj = Event.New("DefendMeleeHit");
			obj.SetParameter("Attacker", Attacker);
			obj.SetParameter("Defender", Defender);
			obj.SetParameter("Weapon", Weapon);
			obj.SetParameter("Damage", Damage);
			obj.SetParameter("Result", Result);
			Defender.FireEvent(obj);
		}
	}
}
