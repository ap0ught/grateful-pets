namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetSlotsRequiredEvent : PooledEvent<GetSlotsRequiredEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public GameObject Actor;

	public string SlotType;

	public int Base;

	public int Increases;

	public int Decreases;

	public bool AllowReduction;

	public bool CanBeTooSmall;

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
		Object = null;
		Actor = null;
		SlotType = null;
		Base = 0;
		Increases = 0;
		Decreases = 0;
		AllowReduction = false;
		CanBeTooSmall = false;
	}

	public static int GetFor(GameObject Object, GameObject Actor, string SlotType, int Base = 1, int Increases = 0, int Decreases = 0, bool AllowReduction = true, bool CanBeTooSmall = false)
	{
		bool flag = true;
		bool flag2 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetSlotsRequiredEvent>.ID, CascadeLevel);
		bool flag3 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetSlotsRequiredEvent>.ID, CascadeLevel);
		int num;
		if (flag2 | flag3)
		{
			GetSlotsRequiredEvent getSlotsRequiredEvent = PooledEvent<GetSlotsRequiredEvent>.FromPool();
			getSlotsRequiredEvent.Object = Object;
			getSlotsRequiredEvent.Actor = Actor;
			getSlotsRequiredEvent.SlotType = SlotType;
			getSlotsRequiredEvent.Base = Base;
			getSlotsRequiredEvent.Increases = Increases;
			getSlotsRequiredEvent.Decreases = Decreases;
			getSlotsRequiredEvent.AllowReduction = AllowReduction;
			getSlotsRequiredEvent.CanBeTooSmall = CanBeTooSmall;
			if (flag2 && GameObject.Validate(ref Object) && (!flag3 || Object.Equipped != Actor))
			{
				flag = Object.HandleEvent(getSlotsRequiredEvent);
			}
			if ((flag & flag3) && GameObject.Validate(ref Actor))
			{
				flag = Actor.HandleEvent(getSlotsRequiredEvent);
			}
			num = getSlotsRequiredEvent.Base;
			if (getSlotsRequiredEvent.Increases > 0)
			{
				num <<= getSlotsRequiredEvent.Increases;
			}
			if (getSlotsRequiredEvent.Decreases > 0 && getSlotsRequiredEvent.AllowReduction)
			{
				num >>= getSlotsRequiredEvent.Decreases;
			}
			if (!getSlotsRequiredEvent.CanBeTooSmall && num < 1)
			{
				num = 1;
			}
		}
		else
		{
			num = Base;
			if (Increases > 0)
			{
				num <<= Increases;
			}
			if ((Decreases > 0) & AllowReduction)
			{
				num >>= Decreases;
			}
			if (!CanBeTooSmall && num < 1)
			{
				num = 1;
			}
		}
		return num;
	}
}
