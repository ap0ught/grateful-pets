using XRL.World.Parts.Skill;

namespace XRL.World;

[GameEvent(Cascade = 256, Cache = Cache.Pool)]
public class RepairedEvent : PooledEvent<RepairedEvent>
{
	public new static readonly int CascadeLevel = 256;

	public GameObject Actor;

	public GameObject Subject;

	public GameObject Tool;

	public BaseSkill Skill;

	public int MaxRepairTier;

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
		Tool = null;
		Skill = null;
		MaxRepairTier = 0;
	}

	public static void Send(GameObject Actor = null, GameObject Subject = null, GameObject Tool = null, BaseSkill Skill = null, int MaxRepairTier = 0)
	{
		if (GameObject.Validate(ref Subject))
		{
			RepairedEvent E = PooledEvent<RepairedEvent>.FromPool();
			E.Actor = Actor;
			E.Subject = Subject;
			E.Tool = Tool;
			E.Skill = Skill;
			E.MaxRepairTier = MaxRepairTier;
			Subject.HandleEvent(E);
			PooledEvent<RepairedEvent>.ResetTo(ref E);
		}
	}
}
