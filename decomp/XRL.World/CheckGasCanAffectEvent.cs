using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Pool)]
public class CheckGasCanAffectEvent : PooledEvent<CheckGasCanAffectEvent>
{
	public new static readonly int CascadeLevel;

	public GameObject Object;

	public GameObject GasObject;

	public Gas Gas;

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
		GasObject = null;
		Gas = null;
	}

	public static bool Check(GameObject Object, GameObject GasObject, Gas Gas = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && GameObject.Validate(ref GasObject))
		{
			CheckGasCanAffectEvent checkGasCanAffectEvent = PooledEvent<CheckGasCanAffectEvent>.FromPool();
			checkGasCanAffectEvent.Object = Object;
			checkGasCanAffectEvent.GasObject = GasObject;
			checkGasCanAffectEvent.Gas = Gas ?? GasObject.GetPart<Gas>();
			flag = Object.HandleEvent(checkGasCanAffectEvent);
		}
		return flag;
	}
}
