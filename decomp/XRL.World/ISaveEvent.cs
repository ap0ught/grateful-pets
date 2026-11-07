namespace XRL.World;

[GameEvent(Base = true, Cascade = 17)]
public abstract class ISaveEvent : MinEvent
{
	public delegate ISaveEvent ISaveEventGenerator(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, int Roll, int BaseDifficulty, int Difficulty, bool IgnoreNatural1, bool IgnoreNatural20, bool Actual);

	public new static readonly int CascadeLevel = 17;

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Source;

	public string Stat;

	public string AttackerStat;

	public string Vs;

	public int NaturalRoll;

	public int Roll;

	public int BaseDifficulty;

	public int Difficulty;

	public bool IgnoreNatural1;

	public bool IgnoreNatural20;

	public bool Actual;

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
		Attacker = null;
		Defender = null;
		Source = null;
		Stat = null;
		AttackerStat = null;
		Vs = null;
		NaturalRoll = 0;
		Roll = 0;
		BaseDifficulty = 0;
		Difficulty = 0;
		IgnoreNatural1 = false;
		IgnoreNatural20 = false;
		Actual = false;
	}

	public static bool Process(GameObject Target, string RegisteredEventID, int ID, int CascadeLevel, ISaveEventGenerator Generator, GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, ref int Roll, int BaseDifficulty, ref int Difficulty, ref bool IgnoreNatural1, ref bool IgnoreNatural20, bool Actual)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Target) && Target.HasRegisteredEvent(RegisteredEventID))
		{
			Event obj = Event.New(RegisteredEventID);
			obj.SetParameter("Attacker", Attacker);
			obj.SetParameter("Defender", Defender);
			obj.SetParameter("Source", Source);
			obj.SetParameter("Stat", Stat);
			obj.SetParameter("AttackerStat", AttackerStat ?? Stat);
			obj.SetParameter("Vs", Vs ?? "");
			obj.SetParameter("NaturalRoll", NaturalRoll);
			obj.SetParameter("Roll", Roll);
			obj.SetParameter("BaseDifficulty", BaseDifficulty);
			obj.SetParameter("Difficulty", Difficulty);
			obj.SetFlag("IgnoreNatural1", IgnoreNatural1);
			obj.SetFlag("IgnoreNatural20", IgnoreNatural20);
			obj.SetFlag("Actual", Actual);
			flag = Target.FireEvent(obj);
			Roll = obj.GetIntParameter("Roll");
			Difficulty = obj.GetIntParameter("Difficulty");
			IgnoreNatural1 = obj.HasFlag("IgnoreNatural1");
			IgnoreNatural20 = obj.HasFlag("IgnoreNatural20");
		}
		if (flag && GameObject.Validate(ref Target) && Target.WantEvent(ID, CascadeLevel))
		{
			ISaveEvent saveEvent = Generator(Attacker, Defender, Source, Stat, AttackerStat ?? Stat, Vs ?? "", NaturalRoll, Roll, BaseDifficulty, Difficulty, IgnoreNatural1, IgnoreNatural20, Actual);
			flag = Target.HandleEvent(saveEvent);
			Roll = saveEvent.Roll;
			Difficulty = saveEvent.Difficulty;
			IgnoreNatural1 = saveEvent.IgnoreNatural1;
			IgnoreNatural20 = saveEvent.IgnoreNatural20;
		}
		return true;
	}
}
