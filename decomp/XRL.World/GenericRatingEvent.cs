namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GenericRatingEvent : PooledEvent<GenericRatingEvent>
{
	public GameObject Object;

	public GameObject Subject;

	public GameObject Source;

	public string Type;

	public int Level;

	public int BaseRating;

	public int Rating;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Subject = null;
		Source = null;
		Type = null;
		Level = 0;
		BaseRating = 0;
		Rating = 0;
	}

	public static int GetFor(GameObject Object, string Type, GameObject Subject = null, GameObject Source = null, int Level = 0, int BaseRating = 0)
	{
		bool flag = true;
		int num = BaseRating;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GenericRating"))
		{
			Event obj = Event.New("GenericRating");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Subject", Subject);
			obj.SetParameter("Source", Source);
			obj.SetParameter("Type", Type);
			obj.SetParameter("Level", Level);
			obj.SetParameter("BaseRating", BaseRating);
			obj.SetParameter("Rating", num);
			flag = Object.FireEvent(obj);
			num = obj.GetIntParameter("Rating");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GenericRatingEvent>.ID, MinEvent.CascadeLevel))
		{
			GenericRatingEvent genericRatingEvent = PooledEvent<GenericRatingEvent>.FromPool();
			genericRatingEvent.Object = Object;
			genericRatingEvent.Subject = Subject;
			genericRatingEvent.Source = Source;
			genericRatingEvent.Type = Type;
			genericRatingEvent.Level = Level;
			genericRatingEvent.BaseRating = BaseRating;
			genericRatingEvent.Rating = num;
			flag = Object.HandleEvent(genericRatingEvent);
			num = genericRatingEvent.Rating;
		}
		return num;
	}
}
