using System.Diagnostics.CodeAnalysis;
using XRL.World.AI;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class AfterAddOpinionEvent : PooledEvent<AfterAddOpinionEvent>
{
	public new static readonly int CascadeLevel = 17;

	[NotNull]
	public GameObject Actor;

	[NotNull]
	public GameObject Subject;

	public GameObject Object;

	[NotNull]
	public IOpinion Opinion;

	public bool Renew;

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
		Opinion = null;
		Renew = false;
	}

	public static void Send(IOpinion Opinion, GameObject Actor, GameObject Subject, GameObject Object = null, bool Renew = false)
	{
		AfterAddOpinionEvent E = PooledEvent<AfterAddOpinionEvent>.FromPool();
		E.Opinion = Opinion;
		E.Actor = Actor;
		E.Subject = Subject;
		E.Object = Object;
		E.Renew = Renew;
		if (Opinion.HandleEvent(E) && Actor.HandleEvent(E))
		{
			Subject.HandleEvent(E);
		}
		PooledEvent<AfterAddOpinionEvent>.ResetTo(ref E);
	}
}
