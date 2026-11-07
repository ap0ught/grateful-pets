namespace XRL.World;

[GameEvent(Cascade = 3, Cache = Cache.Pool)]
public class GetPsionicSifrahSetupEvent : PooledEvent<GetPsionicSifrahSetupEvent>
{
	public new static readonly int CascadeLevel = 3;

	public GameObject Actor;

	public GameObject Subject;

	public string Type;

	public string Subtype;

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
		Subtype = null;
		Interruptable = false;
		Difficulty = 0;
		Rating = 0;
		Turns = 0;
		PsychometryApplied = false;
	}

	public static bool GetFor(GameObject Actor, GameObject Subject, string Type, string Subtype, bool Interruptable, ref int Difficulty, ref int Rating, ref int Turns, ref bool Interrupt, ref bool PsychometryApplied)
	{
		bool flag = true;
		if (Actor.WantEvent(PooledEvent<GetPsionicSifrahSetupEvent>.ID, CascadeLevel))
		{
			GetPsionicSifrahSetupEvent getPsionicSifrahSetupEvent = PooledEvent<GetPsionicSifrahSetupEvent>.FromPool();
			getPsionicSifrahSetupEvent.Actor = Actor;
			getPsionicSifrahSetupEvent.Subject = Subject;
			getPsionicSifrahSetupEvent.Type = Type;
			getPsionicSifrahSetupEvent.Subtype = Subtype;
			getPsionicSifrahSetupEvent.Difficulty = Difficulty;
			getPsionicSifrahSetupEvent.Rating = Rating;
			getPsionicSifrahSetupEvent.Turns = Turns;
			getPsionicSifrahSetupEvent.Interruptable = Interruptable;
			getPsionicSifrahSetupEvent.PsychometryApplied = PsychometryApplied;
			flag = Actor.HandleEvent(getPsionicSifrahSetupEvent);
			if (Interruptable && !flag)
			{
				Interrupt = true;
			}
			Difficulty = getPsionicSifrahSetupEvent.Difficulty;
			Rating = getPsionicSifrahSetupEvent.Rating;
			Turns = getPsionicSifrahSetupEvent.Turns;
			PsychometryApplied = getPsionicSifrahSetupEvent.PsychometryApplied;
		}
		return flag;
	}
}
