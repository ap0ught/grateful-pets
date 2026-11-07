namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class BeforeMeleeAttackEvent : PooledEvent<BeforeMeleeAttackEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Target;

	public GameObject Weapon;

	public string Skill;

	public string Stat;

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
		Target = null;
		Weapon = null;
		Skill = null;
		Stat = null;
	}

	public static void Send(GameObject Actor, GameObject Target, GameObject Weapon, string Skill, string Stat)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("BeforeMeleeAttack"))
		{
			Event obj = Event.New("BeforeMeleeAttack");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			obj.SetParameter("Weapon", Weapon);
			obj.SetParameter("Skill", Skill);
			obj.SetParameter("Stat", Stat);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<BeforeMeleeAttackEvent>.ID, CascadeLevel))
		{
			BeforeMeleeAttackEvent beforeMeleeAttackEvent = PooledEvent<BeforeMeleeAttackEvent>.FromPool();
			beforeMeleeAttackEvent.Actor = Actor;
			beforeMeleeAttackEvent.Target = Target;
			beforeMeleeAttackEvent.Weapon = Weapon;
			beforeMeleeAttackEvent.Skill = Skill;
			beforeMeleeAttackEvent.Stat = Stat;
			flag = Actor.HandleEvent(beforeMeleeAttackEvent);
		}
	}
}
