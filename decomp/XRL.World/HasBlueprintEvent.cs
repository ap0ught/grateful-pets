namespace XRL.World;

[GameEvent(Cascade = 3, Cache = Cache.Pool)]
public class HasBlueprintEvent : PooledEvent<HasBlueprintEvent>
{
	public new static readonly int CascadeLevel = 3;

	public GameObject Object;

	public string Blueprint;

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
		Blueprint = null;
	}

	public static bool Check(GameObject Object, string Blueprint)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("HasBlueprint"))
		{
			Event obj = Event.New("HasBlueprint");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Blueprint", Blueprint);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<HasBlueprintEvent>.ID, CascadeLevel))
		{
			HasBlueprintEvent hasBlueprintEvent = PooledEvent<HasBlueprintEvent>.FromPool();
			hasBlueprintEvent.Object = Object;
			hasBlueprintEvent.Blueprint = Blueprint;
			flag = Object.HandleEvent(hasBlueprintEvent);
		}
		return !flag;
	}
}
