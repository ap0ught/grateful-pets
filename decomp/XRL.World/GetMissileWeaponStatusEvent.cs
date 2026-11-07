using System.Text;
using Qud.UI;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetMissileWeaponStatusEvent : PooledEvent<GetMissileWeaponStatusEvent>
{
	public GameObject Object;

	public StringBuilder Items;

	public MissileWeaponArea.MissileWeaponAreaWeaponStatus Status;

	public IPart Override;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Items = null;
		Status = null;
		Override = null;
	}

	public static void Send(GameObject Object, StringBuilder Items, MissileWeaponArea.MissileWeaponAreaWeaponStatus Status, IPart Override = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetMissileWeaponStatus"))
		{
			Event obj = Event.New("GetMissileWeaponStatus");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Items", Items);
			obj.SetParameter("Status", Status);
			obj.SetParameter("Override", Override);
			flag = Object.FireEvent(obj);
			Override = obj.GetParameter("Override") as IPart;
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetMissileWeaponStatusEvent>.ID, MinEvent.CascadeLevel))
		{
			GetMissileWeaponStatusEvent getMissileWeaponStatusEvent = PooledEvent<GetMissileWeaponStatusEvent>.FromPool();
			getMissileWeaponStatusEvent.Object = Object;
			getMissileWeaponStatusEvent.Items = Items;
			getMissileWeaponStatusEvent.Status = Status;
			getMissileWeaponStatusEvent.Override = Override;
			flag = Object.HandleEvent(getMissileWeaponStatusEvent);
			Override = getMissileWeaponStatusEvent.Override;
		}
	}
}
