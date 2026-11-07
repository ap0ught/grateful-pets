namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class AttackerBeforeTemperatureChangeEvent : PooledEvent<AttackerBeforeTemperatureChangeEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public int Amount;

	public GameObject Actor;

	public bool Radiant;

	public bool MinAmbient;

	public bool MaxAmbient;

	public bool IgnoreResistance;

	public int? Min;

	public int? Max;

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
		Amount = 0;
		Actor = null;
		Radiant = false;
		MinAmbient = false;
		MaxAmbient = false;
		IgnoreResistance = false;
		Min = null;
		Max = null;
	}

	public static int GetFor(GameObject Object, int Amount, GameObject Actor = null, bool Radiant = false, bool MinAmbient = false, bool MaxAmbient = false, bool IgnoreResistance = false, int Phase = 0, int? Min = null, int? Max = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AttackerBeforeTemperatureChange"))
		{
			Event obj = Event.New("AttackerBeforeTemperatureChange");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Amount", Amount);
			obj.SetParameter("Actor", Actor);
			obj.SetFlag("Radiant", Radiant);
			obj.SetFlag("MinAmbient", MinAmbient);
			obj.SetFlag("MaxAmbient", MaxAmbient);
			obj.SetFlag("IgnoreResistance", IgnoreResistance);
			obj.SetParameter("Min", Min);
			obj.SetParameter("Max", Max);
			flag = Actor.FireEvent(obj);
			Amount = obj.GetIntParameter("Amount");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<AttackerBeforeTemperatureChangeEvent>.ID, CascadeLevel))
		{
			AttackerBeforeTemperatureChangeEvent attackerBeforeTemperatureChangeEvent = PooledEvent<AttackerBeforeTemperatureChangeEvent>.FromPool();
			attackerBeforeTemperatureChangeEvent.Object = Object;
			attackerBeforeTemperatureChangeEvent.Amount = Amount;
			attackerBeforeTemperatureChangeEvent.Actor = Actor;
			attackerBeforeTemperatureChangeEvent.Radiant = Radiant;
			attackerBeforeTemperatureChangeEvent.MinAmbient = MinAmbient;
			attackerBeforeTemperatureChangeEvent.MaxAmbient = MaxAmbient;
			attackerBeforeTemperatureChangeEvent.IgnoreResistance = IgnoreResistance;
			attackerBeforeTemperatureChangeEvent.Min = Min;
			attackerBeforeTemperatureChangeEvent.Max = Max;
			flag = Actor.HandleEvent(attackerBeforeTemperatureChangeEvent);
			Amount = attackerBeforeTemperatureChangeEvent.Amount;
		}
		if (!flag)
		{
			Amount = 0;
		}
		return Amount;
	}
}
