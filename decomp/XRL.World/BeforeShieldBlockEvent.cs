using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 17)]
public class BeforeShieldBlockEvent : PooledEvent<BeforeShieldBlockEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Weapon;

	public GameObject Shield;

	public Shield ShieldPart;

	public int Chance;

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
		Chance = 0;
	}

	public static bool Check(GameObject Attacker, GameObject Defender, GameObject Weapon, GameObject Shield, Shield ShieldPart, ref int Chance)
	{
		BeforeShieldBlockEvent E = PooledEvent<BeforeShieldBlockEvent>.FromPool();
		E.Attacker = Attacker;
		E.Defender = Defender;
		E.Weapon = Weapon;
		E.Shield = Shield;
		E.ShieldPart = ShieldPart;
		E.Chance = Chance;
		bool result = Attacker.HandleEvent(E) && Defender.HandleEvent(E);
		Chance = E.Chance;
		PooledEvent<BeforeShieldBlockEvent>.ResetTo(ref E);
		return result;
	}
}
