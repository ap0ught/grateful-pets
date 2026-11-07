using System.Text;

namespace XRL.World;

[GameEvent(Cascade = 17)]
public class BeforeLevelGainedEvent : PassEvent<BeforeLevelGainedEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Kill;

	public GameObject InfluencedBy;

	public StringBuilder Message = new StringBuilder();

	public int Level;

	public bool Detail;

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
		Kill = null;
		InfluencedBy = null;
		Message.Clear();
		Level = 0;
		Detail = false;
	}

	public static BeforeLevelGainedEvent FromPool(GameObject Actor, GameObject Kill = null, GameObject InfluencedBy = null, int Level = 0, bool Detail = false)
	{
		BeforeLevelGainedEvent beforeLevelGainedEvent = PooledEvent<BeforeLevelGainedEvent>.FromPool();
		beforeLevelGainedEvent.Actor = Actor;
		beforeLevelGainedEvent.Kill = Kill;
		beforeLevelGainedEvent.InfluencedBy = InfluencedBy;
		beforeLevelGainedEvent.Level = Level;
		beforeLevelGainedEvent.Detail = Detail;
		return beforeLevelGainedEvent;
	}

	public bool Check()
	{
		if (Actor.HandleEvent(this) && The.Game.HandleEvent(this))
		{
			Postprocess();
			return true;
		}
		return false;
	}
}
