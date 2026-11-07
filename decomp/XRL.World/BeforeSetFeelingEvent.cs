using XRL.World.AI;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeSetFeelingEvent : PooledEvent<BeforeSetFeelingEvent>
{
	public GameObject Actor;

	public GameObject Target;

	public ObjectOpinion Opinion;

	public int Feeling;

	public new static readonly int CascadeLevel;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Target = null;
		Opinion = null;
		Feeling = 0;
	}

	public static bool Check(GameObject Actor, GameObject Target, ObjectOpinion Opinion, ref int Feeling)
	{
		bool flag = true;
		if (Actor.WantEvent(PooledEvent<BeforeSetFeelingEvent>.ID, CascadeLevel))
		{
			BeforeSetFeelingEvent E = PooledEvent<BeforeSetFeelingEvent>.FromPool();
			E.Actor = Actor;
			E.Target = Target;
			E.Opinion = Opinion;
			E.Feeling = Feeling;
			flag = Actor.HandleEvent(E);
			Feeling = E.Feeling;
			PooledEvent<BeforeSetFeelingEvent>.ResetTo(ref E);
		}
		if (flag && Actor.HasRegisteredEvent("BeforeSetFeeling"))
		{
			Event obj = Event.New("BeforeSetFeeling");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			obj.SetParameter("Opinion", Opinion);
			obj.SetParameter("Feeling", Feeling);
			flag = Actor.FireEvent(obj);
			Feeling = obj.GetIntParameter("Feeling");
		}
		return flag;
	}
}
