namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetIdealTemperatureEvent : PooledEvent<GetIdealTemperatureEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public int Temperature;

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
		Temperature = 0;
	}

	public static int GetFor(GameObject Object, int DefaultTemperature = 25)
	{
		bool flag = true;
		int num = DefaultTemperature;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetIdealTemperature"))
		{
			Event obj = Event.New("GetIdealTemperature");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Temperature", num);
			flag = Object.FireEvent(obj);
			num = obj.GetIntParameter("Temperature");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetIdealTemperatureEvent>.ID, CascadeLevel))
		{
			GetIdealTemperatureEvent getIdealTemperatureEvent = PooledEvent<GetIdealTemperatureEvent>.FromPool();
			getIdealTemperatureEvent.Object = Object;
			getIdealTemperatureEvent.Temperature = num;
			flag = Object.HandleEvent(getIdealTemperatureEvent);
			num = getIdealTemperatureEvent.Temperature;
		}
		return num;
	}
}
