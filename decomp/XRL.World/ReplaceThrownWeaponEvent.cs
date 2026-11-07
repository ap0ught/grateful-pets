using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class ReplaceThrownWeaponEvent : PooledEvent<ReplaceThrownWeaponEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject PreviouslyEquipped;

	public BodyPart Part;

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
		PreviouslyEquipped = null;
		Part = null;
	}

	public bool Equip(GameObject Object, bool Silent = false)
	{
		Event obj = Event.New("CommandEquipObject");
		obj.SetParameter("Object", Object);
		obj.SetParameter("BodyPart", Part);
		obj.SetSilent(Silent);
		return Actor.FireEvent(obj);
	}

	public bool EquipOrDestroy(GameObject Object, bool Silent = false)
	{
		Event obj = Event.New("CommandEquipObject");
		obj.SetParameter("Object", Object);
		obj.SetParameter("BodyPart", Part);
		obj.SetSilent(Silent);
		bool flag = Actor.FireEvent(obj);
		if (!flag)
		{
			Object.Obliterate();
		}
		return flag;
	}

	public static bool Check(GameObject Actor, GameObject PreviouslyEquipped, BodyPart Part)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("ReplaceThrownWeapon"))
		{
			Event obj = Event.New("ReplaceThrownWeapon");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("PreviouslyEquipped", PreviouslyEquipped);
			obj.SetParameter("Part", Part);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<ReplaceThrownWeaponEvent>.ID, CascadeLevel))
		{
			ReplaceThrownWeaponEvent replaceThrownWeaponEvent = PooledEvent<ReplaceThrownWeaponEvent>.FromPool();
			replaceThrownWeaponEvent.Actor = Actor;
			replaceThrownWeaponEvent.PreviouslyEquipped = PreviouslyEquipped;
			replaceThrownWeaponEvent.Part = Part;
			flag = Actor.HandleEvent(replaceThrownWeaponEvent);
		}
		return flag;
	}
}
