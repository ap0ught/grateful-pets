using System;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetPsychicGlimmerEvent : PooledEvent<GetPsychicGlimmerEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Subject;

	public int Base;

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
		Subject = null;
		Base = 0;
		Level = 0;
	}

	public static int GetFor(GameObject Subject, int Base = 0)
	{
		int num = Base;
		bool flag = true;
		if (flag && Subject != null && Subject.HasRegisteredEvent("GetPsychicGlimmer"))
		{
			Event obj = Event.New("GetPsychicGlimmer");
			obj.SetParameter("Subject", Subject);
			obj.SetParameter("Base", Base);
			obj.SetParameter("Level", num);
			flag = Subject.FireEvent(obj);
			num = obj.GetIntParameter("Level");
		}
		if (flag && Subject != null && Subject.WantEvent(PooledEvent<GetPsychicGlimmerEvent>.ID, CascadeLevel))
		{
			GetPsychicGlimmerEvent getPsychicGlimmerEvent = PooledEvent<GetPsychicGlimmerEvent>.FromPool();
			getPsychicGlimmerEvent.Subject = Subject;
			getPsychicGlimmerEvent.Base = Base;
			getPsychicGlimmerEvent.Level = num;
			flag = Subject.HandleEvent(getPsychicGlimmerEvent);
			num = getPsychicGlimmerEvent.Level;
		}
		return Math.Max(num, 0);
	}
}
