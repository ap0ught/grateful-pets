namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetSpecialEffectChanceEvent : PooledEvent<GetSpecialEffectChanceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Object;

	public GameObject Subject;

	public GameObject Projectile;

	public string Type;

	public int BaseChance;

	public int Chance;

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
		Object = null;
		Subject = null;
		Projectile = null;
		Type = null;
		BaseChance = 0;
		Chance = 0;
	}

	public static int GetFor(GameObject Actor, GameObject Object, string Type = null, int Chance = 0, GameObject Subject = null, GameObject Projectile = null, bool ConstrainToPercentage = true, bool ConstrainToPermillage = false)
	{
		int num = Chance;
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetSpecialEffectChance");
			bool flag3 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetSpecialEffectChance");
			bool flag4 = GameObject.Validate(ref Subject) && Subject.HasRegisteredEvent("GetSpecialEffectChance");
			bool flag5 = GameObject.Validate(ref Projectile) && Projectile.HasRegisteredEvent("GetSpecialEffectChance");
			if (flag2 | flag3 | flag4 | flag5)
			{
				Event obj = Event.New("GetSpecialEffectChance");
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Object", Object);
				obj.SetParameter("Subject", Subject);
				obj.SetParameter("Projectile", Projectile);
				obj.SetParameter("Type", Type);
				obj.SetParameter("BaseChance", num);
				obj.SetParameter("Chance", Chance);
				if (flag & flag2)
				{
					flag = Actor.FireEvent(obj);
				}
				if (flag & flag3)
				{
					flag = Object.FireEvent(obj);
				}
				if (flag & flag4)
				{
					flag = Subject.FireEvent(obj);
				}
				if (flag & flag5)
				{
					flag = Projectile.FireEvent(obj);
				}
				Chance = obj.GetIntParameter("Chance");
			}
		}
		if (flag)
		{
			bool flag6 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetSpecialEffectChanceEvent>.ID, CascadeLevel);
			bool flag7 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetSpecialEffectChanceEvent>.ID, CascadeLevel);
			bool flag8 = GameObject.Validate(ref Subject) && Subject.WantEvent(PooledEvent<GetSpecialEffectChanceEvent>.ID, CascadeLevel);
			bool flag9 = GameObject.Validate(ref Projectile) && Projectile.WantEvent(PooledEvent<GetSpecialEffectChanceEvent>.ID, CascadeLevel);
			if (flag6 | flag7 | flag8 | flag9)
			{
				GetSpecialEffectChanceEvent getSpecialEffectChanceEvent = PooledEvent<GetSpecialEffectChanceEvent>.FromPool();
				getSpecialEffectChanceEvent.Actor = Actor;
				getSpecialEffectChanceEvent.Object = Object;
				getSpecialEffectChanceEvent.Subject = Subject;
				getSpecialEffectChanceEvent.Projectile = Projectile;
				getSpecialEffectChanceEvent.Type = Type;
				getSpecialEffectChanceEvent.BaseChance = num;
				getSpecialEffectChanceEvent.Chance = Chance;
				if (flag & flag6)
				{
					flag = Actor.HandleEvent(getSpecialEffectChanceEvent);
				}
				if (flag & flag7)
				{
					flag = Object.HandleEvent(getSpecialEffectChanceEvent);
				}
				if (flag & flag8)
				{
					flag = Subject.HandleEvent(getSpecialEffectChanceEvent);
				}
				if (flag & flag9)
				{
					flag = Projectile.HandleEvent(getSpecialEffectChanceEvent);
				}
				Chance = getSpecialEffectChanceEvent.Chance;
			}
		}
		if (ConstrainToPercentage)
		{
			if (Chance > 100)
			{
				Chance = 100;
			}
			else if (Chance < 0)
			{
				Chance = 0;
			}
		}
		else if (ConstrainToPermillage)
		{
			if (Chance > 1000)
			{
				Chance = 1000;
			}
			else if (Chance < 0)
			{
				Chance = 0;
			}
		}
		return Chance;
	}
}
