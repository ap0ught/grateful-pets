namespace XRL.World;

[GameEvent(Base = true, Cascade = 1)]
public abstract class IMentalAttackEvent : MinEvent
{
	public new static readonly int CascadeLevel = 1;

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Source;

	public string Command;

	public string Dice;

	public int Type;

	public int Magnitude;

	public int Penetrations;

	public int Difficulty;

	public int BaseDifficulty;

	public int Modifier;

	public bool Reflected
	{
		get
		{
			return Type.HasBit(16777216);
		}
		set
		{
			Type.SetBit(16777216, value);
		}
	}

	public bool Reflectable
	{
		get
		{
			return Type.HasBit(8388608);
		}
		set
		{
			Type.SetBit(8388608, value);
		}
	}

	public bool Psionic
	{
		get
		{
			return Type.HasBit(1);
		}
		set
		{
			Type.SetBit(1, value);
		}
	}

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
		Command = null;
		Dice = null;
		Type = 0;
		Magnitude = 0;
		Penetrations = 0;
		Difficulty = 0;
		BaseDifficulty = 0;
		Modifier = 0;
	}

	public bool IsPlayerInvolved()
	{
		if (!Attacker.IsPlayer())
		{
			return Defender.IsPlayer();
		}
		return true;
	}

	public void SetFrom(IMentalAttackEvent E)
	{
		Attacker = E.Attacker;
		Defender = E.Defender;
		Source = E.Source;
		Command = E.Command;
		Dice = E.Dice;
		Type = E.Type;
		Magnitude = E.Magnitude;
		Penetrations = E.Penetrations;
		Difficulty = E.Difficulty;
		BaseDifficulty = E.BaseDifficulty;
		Modifier = E.Modifier;
	}

	public void ApplyTo(IMentalAttackEvent E)
	{
		E.SetFrom(this);
	}

	public void SetFrom(Event E)
	{
		Attacker = E.GetGameObjectParameter("Attacker");
		Defender = E.GetGameObjectParameter("Defender");
		Source = E.GetGameObjectParameter("Source");
		Command = E.GetStringParameter("Command");
		Dice = E.GetStringParameter("Dice");
		Type = E.GetIntParameter("Type");
		Magnitude = E.GetIntParameter("Magnitude");
		Penetrations = E.GetIntParameter("Penetrations");
		BaseDifficulty = E.GetIntParameter("BaseDifficulty");
		Difficulty = E.GetIntParameter("Difficulty");
		Modifier = E.GetIntParameter("Modifier");
	}

	public void ApplyTo(Event E)
	{
		E.SetParameter("Attacker", Attacker);
		E.SetParameter("Defender", Defender);
		E.SetParameter("Source", Source);
		E.SetParameter("Command", Command);
		E.SetParameter("Dice", Dice);
		E.SetParameter("Type", Type);
		E.SetParameter("Magnitude", Magnitude);
		E.SetParameter("Penetrations", Penetrations);
		E.SetParameter("BaseDifficulty", BaseDifficulty);
		E.SetParameter("Difficulty", Difficulty);
		E.SetParameter("Modifier", Modifier);
	}
}
