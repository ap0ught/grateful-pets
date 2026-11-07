namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IDeathEvent : MinEvent
{
	public GameObject Dying;

	public GameObject Killer;

	public GameObject Weapon;

	public GameObject Projectile;

	public string KillerText;

	public string Reason;

	public string ThirdPersonReason;

	public bool Accidental;

	public bool AlwaysUsePopups;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Dying = null;
		Killer = null;
		Weapon = null;
		Projectile = null;
		KillerText = null;
		Reason = null;
		ThirdPersonReason = null;
		Accidental = false;
		AlwaysUsePopups = false;
	}
}
