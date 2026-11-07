namespace XRL.World;

[GameEvent(Base = true)]
public abstract class INavigationWeightEvent : MinEvent
{
	public Cell Cell;

	public GameObject Actor;

	public GameObject Object;

	public bool Uncacheable;

	public int Weight;

	public int PriorWeight;

	public int Nav;

	private int _SocialWeight;

	public bool Smart
	{
		get
		{
			return (Nav & 1) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 1;
			}
			else
			{
				Nav &= -2;
			}
		}
	}

	public bool Burrower
	{
		get
		{
			return (Nav & 2) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 2;
			}
			else
			{
				Nav &= -3;
			}
		}
	}

	public bool Autoexploring
	{
		get
		{
			return (Nav & 4) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 4;
			}
			else
			{
				Nav &= -5;
			}
		}
	}

	public bool Flying
	{
		get
		{
			return (Nav & 8) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 8;
			}
			else
			{
				Nav &= -9;
			}
		}
	}

	public bool WallWalker
	{
		get
		{
			return (Nav & 0x10) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 16;
			}
			else
			{
				Nav &= -17;
			}
		}
	}

	public bool IgnoresWalls
	{
		get
		{
			return (Nav & 0x20) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 32;
			}
			else
			{
				Nav &= -33;
			}
		}
	}

	public bool Swimming
	{
		get
		{
			return (Nav & 0x40) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 64;
			}
			else
			{
				Nav &= -65;
			}
		}
	}

	public bool Slimewalking
	{
		get
		{
			return (Nav & 0x80) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 128;
			}
			else
			{
				Nav &= -129;
			}
		}
	}

	public bool Aquatic
	{
		get
		{
			return (Nav & 0x100) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 256;
			}
			else
			{
				Nav &= -257;
			}
		}
	}

	public bool Polypwalking
	{
		get
		{
			return (Nav & 0x200) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 512;
			}
			else
			{
				Nav &= -513;
			}
		}
	}

	public bool Strutwalking
	{
		get
		{
			return (Nav & 0x400) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 1024;
			}
			else
			{
				Nav &= -1025;
			}
		}
	}

	public bool Juggernaut
	{
		get
		{
			return (Nav & 0x800) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 2048;
			}
			else
			{
				Nav &= -2049;
			}
		}
	}

	public bool Reefer
	{
		get
		{
			return (Nav & 0x1000) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 4096;
			}
			else
			{
				Nav &= -4097;
			}
		}
	}

	public bool IgnoreCreatures
	{
		get
		{
			return (Nav & 0x2000) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 8192;
			}
			else
			{
				Nav &= -8193;
			}
		}
	}

	public bool IgnoreGases
	{
		get
		{
			return (Nav & 0x4000) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 16384;
			}
			else
			{
				Nav &= -16385;
			}
		}
	}

	public bool Unbreathing
	{
		get
		{
			return (Nav & 0x8000) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 32768;
			}
			else
			{
				Nav &= -32769;
			}
		}
	}

	public bool FilthAffinity
	{
		get
		{
			return (Nav & 0x10000) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 65536;
			}
			else
			{
				Nav &= -65537;
			}
		}
	}

	public bool OutOfPhase
	{
		get
		{
			return (Nav & 0x20000) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 131072;
			}
			else
			{
				Nav &= -131073;
			}
		}
	}

	public bool Omniphase
	{
		get
		{
			return (Nav & 0x40000) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 262144;
			}
			else
			{
				Nav &= -262145;
			}
		}
	}

	public bool Nullphase
	{
		get
		{
			return (Nav & 0x80000) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 524288;
			}
			else
			{
				Nav &= -524289;
			}
		}
	}

	public bool FlexPhase
	{
		get
		{
			return (Nav & 0x100000) != 0;
		}
		set
		{
			if (value)
			{
				Nav |= 1048576;
			}
			else
			{
				Nav &= -1048577;
			}
		}
	}

	public int SocialWeight
	{
		get
		{
			if (Weight <= _SocialWeight)
			{
				return _SocialWeight;
			}
			return Weight;
		}
		set
		{
			_SocialWeight = value;
		}
	}

	public int Phase
	{
		get
		{
			if (Nullphase)
			{
				return 4;
			}
			if (Omniphase)
			{
				return 3;
			}
			if (OutOfPhase)
			{
				return 2;
			}
			return 1;
		}
		set
		{
		}
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Cell = null;
		Actor = null;
		Object = null;
		Uncacheable = false;
		Weight = 0;
		PriorWeight = 0;
		Nav = 0;
		_SocialWeight = 0;
	}

	public void MinWeight(int Value)
	{
		if (Weight < Value)
		{
			Weight = Value;
		}
	}

	public void MinWeight(int Value, int Max)
	{
		if (Value > Max)
		{
			Value = Max;
		}
		if (Weight < Value)
		{
			Weight = Value;
		}
	}

	public void MinSocialWeight(int Value)
	{
		if (_SocialWeight < Value)
		{
			_SocialWeight = Value;
		}
	}

	public void MinSocialWeight(int Value, int Max)
	{
		if (Value > Max)
		{
			Value = Max;
		}
		if (_SocialWeight < Value)
		{
			_SocialWeight = Value;
		}
	}

	public virtual void ApplyTo(INavigationWeightEvent E)
	{
		E.Cell = Cell;
		E.Actor = Actor;
		E.Object = Object;
		E.Nav = Nav;
		E.Uncacheable = Uncacheable;
		E.Weight = Weight;
		E._SocialWeight = _SocialWeight;
		E.PriorWeight = PriorWeight;
	}

	public bool PhaseMatches(GameObject Object)
	{
		if (!Object.PhaseMatches(Phase))
		{
			return false;
		}
		if (FlexPhase)
		{
			int phase = Object.GetPhase();
			if (phase == 1 || phase == 2)
			{
				return false;
			}
		}
		return true;
	}
}
