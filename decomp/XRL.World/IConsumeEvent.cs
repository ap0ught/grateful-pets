namespace XRL.World;

[GameEvent(Base = true, Cascade = 17)]
public abstract class IConsumeEvent : MinEvent
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Subject;

	public GameObject Object;

	public bool Eat;

	public bool Drink;

	public bool Inject;

	public bool Inhale;

	public bool Absorb;

	public bool Voluntary;

	public bool Ingest
	{
		get
		{
			if (!Eat)
			{
				return Drink;
			}
			return true;
		}
	}

	public bool External => Actor != Subject;

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
		Actor = null;
		Subject = null;
		Object = null;
		Eat = false;
		Drink = false;
		Inject = false;
		Inhale = false;
		Absorb = false;
		Voluntary = false;
	}

	public static bool DispatchAll<T>(T E) where T : IConsumeEvent
	{
		if (!E.Actor.HandleEvent(E))
		{
			return false;
		}
		if (E.Subject != null && E.Subject != E.Actor && !E.Subject.HandleEvent(E))
		{
			return false;
		}
		return E.Object?.HandleEvent(E) ?? true;
	}
}
