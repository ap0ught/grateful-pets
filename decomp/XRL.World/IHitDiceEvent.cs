using System;

namespace XRL.World;

[GameEvent(Base = true, Cascade = 1)]
public abstract class IHitDiceEvent : MinEvent
{
	public new static readonly int CascadeLevel = 1;

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Weapon;

	public int PenetrationBonus;

	public int AV;

	public bool ShieldBlocked;

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
		PenetrationBonus = 0;
		AV = 0;
		ShieldBlocked = false;
	}

	public static bool Process(ref int PenetrationBonus, ref int AV, ref bool ShieldBlocked, GameObject Attacker, GameObject Defender, GameObject Weapon, string RegisteredEvent, GameObject Target, int ID, int CascadeLevel, Func<GameObject, GameObject, GameObject, int, int, bool, IHitDiceEvent> Generator)
	{
		if (GameObject.Validate(ref Target))
		{
			if (Target.HasRegisteredEvent(RegisteredEvent))
			{
				Event obj = Event.New(RegisteredEvent);
				obj.SetParameter("PenetrationBonus", PenetrationBonus);
				obj.SetParameter("Attacker", Attacker);
				obj.SetParameter("Defender", Defender);
				obj.SetParameter("Weapon", Weapon);
				obj.SetParameter("AV", AV);
				obj.SetFlag("ShieldBlocked", ShieldBlocked);
				try
				{
					if (!Target.FireEvent(obj))
					{
						return false;
					}
				}
				finally
				{
					PenetrationBonus = obj.GetIntParameter("PenetrationBonus");
					AV = obj.GetIntParameter("AV");
					ShieldBlocked = obj.HasFlag("ShieldBlocked");
				}
			}
			if (Target.WantEvent(ID, CascadeLevel))
			{
				IHitDiceEvent hitDiceEvent = Generator(Attacker, Defender, Weapon, PenetrationBonus, AV, ShieldBlocked);
				try
				{
					if (!Target.HandleEvent(hitDiceEvent))
					{
						return false;
					}
				}
				finally
				{
					PenetrationBonus = hitDiceEvent.PenetrationBonus;
					AV = hitDiceEvent.AV;
					ShieldBlocked = hitDiceEvent.ShieldBlocked;
				}
			}
		}
		return true;
	}
}
