using System;

namespace XRL.World;

[GameEvent(Cascade = 17, Base = true)]
public abstract class IMeleePenetrationEvent : MinEvent
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Weapon;

	public string Properties;

	public string Hand;

	public int Penetrations;

	public int StatBonus;

	public int MaxStatBonus;

	public int PenetrationBonus;

	public int MaxPenetrationBonus;

	public int AV;

	public bool Critical;

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
		Weapon = null;
		Properties = null;
		Hand = null;
		Penetrations = 0;
		StatBonus = 0;
		MaxStatBonus = 0;
		PenetrationBonus = 0;
		MaxPenetrationBonus = 0;
		AV = 0;
		Critical = false;
	}

	public static bool Process(ref int Penetrations, ref int StatBonus, ref int MaxStatBonus, ref int PenetrationBonus, ref int MaxPenetrationBonus, int AV, bool Critical, string Properties, string Hand, GameObject Attacker, GameObject Defender, GameObject Weapon, string RegisteredEvent, GameObject Target, int ID, int CascadeLevel, Func<IMeleePenetrationEvent> Generator)
	{
		bool flag = GameObject.Validate(ref Target);
		if (flag && Target.HasRegisteredEvent(RegisteredEvent))
		{
			Event obj = Event.New(RegisteredEvent);
			obj.SetParameter("Penetrations", StatBonus);
			obj.SetParameter("MaxStrengthBonus", MaxStatBonus);
			obj.SetParameter("Attacker", Attacker);
			obj.SetParameter("Defender", Defender);
			obj.SetParameter("Weapon", Weapon);
			obj.SetParameter("PenBonus", PenetrationBonus);
			obj.SetParameter("CapBonus", MaxPenetrationBonus);
			obj.SetParameter("AV", AV);
			obj.SetParameter("Hand", Hand);
			obj.SetParameter("Properties", Properties);
			obj.SetFlag("Critical", Critical);
			flag = Target.FireEvent(obj);
			StatBonus = obj.GetIntParameter("Penetrations");
			MaxStatBonus = obj.GetIntParameter("MaxStrengthBonus");
			PenetrationBonus = obj.GetIntParameter("PenBonus");
			MaxPenetrationBonus = obj.GetIntParameter("CapBonus");
		}
		flag = flag && GameObject.Validate(ref Target);
		if (flag && Target.WantEvent(ID, CascadeLevel))
		{
			IMeleePenetrationEvent meleePenetrationEvent = Generator();
			meleePenetrationEvent.Penetrations = Penetrations;
			meleePenetrationEvent.StatBonus = StatBonus;
			meleePenetrationEvent.MaxStatBonus = MaxStatBonus;
			meleePenetrationEvent.Attacker = Attacker;
			meleePenetrationEvent.Defender = Defender;
			meleePenetrationEvent.Weapon = Weapon;
			meleePenetrationEvent.PenetrationBonus = PenetrationBonus;
			meleePenetrationEvent.MaxPenetrationBonus = MaxPenetrationBonus;
			meleePenetrationEvent.AV = AV;
			meleePenetrationEvent.Hand = Hand;
			meleePenetrationEvent.Properties = Properties;
			meleePenetrationEvent.Critical = Critical;
			flag = Target.HandleEvent(meleePenetrationEvent);
			Penetrations = meleePenetrationEvent.Penetrations;
			StatBonus = meleePenetrationEvent.StatBonus;
			MaxStatBonus = meleePenetrationEvent.MaxStatBonus;
			PenetrationBonus = meleePenetrationEvent.PenetrationBonus;
			MaxPenetrationBonus = meleePenetrationEvent.MaxPenetrationBonus;
		}
		return flag;
	}
}
