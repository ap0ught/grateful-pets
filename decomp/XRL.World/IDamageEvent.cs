namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IDamageEvent : MinEvent
{
	public Damage Damage;

	public GameObject Object;

	public GameObject Actor;

	public GameObject Source;

	public GameObject Weapon;

	public GameObject Projectile;

	public bool Indirect;

	public bool WillUseOutcomeMessageFragment;

	public bool DidSpecialEffect;

	public string OutcomeMessageFragment;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Damage = null;
		Object = null;
		Actor = null;
		Source = null;
		Weapon = null;
		Projectile = null;
		Indirect = false;
		WillUseOutcomeMessageFragment = false;
		DidSpecialEffect = false;
		OutcomeMessageFragment = null;
	}
}
