using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 17)]
public class AfterShieldBlockEvent : PooledEvent<AfterShieldBlockEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Weapon;

	public GameObject Shield;

	public Shield ShieldPart;

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
		Shield = null;
		ShieldPart = null;
	}

	public static void Send(GameObject Attacker, GameObject Defender, GameObject Weapon, GameObject Shield, Shield ShieldPart)
	{
		AfterShieldBlockEvent E = PooledEvent<AfterShieldBlockEvent>.FromPool();
		E.Attacker = Attacker;
		E.Defender = Defender;
		E.Weapon = Weapon;
		E.Shield = Shield;
		E.ShieldPart = ShieldPart;
		Attacker.HandleEvent(E);
		Defender.HandleEvent(E);
		PooledEvent<AfterShieldBlockEvent>.ResetTo(ref E);
	}
}
