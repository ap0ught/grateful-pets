namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetToHitModifierEvent : PooledEvent<GetToHitModifierEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Target;

	public GameObject Weapon;

	public GameObject Projectile;

	public GameObject AimedAt;

	public int Modifier;

	public bool Prospective;

	public bool Missile;

	public bool Thrown;

	public string Skill;

	public string Stat;

	public string Checking;

	public bool Melee
	{
		get
		{
			if (!Missile)
			{
				return !Thrown;
			}
			return false;
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
		Actor = null;
		Target = null;
		Weapon = null;
		Projectile = null;
		AimedAt = null;
		Modifier = 0;
		Prospective = false;
		Missile = false;
		Thrown = false;
		Skill = null;
		Stat = null;
		Checking = null;
	}

	public static int GetFor(GameObject Actor, GameObject Target = null, GameObject Weapon = null, int Bonus = 0, GameObject Projectile = null, GameObject AimedAt = null, string Skill = null, string Stat = null, bool Prospective = false, bool Missile = false, bool Thrown = false)
	{
		int num = Bonus + Actor.StatMod("Agility") + Actor.GetIntProperty("HitBonus") + (Weapon?.GetIntProperty("HitBonus") ?? 0) + (Target?.GetIntProperty("IncomingHitBonus") ?? 0);
		if (Missile)
		{
			num += Actor.GetIntProperty("MissileHitBonus") + (Weapon?.GetIntProperty("MissileHitBonus") ?? 0) + (Projectile?.GetIntProperty("HitBonus") ?? 0) + (Projectile?.GetIntProperty("MissileHitBonus") ?? 0) + (Target?.GetIntProperty("IncomingMissileHitBonus") ?? 0);
		}
		else
		{
			num = ((!Thrown) ? (num + (Actor.GetIntProperty("MeleeHitBonus") + (Weapon?.GetIntProperty("MeleeHitBonus") ?? 0) + (Target?.GetIntProperty("IncomingMeleeHitBonus") ?? 0))) : (num + (Actor.GetIntProperty("ThrownHitBonus") + (Weapon?.GetIntProperty("ThrownHitBonus") ?? 0) + (Target?.GetIntProperty("IncomingThrownHitBonus") ?? 0))));
		}
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetToHitModifier");
			bool flag3 = GameObject.Validate(ref Target) && Target.HasRegisteredEvent("GetIncomingToHitModifier");
			if (flag2 | flag3)
			{
				Event obj = Event.New("GetToHitModifier");
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Target", Target);
				obj.SetParameter("Weapon", Weapon);
				obj.SetParameter("Projectile", Projectile);
				obj.SetParameter("AimedAt", AimedAt);
				obj.SetParameter("Modifier", num);
				obj.SetParameter("Skill", Skill);
				obj.SetParameter("Stat", Stat);
				obj.SetFlag("Prospective", Prospective);
				obj.SetFlag("Missile", Missile);
				obj.SetFlag("Thrown", Thrown);
				if (flag & flag2)
				{
					flag = Actor.FireEvent(obj);
				}
				if (flag & flag3)
				{
					obj.ID = "GetIncomingToHitModifier";
					flag = Target.FireEvent(obj);
				}
				num = obj.GetIntParameter("Modifier");
			}
		}
		if (flag)
		{
			bool flag4 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetToHitModifierEvent>.ID, CascadeLevel);
			bool flag5 = GameObject.Validate(ref Target) && Target.WantEvent(PooledEvent<GetToHitModifierEvent>.ID, CascadeLevel);
			if (flag4 | flag5)
			{
				GetToHitModifierEvent getToHitModifierEvent = PooledEvent<GetToHitModifierEvent>.FromPool();
				getToHitModifierEvent.Actor = Actor;
				getToHitModifierEvent.Target = Target;
				getToHitModifierEvent.Weapon = Weapon;
				getToHitModifierEvent.Projectile = Projectile;
				getToHitModifierEvent.AimedAt = AimedAt;
				getToHitModifierEvent.Modifier = num;
				getToHitModifierEvent.Skill = Skill;
				getToHitModifierEvent.Stat = Stat;
				getToHitModifierEvent.Prospective = Prospective;
				getToHitModifierEvent.Missile = Missile;
				getToHitModifierEvent.Thrown = Thrown;
				if (flag & flag4)
				{
					getToHitModifierEvent.Checking = "Actor";
					flag = Actor.HandleEvent(getToHitModifierEvent);
				}
				if (flag & flag5)
				{
					getToHitModifierEvent.Checking = "Target";
					flag = Target.HandleEvent(getToHitModifierEvent);
				}
				num = getToHitModifierEvent.Modifier;
			}
		}
		return num;
	}
}
