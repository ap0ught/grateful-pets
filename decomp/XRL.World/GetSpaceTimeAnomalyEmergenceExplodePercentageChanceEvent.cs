namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent : PooledEvent<GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent>
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
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetSpaceTimeAnomalyEmergenceExplodePercentageChance"))
		{
			Event obj = Event.New("GetSpaceTimeAnomalyEmergenceExplodePercentageChance");
			obj.SetParameter("Object", Object);
			obj.SetParameter("BaseChance", BaseChance);
			obj.SetParameter("Chance", num);
			flag = Object.FireEvent(obj);
			num = obj.GetIntParameter("Chance");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent>.ID, CascadeLevel))
		{
			GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent getSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent = PooledEvent<GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent>.FromPool();
			getSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent.Object = Object;
			getSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent.BaseChance = BaseChance;
			getSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent.Chance = num;
			flag = Object.HandleEvent(getSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent);
			num = getSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent.Chance;
		}
		return num;
	}
}
