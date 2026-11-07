using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class BeforeFireMissileWeaponsEvent : PooledEvent<BeforeFireMissileWeaponsEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject ApparentTarget;

	public Cell TargetCell;

	public MissilePath Path;

	public List<MissileWeapon> MissileWeapons;

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
		ApparentTarget = null;
		TargetCell = null;
		Path = null;
		MissileWeapons = null;
	}

	public static bool Check(GameObject Actor, GameObject ApparentTarget = null, Cell TargetCell = null, MissilePath Path = null, List<MissileWeapon> MissileWeapons = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("BeforeFireMissileWeapons"))
		{
			Event obj = Event.New("BeforeFireMissileWeapons");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("ApparentTarget", ApparentTarget);
			obj.SetParameter("TargetCell", TargetCell);
			obj.SetParameter("Path", Path);
			obj.SetParameter("MissileWeapons", MissileWeapons);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<BeforeFireMissileWeaponsEvent>.ID, CascadeLevel))
		{
			BeforeFireMissileWeaponsEvent beforeFireMissileWeaponsEvent = PooledEvent<BeforeFireMissileWeaponsEvent>.FromPool();
			beforeFireMissileWeaponsEvent.Actor = Actor;
			beforeFireMissileWeaponsEvent.ApparentTarget = ApparentTarget;
			beforeFireMissileWeaponsEvent.TargetCell = TargetCell;
			beforeFireMissileWeaponsEvent.Path = Path;
			beforeFireMissileWeaponsEvent.MissileWeapons = MissileWeapons;
			flag = Actor.HandleEvent(beforeFireMissileWeaponsEvent);
		}
		return flag;
	}
}
