namespace XRL.World;

[GameEvent(Cascade = 3, Cache = Cache.Pool)]
public class GetSocialSifrahSetupEvent : PooledEvent<GetSocialSifrahSetupEvent>
{
	public new static readonly int CascadeLevel = 3;

	public GameObject Actor;

	public GameObject Interlocutor;

	public string Type;

	public int Difficulty;

	public int Rating;

	public int Turns;

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
		Interlocutor = null;
		Type = null;
		Difficulty = 0;
		Rating = 0;
		Turns = 0;
	}

	public static GetSocialSifrahSetupEvent FromPool(GameObject Actor, GameObject Interlocutor, string Type, int Difficulty, int Rating, int Turns)
	{
		GetSocialSifrahSetupEvent getSocialSifrahSetupEvent = PooledEvent<GetSocialSifrahSetupEvent>.FromPool();
		getSocialSifrahSetupEvent.Actor = Actor;
		getSocialSifrahSetupEvent.Interlocutor = Interlocutor;
		getSocialSifrahSetupEvent.Type = Type;
		getSocialSifrahSetupEvent.Difficulty = Difficulty;
		getSocialSifrahSetupEvent.Rating = Rating;
		getSocialSifrahSetupEvent.Turns = Turns;
		return getSocialSifrahSetupEvent;
	}

	public static bool GetFor(GameObject Actor, GameObject Interlocutor, string Type, ref int Difficulty, ref int Rating, ref int Turns)
	{
		bool result = true;
		if (Actor.WantEvent(PooledEvent<GetSocialSifrahSetupEvent>.ID, CascadeLevel))
		{
			GetSocialSifrahSetupEvent getSocialSifrahSetupEvent = FromPool(Actor, Interlocutor, Type, Difficulty, Rating, Turns);
			result = Actor.HandleEvent(getSocialSifrahSetupEvent);
			Difficulty = getSocialSifrahSetupEvent.Difficulty;
			Rating = getSocialSifrahSetupEvent.Rating;
			Turns = getSocialSifrahSetupEvent.Turns;
		}
		return result;
	}
}
