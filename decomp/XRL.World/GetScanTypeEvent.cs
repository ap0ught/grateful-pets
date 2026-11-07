using XRL.World.Capabilities;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetScanTypeEvent : PooledEvent<GetScanTypeEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public Scanning.Scan ScanType;

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
		Object = null;
		ScanType = Scanning.Scan.Bio;
	}

	public static Scanning.Scan GetFor(GameObject Object)
	{
		Scanning.Scan scan = ((!Object.IsAlive) ? Scanning.Scan.Structure : Scanning.Scan.Bio);
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetScanType"))
		{
			Event obj = Event.New("GetScanType");
			obj.SetParameter("Object", Object);
			obj.SetParameter("ScanType", scan);
			flag = Object.FireEvent(obj);
			scan = obj.GetParameter<Scanning.Scan>("ScanType");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetScanTypeEvent>.ID, CascadeLevel))
		{
			GetScanTypeEvent getScanTypeEvent = PooledEvent<GetScanTypeEvent>.FromPool();
			getScanTypeEvent.Object = Object;
			getScanTypeEvent.ScanType = scan;
			Object.HandleEvent(getScanTypeEvent);
			scan = getScanTypeEvent.ScanType;
		}
		return scan;
	}
}
