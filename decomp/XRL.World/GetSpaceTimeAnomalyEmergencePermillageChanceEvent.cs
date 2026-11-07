namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetSpaceTimeAnomalyEmergencePermillageChanceEvent : PooledEvent<GetSpaceTimeAnomalyEmergencePermillageChanceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public int BaseChance;

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
		Object = null;
		BaseChance = 0;
		Chance = 0;
	}

	public static int GetFor(GameObject Object, int BaseChance = 0)
	{
		bool flag = true;
		int num = BaseChance;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetSpaceTimeAnomalyEmergencePermillageChance"))
		{
			Event obj = Event.New("GetSpaceTimeAnomalyEmergencePermillageChance");
			obj.SetParameter("Object", Object);
			obj.SetParameter("BaseChance", BaseChance);
			obj.SetParameter("Chance", num);
			flag = Object.FireEvent(obj);
			num = obj.GetIntParameter("Chance");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetSpaceTimeAnomalyEmergencePermillageChanceEvent>.ID, CascadeLevel))
		{
			GetSpaceTimeAnomalyEmergencePermillageChanceEvent getSpaceTimeAnomalyEmergencePermillageChanceEvent = PooledEvent<GetSpaceTimeAnomalyEmergencePermillageChanceEvent>.FromPool();
			getSpaceTimeAnomalyEmergencePermillageChanceEvent.Object = Object;
			getSpaceTimeAnomalyEmergencePermillageChanceEvent.BaseChance = BaseChance;
			getSpaceTimeAnomalyEmergencePermillageChanceEvent.Chance = num;
			flag = Object.HandleEvent(getSpaceTimeAnomalyEmergencePermillageChanceEvent);
			num = getSpaceTimeAnomalyEmergencePermillageChanceEvent.Chance;
		}
		return num;
	}
}
