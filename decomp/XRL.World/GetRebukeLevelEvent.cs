namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Singleton)]
public class GetRebukeLevelEvent : SingletonEvent<GetRebukeLevelEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Target;

	public int Level;

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
		Level = 0;
	}

	public static int GetFor(GameObject Actor, GameObject Target)
	{
		SingletonEvent<GetRebukeLevelEvent>.Instance.Actor = Actor;
		SingletonEvent<GetRebukeLevelEvent>.Instance.Target = Target;
		SingletonEvent<GetRebukeLevelEvent>.Instance.Level = Actor.Stat("Level");
		Actor.HandleEvent(SingletonEvent<GetRebukeLevelEvent>.Instance);
		int num = SingletonEvent<GetRebukeLevelEvent>.Instance.Level;
		if (Actor == null || Actor.genotypeEntry?.Skills?.Contains("Persuasion_RebukeRobot") != true)
		{
			num -= 5;
		}
		return num;
	}
}
