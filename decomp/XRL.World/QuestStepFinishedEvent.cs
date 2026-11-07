using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class QuestStepFinishedEvent : IQuestEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(QuestStepFinishedEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 15;

	private static List<QuestStepFinishedEvent> Pool;

	private static int PoolCounter;

	public QuestStep Step;

	public QuestStepFinishedEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref QuestStepFinishedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static QuestStepFinishedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Step = null;
	}

	public static void Send(Quest Quest, QuestStep Step)
	{
		QuestStepFinishedEvent E = FromPool();
		E.Quest = Quest;
		E.Step = Step;
		The.Game.HandleEvent(E, DispatchPlayer: true);
		ResetTo(ref E);
	}
}
