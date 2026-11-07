namespace XRL.World;

[GameEvent(Cascade = 3, Cache = Cache.Pool)]
public class GetRitualSifrahSetupEvent : PooledEvent<GetRitualSifrahSetupEvent>
{
	public new static readonly int CascadeLevel = 3;

	public GameObject Actor;

	public GameObject Subject;

	public string Type;

	public bool Interruptable;

	public int Difficulty;

	public int Rating;

	public int Turns;

	public bool PsychometryApplied;

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
		Type = null;
		Interruptable = false;
		Difficulty = 0;
		Rating = 0;
		Turns = 0;
		PsychometryApplied = false;
	}

	public static bool GetFor(GameObject Actor, GameObject Subject, string Type, bool Interruptable, ref int Difficulty, ref int Rating, ref int Turns, ref bool Interrupt, ref bool PsychometryApplied)
	{
		bool flag = true;
		if (Actor.WantEvent(PooledEvent<GetRitualSifrahSetupEvent>.ID, CascadeLevel))
		{
			GetRitualSifrahSetupEvent getRitualSifrahSetupEvent = PooledEvent<GetRitualSifrahSetupEvent>.FromPool();
			getRitualSifrahSetupEvent.Actor = Actor;
			getRitualSifrahSetupEvent.Subject = Subject;
			getRitualSifrahSetupEvent.Type = Type;
			getRitualSifrahSetupEvent.Difficulty = Difficulty;
			getRitualSifrahSetupEvent.Rating = Rating;
			getRitualSifrahSetupEvent.Turns = Turns;
			getRitualSifrahSetupEvent.Interruptable = Interruptable;
			getRitualSifrahSetupEvent.PsychometryApplied = PsychometryApplied;
			flag = Actor.HandleEvent(getRitualSifrahSetupEvent);
			if (Interruptable && !flag)
			{
				Interrupt = true;
			}
			Difficulty = getRitualSifrahSetupEvent.Difficulty;
			Rating = getRitualSifrahSetupEvent.Rating;
			Turns = getRitualSifrahSetupEvent.Turns;
			PsychometryApplied = getRitualSifrahSetupEvent.PsychometryApplied;
		}
		return flag;
	}
}
